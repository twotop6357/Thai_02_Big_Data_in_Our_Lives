using System;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using LegacyInput = UnityEngine.Input;

namespace Exhibition.Blocks
{
    /// <summary>Optional operator UI. Included in Windows Player; F9 only changes its visibility.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RfidReaderHub))]
    public sealed class RfidSimulator : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float panelOpacity = .55f;
        private RfidReaderHub _hub;
        private RfidMappingPanel _mappingPanel;
        private GameManager _manager;
        private bool _initialized, _cursorWasVisible, _oldBackground;
        private bool _f9Latched;
        private CursorLockMode _cursorWasLocked;
        private GUIStyle _title, _label, _status, _button;
        private BlockArtworkCatalog.Entry[] _palette;
        private readonly Rect[] _readers = new Rect[4];
        private Rect[] _cards;
        private string _counts;
        private bool _valid;
        private RfidBlock? _drag;
        private int _dragReader = -1;
        private Vector2 _pointer;
        private static readonly Rect CloseRect = new Rect(1610, 76, 205, 46);
        private static readonly Rect ClearRect = new Rect(1565, 965, 245, 55);
        public bool IsVisible { get; private set; }
        public RfidReaderHub Hub => _hub;
        public bool CanEdit => _initialized && _hub.IsArtworkReady && _manager && _manager.IsInputReady &&
            !_manager.IsTransitioning && !_manager.IsPresentationLocked;

        private void Start()
        {
            _hub = GetComponent<RfidReaderHub>();
            _mappingPanel = GetComponent<RfidMappingPanel>();
            _manager = FindFirstObjectByType<GameManager>();
            if (!_hub.IsPrimary) { enabled = false; return; }
            if (!_manager || !_hub.Artwork)
            {
                Debug.LogError("[RFID Simulator] Start from Idle with the manager and block artwork configured.", this);
                enabled = false; return;
            }
            _palette = _hub.Artwork.entries;
            _cards = new Rect[_palette.Length];
            for (int i = 0; i < 4; i++) _readers[i] = new Rect(110 + i % 2 * 310, 215 + i / 2 * 310, 280, 280);
            for (int i = 0; i < _cards.Length; i++) _cards[i] = new Rect(815 + i % 5 * 176, 180 + i / 5 * 181, 154, 154);
            _hub.Changed += Refresh;
            SceneManager.activeSceneChanged += SceneChanged;
            _initialized = true;
            Refresh();
        }

        private void Update()
        {
            if (!_initialized) return;
            if (_mappingPanel && _mappingPanel.IsVisible) return;
            var keyboard = Keyboard.current;
            bool toggle = keyboard != null && keyboard.f9Key.wasPressedThisFrame;
            bool held = keyboard != null && keyboard.f9Key.isPressed;
#if ENABLE_LEGACY_INPUT_MANAGER
            toggle |= LegacyInput.GetKeyDown(KeyCode.F9);
            held |= LegacyInput.GetKey(KeyCode.F9);
#endif
            if (toggle && !_f9Latched) { _f9Latched = true; Toggle(); }
            if (!held && !toggle) _f9Latched = false;
            if (!IsVisible) return;
            var mouse = Mouse.current;
            bool down = mouse != null && mouse.leftButton.wasPressedThisFrame;
            bool up = mouse != null && mouse.leftButton.wasReleasedThisFrame;
            Vector2 screen = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
#if ENABLE_LEGACY_INPUT_MANAGER
            bool legacyDown = LegacyInput.GetMouseButtonDown(0), legacyUp = LegacyInput.GetMouseButtonUp(0);
            if (legacyDown || legacyUp || LegacyInput.GetMouseButton(0)) screen = LegacyInput.mousePosition;
            down |= legacyDown; up |= legacyUp;
#endif
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            _pointer = new Vector2((screen.x - (Screen.width - 1920 * scale) * .5f) / scale,
                (Screen.height - screen.y - (Screen.height - 1080 * scale) * .5f) / scale);
            if (down) PointerDown(_pointer);
            if (up) PointerUp(_pointer);
        }

        private void OnEnable()
        {
            if (!_initialized) return;
            _hub.Changed += Refresh;
            SceneManager.activeSceneChanged += SceneChanged;
            Refresh();
        }

        private void OnApplicationFocus(bool focused) { if (!focused) CancelDrag(); }

        public void Toggle() => SetVisible(!IsVisible);

        public void SetVisible(bool visible)
        {
            if (!_initialized || IsVisible == visible) return;
            CancelDrag();
            IsVisible = visible;
            if (visible)
            {
                _cursorWasVisible = Cursor.visible; _cursorWasLocked = Cursor.lockState;
                _oldBackground = Application.runInBackground; Application.runInBackground = true;
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            }
            else { Cursor.lockState = _cursorWasLocked; Cursor.visible = _cursorWasVisible; Application.runInBackground = _oldBackground; }
        }

        private void Refresh()
        {
            var state = _hub.Readers;
            _valid = state.HasRequiredComposition;
            _counts = "Group: " + state.GroupCount + " / 1     Product: " + state.ProductCount + " / 1     Colors: " + state.ColorCount + " / 2";
            if (_dragReader >= 0 && !Nullable.Equals(_hub.Readers[_dragReader], _drag)) CancelDrag();
        }

