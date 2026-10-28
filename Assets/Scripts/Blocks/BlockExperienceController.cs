using System;
using System.IO;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using HuliacDev.Core;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Exhibition.Blocks
{
    [DisallowMultipleComponent]
    public sealed class BlockExperienceController : MonoBehaviour
    {
        [SerializeField] private TMP_Text groupText, productText, color1Text, color2Text;
        [SerializeField] private Image resultImage;
        [SerializeField] private GameObject resultTextGroup;
        [Tooltip("Shown only while a generated image is ready to open in the Result scene.")]
        [SerializeField] private GameObject pressButtonText;
        [SerializeField] private bool enableEditorKeyboard = true;

        private readonly BlockSelection _selection = new BlockSelection();
        private BlockCatalog _catalog;
        private BlockColoringCatalog _coloring;
        private BlockJournal _journal;
        private GameManager _manager;
        private InactivityTimer _timer;
        private CancellationTokenSource _screenCts, _colorCts;
        private Sprite _originalSprite, _generatedSprite;
        private Texture2D _generatedTexture, _handedOffTexture;
        private bool _ready;
        private int _popup = -1;
        private Vector2 _scroll;
        public string Status { get; private set; } = "Loading block configuration...";
        public bool IsGenerating => _colorCts != null;
        public bool IsReady => _ready;
        public BlockSelection Selection => _selection;
        public BlockCatalog Catalog => _catalog;

#if UNITY_EDITOR
        private bool _oldCursorVisible;
        private CursorLockMode _oldCursorLock;
#endif

        private void Awake()
        {
            if (resultImage) _originalSprite = resultImage.sprite;
        }

        private void OnEnable()
        {
            ClearImage();
            _screenCts = new CancellationTokenSource();
            InitializeAsync(_screenCts.Token).Forget();
#if UNITY_EDITOR
            _oldCursorVisible = Cursor.visible;
            _oldCursorLock = Cursor.lockState;
            if (enableEditorKeyboard) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
#endif
        }

        private async UniTask InitializeAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                _manager = FindFirstObjectByType<GameManager>();
                _timer = FindFirstObjectByType<InactivityTimer>();
                if (!groupText || !productText || !color1Text || !color2Text || !resultImage || !resultTextGroup || !pressButtonText)
                    throw new InvalidOperationException("Assign all four OptionText fields, ResultImage, TextGroup and PressButtonText.");
                _catalog = JsonUtility.FromJson<BlockCatalog>(await LoadConfigurationTextAsync("BlockCatalog.json", token));
                _catalog.Validate();
                _coloring = JsonUtility.FromJson<BlockColoringCatalog>(await LoadConfigurationTextAsync("BlockColoring.json", token));
                if (_coloring == null) throw new InvalidOperationException("Coloring catalog is missing.");
                _coloring.Validate(_catalog);
                var settings = JsonUtility.FromJson<BlockSettings>(await LoadConfigurationTextAsync("BlockSettings.json", token));
                if (settings == null) throw new InvalidOperationException("Block settings are missing.");
                settings.Validate();
                _journal = new BlockJournal(Path.Combine(Application.persistentDataPath, "BlockData"),
                    settings.maxJournalFiles, settings.maxJournalMegabytes);
                _selection.Clear();
                RefreshLabels();
                await _journal.AppendAsync("session_started", new JObject { ["language"] = Language, ["catalog_schema"] = _catalog.schemaVersion });
                token.ThrowIfCancellationRequested();
                _ready = true;
#if UNITY_EDITOR
                // The persistent manager hides the cursor in Start, after this component's OnEnable.
                if (enableEditorKeyboard) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
#endif
                Status = "Place four blocks. Keys: 2 Group / 3 Product / 4 Color1 / 5 Color2.";
                Debug.Log("[BlockExperience] Block configuration loaded. Language: " + Language + ".", this);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!token.IsCancellationRequested) Fail("Block initialization failed", ex); }
        }

        private string Language => _manager && _manager.HasLanguageSelection ? _manager.LanguageCode : "en";

        // Hardware adapters can use these methods on the Unity main thread later.
        public void RecognizeBlock(BlockSlot slot, string id)
        {
            if (!_ready || (_manager && _manager.IsTransitioning)) return;
            int previousRevision = _selection.Revision;
            _selection.Set(_catalog, slot, id);
            if (previousRevision == _selection.Revision) return;
            _popup = -1;
            SelectionChanged("block_recognized", slot, id);
        }

        public void RemoveBlock(BlockSlot slot)
        {
            if (!_ready) return;
            string previous = _selection[slot];
            if (previous == null) return;
            _selection.Remove(slot);
            SelectionChanged("block_removed", slot, previous);
        }

        public void ToggleSlot(BlockSlot slot)
        {
            if (!_ready) return;
            _timer?.ResetTimer();
            if (_selection[slot] != null) { RemoveBlock(slot); _popup = -1; }
            else _popup = _popup == (int)slot ? -1 : (int)slot;
        }

        private void SelectionChanged(string kind, BlockSlot slot, string id)
        {
            CancelGeneration();
            ClearImage();
            RefreshLabels();
            _timer?.ResetTimer();
            Status = _selection.Complete ? "Applying selected colors..." : "Waiting for four blocks.";
            Debug.Log("[BlockExperience] " + (kind == "block_removed" ? "Block removed" : "Block recognized") +
                ". Slot: " + slot + "; ID: " + id + "; complete: " + _selection.Complete + ".", this);
            WriteEventAsync(kind, new JObject { ["slot"] = slot.ToString(), ["block_id"] = id,
                ["revision"] = _selection.Revision, ["selection"] = Snapshot() }).Forget();
            if (_selection.Complete) GenerateImage();
        }

        private JObject Snapshot() => new JObject {
            ["group"] = _selection[BlockSlot.Group], ["product"] = _selection[BlockSlot.Product],
            ["color1"] = _selection[BlockSlot.Color1], ["color2"] = _selection[BlockSlot.Color2]
        };

        private void RefreshLabels()
        {
            SetLabel(groupText, BlockSlot.Group);
            SetLabel(productText, BlockSlot.Product);
            SetLabel(color1Text, BlockSlot.Color1);
            SetLabel(color2Text, BlockSlot.Color2);
        }

        private void SetLabel(TMP_Text label, BlockSlot slot)
        {
            if (label) label.text = _selection[slot] == null ? "—" : _catalog.Find(slot, _selection[slot]).label;
        }

        // Existing UnityEvents can also retry the current local coloring operation.
        public void GenerateImage()
        {
            if (!_ready || !_selection.Complete || IsGenerating || (_manager && _manager.IsTransitioning)) return;
            ColorImageAsync().Forget();
        }

        private async UniTask ColorImageAsync()
        {
            var source = _manager && _manager.HasLanguageSelection
                ? CancellationTokenSource.CreateLinkedTokenSource(_screenCts.Token, _manager.SessionCancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(_screenCts.Token);
            _colorCts = source;
            var token = source.Token;
            int revision = _selection.Revision;
            var journal = _journal;
            var selection = Snapshot();
            Texture2D pending = null;
            try
            {
                ClearImage();
                Status = "Applying selected colors...";
                var template = _coloring.Find(_selection[BlockSlot.Group], _selection[BlockSlot.Product]);
                var primary = _coloring.GetColor(_selection[BlockSlot.Color1]);
                var secondary = _coloring.GetColor(_selection[BlockSlot.Color2]);
                pending = await LocalBlockColoring.RenderAsync(template, primary, secondary, token);
                token.ThrowIfCancellationRequested();
                if (!TryDisplayResult(pending, revision)) return;
                int width = pending.width, height = pending.height;
                pending = null;
                Debug.Log("[BlockExperience] Local coloring completed. Template: " + template.groupId + "/" + template.resolvedProductId + ".", this);
                await WriteEventAsync("coloring_completed", new JObject { ["revision"] = revision,
                    ["selection"] = selection, ["resolved_product"] = template.resolvedProductId,
                    ["base_image"] = template.baseImage, ["mask_image"] = template.maskImage,
                    ["primary_hex"] = ColorUtility.ToHtmlStringRGB(primary), ["secondary_hex"] = ColorUtility.ToHtmlStringRGB(secondary),
                    ["width"] = width, ["height"] = height }, journal);
            }
            catch (OperationCanceledException)
            {
                if (this && isActiveAndEnabled && revision == _selection.Revision && _colorCts == source)
                    Status = "Coloring cancelled. Retry or change blocks.";
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested && this && isActiveAndEnabled && revision == _selection.Revision)
                    Fail("Coloring failed. Retry or change blocks", ex);
            }
            finally
            {
                if (pending) Destroy(pending);
                if (_colorCts == source) _colorCts = null;
                source.Dispose();
            }
        }

        public void CancelGeneration()
        {
            var source = _colorCts;
            _colorCts = null;
            source?.Cancel();
            if (source != null && isActiveAndEnabled) Status = "Coloring cancelled. Retry or change blocks.";
        }

        /// <summary>
        /// Main-thread result bridge. Capture Selection.Revision before
        /// starting work. A true result transfers ownership; a false result leaves ownership with the caller.
        /// The caller must also cancel its work on screen/session exit.
        /// </summary>
        public bool TryDisplayResult(Texture2D texture, int selectionRevision)
        {
            if (!_ready || !isActiveAndEnabled || !_selection.Complete || !texture ||
                selectionRevision != _selection.Revision || (_manager && _manager.IsTransitioning)) return false;
            if (texture == _generatedTexture || texture == _handedOffTexture) return true;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
            ClearImage();
            _generatedTexture = texture;
            _generatedSprite = sprite;
            resultImage.sprite = sprite;
            resultImage.preserveAspect = true;
            resultImage.gameObject.SetActive(true);
            resultTextGroup.SetActive(false);
            if (_manager && _manager.SetResultTexture(texture))
            {
                _handedOffTexture = texture;
                _generatedTexture = null;
                pressButtonText.SetActive(true);
            }
            Status = "Image ready.";
            return true;
        }

        private static async UniTask<string> LoadConfigurationTextAsync(string file, CancellationToken token)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "BlockGeneration", file);
            using (var request = UnityWebRequest.Get(path.Contains("://") ? path : new Uri(path).AbsoluteUri))
            {
                request.timeout = 15;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                return request.downloadHandler.text;
            }
        }

        private async UniTask WriteEventAsync(string kind, JObject data, BlockJournal target = null)
        {
            try { var journal = target ?? _journal; if (journal != null) await journal.AppendAsync(kind, data); }
            catch (Exception ex) { if (this) Fail("Block data could not be saved", ex); }
        }

        private void Fail(string message, Exception ex)
        {
            Status = message + ". " + ex.Message;
            Debug.LogError("[BlockExperience] " + message + ": " + ex.Message, this);
            Debug.LogException(ex, this);
        }

        private void ClearImage(bool releaseResult = true)
        {
            if (pressButtonText) pressButtonText.SetActive(false);
            if (resultImage)
            {
                resultImage.gameObject.SetActive(false);
                resultImage.sprite = _originalSprite;
            }
            if (resultTextGroup) resultTextGroup.SetActive(true);
            if (_generatedSprite) Destroy(_generatedSprite);
            if (_generatedTexture) Destroy(_generatedTexture);
            if (releaseResult && _handedOffTexture && _manager && _manager.ResultTexture == _handedOffTexture)
                _manager.ClearResultTexture();
            _generatedSprite = null;
            _generatedTexture = null;
            _handedOffTexture = null;
        }

        private void OnDisable()
        {
            _ready = false;
            _popup = -1;
            CancelGeneration();
            _screenCts?.Cancel();
            _screenCts?.Dispose();
            _screenCts = null;
            _selection.Clear();
            if (_catalog != null) RefreshLabels();
            // Keep the handed-off image while a scene transition (e.g. to Result) unloads this screen.
            ClearImage(!(_manager && _manager.IsTransitioning));
            WriteEventAsync("session_ended", new JObject()).Forget();
            Debug.Log("[BlockExperience] Block session ended.", this);
#if UNITY_EDITOR
            if (enableEditorKeyboard) { Cursor.visible = _oldCursorVisible; Cursor.lockState = _oldCursorLock; }
#endif
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!_ready || !enableEditorKeyboard || !Application.isFocused) return;
            var keys = Keyboard.current;
            if (keys == null) return;
            if (keys.digit2Key.wasPressedThisFrame || keys.numpad2Key.wasPressedThisFrame) ToggleSlot(BlockSlot.Group);
            if (keys.digit3Key.wasPressedThisFrame || keys.numpad3Key.wasPressedThisFrame) ToggleSlot(BlockSlot.Product);
            if (keys.digit4Key.wasPressedThisFrame || keys.numpad4Key.wasPressedThisFrame) ToggleSlot(BlockSlot.Color1);
            if (keys.digit5Key.wasPressedThisFrame || keys.numpad5Key.wasPressedThisFrame) ToggleSlot(BlockSlot.Color2);
        }

        private void OnGUI()
        {
            if (!enableEditorKeyboard) return;
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Max(.5f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float w = Mathf.Min(720, Screen.width / scale - 24);
            GUILayout.BeginArea(new Rect(12, 12, w, 220), GUI.skin.box);
            GUILayout.Label("RFID Simulator: 2 Group | 3 Product | 4 Color1 | 5 Color2");
            GUILayout.Label(Status);
            GUI.enabled = _ready && _selection.Complete && !IsGenerating;
            if (GUILayout.Button("Apply colors / Retry", GUILayout.Height(36))) { _timer?.ResetTimer(); GenerateImage(); }
            GUI.enabled = IsGenerating;
            if (GUILayout.Button("Cancel coloring", GUILayout.Height(30)))
            {
                CancelGeneration();
                Status = "Coloring cancelled. Retry or change blocks.";
            }
            GUI.enabled = true;
            GUILayout.EndArea();
            if (_ready && _popup >= 0)
            {
                GUILayout.BeginArea(new Rect(12, 240, w, Mathf.Min(690, Screen.height / scale - 252)), GUI.skin.box);
                GUILayout.Label("Select " + ((BlockSlot)_popup) + " block");
                _scroll = GUILayout.BeginScrollView(_scroll);
                foreach (var option in _catalog.Options((BlockSlot)_popup))
                    if (GUILayout.Button(option.label, GUILayout.Height(38))) { RecognizeBlock((BlockSlot)_popup, option.id); break; }
                GUILayout.EndScrollView();
                if (GUILayout.Button("Close")) _popup = -1;
                GUILayout.EndArea();
            }
            GUI.matrix = oldMatrix;
        }
#endif
    }
}
