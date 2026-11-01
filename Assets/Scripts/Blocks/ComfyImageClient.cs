using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

        // Small, versioned reference images are verified per request. Never replace another server user's input.
        public UniTask EnsureControlGuideAsync(string reference, CancellationToken token) =>
            EnsureReferenceAsync(reference, "Thai02Quality", "ControlGuides", "guide_", token);

        public UniTask EnsureStyleReferenceAsync(string reference, CancellationToken token) =>
            EnsureReferenceAsync(reference, "Thai02Style3", "StyleReferences", "style_user_", token);

        private async UniTask EnsureReferenceAsync(string reference, string folder, string localFolder, string prefix, CancellationToken token)
        {
            string filename = System.IO.Path.GetFileName(reference);
            if (reference != folder + "/" + filename || !filename.StartsWith(prefix, StringComparison.Ordinal) ||
                !filename.EndsWith(".png", StringComparison.Ordinal) || filename.Contains("..") || filename.Contains(":"))
                throw new ArgumentException("Invalid immutable reference image path.");
            string localPath = System.IO.Path.Combine(Application.streamingAssetsPath, "BlockGeneration", localFolder, filename);
            byte[] data;
            using (var source = UnityWebRequest.Get(localPath.Contains("://") ? localPath : new Uri(localPath).AbsoluteUri))
            {
                source.timeout = _settings.requestTimeoutSeconds;
                await source.SendWebRequest().ToUniTask(cancellationToken: token);
                data = source.downloadHandler.data;
            }
            if (prefix == "style_user_")
            {
                using (var sha = SHA256.Create())
                {
                    string digest = BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
                    if (filename != prefix + digest.Substring(0, 16) + ".png")
                        throw new InvalidOperationException("Packaged style reference hash does not match its immutable name.");
                }
            }
            string query = "/view?filename=" + Uri.EscapeDataString(filename) + "&subfolder=" + folder + "&type=input";
            using (var existing = UnityWebRequest.Get(_settings.serverUrl + query))
            {
                existing.timeout = _settings.requestTimeoutSeconds;
                try { await existing.SendWebRequest().ToUniTask(cancellationToken: token); }
                catch (UnityWebRequestException ex) when (ex.ResponseCode == 404) { }
                if (existing.responseCode != 404)
                {
                    using (var sha = SHA256.Create())
                    {
                        string expected = Convert.ToBase64String(sha.ComputeHash(data));
                        string actual = Convert.ToBase64String(sha.ComputeHash(existing.downloadHandler.data));
                        if (expected != actual) throw new InvalidOperationException("Existing reference image differs. Refusing to overwrite: " + filename);
                    }
                    return;
                }
            }
            var form = new List<IMultipartFormSection>(3) {
                new MultipartFormDataSection("subfolder", folder), new MultipartFormDataSection("type", "input"),
                new MultipartFormFileSection("image", data, filename, "image/png")
            };
            using (var upload = UnityWebRequest.Post(_settings.serverUrl + "/upload/image", form))
            {
                upload.timeout = _settings.requestTimeoutSeconds;
                await upload.SendWebRequest().ToUniTask(cancellationToken: token);
                var reply = JObject.Parse(upload.downloadHandler.text);
                if ((string)reply["name"] != filename || (string)reply["subfolder"] != folder)
                    throw new InvalidOperationException("Unexpected product control image upload path.");
            }
        }

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
                // ComfyUI acknowledges a successful pending-queue deletion with an empty body.
                if (path == "/queue" && body != null && string.IsNullOrWhiteSpace(request.downloadHandler.text))
                    return new JObject();
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
            using (var request = UnityWebRequestTexture.GetTexture(url, false))
            {
                request.timeout = _settings.requestTimeoutSeconds;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                var texture = DownloadHandlerTexture.GetContent(request);
                byte[] png = request.downloadHandler.data;
                // This workflow contracts to an 8-bit RGBA PNG, not an opaque image of a checkerboard.
                if (png == null || png.Length < 26 || png[0] != 137 || png[1] != 80 || png[2] != 78 ||
                    png[3] != 71 || png[24] != 8 || png[25] != 6)
                {
                    UnityEngine.Object.Destroy(texture);
                    throw new InvalidOperationException("Expected an RGBA PNG with an alpha channel. Check the background removal workflow.");
                }
                if (texture.width != 480 || texture.height != 480)
                {
                    UnityEngine.Object.Destroy(texture);
                    throw new InvalidOperationException("Expected a 480 x 480 image.");
                }
                try
                {
                    var pixels = texture.GetPixels32();
                    var centered = await UniTask.RunOnThreadPool(() => GeneratedImageFraming.Center(
                        pixels, GeneratedImageFraming.CanvasSize, GeneratedImageFraming.CanvasSize, token, requireComplete: true), cancellationToken: token);
                    token.ThrowIfCancellationRequested();
                    texture.SetPixels32(centered);
                    texture.Apply(false, true);
                    return texture;
                }
                catch
                {
                    await UniTask.SwitchToMainThread();
                    UnityEngine.Object.Destroy(texture);
                    throw;
                }
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
