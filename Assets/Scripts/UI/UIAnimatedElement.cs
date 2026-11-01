using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Exhibition.UI
{
    /// <summary>Owns only this element's tween. Opposite requests reverse from the current rendered state.</summary>
    [DefaultExecutionOrder(-400), DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
    public sealed class UIAnimatedElement : MonoBehaviour
    {
        public enum Motion { Hint, Card, Panel }
        [SerializeField] private Motion motion;
        [SerializeField] private bool animateOnEnable;
        private CanvasGroup _group;
        private Vector3 _designScale;
        private bool _initialized, _visible, _running, _originalInteractable, _originalRaycasts;
        private CancellationTokenSource _motionCts;
        private UniTask<bool> _operation;
        private int _version;
        public Vector3 DesignScale { get { Initialize(); return _designScale; } }
        public bool IsAnimating => _running;
        public bool IsVisible => _visible;
        public Motion Style => motion;

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _group = GetComponent<CanvasGroup>();
            _designScale = transform.localScale;
            _originalInteractable = _group.interactable;
            _originalRaycasts = _group.blocksRaycasts;
            _visible = gameObject.activeSelf;
        }

        public void PrepareHidden()
        {
            Initialize();
            Cancel();
            _visible = false;
            _group.alpha = 0;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            transform.localScale = motion == Motion.Card ? Vector3.zero : _designScale;
        }

        public void SnapVisible()
        {
            Initialize(); Cancel(); _visible = true;
            transform.localScale = _designScale;
            _group.alpha = 1;
            _group.interactable = _originalInteractable;
            _group.blocksRaycasts = _originalRaycasts;
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            Initialize();
            if (!animateOnEnable || _running) return;
            PrepareHidden();
            ShowAutomaticallyAsync().Forget();
        }

        private async UniTask ShowAutomaticallyAsync()
        {
            var manager = FindFirstObjectByType<GameManager>();
            using (manager ? manager.Presentation.LockInput() : null)
                await SetVisibleAsync(true, this.GetCancellationTokenOnDestroy());
        }

        public UniTask<bool> SetVisibleAsync(bool visible, CancellationToken token = default)
        {
            Initialize();
            if (_visible == visible && _running) return _operation;
            if (_visible == visible && gameObject.activeSelf == visible &&
                Mathf.Abs(_group.alpha - (visible ? 1 : 0)) < .0001f &&
                (motion != Motion.Card || !visible || transform.localScale == _designScale)) return UniTask.FromResult(true);
            Cancel();
            _visible = visible;
            int version = ++_version;
            _motionCts = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
            _running = true;
            _operation = AnimateAsync(visible, version, _motionCts, motion).Preserve();
            return _operation;
        }

        public UniTask<bool> PlayPlacementAsync(CancellationToken token)
        {
            Initialize();
            if (!isActiveAndEnabled) return UniTask.FromResult(false);
            Cancel();
            _visible = true;
            transform.localScale = Vector3.zero;
            int version = ++_version;
            _motionCts = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
            _running = true;
            _operation = AnimateAsync(true, version, _motionCts, Motion.Card).Preserve();
            return _operation;
        }

        private async UniTask<bool> AnimateAsync(bool visible, int version, CancellationTokenSource source, Motion animationMotion)
        {
            try
            {
                source.Token.ThrowIfCancellationRequested();
                // Initialize alpha/scale BEFORE activation; a reversed animation keeps its current values.
                if (visible && !gameObject.activeSelf)
                {
                    _group.alpha = 0;
                    transform.localScale = animationMotion == Motion.Card ? Vector3.zero : _designScale;
                    gameObject.SetActive(true);
                }
                _group.interactable = false;
                _group.blocksRaycasts = false;
                float duration = animationMotion == Motion.Panel ? UIMotion.PanelSeconds :
                    animationMotion == Motion.Card ? UIMotion.PopupSeconds : UIMotion.HintSeconds;
                if (animationMotion == Motion.Card)
                {
                    var sequence = DOTween.Sequence().SetUpdate(true);
                    sequence.Append(transform.DOScale(visible ? _designScale : Vector3.zero, duration)
                        .SetEase(visible ? Ease.OutBack : Ease.InOutSine));
                    sequence.Join(_group.DOFade(visible ? 1 : 0, duration).SetEase(Ease.InOutSine));
                    await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, source.Token);
                }
                else await UIMotion.FadeAsync(_group, visible ? 1 : 0, duration, source.Token);
                source.Token.ThrowIfCancellationRequested();
                if (version != _version) return false;
                _group.interactable = visible && _originalInteractable;
                _group.blocksRaycasts = visible && _originalRaycasts;
                _running = false;
                if (!visible) gameObject.SetActive(false);
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex) { if (this) Debug.LogException(ex, this); return false; }
            finally
            {
                if (_motionCts == source) { _motionCts = null; _running = false; }
                source.Dispose();
            }
        }

        private void Cancel()
        {
            _version++;
            var source = _motionCts;
            _motionCts = null;
            source?.Cancel();
            _running = false;
        }

        private void OnDisable()
        {
            Cancel();
            if (!_initialized) return;
            _group.alpha = 0;
            transform.localScale = _designScale;
            _visible = false;
        }
    }
}
