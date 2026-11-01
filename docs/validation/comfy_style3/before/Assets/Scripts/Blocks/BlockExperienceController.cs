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
        [Tooltip("Image hint for the physical continue button; incomplete selections are guarded by GameManager.")]
        [SerializeField] private GameObject pressButtonText;
        [SerializeField] private bool enableEditorKeyboard = true;

        public event Action PresentationChanged;
        [SerializeField] private bool showEditorDiagnostics;

        private readonly BlockSelection _selection = new BlockSelection();
        private BlockCatalog _catalog;
        private ComfyBlockSettings _settings;
        private ComfyImageClient _client;
        private GeneratedImageRetention _retention;
        private IDisposable _generationLock;
        private bool _pausedTimer;
        private BlockJournal _journal;
        private RfidReaderHub _readerHub;
        private GameManager _manager;
        private InactivityTimer _timer;
        private CancellationTokenSource _screenCts, _requestCts;
        private Sprite _originalSprite, _generatedSprite;
        private Texture2D _generatedTexture, _handedOffTexture;
        private bool _ready;
        private int _popup = -1;
        private Vector2 _scroll;
        public string Status { get; private set; } = "Loading block configuration...";
        public bool IsGenerating => _requestCts != null;
        public bool HasGeneratedImage => _handedOffTexture || _generatedTexture;
        public bool IsReady => _ready;
        public BlockSelection Selection => _selection;
        public BlockCatalog Catalog => _catalog;
        public RfidReaderHub ReaderHub => _readerHub;
        public bool HasRequiredComposition => _readerHub && _readerHub.Readers.HasRequiredComposition && _selection.Complete;
        public bool HasSingleGroup => _readerHub && _readerHub.Readers.GroupCount == 1 && _selection[BlockSlot.Group] != null;

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
                _readerHub = FindFirstObjectByType<RfidReaderHub>();
                _retention = FindFirstObjectByType<GeneratedImageRetention>();
                if (!_readerHub) throw new InvalidOperationException("RFID reader hub is missing. Start from Idle.");
                if (!resultImage || !resultTextGroup || !pressButtonText)
                    throw new InvalidOperationException("Assign ResultImage, TextGroup and PressButtonText.");
                _catalog = JsonUtility.FromJson<BlockCatalog>(await LoadConfigurationTextAsync("BlockCatalog.json", token));
                _catalog.Validate();
                _settings = JsonUtility.FromJson<ComfyBlockSettings>(await LoadConfigurationTextAsync("ComfyUISettings.json", token));
                if (_settings == null) throw new InvalidOperationException("ComfyUI settings are missing.");
                _settings.Validate();
                _client = new ComfyImageClient(_settings);
                _journal = new BlockJournal(Path.Combine(Application.persistentDataPath, "BlockData"),
                    _settings.maxJournalFiles, _settings.maxJournalMegabytes);
                _selection.Clear();
                RefreshLabels();
                await _journal.AppendAsync("session_started", new JObject { ["language"] = Language, ["catalog_schema"] = _catalog.schemaVersion });
                token.ThrowIfCancellationRequested();
                _ready = true;
                _readerHub.Changed += SynchronizeReaders;
                _readerHub.ReaderChanged += RecordReaderChange;
                SynchronizeReaders();
                for (int reader = 0; reader < 4; reader++)
                    if (_readerHub.Readers[reader].HasValue) RecordReaderChange(reader, null, _readerHub.Readers[reader]);
#if UNITY_EDITOR
                // The persistent manager hides the cursor in Start, after this component's OnEnable.
                if (enableEditorKeyboard) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
