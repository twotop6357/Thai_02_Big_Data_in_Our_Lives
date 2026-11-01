using System;
using System.Threading;
using AssetKits.ParticleImage;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Exhibition.UI
{
    /// <summary>Finite entrance effects lock input; ambient aura stays until the advertisement exits.</summary>
    [DisallowMultipleComponent]
    public sealed class AdProductHighlight : MonoBehaviour
    {
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
            using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
            _playback = source;
            bool completed = false;
            try
            {
                token = source.Token;
                token.ThrowIfCancellationRequested();
                _inputLock = manager.Presentation.LockInput();
                IsPlaying = true;
                effectOpacity.alpha = 0;
                effectRoot.SetActive(true);
                foreach (var emitter in emitters) if (emitter) { emitter.Stop(true); emitter.Play(); }
                if (confettiRoot && confettiEmitter)
                {
                    if (confettiOpacity) confettiOpacity.alpha = 0;
                    confettiRoot.SetActive(true);
                    confettiEmitter.Stop(true); confettiEmitter.Play();
                }
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
                        foreach (var emitter in emitters) if (emitter && !emitter.loop) emitter.Stop(true);
                        burstStopped = true;
                    }
                    // Infinite aura is deliberately excluded from the finite entrance/input barrier.
                    if (burstStopped && (!confettiEmitter || !confettiEmitter.isPlaying)) break;
                    if (elapsed >= 12)
                    {
                        Debug.LogWarning("[AdProductHighlight] Confetti completion timed out. Clearing the finite effect and restoring input.", this);
                        break;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                ClearConfetti();
                completed = true;
            }
            finally
            {
                if (version == _version)
                {
                    _playback = null;
                    if (!completed) ClearEffect();
                    IsPlaying = false;
                    ReleaseInput();
                }
            }
        }

        private void ClearEffect()
        {
            if (emitters != null)
                foreach (var emitter in emitters) if (emitter && emitter.gameObject.activeInHierarchy) emitter.Stop(true);
            if (effectOpacity) effectOpacity.alpha = 0;
            if (effectRoot) effectRoot.SetActive(false);
            ClearConfetti();
            IsPlaying = false;
        }

        private void ClearConfetti()
        {
            if (confettiEmitter && confettiEmitter.gameObject.activeInHierarchy) confettiEmitter.Stop(true);
            if (confettiOpacity) confettiOpacity.alpha = 0;
            if (confettiRoot) confettiRoot.SetActive(false);
        }

        public void ResumeAmbient()
        {
            if (!_played || !isActiveAndEnabled || IsPlaying || AreEffectsVisible || !effectRoot) return;
            effectOpacity.alpha = 0;
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
