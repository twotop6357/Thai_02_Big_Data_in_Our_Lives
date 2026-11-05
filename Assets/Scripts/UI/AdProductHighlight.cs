using System;
using System.Threading;
using AssetKits.ParticleImage;
using AssetKits.ParticleImage.Enumerations;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    /// <summary>
    /// Finite entrance effects lock input for at most InputLockSeconds and then finish on their own.
    /// Ambient aura stays until the advertisement exits.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdProductHighlight : MonoBehaviour
    {
        private const float InputLockSeconds = 3f;
        [SerializeField] private GameObject effectRoot;
        [SerializeField] private CanvasGroup effectOpacity;
        [SerializeField] private ParticleImage[] emitters;
        [SerializeField] private GameObject confettiRoot;
        [SerializeField] private CanvasGroup confettiOpacity;
        [SerializeField] private ParticleImage confettiEmitter;
        [SerializeField, Range(.2f, 1.8f)] private float duration = 1.8f;
        private CancellationTokenSource _playback;
        private IDisposable _inputLock;
        private bool _played;
        private int _version;
        private readonly System.Random _random = new System.Random(Guid.NewGuid().GetHashCode());
        private ParticleImage _glow, _wave;
        private ParticleImage[] _appearanceEmitters;
        private ParticleImage[] _confettiEmitters;
        private RectTransform _effectMask;
        private Canvas _confettiCanvas;
        private double _nextGlowVariation, _nextWaveVariation;
        public bool IsPlaying { get; private set; }
        public bool AreEffectsVisible => effectRoot && effectRoot.activeSelf;

        private void OnEnable()
        {
            _played = false;
            ClearEffect();
        }

        public async UniTask PlayEntranceAsync(GameManager manager, CancellationToken token)
        {
            if (_played || !isActiveAndEnabled) return;
            _played = true;
            if (!effectRoot || !effectOpacity || emitters == null || emitters.Length == 0)
            {
                Debug.LogWarning("[AdProductHighlight] Product emphasis references are missing. Skipping the effect.", this);
                return;
            }
            int version = ++_version;
            bool started = false;
            try
            {
                token.ThrowIfCancellationRequested();
                _inputLock = manager.Presentation.LockInput();
                ConfigureAppearance(manager);
                effectOpacity.alpha = 0;
                _effectMask.gameObject.SetActive(true);
                effectRoot.SetActive(true);
                foreach (var emitter in emitters) if (emitter) { emitter.Stop(true); emitter.Play(); }
                if (confettiRoot && confettiEmitter)
                {
                    if (confettiOpacity) confettiOpacity.alpha = 0;
                    confettiRoot.SetActive(true);
                    if (_confettiCanvas)
                    {
                        _confettiCanvas.enabled = true;
                        _confettiCanvas.overrideSorting = true;
                        _confettiCanvas.sortingOrder = short.MaxValue - 1;
                    }
                    confettiEmitter.Stop(true); confettiEmitter.Play();
                }
                IsPlaying = true;
                // Owned here, not by the caller's token: the effects outlive this method by design.
                _playback = new CancellationTokenSource();
                RunFiniteEffectsAsync(version, _playback).Forget();
                started = true;
                // The scene entrance, and with it the input lock, ends when this returns.
                while (IsPlaying)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                if (version == _version) StopEffects();
                throw;
            }
            finally
            {
                if (!started && version == _version) StopEffects();
            }
        }

        private async UniTask RunFiniteEffectsAsync(int version, CancellationTokenSource source)
        {
            var token = source.Token;
            bool completed = false;
            try
            {
                double start = Time.realtimeSinceStartupAsDouble;
                float total = Mathf.Clamp(duration, .2f, 1.8f);
                bool burstStopped = false;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    float elapsed = (float)(Time.realtimeSinceStartupAsDouble - start);
                    effectOpacity.alpha = Mathf.Clamp01(elapsed / .25f);
                    if (confettiOpacity) confettiOpacity.alpha = effectOpacity.alpha;
                    if (elapsed >= total && !burstStopped)
                    {
                        foreach (var emitter in emitters) if (emitter && !emitter.loop) emitter.Stop(false);
                        burstStopped = true;
                    }
                    if (IsPlaying && elapsed >= InputLockSeconds)
                    {
                        IsPlaying = false;
                        ReleaseInput();
                    }
                    // Infinite aura is deliberately excluded from the finite entrance.
                    if (burstStopped && (!confettiEmitter || !confettiEmitter.isPlaying)) break;
                    if (elapsed >= 12)
                    {
                        Debug.LogWarning("[AdProductHighlight] Confetti completion timed out. Clearing the finite effect.", this);
                        break;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                ClearConfetti();
                completed = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogException(ex, this); }
            finally
            {
                if (version == _version)
                {
                    _playback = null;
                    if (!completed) ClearEffect();
                    IsPlaying = false;
                    ReleaseInput();
                }
                source.Dispose();
            }
        }

        private float Range(float min, float max) => Mathf.Lerp(min, max, (float)_random.NextDouble());

        private static SeparatedMinMaxCurve Between(float min, float max)
        {
            var value = new SeparatedMinMaxCurve(1);
            value.mainCurve = new ParticleSystem.MinMaxCurve(min, max);
            return value;
        }

        private static Color Tint(Color color, float white, float alpha)
        {
            color = Color.Lerp(color, Color.white, white);
            // Pure white on the pale board needs a shaded variation to remain legible.
            if (Mathf.Min(color.r, Mathf.Min(color.g, color.b)) > .84f)
                color = Color.Lerp(color, new Color(.32f, .36f, .44f), .28f);
            color.a = alpha;
            return color;
        }

        private void ConfigureLayers()
        {
            if (!_effectMask)
            {
                var board = (RectTransform)effectRoot.transform.parent;
                var boardImage = board.GetComponent<Image>();
                if (!boardImage || !boardImage.sprite)
                    throw new InvalidOperationException("The advertisement board requires its rounded mask sprite.");
                var maskObject = new GameObject("AdInnerEffectsMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
                _effectMask = (RectTransform)maskObject.transform;
                _effectMask.SetParent(board, false);
                _effectMask.SetSiblingIndex(effectRoot.transform.GetSiblingIndex());
                _effectMask.anchorMin = Vector2.zero;
                _effectMask.anchorMax = Vector2.one;
                _effectMask.offsetMin = new Vector2(12, 12);
                _effectMask.offsetMax = new Vector2(-12, -12);
                var image = maskObject.GetComponent<Image>();
                image.sprite = boardImage.sprite;
                image.type = boardImage.type;
                image.color = Color.white;
                image.raycastTarget = false;
                maskObject.GetComponent<Mask>().showMaskGraphic = false;
                effectRoot.transform.SetParent(_effectMask, true);
                foreach (var graphic in effectRoot.GetComponentsInChildren<MaskableGraphic>(true))
                {
                    graphic.maskable = true;
                    graphic.RecalculateMasking();
                }
            }
            if (confettiRoot && !_confettiCanvas)
            {
                // Separate from the inner mask; draw over every advertisement image, title and hint.
                _confettiCanvas = confettiRoot.GetComponent<Canvas>();
                if (!_confettiCanvas) _confettiCanvas = confettiRoot.AddComponent<Canvas>();
                _confettiCanvas.overrideSorting = true;
                _confettiCanvas.sortingLayerID = GetComponentInParent<Canvas>().sortingLayerID;
                _confettiCanvas.sortingOrder = short.MaxValue - 1;
                confettiRoot.transform.SetAsLastSibling();
            }
        }

        private void ConfigureAppearance(GameManager manager)
        {
            ConfigureLayers();
            _glow = _wave = null;
            // The authored list contains emitter roots; the wave is a nested child of the glow.
            if (_appearanceEmitters == null) _appearanceEmitters = effectRoot.GetComponentsInChildren<ParticleImage>(true);
            foreach (var emitter in _appearanceEmitters)
            {
                if (!emitter) continue;
                bool glow = emitter.name == "ProductGlowRays", wave = emitter.name == "ProductRing";
                // Each particle picks a different blend of the two generation colors and their soft tints.
                // Missing metadata retains the authored palette; it never reads later RFID input.
                if (manager.HasResultPalette)
                {
                    emitter.startColor = new ParticleSystem.MinMaxGradient(
                        Tint(manager.ResultPrimaryColor, glow ? .02f : 0, glow ? .76f : wave ? .88f : 1),
                        Tint(manager.ResultSecondaryColor, glow ? .10f : .06f, glow ? .86f : wave ? .96f : 1));
                }
                emitter.spreadType = SpreadType.Random;
                if (glow)
                {
                    _glow = emitter;
                    emitter.lifetime = new ParticleSystem.MinMaxCurve(5.8f, 8.2f);
                    emitter.startSize = Between(740, 860);
                    emitter.startRotation = Between(-180, 180);
                    emitter.rotationOverLifetime = Between(-58, 64);
                    emitter.duration = Range(5.9f, 8.1f);
                }
                else if (wave)
                {
                    _wave = emitter;
                    // Continuous emission replaces the metronomic loop burst. Update varies its spacing.
                    emitter.SetBurst(0, 0, 0);
                    emitter.rateOverTime = Range(.62f, .9f);
                    emitter.lifetime = new ParticleSystem.MinMaxCurve(2.8f, 3.6f);
                    emitter.startSize = Between(620, 780);
                    emitter.shape = EmitterShape.Circle;
                    emitter.circleRadius = 7;
                    emitter.emitOnSurface = false;
                }
                else
                {
                    emitter.SetBurst(0, Range(0, .12f), _random.Next(10, 16));
                    emitter.rateOverTime = Range(18, 26);
                    emitter.startDelay = Range(.02f, .16f);
                    emitter.startSize = Between(54, 94);
                    emitter.lifetime = new ParticleSystem.MinMaxCurve(1.05f, 1.6f);
                    emitter.startRotation = Between(-35, 35);
                    emitter.rotationOverLifetime = Between(-22, 28);
                    // ParticleImage converts speed to pixels with a factor of 100.
                    emitter.startSpeed = new ParticleSystem.MinMaxCurve(.04f, .14f);
                    emitter.fitRect = false;
                    emitter.circleRadius = 260;
                    emitter.emitOnSurface = true;
                    emitter.emitterThickness = 85;
                }
            }
            _nextGlowVariation = Time.realtimeSinceStartupAsDouble + Range(3.7f, 6.1f);
            _nextWaveVariation = Time.realtimeSinceStartupAsDouble + Range(1.6f, 3.8f);
            if (confettiRoot && confettiEmitter)
            {
                if (_confettiEmitters == null) _confettiEmitters = confettiRoot.GetComponentsInChildren<ParticleImage>(true);
                foreach (var emitter in _confettiEmitters)
                    if (emitter) emitter.startDelay = emitter == confettiEmitter ? Range(.78f, 1.18f) : Range(.02f, .32f);
                confettiEmitter.rateOverTime = Range(235, 295);
            }
        }

        private void Update()
        {
            if (!AreEffectsVisible) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (_wave && now >= _nextWaveVariation)
            {
                _wave.rateOverTime = Range(.6f, .95f);
                _nextWaveVariation = now + Range(1.7f, 4.1f);
            }
            if (_glow && now >= _nextGlowVariation)
            {
                _glow.duration = Range(5.9f, 8.1f);
                _nextGlowVariation = now + Range(3.7f, 6.1f);
            }
        }

        private void ClearEffect()
        {
            if (emitters != null)
                foreach (var emitter in emitters) if (emitter && emitter.gameObject.activeInHierarchy) emitter.Stop(true);
            if (effectOpacity) effectOpacity.alpha = 0;
            if (effectRoot) effectRoot.SetActive(false);
            if (_effectMask) _effectMask.gameObject.SetActive(false);
            ClearConfetti();
            IsPlaying = false;
        }

        private void ClearConfetti()
        {
            if (confettiEmitter && confettiEmitter.gameObject.activeInHierarchy) confettiEmitter.Stop(true);
            if (confettiOpacity) confettiOpacity.alpha = 0;
            if (confettiRoot) confettiRoot.SetActive(false);
            if (_confettiCanvas) _confettiCanvas.enabled = false;
        }

        public void ResumeAmbient()
        {
            if (!_played || !isActiveAndEnabled || IsPlaying || AreEffectsVisible || !effectRoot) return;
            effectOpacity.alpha = 0;
            if (_effectMask) _effectMask.gameObject.SetActive(true);
            effectRoot.SetActive(true);
            foreach (var emitter in emitters) if (emitter && emitter.loop) { emitter.Stop(true); emitter.Play(); }
            effectOpacity.alpha = 1;
        }

        private void ReleaseInput() { _inputLock?.Dispose(); _inputLock = null; }

        public void StopEffects()
        {
            ++_version;
            _playback?.Cancel();
            _playback = null;
            ClearEffect();
            ReleaseInput();
        }

        private void OnDisable() => StopEffects();
    }
}
