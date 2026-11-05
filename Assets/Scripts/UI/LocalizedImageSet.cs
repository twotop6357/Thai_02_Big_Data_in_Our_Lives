using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Exhibition.UI
{
    [Serializable]
    public sealed class ImageLanguageEntry
    {
        public string id, sprite, figmaNode;
        public float x, y, width, height;
    }

    [Serializable]
    public sealed class ImageLanguageDocument
    {
        public int schemaVersion;
        public string language, video;
        public ImageLanguageEntry[] images;
    }

    /// <summary>JSON maps stable UI IDs to a serialized sprite library; backgrounds stay outside the fade groups.</summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedImageSet : MonoBehaviour
    {
        [Serializable]
        public struct Binding
        {
            public string id;
            public Image image;
            // Block slots keep their object names; only their sprite and rect follow the language.
            public bool keepObjectName;
        }
        [SerializeField] private ExhibitionUIAssets assets;
        [SerializeField] private Binding[] bindings;
        [SerializeField] private CanvasGroup[] localizedGroups;

        private ImageLanguageDocument _thai, _english;
        private Sprite[] _thaiSprites, _englishSprites;
        private CancellationTokenSource _screenCts;
        private bool _applying;
        private GameManager _manager;
        private Image _titleButtonHelp;
        private int _screenVersion;
        public bool IsReady { get; private set; }
        public bool IsInitializationComplete { get; private set; }
        public SessionLanguage AppliedLanguage { get; private set; }
        public bool UsingFallback { get; private set; }
        public string VideoResource => AppliedLanguage == SessionLanguage.Thai ? _thai?.video : _english?.video;

        public void BindInputManager(GameManager manager) => _manager = manager;

        private void OnEnable()
        {
            // Disable the actual Image so localization fades and entrance tweens cannot reveal it early.
            if (bindings != null)
                foreach (var binding in bindings)
                    if (binding.id == "idle.help") { _titleButtonHelp = binding.image; break; }
            if (_titleButtonHelp) _titleButtonHelp.enabled = false;
            _screenVersion++;
            IsInitializationComplete = false;
            _screenCts = new CancellationTokenSource();
            SetAlpha(0);
            InitializeAsync(_screenCts.Token).Forget();
        }

        private async UniTask InitializeAsync(CancellationToken token)
        {
            try
            {
                if (!assets || bindings == null || localizedGroups == null)
                    throw new InvalidOperationException("Assign the UI asset library, bindings and fade groups.");
                try
                {
                    _thai = await LoadAsync("Lang_Th.json", "th", token);
                    _english = await LoadAsync("Lang_En.json", "en", token);
                    ValidateMatchingKeys(_thai, _english);
                    _thaiSprites = Resolve(_thai);
                    _englishSprites = Resolve(_english);
                    UsingFallback = false;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    Debug.LogWarning("[LocalizedImageSet] Language JSON is unavailable or invalid. Using the last authored language configuration.", this);
                    Debug.LogException(ex, this);
                    _thai = Parse(assets.defaultThai, "th");
                    _english = Parse(assets.defaultEnglish, "en");
                    ValidateMatchingKeys(_thai, _english);
                    _thaiSprites = Resolve(_thai);
                    _englishSprites = Resolve(_english);
                    UsingFallback = true;
                }
                if (!_manager) _manager = FindFirstObjectByType<GameManager>();
                AppliedLanguage = _manager ? _manager.CurrentLanguage : SessionLanguage.Thai;
                Apply(AppliedLanguage);
                SetAlpha(1);
                IsReady = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Debug.LogError("[LocalizedImageSet] Language configuration failed. Keeping the serialized UI.", this);
                    Debug.LogException(ex, this);
                    SetAlpha(1);
                }
            }
            finally { if (!token.IsCancellationRequested) IsInitializationComplete = true; }
        }

        private async UniTask<ImageLanguageDocument> LoadAsync(string file, string language, CancellationToken token)
        {
            string path = Path.Combine(Application.streamingAssetsPath, file);
            using (var request = UnityWebRequest.Get(path.Contains("://") ? path : new Uri(path).AbsoluteUri))
            {
                request.timeout = 10;
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                return Parse(request.downloadHandler.text, language);
            }
        }

        private ImageLanguageDocument Parse(string json, string language)
        {
            var doc = JsonUtility.FromJson<ImageLanguageDocument>(json);
            if (doc == null || doc.schemaVersion != 1 || doc.language != language || doc.images == null ||
                string.IsNullOrWhiteSpace(doc.video) || !doc.video.StartsWith("Videos/", StringComparison.Ordinal) || doc.video.Contains(".."))
                throw new ArgumentException("Invalid language document: " + language);
            var ids = new HashSet<string>();
            foreach (var entry in doc.images)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) ||
                    string.IsNullOrWhiteSpace(entry.sprite) || entry.sprite.EndsWith("_bg", StringComparison.OrdinalIgnoreCase) ||
                    !Finite(entry.x) || !Finite(entry.y) || !Finite(entry.width) || !Finite(entry.height) || entry.width <= 0 || entry.height <= 0)
                    throw new ArgumentException("Invalid or duplicate localized image in " + language);
                assets.Find(entry.sprite);
            }
            return doc;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void ValidateMatchingKeys(ImageLanguageDocument first, ImageLanguageDocument second)
        {
            var keys = new HashSet<string>();
            foreach (var entry in first.images) keys.Add(entry.id);
            if (keys.Count != second.images.Length) throw new ArgumentException("Language documents have different image IDs.");
            foreach (var entry in second.images)
                if (!keys.Contains(entry.id)) throw new ArgumentException("Language image ID is missing: " + entry.id);
        }

        private Sprite[] Resolve(ImageLanguageDocument document)
        {
            var result = new Sprite[bindings.Length];
            for (int i = 0; i < bindings.Length; i++)
            {
                if (!bindings[i].image) throw new ArgumentException("A localized image binding is missing.");
                foreach (var entry in document.images)
                    if (entry.id == bindings[i].id) { result[i] = assets.Find(entry.sprite); break; }
                if (!result[i]) throw new ArgumentException("Language image ID is missing: " + bindings[i].id);
            }
            return result;
        }

        public async UniTask<bool> ApplyLanguageAsync(SessionLanguage language, bool fade)
        {
            if (!IsReady || _applying || !isActiveAndEnabled) return false;
            int version = _screenVersion;
            var previous = AppliedLanguage;
            var token = _screenCts.Token;
            bool completed = false;
            _applying = true;
            var manager = FindFirstObjectByType<GameManager>();
            using var inputLock = manager ? manager.Presentation.LockInput() : null;
            try
            {
                if (fade) await FadeAsync(1, 0, token);
                token.ThrowIfCancellationRequested();
                Apply(language);
                if (fade) await FadeAsync(0, 1, token);
                token.ThrowIfCancellationRequested();
                AppliedLanguage = language;
                completed = true;
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex) { Debug.LogException(ex, this); return false; }
            finally
            {
                if (this && version == _screenVersion)
                {
                    if (!completed) Apply(previous);
                    SetAlpha(1);
                }
                if (version == _screenVersion) _applying = false;
            }
        }

        private async UniTask FadeAsync(float from, float to, CancellationToken token)
        {
            double start = Time.realtimeSinceStartupAsDouble;
            float progress;
            do
            {
                progress = Mathf.Clamp01((float)(Time.realtimeSinceStartupAsDouble - start) / UIMotion.HintSeconds);
                SetAlpha(Mathf.Lerp(from, to, .5f - .5f * Mathf.Cos(Mathf.PI * progress)));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            } while (progress < 1);
        }

        private void Apply(SessionLanguage language)
        {
            var sprites = language == SessionLanguage.Thai ? _thaiSprites : _englishSprites;
            var document = language == SessionLanguage.Thai ? _thai : _english;
            for (int i = 0; i < bindings.Length; i++)
            {
                if (!bindings[i].image) continue;
                bindings[i].image.sprite = sprites[i];
                if (!bindings[i].keepObjectName) bindings[i].image.gameObject.name = sprites[i].name;
                foreach (var entry in document.images)
                    if (entry.id == bindings[i].id)
                    {
                        var rect = bindings[i].image.rectTransform;
                        // A stretched background keeps filling its panel; only its sprite follows the language.
                        if (rect.anchorMin != rect.anchorMax) break;
                        // JSON stores the top-left corner; slots use a centered pivot for their placement pop.
                        rect.anchoredPosition = new Vector2(entry.x + entry.width * rect.pivot.x,
                            -(entry.y + entry.height * (1 - rect.pivot.y)));
                        rect.sizeDelta = new Vector2(entry.width, entry.height);
                        break;
                    }
            }
        }

        private void SetAlpha(float alpha)
        {
            if (localizedGroups == null) return;
            foreach (var group in localizedGroups) if (group) group.alpha = alpha;
        }

        private void LateUpdate()
        {
            if (_titleButtonHelp)
                _titleButtonHelp.enabled = IsReady && _manager && _manager.IsInputReady;
        }

        private void OnDisable()
        {
            _screenVersion++;
            IsReady = false;
            _applying = false;
            _screenCts?.Cancel();
            _screenCts?.Dispose();
            _screenCts = null;
            SetAlpha(1);
            if (_titleButtonHelp) _titleButtonHelp.enabled = true;
        }
    }
}
