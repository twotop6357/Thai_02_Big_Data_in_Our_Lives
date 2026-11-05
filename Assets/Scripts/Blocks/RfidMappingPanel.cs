using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Exhibition.Blocks
{
    /// <summary>Operator-only F10 monitor/calibration panel, available in release Players.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RfidReaderHub))]
    public sealed class RfidMappingPanel : MonoBehaviour
    {
        private RfidReaderHub _hub;
        private RfidTcpReaders _tcp;
        private GameManager _manager;
        private RfidSimulator _simulator;
        private IDisposable _inputLock;
        private bool _cursorVisible, _background, _f10Latched;
        private CursorLockMode _cursorLock;
        private string _uid = "", _notice = "";
        private int _entry;
        private Vector2 _mappingScroll;
        private GUIStyle _label, _title, _button, _field;
        public bool IsVisible { get; private set; }
        public bool CanSave => _tcp && _tcp.IsReady && !_tcp.IsBusy && !_hub.Readers.IsInputLocked;

        private void Start()
        {
            _hub = GetComponent<RfidReaderHub>(); _tcp = GetComponent<RfidTcpReaders>();
            _simulator = GetComponent<RfidSimulator>(); _manager = FindFirstObjectByType<GameManager>();
        }
        private void Update()
        {
            var keyboard = Keyboard.current;
            bool pressed = keyboard != null && keyboard.f10Key.wasPressedThisFrame;
            bool held = keyboard != null && keyboard.f10Key.isPressed;
#if ENABLE_LEGACY_INPUT_MANAGER
            pressed |= UnityEngine.Input.GetKeyDown(KeyCode.F10); held |= UnityEngine.Input.GetKey(KeyCode.F10);
#endif
            if (pressed && !_f10Latched) { _f10Latched = true; SetVisible(!IsVisible); }
            if (!held && !pressed) _f10Latched = false;
        }
        public void SetVisible(bool visible)
        {
            if (IsVisible == visible || !_hub || !_manager) return;
            IsVisible = visible;
            if (visible)
            {
                if (_simulator && _simulator.IsVisible) _simulator.SetVisible(false);
                _cursorVisible = Cursor.visible; _cursorLock = Cursor.lockState;
                _background = Application.runInBackground; Application.runInBackground = true;
                Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
                _inputLock = _manager.Presentation.LockInput();
                _manager.ResetButtonInput();
            }
            else
            {
                _inputLock?.Dispose(); _inputLock = null;
                Cursor.visible = _cursorVisible; Cursor.lockState = _cursorLock;
                Application.runInBackground = _background;
                _manager.ResetButtonInput();
            }
        }
        private void OnDisable() { if (IsVisible) SetVisible(false); }

        private void Styles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
            _title = new GUIStyle(_label) { fontSize = 27, fontStyle = FontStyle.Bold };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 19, wordWrap = true };
            _field = new GUIStyle(GUI.skin.textField) { fontSize = 19 };
        }
        private void OnGUI()
        {
            if (!IsVisible || !_tcp) return;
            Styles();
            var matrix = GUI.matrix; int depth = GUI.depth; bool guiEnabled = GUI.enabled; var color = GUI.color;
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600 * scale) / 2, (Screen.height - 900 * scale) / 2),
                Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.depth = -10000;
            try
            {
                GUI.color = new Color(.06f, .07f, .09f, .98f);
                GUI.DrawTexture(new Rect(10, 10, 1580, 880), Texture2D.whiteTexture); GUI.color = Color.white;
                GUI.Label(new Rect(30, 22, 1200, 40), "RFID SETUP / TCP monitor & card mapping", _title);
                if (GUI.Button(new Rect(1370, 22, 200, 40), "Close (F10)", _button)) SetVisible(false);
                GUI.Label(new Rect(30, 68, 760, 30), _tcp.ListenerStatus + " / Check every " + _tcp.Presence.checkIntervalMilliseconds + " ms" +
                    (_tcp.RejectedAddress == null ? "" : " / Rejected IP: " + _tcp.RejectedAddress), _label);
                GUI.Label(new Rect(800, 68, 770, 30), _tcp.ConfigPath, _label);
                for (int slot = 0; slot < 4; slot++) DrawReader(slot);
                GUI.Label(new Rect(30, 405, 700, 30), "Selected card HEX (click a reader, or enter manually)", _label);
                _uid = GUI.TextField(new Rect(30, 437, 1100, 36), _uid, 1024, _field);
                GUI.Label(new Rect(30, 482, 1500, 30), "Choose block / Group, Product or Color. The mapping works in both languages.", _label);
                var entries = _hub.Artwork ? _hub.Artwork.entries : null;
                if (entries != null)
                {
                    for (int i = 0; i < entries.Length; i++)
                    {
                        var entry = entries[i];
                        GUI.color = i == _entry ? new Color(.55f, 1f, .75f) : Color.white;
                        if (GUI.Button(new Rect(30 + i % 5 * 308, 520 + i / 5 * 44, 300, 40), entry.kind + "/" + entry.id, _button)) _entry = i;
                    }
                    GUI.color = Color.white; GUI.enabled = CanSave && _uid.Length > 0;
                    if (GUI.Button(new Rect(30, 705, 280, 42), "Save / replace mapping", _button) && _entry < entries.Length)
                        SaveAsync(entries[_entry], false).Forget();
                    if (GUI.Button(new Rect(325, 705, 230, 42), "Delete mapping", _button)) SaveAsync(entries[0], true).Forget();
                }
                GUI.enabled = !_tcp.IsBusy && !_hub.Readers.IsInputLocked;
                if (GUI.Button(new Rect(570, 705, 220, 42), "Reload JSON", _button)) _tcp.ReloadAsync().Forget();
                GUI.enabled = true;
                GUI.Label(new Rect(810, 705, 750, 45), _hub.Readers.IsInputLocked ? "Generating: live monitor only. Mapping is locked." : _notice, _label);
                GUI.Label(new Rect(30, 752, 1520, 36), string.IsNullOrEmpty(_tcp.Error)
                    ? "Raw = complete burst/frame. RX = TCP bytes. UID = confirmed single value; repeated copies are merged in UidBurst mode."
                    : _tcp.Error, _label);
                int count = _tcp.Configuration?.cards.Length ?? 0;
                _mappingScroll = GUI.BeginScrollView(new Rect(30, 797, 1530, 73), _mappingScroll, new Rect(0, 0, 1490, Mathf.Max(65, count * 29)));
                if (_tcp.Configuration != null)
                    for (int i = 0; i < count; i++)
                    {
                        var card = _tcp.Configuration.cards[i];
                        if (GUI.Button(new Rect(0, i * 29, 1470, 27), card.uid + "  ->  " + card.kind + "/" + card.id, _button)) _uid = card.uid;
                    }
                GUI.EndScrollView();
            }
            finally { GUI.matrix = matrix; GUI.depth = depth; GUI.enabled = guiEnabled; GUI.color = color; }
        }

        private void DrawReader(int slot)
        {
            float x = 30 + slot % 2 * 780, y = 107 + slot / 2 * 145;
            var state = _tcp.State(slot);
            RfidEndpoint endpoint = null;
            if (_tcp.Configuration != null)
                foreach (var item in _tcp.Configuration.readers) if (item.slot == slot) { endpoint = item; break; }
            string position = slot == 0 ? "TOP LEFT" : slot == 1 ? "TOP RIGHT" : slot == 2 ? "BOTTOM LEFT" : "BOTTOM RIGHT";
            GUI.Box(new Rect(x, y, 760, 137), "");
            GUI.Label(new Rect(x + 10, y + 4, 735, 28), position + " / " + (endpoint == null ? "Loading..." : endpoint.ip) +
                (state.Connected ? "  CONNECTED" : "  OFFLINE"), _label);
            GUI.TextField(new Rect(x + 10, y + 34, 735, 28), "UID: " + (state.Uid ?? "(none)"), _field);
            GUI.TextField(new Rect(x + 10, y + 65, 735, 28), string.IsNullOrEmpty(state.RawHex)
                ? "RX: " + state.LastChunkHex : "Raw: " + state.RawHex, _field);
            GUI.Label(new Rect(x + 10, y + 98, 490, 32), state.Status + " / " + state.UidBytes + " UID bytes / frames " + state.Frames, _label);
            GUI.enabled = state.Uid != null;
            if (GUI.Button(new Rect(x + 510, y + 97, 235, 32), "Select current card", _button))
            {
                _uid = state.Uid; _notice = "Card selected. Choose a block, then save.";
                var mapping = _tcp.Configuration?.Find(_uid);
                if (mapping != null && _hub.Artwork)
                    for (int i = 0; i < _hub.Artwork.entries.Length; i++)
                        if (_hub.Artwork.entries[i].kind.ToString() == mapping.kind && _hub.Artwork.entries[i].id == mapping.id) _entry = i;
            }
            GUI.enabled = true;
        }
        private async UniTask SaveAsync(BlockArtworkCatalog.Entry entry, bool remove)
        {
            string uid = _uid;
            bool saved = await _tcp.SaveMappingAsync(uid, entry.kind, entry.id, remove);
            if (this) _notice = saved ? (remove ? "Mapping deleted: " : "Mapping saved: ") + uid : _tcp.Error;
        }
    }
}
