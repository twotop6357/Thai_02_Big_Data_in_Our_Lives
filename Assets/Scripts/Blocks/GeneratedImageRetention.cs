using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Exhibition.Blocks
{
    /// <summary>Lives on the persistent root, so Idle also cleans late/cancelled server results.</summary>
    [DisallowMultipleComponent]
    public sealed class GeneratedImageRetention : MonoBehaviour
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _lifetime;
        private ComfyBlockSettings _settings;
        private string _outputDirectory, _configError, _activeRequest, _lastWarning;
        private bool _configured;
        public string OutputDirectory => _outputDirectory;
        public string LastStatus { get; private set; } = "Image retention is initializing.";
        public int DeletedFiles { get; private set; }

        private void OnEnable()
        {
            _configured = false;
            _configError = null;
            _outputDirectory = null;
            _lifetime = new CancellationTokenSource();
            MaintainAsync(_lifetime.Token).Forget();
        }

        private void OnDisable()
        {
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
        }

        private async UniTask MaintainAsync(CancellationToken token)
        {
            try
            {
                _settings = JsonUtility.FromJson<ComfyBlockSettings>(await ComfyImageClient.LoadText("ComfyUISettings.json", token));
                _settings.Validate();
                _configured = true;
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex) { _configError = ex.Message; Warn(ex.Message); return; }
            while (!token.IsCancellationRequested)
            {
                try { await SweepAsync(token); }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Warn(ex.Message); }
                try { await UniTask.Delay(TimeSpan.FromSeconds(_settings.imageCleanupIntervalSeconds),
                    DelayType.Realtime, cancellationToken: token); }
                catch (OperationCanceledException) { return; }
            }
        }

        public void ProtectRequest(string id) => _activeRequest = id;
        public void ReleaseRequest(string id) { if (_activeRequest == id) _activeRequest = null; }

        public async UniTask EnsureCapacityAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(() => _configured || _configError != null, cancellationToken: token);
            if (_configError != null) throw new InvalidOperationException("Image retention settings are invalid: " + _configError);
            var report = await SweepAsync(token);
            if (!report.WithinLimits)
                throw new IOException("Image storage is over its limit. Wait for the write grace period or check deletion permissions.");
        }

        private async UniTask<string> ResolveOutputDirectory(CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(_settings.comfyOutputDirectory)) return _settings.comfyOutputDirectory;
            // A remote server's absolute path must never be interpreted as this PC's local path.
            if (!new Uri(_settings.serverUrl).IsLoopback)
                throw new InvalidOperationException("Set comfyOutputDirectory to the server's shared output folder for remote ComfyUI.");
            var stats = await new ComfyImageClient(_settings).Json("/system_stats", null, token);
            var args = stats["system"]?["argv"] as JArray;
            if (args != null)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    string arg = (string)args[i];
                    if (arg == "--output-directory" && i + 1 < args.Count) return (string)args[i + 1];
                    if (arg != null && arg.StartsWith("--output-directory=", StringComparison.Ordinal))
                        return arg.Substring("--output-directory=".Length);
                }
            }
            throw new InvalidOperationException("Cannot discover ComfyUI output directory. Set comfyOutputDirectory explicitly.");
        }

        public async UniTask<ImageRetentionReport> SweepAsync(CancellationToken token)
        {
            await _gate.WaitAsync(token);
            try
            {
                string root = _outputDirectory ?? await ResolveOutputDirectory(token);
                int count = _settings.maxGeneratedImages;
                long bytes = _settings.maxGeneratedImageMegabytes * 1024L * 1024L;
                var grace = TimeSpan.FromSeconds(_settings.imageWriteGraceSeconds);
                string active = _activeRequest;
                var report = await Task.Run(() => GeneratedImageRetentionPolicy.Sweep(root, count, bytes, grace,
                    DateTime.UtcNow, active, token), token);
                _outputDirectory = root;
                DeletedFiles += report.DeletedFiles;
                if (report.WithinLimits)
                {
                    _lastWarning = null;
                    LastStatus = "Image retention active. Files: " + report.RemainingFiles + "; bytes: " + report.RemainingBytes + ".";
                    if (report.DeletedFiles > 0)
                        Debug.Log("[ImageRetention] Removed " + report.DeletedFiles + " old project image(s), " + report.DeletedBytes + " bytes.", this);
                }
                else Warn("Project image limits are not yet satisfied. Recent or protected files are retained; failed deletes: " +
                    report.FailedDeletes + ". " + report.FirstError);
                return report;
            }
            finally { _gate.Release(); }
        }

        private void Warn(string message)
        {
            LastStatus = message;
            if (_lastWarning == message) return;
            _lastWarning = message;
            Debug.LogWarning("[ImageRetention] " + message, this);
        }
    }
}
