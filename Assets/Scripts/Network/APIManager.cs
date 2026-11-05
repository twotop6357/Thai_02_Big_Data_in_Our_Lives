using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Core;
using Exhibition.Blocks;
using Cysharp.Threading.Tasks;
using HuliacDev.Network;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Network
{
    public class APIManager : ApiManagerBase
    {
        private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);
        private readonly Queue<bool> _externalCalls = new Queue<bool>();
        private GameManager _manager;
        private ComfyServerLauncher _generationServer;
        private Process _watcher;
        private string _normalExitMarker;
        private bool _watcherStarted;

        public int SentCount { get; private set; }
        public string LastSentMessage { get; private set; }

        protected override async UniTaskVoid SendStartupLogAsync(CancellationToken cancellationToken)
        {
            try
            {
                _manager = FindFirstObjectByType<GameManager>();
                var settings = await SettingsProvider.GetAsync(cancellationToken);
                if (_manager) _generationServer = _manager.GetComponent<ComfyServerLauncher>();
                if (!TryEndpoint(settings.apiUrl, out var endpoint)) return;
                if (CanTransmit(endpoint)) StartExitWatcher(endpoint.AbsoluteUri);
                await SendMessageAsync("start", cancellationToken);
#if DEVELOPMENT_BUILD
                // Explicit loopback-only Player smoke test; absent from release builds and normal launches.
                if (endpoint.IsLoopback && Array.IndexOf(Environment.GetCommandLineArgs(), "-validate-content-log-exit") >= 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(2), DelayType.Realtime, cancellationToken: cancellationToken);
                    Application.Quit();
                }
#endif
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogException(ex, this); }
        }

        protected override async UniTask SendExitLogAsync()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await SendMessageAsync("end", timeout.Token); }
            catch (OperationCanceledException) { Debug.LogWarning("[ContentLog] Exit logging timed out. Continuing normal shutdown.", this); }
            finally { MarkNormalExit(); }
        }

        protected override void OnMoveIdle() => SendObservedAsync("move_idle").Forget();

        protected override void OnInactivityTimeout()
        {
            // Idle's own inactivity tick is not a screen move. The common event remains the sole timeout source.
            if (!_manager) _manager = FindFirstObjectByType<GameManager>();
            if (_manager && !_manager.IsTitleScreen) SendObservedAsync("move_idle_timeout").Forget();
        }

        protected override void OnExternalApiCall(string requestUrl)
        {
            if (_externalCalls.Count >= 64)
            {
                Debug.LogWarning("[ContentLog] External API event capacity reached. Use the paired request helper.", this);
                return;
            }
            bool included = IsExternalContentApi(requestUrl);
            _externalCalls.Enqueue(included);
            if (included) SendObservedAsync("Call API " + SafeRequestUrl(requestUrl)).Forget();
        }

        protected override void OnExternalApiReturn(bool isSuccess, string failReason = null)
        {
            if (_externalCalls.Count == 0 || !_externalCalls.Dequeue()) return;
            SendObservedAsync(isSuccess ? "Return OK" : "Return fail").Forget();
        }

        /// <summary>Per-request logging, including concurrent requests. Local Comfy traffic bypasses both messages.</summary>
        public new async UniTask<T> ExecuteWithExternalApiLoggingAsync<T>(string requestUrl, Func<UniTask<T>> apiAction, CancellationToken cancellationToken = default)
        {
            if (apiAction == null) throw new ArgumentNullException(nameof(apiAction));
            if (!IsExternalContentApi(requestUrl)) return await apiAction();
            await SendMessageAsync("Call API " + SafeRequestUrl(requestUrl), cancellationToken);
            try
            {
                T result = await apiAction();
                await SendObservedAsync("Return OK");
                return result;
            }
            catch
            {
                await SendObservedAsync("Return fail");
                throw;
            }
        }

        public new async UniTask ExecuteWithExternalApiLoggingAsync(string requestUrl, Func<UniTask> apiAction, CancellationToken cancellationToken = default)
        {
            if (apiAction == null) throw new ArgumentNullException(nameof(apiAction));
            await ExecuteWithExternalApiLoggingAsync(requestUrl, async () => { await apiAction(); return true; }, cancellationToken);
        }

        private bool IsExternalContentApi(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https")) return false;
            if (_generationServer && Uri.TryCreate(_generationServer.ServerUrl, UriKind.Absolute, out var generation) &&
                Uri.Compare(uri, generation, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0) return false;
            // ComfyUI and file/asset requests never publish telemetry. Do not recursively log the log API itself.
            return !uri.IsLoopback && uri.Host.IndexOf("comfy", StringComparison.OrdinalIgnoreCase) < 0 &&
                !uri.AbsolutePath.EndsWith("/content_log.cfm", StringComparison.OrdinalIgnoreCase);
        }

        private static string SafeRequestUrl(string url)
        {
            var uri = new UriBuilder(url) { UserName = "", Password = "" };
            return Regex.Replace(uri.Uri.AbsoluteUri,
                @"([?&](?:api[_-]?key|access[_-]?token|token|password|secret|authorization)=)[^&#]*",
                "$1[REDACTED]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private async UniTask SendObservedAsync(string message)
        {
            try { await SendMessageAsync(message, this.GetCancellationTokenOnDestroy()); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogException(ex, this); }
        }

        private async UniTask SendMessageAsync(string message, CancellationToken token)
        {
            var settings = await SettingsProvider.GetAsync(token);
            if (!TryEndpoint(settings.apiUrl, out var endpoint) || !CanTransmit(endpoint)) return;
            await _sendGate.WaitAsync(token);
            try
            {
                var response = await ApiRetryUtil.GetTextWithRetryAsync(endpoint.AbsoluteUri + Uri.EscapeDataString(message),
                    "content log " + message, Logger, token, maxAttemptCount: 2, retryDelaySeconds: .5f,
                    skipInEditorAndDevelopmentBuild: !endpoint.IsLoopback, attemptTimeoutSeconds: 3);
                if (response.isSuccess) { LastSentMessage = message; SentCount++; }
            }
            finally { _sendGate.Release(); }
        }

        private static bool TryEndpoint(string value, out Uri endpoint)
        {
            endpoint = null;
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (!Uri.TryCreate(value, UriKind.Absolute, out endpoint) ||
                (endpoint.Scheme != "http" && endpoint.Scheme != "https") || !string.IsNullOrEmpty(endpoint.UserInfo) ||
                !value.EndsWith("message=", StringComparison.Ordinal))
                throw new ArgumentException("Content log apiUrl must be an HTTP(S) endpoint ending in message=.");
            return true;
        }

        private static bool CanTransmit(Uri endpoint) => endpoint.IsLoopback || (!Application.isEditor && !Debug.isDebugBuild);

        private void StartExitWatcher(string endpoint)
        {
            if (_watcherStarted || Application.isEditor || Application.platform != RuntimePlatform.WindowsPlayer) return;
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, "ContentLog");
                Directory.CreateDirectory(directory);
                _normalExitMarker = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".normal");
                string script = Path.Combine(Application.streamingAssetsPath, "ContentLog", "WatchProcess.ps1");
                if (!File.Exists(script)) throw new FileNotFoundException("The content log process watcher is missing.", script);
                using var current = Process.GetCurrentProcess();
                var info = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"))
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script +
                        "\" -TargetProcessId " + current.Id + " -StartTimeUtcTicks " + current.StartTime.ToUniversalTime().Ticks +
                        " -Endpoint \"" + endpoint + "\" -NormalExitMarker \"" + _normalExitMarker + "\""
                };
                _watcher = Process.Start(info) ?? throw new InvalidOperationException("The content log watcher did not start.");
                _watcherStarted = true;
                Debug.Log("[ContentLog] Forced-exit watcher started for this application process.", this);
            }
            catch (Exception ex) { Debug.LogWarning("[ContentLog] Forced-exit monitoring could not start.", this); Debug.LogException(ex, this); }
        }

        private void MarkNormalExit()
        {
            if (!_watcherStarted) return;
            try { File.WriteAllText(_normalExitMarker, "end"); }
            catch (Exception ex) { Debug.LogException(ex, this); }
        }

        protected override void OnDisable()
        {
            _externalCalls.Clear();
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            // The watcher owns its process lifetime and must survive a killed Player.
            _watcher?.Dispose();
            base.OnDestroy();
        }
    }
}
