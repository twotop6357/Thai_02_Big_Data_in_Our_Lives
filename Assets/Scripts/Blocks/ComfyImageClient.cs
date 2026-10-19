using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Exhibition.Blocks
{
    [Serializable]
    public sealed class ComfyBlockSettings
    {
        public string serverUrl = "http://127.0.0.1:8188";
        public int requestTimeoutSeconds = 15;
        public int generationTimeoutSeconds = 180;
        public int maxJournalFiles = 30;
        public int maxJournalMegabytes = 4;
        public string comfyOutputDirectory = "";
        public int maxGeneratedImages = 1000;
        public int maxGeneratedImageMegabytes = 512;
        public int imageCleanupIntervalSeconds = 30;
        public int imageWriteGraceSeconds = 60;
        // Local server auto launch. Paths and arguments expand %ENV% variables.
        public bool autoLaunchServer = true;
        public bool launchServerInEditor;
        public string serverExecutable = "";
        public string serverArguments = "";
        public string serverWorkingDirectory = "";
        public int serverStartupTimeoutSeconds = 180;

        public void ValidateLaunch()
        {
            if (string.IsNullOrWhiteSpace(serverExecutable) ||
                serverStartupTimeoutSeconds < 10 || serverStartupTimeoutSeconds > 900)
                throw new ArgumentException("Set serverExecutable and a 10-900 second serverStartupTimeoutSeconds.");
            // A remote server cannot be started from this PC.
            if (!new Uri(serverUrl).IsLoopback)
                throw new ArgumentException("Auto launch requires a loopback serverUrl.");
        }

        public void Validate()
        {
            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https") || !string.IsNullOrEmpty(uri.UserInfo) ||
                requestTimeoutSeconds < 1 || requestTimeoutSeconds > 120 ||
                generationTimeoutSeconds < requestTimeoutSeconds || generationTimeoutSeconds > 600)
                throw new ArgumentException("Invalid ComfyUI server or timeout settings.");
            serverUrl = serverUrl.TrimEnd('/');
            if (maxGeneratedImages < 1 || maxGeneratedImages > 100000 ||
                maxGeneratedImageMegabytes < 1 || maxGeneratedImageMegabytes > 102400 ||
                imageCleanupIntervalSeconds < 5 || imageCleanupIntervalSeconds > 3600 ||
                imageWriteGraceSeconds < 5 || imageWriteGraceSeconds > 600)
                throw new ArgumentException("Invalid generated-image retention limits, interval or write grace.");
        }
    }

    public sealed class ComfyImageClient
    {
        private readonly ComfyBlockSettings _settings;
        public ComfyImageClient(ComfyBlockSettings settings) => _settings = settings;

        public static async UniTask<string> LoadText(string file, CancellationToken token)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "BlockGeneration", file);
            string url = path.Contains("://") ? path : new Uri(path).AbsoluteUri;
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 15;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                return request.downloadHandler.text;
            }
        }

        public async UniTask<JObject> Json(string path, JObject body, CancellationToken token)
        {
            using (var request = new UnityWebRequest(_settings.serverUrl + path, body == null ? "GET" : "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                if (body != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Newtonsoft.Json.Formatting.None)));
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                request.timeout = _settings.requestTimeoutSeconds;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                return JObject.Parse(request.downloadHandler.text);
            }
        }

        // GET is safe to retry. Submission is never retried because a lost response may already have queued work.
        private async UniTask<JObject> ReadHistory(string id, CancellationToken token)
        {
            for (int attempt = 0; ; attempt++)
            {
                try { return await Json("/history/" + Uri.EscapeDataString(id), null, token); }
                catch (UnityWebRequestException ex) when (attempt < 2 &&
                    (ex.ResponseCode == 0 || ex.ResponseCode >= 500))
                {
                    Debug.LogWarning("[ComfyImageClient] History request failed (" + ex.ResponseCode + "). Retry " + (attempt + 1) + " of 2.");
                    await UniTask.Delay(500 * (attempt + 1), ignoreTimeScale: true, cancellationToken: token);
                }
            }
        }

        public async UniTask<JObject> WaitForImage(string id, CancellationToken token)
        {
            while (true)
            {
                var history = (await ReadHistory(id, token))[id];
                if (history != null)
                {
                    if ((string)history["status"]?["status_str"] == "error")
                        throw new InvalidOperationException("ComfyUI generation failed. Inspect the server history.");
                    if ((bool?)history["status"]?["completed"] == true)
                    {
                        var images = history["outputs"]?["7"]?["images"] as JArray;
                        if (images == null || images.Count == 0) throw new InvalidOperationException("ComfyUI returned no final image.");
                        return (JObject)images[0];
                    }
                }
                await UniTask.Delay(250, ignoreTimeScale: true, cancellationToken: token);
            }
        }

        public async UniTask<Texture2D> Download(JObject file, CancellationToken token)
        {
            string url = _settings.serverUrl + "/view?filename=" + Uri.EscapeDataString((string)file["filename"] ?? "") +
                "&subfolder=" + Uri.EscapeDataString((string)file["subfolder"] ?? "") + "&type=output";
            using (var request = UnityWebRequestTexture.GetTexture(url, true))
            {
                request.timeout = _settings.requestTimeoutSeconds;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                var texture = DownloadHandlerTexture.GetContent(request);
                if (texture.width != 480 || texture.height != 480)
                {
                    UnityEngine.Object.Destroy(texture);
                    throw new InvalidOperationException("Expected a 480 x 480 image.");
                }
                return texture;
            }
        }

        public async UniTask RemovePending(string id)
        {
            try
            {
                // Only delete our known pending prompt. Never interrupt the shared running job.
                await Json("/queue", new JObject { ["delete"] = new JArray(id) }, CancellationToken.None);
            }
            catch (Exception ex) { Debug.LogWarning("[ComfyImageClient] Pending cleanup failed: " + ex.Message); }
        }
    }
}
