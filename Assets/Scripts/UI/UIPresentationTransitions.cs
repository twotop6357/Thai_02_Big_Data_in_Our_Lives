using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    /// <summary>Manager-owned visual transitions and a shared mouse/touch/key/button lock. No session decisions.</summary>
    [DisallowMultipleComponent]
    public sealed class UIPresentationTransitions : MonoBehaviour
    {
        private GameManager _manager;
        private Canvas _canvas;
        private CanvasGroup _blackout;
        private int _locks;
        public bool IsInputLocked => _locks > 0;
        public float BlackoutAlpha => _blackout ? _blackout.alpha : 0;

        private void Awake()
        {
            _manager = GetComponent<GameManager>();
            var canvasObject = new GameObject("PresentationOverlay", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;
            var imageObject = new GameObject("BlackoutAndInputShield", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            imageObject.transform.SetParent(canvasObject.transform, false);
            var image = imageObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            _blackout = imageObject.GetComponent<CanvasGroup>();
            _blackout.alpha = 0;
            _canvas.enabled = false;
        }

        public IDisposable LockInput()
        {
            if (_locks++ == 0) _manager.ResetButtonInput();
            UpdateShield();
            return new InputLease(this);
        }

        private sealed class InputLease : IDisposable
        {
            private UIPresentationTransitions _owner;
            public InputLease(UIPresentationTransitions owner) => _owner = owner;
            public void Dispose()
            {
                var owner = _owner; _owner = null;
                if (!owner) return;
                owner._locks = Mathf.Max(0, owner._locks - 1);
                if (owner._locks == 0) owner._manager.ResetButtonInput();
                owner.UpdateShield();
            }
        }

        private void UpdateShield()
        {
            if (_canvas) _canvas.enabled = _locks > 0 || _blackout.alpha > 0;
        }

        public async UniTask CoverAsync(CancellationToken token)
        {
            _canvas.enabled = true;
            await UIMotion.FadeAsync(_blackout, 1, UIMotion.SceneSeconds, token);
        }

        public void CoverImmediately() { _blackout.alpha = 1; _canvas.enabled = true; }

        public async UniTask UncoverAsync(CancellationToken token)
        {
            await UIMotion.FadeAsync(_blackout, 0, UIMotion.SceneSeconds, token);
            UpdateShield();
        }

        public async UniTask RevealReadySceneAsync(CancellationToken token)
        {
            var scene = FindFirstObjectByType<UIScenePresentation>();
            if (scene) await scene.PrepareAsync(token);
            await UIMotion.FadeAsync(_blackout, 0, UIMotion.SceneSeconds, token);
            if (scene) await scene.PlayEntranceAsync(token);
            UpdateShield();
        }

        public async UniTask SwitchPanelsAsync(UIAnimatedElement from, UIAnimatedElement to, CancellationToken token)
        {
            using (LockInput())
            {
                if (from && from != to && !await from.SetVisibleAsync(false, token)) return;
                token.ThrowIfCancellationRequested();
                if (to) await to.SetVisibleAsync(true, token);
            }
        }

        public void ClearBlackout() { if (_blackout) _blackout.alpha = 0; UpdateShield(); }
        public void RestoreVisibleScene()
        {
            var scene = FindFirstObjectByType<UIScenePresentation>();
            if (scene) scene.RestoreVisible();
            ClearBlackout();
        }
        private void OnDisable() { _locks = 0; ClearBlackout(); }
    }
}
