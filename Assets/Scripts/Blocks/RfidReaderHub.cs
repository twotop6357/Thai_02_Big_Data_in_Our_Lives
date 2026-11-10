using System;
using Cysharp.Threading.Tasks;
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
        public RfidReaderSelection Readers { get; } = new RfidReaderSelection { RequireGroupFirst = true };
        public bool IsPrimary { get; private set; }
        public event Action Changed;
        public event Action ArtworkChanged;
        public bool IsArtworkReady { get; private set; }
        private StreamingBlockArtwork _streamingArtwork;
        private System.Threading.Tasks.Task _artworkLoad;
        private bool _loadingArtwork;
        public event Action<int, RfidBlock?, RfidBlock?> ReaderChanged;
        private ResultVideoController _video;
        private InactivityTimer _timer;
        private readonly string[] _rejected = new string[4];
        // Registered cards physically on each reader, including ones the session rules ignore (group-first,
        // generation lock) and ones kept after a session reset. The title screen uses it to ask for an empty board.
        private readonly RfidBlock?[] _present = new RfidBlock?[4];
        public event Action PresenceChanged;

        public bool HasPresentBlocks
        {
            get
            {
                for (int i = 0; i < _present.Length; i++) if (_present[i].HasValue) return true;
                return false;
            }
        }

        /// <summary>Hardware adapters report every card change here, before session rules accept or ignore it.</summary>
        public void ReportPresence(int reader, RfidBlock? block)
        {
            if (!IsPrimary || Nullable.Equals(_present[reader], block)) return;
            _present[reader] = block;
            PresenceChanged?.Invoke();
        }

        private void Awake()
        {
            foreach (var hub in FindObjectsByType<RfidReaderHub>(FindObjectsSortMode.None))
                if (hub != this && hub.IsPrimary) { Destroy(gameObject); return; }
            IsPrimary = true;
            if (artwork)
            {
                _streamingArtwork = new StreamingBlockArtwork(artwork);
                artwork = Instantiate(artwork);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (IsPrimary) SceneManager.activeSceneChanged += SceneChanged;
        }

        private void Start()
        {
            if (!IsPrimary) return;
            _timer = FindFirstObjectByType<InactivityTimer>();
            if (!GetComponent<RfidTcpReaders>()) gameObject.AddComponent<RfidTcpReaders>();
            if (!GetComponent<RfidMappingPanel>()) gameObject.AddComponent<RfidMappingPanel>();
            ReloadArtworkAsync().Forget();
        }

        public UniTask ReloadArtworkAsync()
        {
            // Start and scene-loaded callbacks may overlap. Task supports concurrent awaiters.
            if (_loadingArtwork) return _artworkLoad.AsUniTask();
            _artworkLoad = LoadArtworkAsync().AsTask();
            return _artworkLoad.AsUniTask();
        }

        private async UniTask LoadArtworkAsync()
        {
            _loadingArtwork = true;
            IsArtworkReady = false;
            try
            {
                if (_streamingArtwork != null)
                    await _streamingArtwork.LoadAsync(artwork, this.GetCancellationTokenOnDestroy(), () =>
                    {
                        IsArtworkReady = true;
                        ArtworkChanged?.Invoke();
                    });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogError("[BlockArtwork] Artwork loading failed.", this); Debug.LogException(ex, this); }
            finally { _loadingArtwork = false; if (this) IsArtworkReady = true; }
        }

        public bool RecognizeReader(int reader, BlockKind kind, string id)
        {
            if (!IsPrimary || !IsArtworkReady) return false;
            bool registered = artwork && artwork.Find(kind, id) != null;
            ReportPresence(reader, registered ? new RfidBlock(kind, id) : (RfidBlock?)null);
            if (Readers.IsInputLocked) return false;
            var old = Readers[reader];
            if (!registered)
            {
                string rejected = kind + "/" + id;
                if (_rejected[reader] != rejected) Debug.LogWarning("[RFID] Ignored block without registered artwork: " + rejected + ".", this);
                _rejected[reader] = rejected;
                ClearReader(reader);
                return false;
            }
            _rejected[reader] = null;
            var block = new RfidBlock(kind, id);
            if (!Readers.CanRecognize(reader, block))
            {
                ClearReader(reader);
                return false;
            }
            if (!Readers.Set(reader, block)) return false;
            Publish(reader, old); return true;
        }

        /// <summary>The card left the reader.</summary>
        public bool RemoveReader(int reader)
        {
            if (IsPrimary) ReportPresence(reader, null);
            return ClearReader(reader);
        }

        // Session state only: an ignored card is still physically present.
        private bool ClearReader(int reader)
        {
            var old = Readers[reader];
            if (!Readers.Set(reader, null)) return false;
            Publish(reader, old); return true;
        }

        public bool MoveReader(int from, int to)
        {
            var source = Readers[from]; var destination = Readers[to];
            if (!Readers.Move(from, to)) return false;
            // The simulator drag moves the card itself.
            if (IsPrimary && !Readers.IsInputLocked)
            {
                var moved = _present[from];
                ReportPresence(from, null);
                ReportPresence(to, moved);
            }
            // Both changes are already committed before observers derive the semantic selection.
            Notify();
            ReaderChanged?.Invoke(from, source, null);
            ReaderChanged?.Invoke(to, destination, Readers[to]);
            return true;
        }

        /// <summary>Removes every card (simulator "clear all").</summary>
        public void ClearReaders(bool force = false)
        {
            if (Readers.IsInputLocked && !force) return;
            for (int i = 0; i < _present.Length; i++) ReportPresence(i, null);
            ResetSession(force);
        }

        // A new session forgets accepted cards but cards on the board stay physically present.
        private void ResetSession(bool force)
        {
            if (Readers.IsInputLocked && !force) return;
            var before = new RfidBlock?[4];
            for (int i = 0; i < 4; i++) before[i] = Readers[i];
            if (!Readers.Clear(force)) return;
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
            if (next.name == "0_Idle") { Readers.RequireGroupFirst = true; ResetSession(true); ReloadArtworkAsync().Forget(); }
            else if (next.name == "2_Result")
            {
                _video = FindFirstObjectByType<ResultVideoController>();
                if (_video) for (int i = 0; i < 4; i++) _video.SetBlockPresent(i, Readers[i].HasValue);
            }
        }

        private void OnDisable() { if (IsPrimary) SceneManager.activeSceneChanged -= SceneChanged; }
        private void OnDestroy()
        {
            if (_streamingArtwork == null) return;
            _streamingArtwork.Dispose();
            if (artwork) Destroy(artwork);
        }
    }
}
