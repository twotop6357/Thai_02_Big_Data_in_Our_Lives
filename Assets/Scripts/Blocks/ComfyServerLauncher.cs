using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Exhibition.Blocks
{
    public enum ComfyServerState { Checking, Starting, Ready, Unavailable }

    /// <summary>
    /// Lives on the persistent root and runs once per app run: checks the local ComfyUI server and,
    /// if it is down, starts it without a window. A server started here is left running on quit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComfyServerLauncher : MonoBehaviour
    {
        private const int ProbeTimeoutSeconds = 3;
        private const float StartingProbeSeconds = 2f, UnavailableProbeSeconds = 10f;

        private CancellationTokenSource _lifetime;
        private ComfyBlockSettings _settings;
        // 이 앱이 시작한 서버 프로세스. 핸들을 물려주지 않아 RFID 수신 포트를 붙잡지 않는다.
        private DetachedProcess _process;
        private bool _started;

        public ComfyServerState State { get; private set; } = ComfyServerState.Checking;
        public bool IsReady => isActiveAndEnabled && State == ComfyServerState.Ready;
        public string ServerUrl => _settings?.serverUrl;
        public string LastError { get; private set; }
        public event Action<ComfyServerState> StateChanged;

        private void OnEnable()
        {
            _lifetime = new CancellationTokenSource();
            RunAsync(_lifetime.Token).Forget();
        }

        private void OnDisable()
        {
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
        }

        private void OnDestroy()
        {
            // Release only the handle. The server keeps running so the model stays warm for the next run.
            _process?.Dispose();
            _process = null;
        }

        private async UniTask RunAsync(CancellationToken token)
        {
            try
            {
                if (_settings == null)
                {
                    _settings = ComfyBlockSettings.Parse(await ComfyImageClient.LoadText("ComfyUISettings.json", token),
                        ComfyBlockSettings.ApplicationRoot);
                    if (_settings.bundledRuntime != null)
                        Debug.Log("[ComfyServerLauncher] Using the bundled ComfyRuntime at " + _settings.bundledRuntime.Root + ".", this);
                }
                if (State != ComfyServerState.Starting)
                {
                    if (await ProbeAsync(token)) { SetState(ComfyServerState.Ready, null); return; }
                    if (!_started) TryStart();
                }
                if (State == ComfyServerState.Starting) await WaitForStartAsync(token);
                // Keep probing after a failure so the status clears if an operator starts ComfyUI manually.
                while (State == ComfyServerState.Unavailable)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(UnavailableProbeSeconds), DelayType.Realtime, cancellationToken: token);
                    if (!await ProbeAsync(token)) continue;
                    Debug.Log("[ComfyServerLauncher] ComfyUI server became available.", this);
                    SetState(ComfyServerState.Ready, null);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { SetState(ComfyServerState.Unavailable, ex.Message); }
        }

        private void TryStart()
        {
            _started = true;
            if (!_settings.autoLaunchServer) { SetState(ComfyServerState.Unavailable, "ComfyUI is not running and autoLaunchServer is off."); return; }
            if (Application.isEditor && !_settings.launchServerInEditor)
            {
                SetState(ComfyServerState.Unavailable, "ComfyUI is not running. Auto launch is skipped in the Editor (launchServerInEditor is off).");
                return;
            }
            try
            {
                _settings.ValidateLaunch();
                // 동봉 런타임은 복사가 덜 된 상태로 실행하지 않도록 파일·모델을 먼저 확인한다.
                _settings.bundledRuntime?.EnsureReady();
                _process = DetachedProcess.Start(Expand(_settings.serverExecutable), Expand(_settings.serverArguments),
                    string.IsNullOrWhiteSpace(_settings.serverWorkingDirectory) ? null : Expand(_settings.serverWorkingDirectory));
                Debug.Log("[ComfyServerLauncher] ComfyUI was not running. Started server process " + _process.Id + ".", this);
                SetState(ComfyServerState.Starting, null);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                SetState(ComfyServerState.Unavailable, "Failed to start ComfyUI: " + ex.Message);
            }
        }

        private async UniTask WaitForStartAsync(CancellationToken token)
        {
            float deadline = Time.realtimeSinceStartup + _settings.serverStartupTimeoutSeconds;
            while (true)
            {
                // A server that answers wins even if our process exited (e.g. another instance took the port).
                if (await ProbeAsync(token))
                {
                    Debug.Log("[ComfyServerLauncher] ComfyUI server is ready.", this);
                    SetState(ComfyServerState.Ready, null);
                    return;
                }
                if (_process != null && _process.HasExited)
                {
                    SetState(ComfyServerState.Unavailable, "ComfyUI process exited with code " + _process.ExitCode + " before it was ready.");
                    return;
                }
                if (Time.realtimeSinceStartup >= deadline)
                {
                    SetState(ComfyServerState.Unavailable, "ComfyUI did not respond within " + _settings.serverStartupTimeoutSeconds + " seconds.");
                    return;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(StartingProbeSeconds), DelayType.Realtime, cancellationToken: token);
            }
        }

        private async UniTask<bool> ProbeAsync(CancellationToken token)
        {
            using (var request = UnityWebRequest.Get(_settings.serverUrl + "/system_stats"))
            {
                request.timeout = ProbeTimeoutSeconds;
                try { await request.SendWebRequest().ToUniTask(cancellationToken: token); }
                catch (UnityWebRequestException) { return false; }
                // Require a ComfyUI-shaped answer so another app on the same port is not taken as ready.
                try { return JObject.Parse(request.downloadHandler.text)["system"] != null; }
                catch (Newtonsoft.Json.JsonException) { return false; }
            }
        }

        private void SetState(ComfyServerState state, string error)
        {
            if (state == ComfyServerState.Unavailable && error != null && error != LastError)
                Debug.LogWarning("[ComfyServerLauncher] " + error, this);
            if (state == ComfyServerState.Ready && State == ComfyServerState.Checking)
                Debug.Log("[ComfyServerLauncher] ComfyUI server is already running.", this);
            LastError = error;
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(state);
        }

        private static string Expand(string value) => Environment.ExpandEnvironmentVariables(value.Trim());
    }
}
