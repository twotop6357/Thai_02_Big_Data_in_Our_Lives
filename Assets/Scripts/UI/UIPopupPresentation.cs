using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    /// <summary>The shared popup sequence: dim first, then reveal the card at its design scale.</summary>
    [DisallowMultipleComponent]
    public sealed class UIPopupPresentation : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private Graphic backdrop;
        [SerializeField] private UIAnimatedElement content;
        private float _designAlpha;
        private bool _initialized, _visible, _running;
        private CancellationTokenSource _source;
        private UniTask<bool> _operation;
        private int _version;
        public bool IsAnimating => _running;

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _designAlpha = backdrop.color.a;
        }

        public void ResetHidden()
        {
            Initialize();
            _version++;
            var source = _source; _source = null; source?.Cancel();
            _running = _visible = false;
            content.PrepareHidden();
            var color = backdrop.color; color.a = 0; backdrop.color = color;
            overlay.SetActive(false);
        }

        public UniTask<bool> SetVisibleAsync(bool visible, CancellationToken token)
        {
            Initialize();
            if (_visible == visible && _running) return _operation;
            if (_visible == visible && overlay.activeSelf == visible) return UniTask.FromResult(true);
            var previous = _source; _source = null; previous?.Cancel();
            _visible = visible; _running = true;
            int version = ++_version;
            _source = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
            _operation = AnimateAsync(visible, version, _source).Preserve();
            return _operation;
        }

        private async UniTask<bool> AnimateAsync(bool visible, int version, CancellationTokenSource source)
        {
            var manager = FindFirstObjectByType<GameManager>();
            using (manager ? manager.Presentation.LockInput() : null)
            {
                try
                {
                    source.Token.ThrowIfCancellationRequested();
                    if (visible)
                    {
                        if (!overlay.activeSelf)
                        {
                            content.PrepareHidden();
                            var color = backdrop.color; color.a = 0; backdrop.color = color;
                            overlay.SetActive(true);
                        }
                        await backdrop.DOFade(_designAlpha, UIMotion.BackdropSeconds).SetEase(Ease.InOutSine).SetUpdate(true)
                            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, source.Token);
                        if (!await content.SetVisibleAsync(true, source.Token)) return false;
                    }
                    else
                    {
                        if (!await content.SetVisibleAsync(false, source.Token)) return false;
                        await backdrop.DOFade(0, UIMotion.BackdropSeconds).SetEase(Ease.InOutSine).SetUpdate(true)
                            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, source.Token);
                        if (version == _version) overlay.SetActive(false);
                    }
                    source.Token.ThrowIfCancellationRequested();
                    return version == _version;
                }
                catch (OperationCanceledException) { return false; }
                catch (Exception ex) { if (this) Debug.LogException(ex, this); return false; }
                finally
                {
                    if (_source == source)
                    {
                        _source = null; _running = false;
                        if (source.IsCancellationRequested && this) ResetHidden();
                    }
                    source.Dispose();
                }
            }
        }

        private void OnDisable() { if (_initialized) ResetHidden(); }
    }
}
