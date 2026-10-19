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
        private ComfyBlockSettings _settings;
        private ComfyImageClient _client;
        private BlockJournal _journal;
        private GameManager _manager;
        private InactivityTimer _timer;
        private GeneratedImageRetention _retention;
        private CancellationTokenSource _screenCts, _requestCts;
        private Sprite _originalSprite, _generatedSprite;
        private Texture2D _generatedTexture, _handedOffTexture;
        private bool _ready, _pausedTimer;
        private int _popup = -1;
        private Vector2 _scroll;
        public string Status { get; private set; } = "Loading block configuration...";
        public bool IsGenerating { get; private set; }
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
                _retention = FindFirstObjectByType<GeneratedImageRetention>();
                if (!groupText || !productText || !color1Text || !color2Text || !resultImage || !resultTextGroup || !pressButtonText)
                    throw new InvalidOperationException("Assign all four OptionText fields, ResultImage, TextGroup and PressButtonText.");
                _catalog = JsonUtility.FromJson<BlockCatalog>(await ComfyImageClient.LoadText("BlockCatalog.json", token));
                _catalog.Validate();
                _settings = JsonUtility.FromJson<ComfyBlockSettings>(await ComfyImageClient.LoadText("ComfyUISettings.json", token));
                _settings.Validate();
                _client = new ComfyImageClient(_settings);
                _journal = new BlockJournal(Path.Combine(Application.persistentDataPath, "BlockData"),
                    _settings.maxJournalFiles, _settings.maxJournalMegabytes);
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
            catch (Exception ex) { Fail("Block initialization failed", ex); }
        }

        private string Language => _manager && _manager.HasLanguageSelection ? _manager.LanguageCode : "en";

        // Hardware adapters can use these methods on the Unity main thread later.
        public void RecognizeBlock(BlockSlot slot, string id)
        {
            if (!_ready || (_manager && _manager.IsTransitioning)) return;
            _selection.Set(_catalog, slot, id);
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
            Status = _selection.Complete ? "Four blocks recognized. Ready to generate." : "Waiting for four blocks.";
            Debug.Log("[BlockExperience] " + (kind == "block_removed" ? "Block removed" : "Block recognized") +
                ". Slot: " + slot + "; ID: " + id + "; complete: " + _selection.Complete + ".", this);
            WriteEventAsync(kind, new JObject { ["slot"] = slot.ToString(), ["block_id"] = id,
                ["revision"] = _selection.Revision, ["selection"] = Snapshot() }).Forget();
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

        public void GenerateImage()
        {
            if (!_ready || !_selection.Complete || IsGenerating || (_manager && _manager.IsTransitioning)) return;
            GenerateAsync().Forget();
        }

        private async UniTask GenerateAsync()
        {
            IsGenerating = true;
            var screenToken = _screenCts.Token;
            var client = _client;
            var journal = _journal;
            var source = _manager && _manager.HasLanguageSelection
                ? CancellationTokenSource.CreateLinkedTokenSource(screenToken, _manager.SessionCancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(screenToken);
            _requestCts = source;
            var timeout = source.CancelAfterSlim(TimeSpan.FromSeconds(_settings.generationTimeoutSeconds), DelayType.Realtime);
            var token = source.Token;
            string requestId = Guid.NewGuid().ToString("N");
            int revision = _selection.Revision;
            string promptId = null;
            Texture2D pendingTexture = null;
            try
            {
                ClearImage();
                Status = "Preparing image generation...";
                Debug.Log("[BlockExperience] Image generation started. Request: " + requestId + ".", this);
                _timer?.Pause();
                _pausedTimer = true;
                var prompt = _selection.CreatePrompt(_catalog, UnityEngine.Random.Range(0, _catalog.equipmentVariants.Length));
                long seed = DateTime.UtcNow.Ticks & 0x7FFFFFFFFFFFFFFF;
                var graph = JObject.Parse(await ComfyImageClient.LoadText("product_480.api.json", token));
                graph["10"]["inputs"]["value"] = prompt.audienceFragment;
                graph["11"]["inputs"]["value"] = prompt.productFragment;
                graph["12"]["inputs"]["value"] = prompt.primaryFragment;
                graph["13"]["inputs"]["value"] = prompt.secondaryFragment;
                graph["17"]["inputs"]["string_a"] = _catalog.stylePrefix;
                graph["18"]["inputs"]["string_b"] = _catalog.styleSuffix;
                graph["3"]["inputs"]["text"] = prompt.negative;
                graph["5"]["inputs"]["seed"] = seed;
                graph["7"]["inputs"]["filename_prefix"] = "Thai02/blocks_" + requestId;
                // Persist exact inputs, resolved equipment, prompts, seed, model and graph BEFORE submitting.
                await journal.AppendAsync("generation_requested", new JObject {
                    ["request_id"] = requestId, ["language"] = Language, ["revision"] = revision,
                    ["prompt"] = JObject.FromObject(prompt), ["seed"] = seed, ["workflow"] = graph.DeepClone()
                });
                token.ThrowIfCancellationRequested();
                if (!_retention) throw new InvalidOperationException("Image retention service is unavailable. Check GameLifetimeScope.");
                _retention.ProtectRequest(requestId);
                await _retention.EnsureCapacityAsync(token);
                var queue = await client.Json("/queue", null, token);
                if ((queue["queue_running"] as JArray)?.Count > 0 || (queue["queue_pending"] as JArray)?.Count > 0)
                    throw new InvalidOperationException("ComfyUI is busy. Try again when the queue is empty.");
                var response = await client.Json("/prompt", new JObject { ["client_id"] = requestId, ["prompt"] = graph }, token);
                promptId = (string)response["prompt_id"];
                if (string.IsNullOrEmpty(promptId)) throw new InvalidOperationException("ComfyUI rejected the workflow.");
                await journal.AppendAsync("generation_queued", new JObject { ["request_id"] = requestId, ["prompt_id"] = promptId });
                Status = "Generating image...";
                Debug.Log("[BlockExperience] Image generation queued. Prompt: " + promptId + ".", this);
                var file = await client.WaitForImage(promptId, token);
                pendingTexture = await client.Download(file, token);
                await journal.AppendAsync("generation_completed", new JObject {
                    ["request_id"] = requestId, ["prompt_id"] = promptId, ["server_image"] = file.DeepClone(),
                    ["width"] = pendingTexture.width, ["height"] = pendingTexture.height
                });
                token.ThrowIfCancellationRequested();
                if (!this || !isActiveAndEnabled || revision != _selection.Revision)
                {
                    Debug.Log("[BlockExperience] Generated image discarded because the screen or selection changed. Request: " + requestId + ".");
                    return;
                }
                _generatedTexture = pendingTexture;
                pendingTexture = null;
                _generatedSprite = Sprite.Create(_generatedTexture, new Rect(0, 0, 480, 480), new Vector2(.5f, .5f));
                resultImage.sprite = _generatedSprite;
                resultImage.preserveAspect = true;
                // Bind the fully downloaded image before enabling the view; never show an empty Image.
                resultImage.gameObject.SetActive(true);
                resultTextGroup.SetActive(false);
                // The persistent manager owns the texture from here so the Result scene can show it.
                if (_manager && _manager.SetResultTexture(_generatedTexture))
                {
                    _handedOffTexture = _generatedTexture;
                    _generatedTexture = null;
                    pressButtonText.SetActive(true);
                }
                Status = "Image ready. Change blocks or generate again.";
                Debug.Log("[BlockExperience] Image generation completed.", this);
            }
            catch (OperationCanceledException)
            {
                Debug.Log(screenToken.IsCancellationRequested
                    ? "[BlockExperience] Image generation cancelled by screen exit. Request: " + requestId + "."
                    : "[BlockExperience] Image generation cancelled or timed out. Request: " + requestId + ".");
                if (this && isActiveAndEnabled && revision == _selection.Revision)
                    Status = screenToken.IsCancellationRequested ? "Generation cancelled." : "Generation cancelled or timed out. Try again.";
                await WriteEventAsync("generation_cancelled", new JObject { ["request_id"] = requestId,
                    ["prompt_id"] = promptId, ["server_work_may_continue"] = true }, journal);
            }
            catch (Exception ex)
            {
                if (this && isActiveAndEnabled) Fail("Image generation failed. Try again", ex);
                await WriteEventAsync("generation_failed", new JObject { ["request_id"] = requestId,
                    ["prompt_id"] = promptId, ["error_type"] = ex.GetType().Name, ["message"] = ex.Message }, journal);
            }
            finally
            {
                if (_retention) _retention.ReleaseRequest(requestId);
                if (pendingTexture) Destroy(pendingTexture);
                if (source.IsCancellationRequested && promptId != null) client.RemovePending(promptId).Forget();
                if (_pausedTimer) { _timer?.Resume(); _pausedTimer = false; }
                if (_requestCts == source) _requestCts = null;
                timeout.Dispose();
                source.Dispose();
                IsGenerating = false;
            }
        }

        public void CancelGeneration() => _requestCts?.Cancel();

        private async UniTask WriteEventAsync(string kind, JObject data, BlockJournal target = null)
        {
            try { var journal = target ?? _journal; if (journal != null) await journal.AppendAsync(kind, data); }
            catch (Exception ex) { if (this) Fail("Block data could not be saved", ex); }
        }

        private void Fail(string message, Exception ex)
        {
            Status = message + ". " + ex.Message;
            Debug.LogError("[BlockExperience] " + message + ": " + ex.Message, this);
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
            if (_pausedTimer) { _timer?.Resume(); _pausedTimer = false; }
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
            if (GUILayout.Button("Generate image", GUILayout.Height(36))) { _timer?.ResetTimer(); GenerateImage(); }
            GUI.enabled = IsGenerating;
            if (GUILayout.Button("Cancel generation", GUILayout.Height(30))) CancelGeneration();
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
