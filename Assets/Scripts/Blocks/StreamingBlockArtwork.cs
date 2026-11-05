using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Exhibition.Blocks
{
    /// <summary>Owns decoded external textures; bundled sprites remain Unity-owned fallbacks.</summary>
    public sealed class StreamingBlockArtwork : IDisposable
    {
        private readonly BlockArtworkCatalog _fallback;
        private List<UnityEngine.Object> _owned = new List<UnityEngine.Object>();
        public StreamingBlockArtwork(BlockArtworkCatalog fallback) => _fallback = fallback;

        public async UniTask LoadAsync(BlockArtworkCatalog target, CancellationToken token, Action applied)
        {
            var owned = new List<UnityEngine.Object>();
            var cache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            var languages = new Dictionary<string, Dictionary<string, Sprite>>(StringComparer.Ordinal);
            bool committed = false;
            try
            {
                foreach (string language in new[] { "en", "th" })
                {
                    var sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                    languages[language] = sprites;
                    BlockArtworkDocument document;
                    try
                    {
                        using var request = UnityWebRequest.Get(Url("blocks_" + language + ".json"));
                        request.timeout = 10;
                        await request.SendWebRequest().ToUniTask(cancellationToken: token);
                        document = JsonUtility.FromJson<BlockArtworkDocument>(request.downloadHandler.text);
                        if (document == null) throw new ArgumentException("Empty block artwork document.");
                        document.Validate(language);
                        foreach (var entry in document.blocks)
                            if (_fallback.Find(entry.Kind, entry.id) == null) throw new ArgumentException("Unregistered block artwork: " + entry.Key);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[BlockArtwork] Could not load " + language + " mapping. Using bundled images. " + ex.Message);
                        Debug.LogException(ex);
                        continue;
                    }
                    foreach (var entry in document.blocks)
                    {
                        token.ThrowIfCancellationRequested();
                        int ownedStart = owned.Count;
                        try
                        {
                            if (!cache.TryGetValue(entry.image, out var sprite))
                            {
                                using var request = UnityWebRequest.Get(Url(entry.image));
                                request.timeout = 10;
                                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                                byte[] bytes = request.downloadHandler.data;
                                if (bytes == null || bytes.Length > 16 * 1024 * 1024) throw new InvalidDataException("Block image exceeds 16 MiB.");
                                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false); owned.Add(texture);
                                if (!ImageConversion.LoadImage(texture, bytes, true) || texture.width > 4096 || texture.height > 4096)
                                    throw new InvalidDataException("Invalid block image or dimensions above 4096 pixels.");
                                texture.name = "ExternalBlock/" + entry.image; texture.wrapMode = TextureWrapMode.Clamp;
                                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                                sprite.name = Path.GetFileNameWithoutExtension(entry.image); owned.Add(sprite); cache.Add(entry.image, sprite);
                            }
                            sprites.Add(entry.Key, sprite);
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            for (int i = owned.Count - 1; i >= ownedStart; i--)
                            {
                                if (owned[i]) UnityEngine.Object.Destroy(owned[i]);
                                owned.RemoveAt(i);
                            }
                            Debug.LogWarning("[BlockArtwork] Could not load " + language + "/" + entry.Key + ". Using its bundled image. " + ex.Message);
                            Debug.LogException(ex);
                        }
                    }
                }
                token.ThrowIfCancellationRequested();
                foreach (var entry in target.entries)
                {
                    string key = entry.kind.ToString().ToLowerInvariant() + "/" + entry.id;
                    var fallback = _fallback.Find(entry.kind, entry.id);
                    entry.sprite = languages["en"].TryGetValue(key, out var en) ? en : fallback.sprite;
                    entry.thaiSprite = languages["th"].TryGetValue(key, out var th) ? th : fallback.GetSprite(Core.SessionLanguage.Thai);
                }
                var previous = _owned; _owned = owned; committed = true;
                try { applied?.Invoke(); }
                finally { Release(previous); }
            }
            finally { if (!committed) Release(owned); }
        }

        private static string Url(string relative)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "BlockArtwork", relative);
            return path.Contains("://") ? path : new Uri(path).AbsoluteUri;
        }
        private static void Release(List<UnityEngine.Object> objects)
        {
            // Sprites are released before their backing textures.
            for (int i = objects.Count - 1; i >= 0; i--) if (objects[i]) UnityEngine.Object.Destroy(objects[i]);
            objects.Clear();
        }
        public void Dispose() => Release(_owned);
    }
}
