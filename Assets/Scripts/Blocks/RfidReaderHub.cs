using System;
using Exhibition.UI;
using HuliacDev.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exhibition.Blocks
{
    /// <summary>Session reader occupancy. Hardware and the optional F9 simulator use the same main-thread API.</summary>
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class RfidReaderHub : MonoBehaviour
    {
        [SerializeField] private BlockArtworkCatalog artwork;
        public BlockArtworkCatalog Artwork => artwork;
        public RfidReaderSelection Readers { get; } = new RfidReaderSelection();
        public bool IsPrimary { get; private set; }
        public event Action Changed;
        public event Action<int, RfidBlock?, RfidBlock?> ReaderChanged;
        private ResultVideoController _video;
        private InactivityTimer _timer;
        private readonly string[] _rejected = new string[4];

        private void Awake()
        {
            foreach (var hub in FindObjectsByType<RfidReaderHub>(FindObjectsSortMode.None))
                if (hub != this && hub.IsPrimary) { Destroy(gameObject); return; }
            IsPrimary = true;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (IsPrimary) SceneManager.activeSceneChanged += SceneChanged;
        }

        private void Start() => _timer = FindFirstObjectByType<InactivityTimer>();

        public bool RecognizeReader(int reader, BlockKind kind, string id)
        {
            if (!IsPrimary) return false;
            var old = Readers[reader];
            if (!artwork || artwork.Find(kind, id) == null)
            {
                string rejected = kind + "/" + id;
                if (_rejected[reader] != rejected) Debug.LogWarning("[RFID] Ignored block without registered artwork: " + rejected + ".", this);
                _rejected[reader] = rejected;
                RemoveReader(reader);
                return false;
            }
            _rejected[reader] = null;
            if (!Readers.Set(reader, new RfidBlock(kind, id))) return false;
            Publish(reader, old); return true;
        }

        public bool RemoveReader(int reader)
        {
            var old = Readers[reader];
            if (!Readers.Set(reader, null)) return false;
            Publish(reader, old); return true;
        }

        public bool MoveReader(int from, int to)
        {
            var source = Readers[from]; var destination = Readers[to];
            if (!Readers.Move(from, to)) return false;
            // Both changes are already committed before observers derive the semantic selection.
            Notify();
            ReaderChanged?.Invoke(from, source, null);
            ReaderChanged?.Invoke(to, destination, Readers[to]);
            return true;
        }

        public void ClearReaders()
        {
            var before = new RfidBlock?[4];
            for (int i = 0; i < 4; i++) before[i] = Readers[i];
            if (!Readers.Clear()) return;
            Notify();
            for (int i = 0; i < 4; i++) if (before[i].HasValue) ReaderChanged?.Invoke(i, before[i], null);
        }

        private void Publish(int reader, RfidBlock? old)
        {
            Notify();
            ReaderChanged?.Invoke(reader, old, Readers[reader]);
        }

        private void Notify()
        {
            _timer?.ResetTimer();
            if (_video)
                for (int i = 0; i < 4; i++) _video.SetBlockPresent(i, Readers[i].HasValue);
            Changed?.Invoke();
        }

        private void SceneChanged(Scene previous, Scene next)
        {
            _video = null;
            if (next.name == "0_Idle") ClearReaders();
            else if (next.name == "2_Result")
            {
                _video = FindFirstObjectByType<ResultVideoController>();
                if (_video) for (int i = 0; i < 4; i++) _video.SetBlockPresent(i, Readers[i].HasValue);
            }
        }

        private void OnDisable() { if (IsPrimary) SceneManager.activeSceneChanged -= SceneChanged; }
    }
}