#endif
                if (!HasRequiredComposition) Status = "Place 1 group, 1 product and 2 color blocks. F9 opens the RFID simulator.";
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
            _catalog.Find(slot, id);
            _readerHub.RecognizeReader(DefaultReader(slot), Kind(slot), id);
        }

        public void RemoveBlock(BlockSlot slot)
        {
            if (!_ready) return;
            _readerHub.RemoveReader(DefaultReader(slot));
        }

        public void ToggleSlot(BlockSlot slot)
        {
            if (!_ready || (_manager && _manager.IsPresentationLocked)) return;
            _timer?.ResetTimer();
            if (_selection[slot] != null) { RemoveBlock(slot); _popup = -1; }
            else _popup = _popup == (int)slot ? -1 : (int)slot;
        }

        private static int DefaultReader(BlockSlot slot) => slot == BlockSlot.Group ? 3 : (int)slot - 1;
        private static BlockKind Kind(BlockSlot slot) => slot == BlockSlot.Group ? BlockKind.Group :
            slot == BlockSlot.Product ? BlockKind.Product : BlockKind.Color;

        private void SynchronizeReaders()
        {
            if (!_ready) return;
            _popup = -1;
            CancelGeneration();
            ClearImage();
            var readers = _readerHub.Readers;
            SetSelection(BlockSlot.Group, readers.GroupId);
            SetSelection(BlockSlot.Product, readers.ProductId);
            SetSelection(BlockSlot.Color1, readers.Color1Id);
            SetSelection(BlockSlot.Color2, readers.Color2Id);
            _timer?.ResetTimer();
            Status = "Waiting for 1 group, 1 product and 2 color blocks.";
            if (HasRequiredComposition) GenerateImage();
            RefreshLabels();
        }

        private void SetSelection(BlockSlot slot, string id)
        {
            if (id == null) _selection.Remove(slot);
            else _selection.Set(_catalog, slot, id);
        }

        private void RecordReaderChange(int reader, RfidBlock? previous, RfidBlock? current)
        {
            if (previous.HasValue) RecordReader("block_removed", reader, previous.Value);
            if (current.HasValue) RecordReader("block_recognized", reader, current.Value);
        }

        private void RecordReader(string kind, int reader, RfidBlock block)
        {
            int colorsBefore = 0;
            for (int i = 0; i < reader; i++) if (_readerHub.Readers[i]?.Kind == BlockKind.Color) colorsBefore++;
            var slot = block.Kind == BlockKind.Group ? BlockSlot.Group : block.Kind == BlockKind.Product ? BlockSlot.Product :
                colorsBefore == 0 ? BlockSlot.Color1 : BlockSlot.Color2;
            Debug.Log("[RFID] " + kind + ". Reader: " + (reader + 1) + "; kind: " + block.Kind + "; ID: " + block.Id + ".", this);
            WriteEventAsync(kind, new JObject { ["slot"] = slot.ToString(), ["reader_index"] = reader,
                ["block_kind"] = block.Kind.ToString(), ["block_id"] = block.Id, ["revision"] = _selection.Revision,
                ["reader_revision"] = _readerHub.Readers.Revision, ["selection"] = Snapshot() }).Forget();
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
            PresentationChanged?.Invoke();
        }

        private void SetLabel(TMP_Text label, BlockSlot slot)
        {
            if (label) label.text = _selection[slot] == null ? "—" : _catalog.Find(slot, _selection[slot]).label;
        }

        // Existing UnityEvents can retry a failed or cancelled generation.
        public void GenerateImage()
        {
            if (!_ready || !HasRequiredComposition || IsGenerating || (_manager && _manager.IsTransitioning)) return;
            GenerateAsync().Forget();
        }

        private async UniTask GenerateAsync()
        {
            var source = _manager && _manager.HasLanguageSelection
                ? CancellationTokenSource.CreateLinkedTokenSource(_screenCts.Token, _manager.SessionCancellationToken)
                : CancellationTokenSource.CreateLinkedTokenSource(_screenCts.Token);
            _requestCts = source;
            var timeout = source.CancelAfterSlim(TimeSpan.FromSeconds(_settings.generationTimeoutSeconds), DelayType.Realtime);
            var token = source.Token;
            int revision = _selection.Revision;
            var journal = _journal;
            var client = _client;
            string requestId = Guid.NewGuid().ToString("N"), promptId = null;
            Texture2D pending = null;
            try
            {
                _generationLock = _manager ? _manager.Presentation.LockInput() : null;
                _timer?.Pause();
                _pausedTimer = true;
                ClearImage();
                Status = "Preparing image generation...";
                PresentationChanged?.Invoke();
                var prompt = _selection.CreatePrompt(_catalog, 0);
                long seed = DateTime.UtcNow.Ticks & 0x7FFFFFFFFFFFFFFF;
                var graph = JObject.Parse(await ComfyImageClient.LoadText("product_480.api.json", token));
                graph["10"]["inputs"]["value"] = prompt.audienceFragment;
                graph["11"]["inputs"]["value"] = prompt.productFragment;
                graph["12"]["inputs"]["value"] = prompt.primaryFragment;
                graph["13"]["inputs"]["value"] = prompt.secondaryFragment;
                graph["12"]["_meta"]["color_reference"] = prompt.primaryReference;
                graph["13"]["_meta"]["color_reference"] = prompt.secondaryReference;
                graph["17"]["inputs"]["string_b"] = _catalog.stylePrefix;
                graph["18"]["inputs"]["string_b"] = _catalog.styleSuffix;
                graph["3"]["inputs"]["text"] = prompt.negative;
                bool turboSampler = (string)graph["5"]["class_type"] == "SamplerCustom";
                graph["5"]["inputs"][turboSampler ? "noise_seed" : "seed"] = seed;
                graph["7"]["inputs"]["filename_prefix"] = "Thai02/blocks_" + requestId;
                graph["5"]["inputs"]["positive"] = new JArray("2", 0);
                // Turbo runs without classifier-free guidance; reuse positive conditioning to skip an unused encode.
                graph["5"]["inputs"]["negative"] = new JArray(turboSampler ? "2" : "3", 0);
                string controlModel = (string)graph["25"]?["inputs"]?["control_net_name"];
                foreach (string node in new[] { "24", "25", "26", "27" }) graph.Remove(node);
                if (!string.IsNullOrEmpty(prompt.poseGuide))
                {
                    if (string.IsNullOrEmpty(controlModel)) throw new InvalidOperationException("The workflow is missing its product control model.");
                    graph["24"] = new JObject { ["class_type"] = "LoadImage", ["inputs"] = new JObject { ["image"] = "Thai02Quality/" + prompt.poseGuide } };
                    graph["25"] = new JObject { ["class_type"] = "ControlNetLoader", ["inputs"] = new JObject { ["control_net_name"] = controlModel } };
                    // The block artwork is intentionally pale; photo thresholds erase its headbands/bodies.
                    graph["27"] = new JObject { ["class_type"] = "Canny", ["inputs"] = new JObject { ["image"] = new JArray("24", 0), ["low_threshold"] = prompt.poseLowThreshold, ["high_threshold"] = prompt.poseHighThreshold } };
                    graph["26"] = new JObject { ["class_type"] = "ControlNetApplyAdvanced", ["inputs"] = new JObject {
                        ["positive"] = new JArray("2", 0), ["negative"] = new JArray("3", 0), ["control_net"] = new JArray("25", 0),
                        ["image"] = new JArray("27", 0), ["vae"] = new JArray("1", 2), ["strength"] = prompt.poseStrength,
                        ["start_percent"] = 0, ["end_percent"] = prompt.poseEndPercent } };
                    graph["5"]["inputs"]["positive"] = new JArray("26", 0);
                    graph["5"]["inputs"]["negative"] = new JArray("26", 1);
                }
                await journal.AppendAsync("generation_requested", new JObject {
                    ["request_id"] = requestId, ["language"] = Language, ["revision"] = revision,
                    ["prompt"] = JObject.FromObject(prompt), ["seed"] = seed, ["workflow"] = graph.DeepClone()
                });
                token.ThrowIfCancellationRequested();
                if (!_retention) throw new InvalidOperationException("Image retention service is unavailable. Start from Idle.");
                _retention.ProtectRequest(requestId);
                await _retention.EnsureCapacityAsync(token);
                var queue = await client.Json("/queue", null, token);
                if ((queue["queue_running"] as JArray)?.Count > 0 || (queue["queue_pending"] as JArray)?.Count > 0)
                    throw new InvalidOperationException("ComfyUI is busy. Retry when the queue is empty.");
                if (!string.IsNullOrEmpty(prompt.poseGuide))
                    await client.EnsureControlGuideAsync("Thai02Quality/" + prompt.poseGuide, token);
                var response = await client.Json("/prompt", new JObject { ["client_id"] = requestId, ["prompt"] = graph }, token);
                promptId = (string)response["prompt_id"];
                if (string.IsNullOrEmpty(promptId)) throw new InvalidOperationException("ComfyUI rejected the workflow.");
                await journal.AppendAsync("generation_queued", new JObject { ["request_id"] = requestId, ["prompt_id"] = promptId });
                Status = "Generating image...";
                Debug.Log("[BlockExperience] Image generation queued. Prompt: " + promptId + ".", this);
                var file = await client.WaitForImage(promptId, token);
                pending = await client.Download(file, token);
                await journal.AppendAsync("generation_completed", new JObject {
                    ["request_id"] = requestId, ["prompt_id"] = promptId, ["server_image"] = file.DeepClone(),
                    ["width"] = pending.width, ["height"] = pending.height
                });
                token.ThrowIfCancellationRequested();
                if (!TryDisplayResult(pending, revision)) return;
                pending = null;
                Debug.Log("[BlockExperience] Image generation completed. Product: " + prompt.resolvedProduct + ".", this);
            }
            catch (OperationCanceledException)
            {
                if (this && isActiveAndEnabled && revision == _selection.Revision && _requestCts == source)
                    Status = "Generation cancelled or timed out. Press the button to retry.";
                await WriteEventAsync("generation_cancelled", new JObject { ["request_id"] = requestId,
                    ["prompt_id"] = promptId, ["server_work_may_continue"] = true }, journal);
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested && this && isActiveAndEnabled && revision == _selection.Revision)
                    Fail("Image generation failed. Press the button to retry or change blocks", ex);
                await WriteEventAsync("generation_failed", new JObject { ["request_id"] = requestId,
                    ["prompt_id"] = promptId, ["error_type"] = ex.GetType().Name, ["message"] = ex.Message }, journal);
            }
            finally
            {
                if (pending) Destroy(pending);
                if (_retention) _retention.ReleaseRequest(requestId);
                if (source.IsCancellationRequested && promptId != null) client.RemovePending(promptId).Forget();
                if (_requestCts == source)
                {
                    _requestCts = null;
                    ReleaseGenerationInput();
                    if (this && isActiveAndEnabled) PresentationChanged?.Invoke();
                }
                timeout.Dispose();
                source.Dispose();
            }
        }

        public void CancelGeneration()
        {
            var source = _requestCts;
            _requestCts = null;
            source?.Cancel();
            ReleaseGenerationInput();
            if (source != null && isActiveAndEnabled)
            {
                Status = "Generation cancelled. Press the button to retry or change blocks.";
                PresentationChanged?.Invoke();
            }
        }

        private void ReleaseGenerationInput()
        {
            _generationLock?.Dispose();
            _generationLock = null;
            if (_pausedTimer) { _timer?.Resume(); _timer?.ResetTimer(); _pausedTimer = false; }
        }

        /// <summary>
        /// Main-thread result bridge. Capture Selection.Revision before
        /// starting work. A true result transfers ownership; a false result leaves ownership with the caller.
        /// The caller must also cancel its work on screen/session exit.
        /// </summary>
        public bool TryDisplayResult(Texture2D texture, int selectionRevision)
        {
            if (!_ready || !isActiveAndEnabled || !HasRequiredComposition || !texture ||
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
            }
            Status = "Image ready.";
            PresentationChanged?.Invoke();
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
            if (_readerHub)
            {
                _readerHub.Changed -= SynchronizeReaders;
                _readerHub.ReaderChanged -= RecordReaderChange;
            }
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
            if (!enableEditorKeyboard || (_manager && _manager.IsPresentationLocked) || (!showEditorDiagnostics && _popup < 0)) return;
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Max(.5f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float w = Mathf.Min(720, Screen.width / scale - 24);
            if (showEditorDiagnostics)
            {
                GUILayout.BeginArea(new Rect(12, 12, w, 220), GUI.skin.box);
                GUILayout.Label("RFID Simulator: 2 Group | 3 Product | 4 Color1 | 5 Color2");
                GUILayout.Label(Status);
                GUI.enabled = _ready && _selection.Complete && !IsGenerating;
                if (GUILayout.Button("Generate image / Retry", GUILayout.Height(36))) { _timer?.ResetTimer(); GenerateImage(); }
                GUI.enabled = IsGenerating;
                if (GUILayout.Button("Cancel generation", GUILayout.Height(30)))
                {
                    CancelGeneration();
                    Status = "Generation cancelled. Press the button to retry or change blocks.";
                }
                GUI.enabled = true;
                GUILayout.EndArea();
            }
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