        // Public operations are also convenient for equipment/integration tests; GUI calls these same paths.
        public bool Place(int reader, BlockKind kind, string id) => CanEdit && _hub.RecognizeReader(reader, kind, id);
        public bool Remove(int reader) => CanEdit && _hub.RemoveReader(reader);
        public bool Move(int from, int to) => CanEdit && _hub.MoveReader(from, to);

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold };
            _label = new GUIStyle(GUI.skin.label) { fontSize = 23 };
            _status = new GUIStyle(_label) { fontStyle = FontStyle.Bold };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        }

        private void OnGUI()
        {
            if (!_initialized || !IsVisible) return;
            EnsureStyles();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            bool oldEnabled = GUI.enabled;
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1920 * scale) * .5f, (Screen.height - 1080 * scale) * .5f),
                Quaternion.identity, new Vector3(scale, scale, 1));
            try
            {
                GUI.color = new Color(.025f, .04f, .065f, panelOpacity);
                GUI.DrawTexture(new Rect(45, 40, 1830, 1000), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(90, 73, 1450, 54), "RFID TEST  /  Four independent readers", _title);
                GUI.Box(CloseRect, "Close (F9)", _button);
                GUI.Label(new Rect(110, 155, 650, 40), "Reader positions match the four screen slots", _label);
                GUI.Label(new Rect(815, 130, 980, 40), "Drag any block to any reader", _label);
                for (int i = 0; i < 4; i++)
                {
                    var rect = _readers[i];
                    GUI.color = new Color(.3f, .6f, .68f, .12f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(rect.x + 12, rect.y + 6, 185, 34), ReaderName(i), _label);
                    bool before = GUI.enabled; GUI.enabled = CanEdit && _hub.Readers[i].HasValue;
                    GUI.Box(new Rect(rect.xMax - 48, rect.y + 5, 42, 35), "X", _button);
                    GUI.enabled = before;
                    var sprite = _hub.Artwork.Find(_hub.Readers[i], _manager.CurrentLanguage);
                    if (sprite) GUI.DrawTexture(new Rect(rect.x + 20, rect.y + 43, 240, 230), sprite.texture, ScaleMode.ScaleToFit, true);
                }
                for (int i = 0; i < _cards.Length; i++)
                {
                    var sprite = _palette[i].GetSprite(_manager.CurrentLanguage);
                    if (sprite) GUI.DrawTexture(_cards[i], sprite.texture, ScaleMode.ScaleToFit, true);
                }
                GUI.color = _valid ? new Color(.45f, 1, .65f) : new Color(1, .83f, .4f);
                GUI.Label(new Rect(110, 895, 1610, 42), _counts, _status);
                GUI.color = Color.white;
                GUI.Label(new Rect(110, 945, 1390, 35), "Drag an occupied reader outside to remove it.  Key 1: Continue / hold 5s: Home", _label);
                GUI.Label(new Rect(110, 988, 1350, 35), CanEdit ? "F9 hides this UI without changing reader occupancy." : "Screen animation in progress; drops are temporarily ignored.", _label);
                GUI.enabled = CanEdit;
                GUI.Box(ClearRect, "Clear readers", _button);
                GUI.enabled = true;
                if (_drag.HasValue)
                {
                    var sprite = _hub.Artwork.Find(_drag, _manager.CurrentLanguage);
                    if (sprite)
                    {
                        GUI.color = new Color(1, 1, 1, .85f);
                        GUI.DrawTexture(new Rect(_pointer.x - 76, _pointer.y - 76, 152, 152), sprite.texture, ScaleMode.ScaleToFit, true);
                    }
                }
            }
            finally { GUI.matrix = oldMatrix; GUI.color = oldColor; GUI.enabled = oldEnabled; }
        }

        private void PointerDown(Vector2 pointer)
        {
            if (CloseRect.Contains(pointer)) { SetVisible(false); return; }
            if (!CanEdit) return;
            if (ClearRect.Contains(pointer)) { _hub.ClearReaders(); return; }
            for (int i = 0; i < 4; i++)
                if (new Rect(_readers[i].xMax - 48, _readers[i].y + 5, 42, 35).Contains(pointer)) { Remove(i); return; }
            for (int i = 0; i < _cards.Length; i++)
                if (_cards[i].Contains(pointer)) { _drag = _palette[i].Block; _dragReader = -1; return; }
            for (int i = 0; i < 4; i++)
                if (_readers[i].Contains(pointer) && _hub.Readers[i].HasValue) { _drag = _hub.Readers[i]; _dragReader = i; return; }
        }

        private void PointerUp(Vector2 pointer)
        {
            if (!_drag.HasValue) return;
            var block = _drag.Value; int from = _dragReader, target = -1;
            for (int i = 0; i < 4; i++) if (_readers[i].Contains(pointer)) { target = i; break; }
            bool canApply = CanEdit && (from < 0 || Nullable.Equals(_hub.Readers[from], _drag));
            CancelDrag();
            if (!canApply) return;
            if (target >= 0) { if (from < 0) Place(target, block.Kind, block.Id); else Move(from, target); }
            else if (from >= 0) Remove(from);
        }

        private void CancelDrag()
        {
            _drag = null; _dragReader = -1;
        }

        private static string ReaderName(int reader) => reader == 0 ? "RFID 1" : reader == 1 ? "RFID 2" : reader == 2 ? "RFID 3" : "RFID 4";

        private void SceneChanged(Scene before, Scene after)
        {
            CancelDrag();
            if (IsVisible) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        private void OnDisable()
        {
            if (_hub) _hub.Changed -= Refresh;
            SceneManager.activeSceneChanged -= SceneChanged;
            if (IsVisible) SetVisible(false);
        }
    }
}
