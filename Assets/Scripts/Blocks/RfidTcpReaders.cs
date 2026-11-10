using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Exhibition.Blocks
{
    /// <summary>TCP server for four client readers, matched to slots by source IP. Worker threads never access Unity objects.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RfidReaderHub))]
    public sealed class RfidTcpReaders : MonoBehaviour
    {
        private const int ListenRetryMilliseconds = 2000;
        private const int SessionStopMilliseconds = 1000;
        private const int MaxReportedRejections = 64;
        // Windows sends 10 probes, so a powered-off reader is detected in about 15 seconds.
        private const int KeepAliveIdleMilliseconds = 5000;
        private const int KeepAliveIntervalMilliseconds = 1000;

        public sealed class ReaderState
        {
            public bool Connected { get; internal set; }
            public string Status { get; internal set; } = "Not connected";
            public string Uid { get; internal set; }
            public string RawHex { get; internal set; } = "";
            public string LastChunkHex { get; internal set; } = "";
            public int Frames { get; internal set; }
            public int UidBytes { get; internal set; }
            internal readonly RfidUidStability Stability = new RfidUidStability();
            internal string Pending;
            internal int Session;
        }
        private sealed class Message
        {
            public int Epoch, Slot, Session;
            public byte[] Frame;
            public string ChunkHex;
            public bool Connected;
            public string Status;
            public string Listener;
            public bool ListenerFailed;
            public string Rejected;
        }
        private readonly ReaderState[] _states = { new ReaderState(), new ReaderState(), new ReaderState(), new ReaderState() };
        private readonly ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();
        private CancellationTokenSource _connections;
        private Task[] _workers = Array.Empty<Task>();
        private RfidReaderHub _hub;
        private int _epoch, _queued, _dropped;
        private bool _busy;
        private float _nextCheck;
        public RfidPresenceSettings Presence { get; private set; } = new RfidPresenceSettings();
        public string PresencePath { get; private set; }
        public bool IsReady { get; private set; }
        public string Error { get; private set; } = "Loading RFID configuration...";
        public string ConfigPath { get; private set; }
        public RfidConfiguration Configuration { get; private set; }
        public string ListenerStatus { get; private set; } = "Not listening";
        public string RejectedAddress { get; private set; }
        public ReaderState State(int slot) => _states[slot];
        public bool IsBusy => _busy;

        private void Awake()
        {
            _hub = GetComponent<RfidReaderHub>();
            ConfigPath = Path.Combine(Application.streamingAssetsPath, "RFID", "RfidSettings.json");
            PresencePath = Path.Combine(Application.streamingAssetsPath, "RFID", "RfidPresenceSettings.json");
        }
        private void Start()
        {
            ReloadAsync().Forget();
        }

        public async UniTask ReloadAsync()
        {
            if (_busy) return;
            _busy = true;
            try
            {
                var token = this.GetCancellationTokenOnDestroy();
                string json = await File.ReadAllTextAsync(ConfigPath, token);
                string presenceJson = await File.ReadAllTextAsync(PresencePath, token);
                var document = Parse(json);
                var presence = ParsePresence(presenceJson);
                ValidateArtwork(document);
                await UniTask.WaitUntil(() => _hub.IsArtworkReady, cancellationToken: token);
                token.ThrowIfCancellationRequested();
                Presence = presence;
                ApplyConfiguration(document);
                Error = "";
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Error = "RFID settings were not applied: " + ex.Message;
                Debug.LogError("[RFID TCP] " + Error, this); Debug.LogException(ex, this);
            }
            finally { _busy = false; }
        }

        private static RfidConfiguration Parse(string json)
        {
            // Reject incomplete documents instead of silently restoring enabled network defaults.
            var root = Newtonsoft.Json.Linq.JObject.Parse(json);
            foreach (string key in new[] { "version", "enabled", "listenPort", "readers", "format", "cards" })
                if (root[key] == null) throw new FormatException("Missing RFID setting: " + key);
            var document = root.ToObject<RfidConfiguration>();
            document.Validate(); return document;
        }

        private static RfidPresenceSettings ParsePresence(string json)
        {
            var root = Newtonsoft.Json.Linq.JObject.Parse(json);
            if (root["checkIntervalMilliseconds"] == null) throw new FormatException("Missing RFID setting: checkIntervalMilliseconds");
            var presence = root.ToObject<RfidPresenceSettings>();
            presence.Validate(); return presence;
        }

        private void ValidateArtwork(RfidConfiguration document)
        {
            if (!_hub.Artwork) throw new InvalidOperationException("RFID artwork catalog is missing.");
            foreach (var card in document.cards)
                if (_hub.Artwork.Find((BlockKind)Enum.Parse(typeof(BlockKind), card.kind), card.id) == null)
                    throw new ArgumentException("RFID mapping references an unknown block: " + card.kind + "/" + card.id);
        }

        /// <summary>Main-thread entry point; also allows equipment tests to use a loopback configuration.</summary>
        public void ApplyConfiguration(RfidConfiguration document)
        {
            // Clone before starting workers so UI/external callers cannot mutate live protocol state.
            var copy = Parse(JsonConvert.SerializeObject(document));
            ValidateArtwork(copy);
            StopConnections();
            for (int i = 0; i < 4; i++)
            {
                _hub.RemoveReader(i);
                _states[i].Connected = false; _states[i].Uid = null; _states[i].RawHex = "";
                _states[i].LastChunkHex = "";
                _states[i].UidBytes = 0; _states[i].Stability.Reset();
                _states[i].Frames = 0; _states[i].Session = 0; _states[i].Pending = null; _states[i].Status = copy.enabled ? "Waiting for reader..." : "Disabled in JSON";
            }
            Configuration = copy; IsReady = true;
            RejectedAddress = null; ListenerStatus = copy.enabled ? "Starting TCP listener..." : "Disabled in JSON";
            if (!isActiveAndEnabled || !copy.enabled) return;
            _connections = new CancellationTokenSource();
            int epoch = _epoch; var token = _connections.Token;
            _workers = new[] { Task.Run(() => RunListener(copy, epoch, token)) };
        }

        public async UniTask<bool> SaveMappingAsync(string uid, BlockKind kind, string id, bool remove = false)
        {
            if (_busy || !IsReady || _hub.Readers.IsInputLocked) return false;
            _busy = true;
            try
            {
                uid = RfidHex.Normalize(uid);
                var copy = Parse(JsonConvert.SerializeObject(Configuration));
                if (!remove && _hub.Artwork.Find(kind, id) == null) throw new ArgumentException("Select an existing block.");
                var cards = new System.Collections.Generic.List<RfidCardMapping>(copy.cards);
                cards.RemoveAll(card => card.uid == uid);
                if (!remove) cards.Add(new RfidCardMapping { uid = uid, kind = kind.ToString(), id = id });
                copy.cards = cards.ToArray(); copy.Validate(); ValidateArtwork(copy);
                var token = this.GetCancellationTokenOnDestroy();
                string json = JsonConvert.SerializeObject(copy, Formatting.Indented) + Environment.NewLine;
                await WriteAtomicallyAsync(ConfigPath, json, token);
                token.ThrowIfCancellationRequested();
                // Endpoints/protocol are unchanged. Do not reconnect or replay ignored card readings.
                Configuration = copy; Error = "";
                for (int slot = 0; slot < 4 && !_hub.Readers.IsInputLocked; slot++)
                    if (_states[slot].Uid == uid)
                    {
                        _hub.RemoveReader(slot);
                        if (!remove) _hub.RecognizeReader(slot, kind, id);
                        _states[slot].Status = remove ? "Unmapped card" : kind + "/" + id;
                    }
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                Error = "RFID mapping was not saved: " + ex.Message;
                Debug.LogError("[RFID TCP] " + Error, this); Debug.LogException(ex, this);
                return false;
            }
            finally { _busy = false; }
        }

        private static async Task WriteAtomicallyAsync(string path, string json, CancellationToken token)
        {
            string temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllTextAsync(temporary, json, new UTF8Encoding(false), token);
                token.ThrowIfCancellationRequested();
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        private void RunListener(RfidConfiguration settings, int epoch, CancellationToken token)
        {
            var sessions = new TcpClient[4];
            var tasks = new Task[4];
            var rejected = new HashSet<string>();
            int nextSession = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var listener = new TcpListener(IPAddress.Any, settings.listenPort);
                    try
                    {
                        listener.Start();
                        using var stop = token.Register(listener.Stop);
                        Enqueue(new Message { Epoch = epoch, Listener = "Listening on TCP port " + settings.listenPort });
                        while (!token.IsCancellationRequested)
                        {
                            var client = listener.AcceptTcpClient();
                            // A reader that vanishes during setup only loses this attempt; it connects again itself.
                            try { Attach(client, settings, epoch, token, sessions, tasks, rejected, ref nextSession); }
                            catch (SocketException) { client.Close(); }
                        }
                    }
                    catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException || ex is InvalidOperationException)
                    {
                        if (!token.IsCancellationRequested)
                            // SocketException.Message follows the OS language; log the error code to keep logs in English.
                            Enqueue(new Message { Epoch = epoch, ListenerFailed = true, Listener = "Listen failed on TCP port " + settings.listenPort + ": " +
                                (ex is SocketException socketError ? socketError.SocketErrorCode.ToString() : ex.Message) });
                    }
                    finally { listener.Stop(); }
                    if (token.WaitHandle.WaitOne(ListenRetryMilliseconds)) return;
                }
            }
            catch (Exception ex)
            {
                Enqueue(new Message { Epoch = epoch, ListenerFailed = true, Listener = "Listener stopped: " + ex });
            }
            finally
            {
                // The cancellation source is disposed after this worker, so sessions must end first.
                foreach (var session in sessions) session?.Close();
                foreach (var task in tasks) task?.Wait(SessionStopMilliseconds);
            }
        }

        private void Attach(TcpClient client, RfidConfiguration settings, int epoch, CancellationToken token,
            TcpClient[] sessions, Task[] tasks, HashSet<string> rejected, ref int nextSession)
        {
            var address = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var endpoint = settings.FindReader(address);
            if (endpoint == null)
            {
                client.Close();
                // Report each address once: a retrying device must not fill the queue or the log.
                if (rejected.Count < MaxReportedRejections && rejected.Add(address.ToString()))
                    Enqueue(new Message { Epoch = epoch, Rejected = address.ToString() });
                return;
            }
            EnableKeepAlive(client.Client);
            int slot = endpoint.slot, session = ++nextSession;
            // The newest connection wins: a rebooted reader cannot close its previous half-open socket.
            sessions[slot]?.Close();
            tasks[slot]?.Wait(SessionStopMilliseconds);
            sessions[slot] = client;
            Enqueue(new Message { Epoch = epoch, Slot = slot, Session = session, Connected = true, Status = "Connected" });
            tasks[slot] = Task.Run(() => RunSession(client, settings, slot, session, epoch, token));
        }

        private static void EnableKeepAlive(Socket socket)
        {
            var values = new byte[12];
            BitConverter.GetBytes(1).CopyTo(values, 0);
            BitConverter.GetBytes(KeepAliveIdleMilliseconds).CopyTo(values, 4);
            BitConverter.GetBytes(KeepAliveIntervalMilliseconds).CopyTo(values, 8);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.IOControl(IOControlCode.KeepAliveValues, values, null);
        }

        private void RunSession(TcpClient client, RfidConfiguration settings, int slot, int session, int epoch, CancellationToken token)
        {
            try
            {
                using (client)
                {
                    // TcpClient.Close clears its Client property; keep the socket so closing raises a handled disposal error.
                    var socket = client.Client;
                    var stream = client.GetStream();
                    var decoder = new RfidFrameDecoder(settings.format);
                    var bytes = new byte[1024];
                    var clock = Stopwatch.StartNew();
                    Action<byte[]> receive = frame => Enqueue(new Message { Epoch = epoch, Slot = slot, Session = session, Frame = frame });
                    while (!token.IsCancellationRequested)
                    {
                        if (socket.Poll(10000, SelectMode.SelectRead))
                        {
                            int count = stream.Read(bytes, 0, bytes.Length);
                            if (count == 0) throw new IOException("Reader closed the connection.");
                            Enqueue(new Message { Epoch = epoch, Slot = slot, Session = session,
                                ChunkHex = BitConverter.ToString(bytes, 0, Math.Min(count, 512)).Replace("-", " ") });
                            decoder.Feed(bytes, count, clock.ElapsedMilliseconds, receive);
                        }
                        decoder.FlushIdle(clock.ElapsedMilliseconds, receive);
                    }
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
            {
                if (!token.IsCancellationRequested)
                    Enqueue(new Message { Epoch = epoch, Slot = slot, Session = session, Status = "Disconnected: " + ex.Message });
            }
            catch (Exception ex)
            {
                Enqueue(new Message { Epoch = epoch, Slot = slot, Session = session, Status = "Reader stopped: " + ex });
            }
        }

        private void Enqueue(Message message)
        {
            if (Interlocked.Increment(ref _queued) > 256)
            {
                Interlocked.Decrement(ref _queued); Interlocked.Increment(ref _dropped); return;
            }
            _messages.Enqueue(message);
        }

        private void Update()
        {
            int dropped = Interlocked.Exchange(ref _dropped, 0);
            if (dropped > 0)
            {
                // Overflow makes occupancy uncertain. Reconnect and clear rather than retaining a lost removal.
                Debug.LogWarning("[RFID TCP] Input queue overflow; reconnecting readers.", this);
                if (Configuration != null) ApplyConfiguration(Configuration);
                return;
            }
            for (int n = 0; n < 64 && _messages.TryDequeue(out var message); n++)
            {
                Interlocked.Decrement(ref _queued);
                if (message.Epoch != _epoch) continue;
                if (message.Listener != null)
                {
                    if (message.ListenerFailed && message.Listener != ListenerStatus) Debug.LogWarning("[RFID TCP] " + message.Listener, this);
                    ListenerStatus = message.Listener;
                    continue;
                }
                if (message.Rejected != null)
                {
                    RejectedAddress = message.Rejected;
                    Debug.LogWarning("[RFID TCP] Rejected a connection from unconfigured address " + message.Rejected + ".", this);
                    continue;
                }
                var state = _states[message.Slot];
                if (message.Connected)
                {
                    // Card state is unknown after a replacement. The new session's next report restores it.
                    if (state.Connected) { state.Uid = null; ApplyRemoval(message.Slot); }
                    state.Pending = null; state.Stability.Reset(); state.UidBytes = 0;
                    state.Session = message.Session; state.Connected = true; state.Status = message.Status;
                    continue;
                }
                // A replaced session must not clear the reader that took over its slot.
                if (message.Session != state.Session) continue;
                if (message.ChunkHex != null)
                {
                    state.LastChunkHex = message.ChunkHex;
                    if (state.Frames == 0) state.Status = "Receiving bytes; waiting for a complete frame";
                }
                else if (message.Frame != null) ReceiveFrame(message.Slot, message.Frame);
                else
                {
                    state.Connected = false; state.Status = message.Status;
                    state.Uid = null; state.Pending = null; state.Stability.Reset(); state.UidBytes = 0; ApplyRemoval(message.Slot);
                }
            }
            if (Configuration == null || Time.realtimeSinceStartup < _nextCheck) return;
            _nextCheck = Time.realtimeSinceStartup + Presence.checkIntervalMilliseconds / 1000f;
            for (int i = 0; i < 4; i++) CheckPresence(i);
        }

        private void ReceiveFrame(int slot, byte[] bytes)
        {
            var state = _states[slot];
            state.RawHex = BitConverter.ToString(bytes).Replace("-", " "); state.Frames++;
            try
            {
                string uid = Configuration.format.Extract(bytes, out int reports);
                if (Configuration.format.IsRemoval(uid))
                {
                    state.Stability.Reset(); state.Pending = null; return;
                }
                if (Configuration.format.framing == "UidBurst" && !state.Stability.Observe(uid, reports))
                {
                    state.Status = "Waiting for a repeated complete UID"; return;
                }
                state.UidBytes = uid.Length / 2;
                state.Pending = uid;
            }
            catch (FormatException ex) { state.Status = "Invalid frame: " + ex.Message; }
        }

        /// <summary>The latest value since the previous check: same = held, different = replaced, none = removed.</summary>
        private void CheckPresence(int slot)
        {
            var state = _states[slot];
            string uid = state.Pending;
            state.Pending = null;
            if (uid == state.Uid) return;
            state.Uid = uid;
            if (uid == null)
            {
                state.Status = "Card removed"; ApplyRemoval(slot);
                return;
            }
            var mapping = Configuration.Find(uid);
            state.Status = mapping == null ? "Unmapped card" : mapping.kind + "/" + mapping.id;
            // Presence is reported even when the session ignores the card (title popup asks for an empty board).
            _hub.ReportPresence(slot, mapping == null ? (RfidBlock?)null :
                new RfidBlock((BlockKind)Enum.Parse(typeof(BlockKind), mapping.kind), mapping.id));
            // A held card must not replay a non-group card ignored before the first group,
            // nor replay changes discarded during generation.
            if (_hub.Readers.IsInputLocked) return;
            if (mapping == null) _hub.RemoveReader(slot);
            else _hub.RecognizeReader(slot, (BlockKind)Enum.Parse(typeof(BlockKind), mapping.kind), mapping.id);
        }

        private void ApplyRemoval(int slot) => _hub.RemoveReader(slot);

        private void StopConnections()
        {
            int stoppedEpoch = _epoch;
            _epoch++;
            var cancellation = _connections; _connections = null;
            if (cancellation == null) return;
            cancellation.Cancel();
            // Dispose only after workers finish using the token's WaitHandle. Never block Unity.
            Task.WhenAll(_workers).ContinueWith(task =>
            {
                cancellation.Dispose();
                if (task.IsFaulted) Enqueue(new Message { Epoch = stoppedEpoch, Slot = 0, Status = "Reader worker failed: " + task.Exception });
            }, TaskScheduler.Default);
            _workers = Array.Empty<Task>();
        }
        private void OnDisable() => StopConnections();
        private void OnEnable()
        {
            if (IsReady && Configuration != null) ApplyConfiguration(Configuration);
        }
    }
}
