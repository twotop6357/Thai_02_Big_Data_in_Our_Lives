using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Exhibition.Blocks;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    [DefaultExecutionOrder(-500), DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
    public sealed class UIScenePresentation : MonoBehaviour
    {
        [SerializeField] private LocalizedImageSet localization;
        [SerializeField] private BlockExperienceController blocks;
        [SerializeField] private AdProductHighlight productHighlight;
        [SerializeField] private UIAnimatedElement[] cards;
        private CanvasGroup _root;
        private GameManager _manager;
        private CancellationTokenSource _screenCts;
        private bool _started;
        public bool IsPrepared { get; private set; }

        private void OnEnable()
        {
            _root = GetComponent<CanvasGroup>();
            _root.alpha = 0;
            IsPrepared = false;
            _screenCts = new CancellationTokenSource();
            foreach (var card in cards) if (card) card.PrepareHidden();
            if (_started && _manager && !_manager.IsTransitioning) EnterInitialSceneAsync().Forget();
        }

        private void Start()
        {
            _started = true;
            _manager = FindFirstObjectByType<GameManager>();
            if (_manager && !_manager.IsTransitioning) EnterInitialSceneAsync().Forget();
        }

        private async UniTask EnterInitialSceneAsync()
        {
            var transitions = _manager.Presentation;
            using (transitions.LockInput())
            {
                transitions.CoverImmediately();
                try { await transitions.RevealReadySceneAsync(_screenCts.Token); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { if (this) { _root.alpha = 1; Debug.LogException(ex, this); } }
                finally { if (transitions) transitions.ClearBlackout(); }
            }
        }

        public async UniTask PrepareAsync(CancellationToken token)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _screenCts.Token);
            token = linked.Token;
            double start = Time.realtimeSinceStartupAsDouble;
            while ((localization && !localization.IsInitializationComplete) || (blocks && !blocks.IsReady))
            {
                token.ThrowIfCancellationRequested();
                if (Time.realtimeSinceStartupAsDouble - start >= 16)
                {
                    Debug.LogWarning("[UIScenePresentation] Content preparation timed out. Revealing the available fallback UI.", this);
                    break;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
            _root.alpha = 1;
            IsPrepared = true;
        }

        public async UniTask PlayEntranceAsync(CancellationToken token)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _screenCts.Token);
            token = linked.Token;
            var tasks = new System.Collections.Generic.List<UniTask>();
            int index = 0;
            foreach (var card in cards)
            {
                if (!card || !card.gameObject.activeInHierarchy) continue;
                // A live block removal may already be hiding its hint during a panel crossfade.
                if (card.IsAnimating && !card.IsVisible) continue;
                tasks.Add(ShowAfterAsync(card, card.Style == UIAnimatedElement.Motion.Card ? index++ * UIMotion.ItemInterval : 0, token));
            }
            if (productHighlight) tasks.Add(productHighlight.PlayEntranceAsync(_manager, token));
            await UniTask.WhenAll(tasks);
        }

        private static async UniTask ShowAfterAsync(UIAnimatedElement card, float delay, CancellationToken token)
        {
            if (delay > 0) await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.UnscaledDeltaTime, cancellationToken: token);
            token.ThrowIfCancellationRequested();
            if (card && card.gameObject.activeInHierarchy) await card.SetVisibleAsync(true, token);
        }

        public void PrepareCards(Transform panel)
        {
            foreach (var card in cards) if (card && card.transform.IsChildOf(panel)) card.PrepareHidden();
        }

        public void RestoreVisible()
        {
            _root.alpha = 1;
            foreach (var card in cards) if (card && card.gameObject.activeInHierarchy) card.SnapVisible();
        }

        private void OnDisable()
        {
            _screenCts?.Cancel(); _screenCts?.Dispose(); _screenCts = null;
            IsPrepared = false;
        }
    }
}
