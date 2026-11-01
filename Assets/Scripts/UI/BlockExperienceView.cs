using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Exhibition.Blocks;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    [DisallowMultipleComponent]
    public sealed class BlockExperienceView : MonoBehaviour
    {
        [SerializeField] private BlockExperienceController controller;
        [SerializeField] private GameObject groupPanel, selectionPanel, groupContinue, selectionContinue;
        [SerializeField] private UIAnimatedElement[] groupSlots, selectionSlots;
        private readonly RfidBlock?[] _lastBlocks = new RfidBlock?[4];
        private readonly Image[] _groupArtwork = new Image[4], _selectionArtwork = new Image[4];
        private RfidReaderHub _readerHub;
        private CancellationTokenSource _viewCts;
        private GameManager _manager;
        private bool _choosingProduct, _hasPanelRequest, _requestedProduct;
        private CancellationTokenSource _panelCts;
        private UIBlockSlotTransfer _slotTransfer;
        private UIAnimatedElement _groupMotion, _selectionMotion;
        private UIScenePresentation _scenePresentation;
        private UIAnimatedElement _groupHint, _selectionHint;
        private bool _groupHintRequested, _selectionHintRequested;
        public bool IsChoosingProduct => _choosingProduct;
        public bool AreRequiredBlocksRecognized => controller && controller.IsReady &&
            (_choosingProduct ? controller.HasRequiredComposition : controller.HasSingleGroup);

        private void OnEnable()
        {
            _manager = FindFirstObjectByType<GameManager>();
            _readerHub = FindFirstObjectByType<RfidReaderHub>();
            if (!controller || !_manager || !_readerHub)
            {
                Debug.LogError("[BlockExperienceView] Assign the block controller and start from Idle.", this);
                enabled = false;
                return;
            }
            _groupMotion = groupPanel.GetComponent<UIAnimatedElement>();
            _selectionMotion = selectionPanel.GetComponent<UIAnimatedElement>();
            _scenePresentation = GetComponent<UIScenePresentation>();
            _hasPanelRequest = false;
            _viewCts = new CancellationTokenSource();
            _groupHint = groupContinue.GetComponent<UIAnimatedElement>();
            _selectionHint = selectionContinue ? selectionContinue.GetComponent<UIAnimatedElement>() : null;
            _groupHintRequested = _selectionHintRequested = false;
            _groupHint?.PrepareHidden(); groupContinue.SetActive(false);
            _selectionHint?.PrepareHidden(); if (selectionContinue) selectionContinue.SetActive(false);
            for (int i = 0; i < 4; i++)
            {
                _lastBlocks[i] = _readerHub.Readers[i];
                _groupArtwork[i] = groupSlots[i].transform.Find("BlockContent").GetComponent<Image>();
                _selectionArtwork[i] = selectionSlots[i].transform.Find("BlockContent").GetComponent<Image>();
                foreach (var image in new[] { _groupArtwork[i], _selectionArtwork[i] })
                {
                    image.preserveAspect = true;
                    image.color = Color.white;
                    image.rectTransform.anchorMin = Vector2.zero; image.rectTransform.anchorMax = Vector2.one;
                    image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
                }
            }
            controller.PresentationChanged += Refresh;
            _manager.ShortButtonPressed += Continue;
            Refresh();
        }

        private void Continue()
        {
            if (!AreRequiredBlocksRecognized || controller.IsGenerating || _manager.IsPresentationLocked) return;
            if (!_choosingProduct)
            {
                if (controller.Selection[BlockSlot.Group] == null) return;
                _choosingProduct = true;
                Refresh();
            }
            else if (controller.HasGeneratedImage) _manager.TryOpenResult();
            else controller.GenerateImage();
        }

        private void Refresh()
        {
            if (!isActiveAndEnabled || !controller || !groupPanel || !selectionPanel) return;
            RefreshArtwork();
            AnimateNewPlacements();
            UpdateHint(_groupHint, groupContinue, controller.IsReady && controller.HasSingleGroup && !controller.IsGenerating, ref _groupHintRequested);
            UpdateHint(_selectionHint, selectionContinue, controller.IsReady && controller.HasRequiredComposition && !controller.IsGenerating, ref _selectionHintRequested);
            if (!_hasPanelRequest || !_groupMotion || !_selectionMotion)
            {
                groupPanel.SetActive(!_choosingProduct);
                selectionPanel.SetActive(_choosingProduct);
                _requestedProduct = _choosingProduct;
                _hasPanelRequest = true;
                return;
            }
            if (_requestedProduct == _choosingProduct) return;
            _requestedProduct = _choosingProduct;
            var previous = _panelCts; _panelCts = null; previous?.Cancel();
            _panelCts = new CancellationTokenSource();
            AnimatePanelAsync(_choosingProduct, _panelCts).Forget();
        }

        private void UpdateHint(UIAnimatedElement motion, GameObject hint, bool visible, ref bool requested)
        {
            if (!hint || requested == visible) return;
            requested = visible;
            if (motion) AnimateHintAsync(motion, visible, _viewCts.Token).Forget();
            else hint.SetActive(visible);
        }

        private async UniTask AnimateHintAsync(UIAnimatedElement hint, bool visible, CancellationToken token)
        {
            using (_manager.Presentation.LockInput()) await hint.SetVisibleAsync(visible, token);
        }

        private void AnimateNewPlacements()
        {
            for (int i = 0; i < 4; i++)
            {
                var current = _readerHub.Readers[i];
                bool placed = current.HasValue && !Nullable.Equals(current, _lastBlocks[i]);
                _lastBlocks[i] = current;
                if (!placed || _manager.IsTransitioning || _panelCts != null) continue;
                var slots = groupPanel.activeInHierarchy ? groupSlots : selectionSlots;
                if (slots != null && slots.Length > i && slots[i] && slots[i].gameObject.activeInHierarchy)
                    AnimatePlacementAsync(slots[i], _viewCts.Token).Forget();
            }
        }

        private void RefreshArtwork()
        {
            for (int i = 0; i < 4; i++)
            {
                var sprite = _readerHub.Artwork.Find(_readerHub.Readers[i]);
                _groupArtwork[i].sprite = sprite; _groupArtwork[i].enabled = sprite;
                _selectionArtwork[i].sprite = sprite; _selectionArtwork[i].enabled = sprite;
            }
        }

        private async UniTask AnimatePlacementAsync(UIAnimatedElement slot, CancellationToken token)
        {
            using (_manager.Presentation.LockInput()) await slot.PlayPlacementAsync(token);
        }

        private async UniTask AnimatePanelAsync(bool product, CancellationTokenSource source)
        {
            using (_manager.Presentation.LockInput())
            {
                var target = product ? _selectionMotion : _groupMotion;
                var previous = product ? _groupMotion : _selectionMotion;
                try
                {
                    if (!target.gameObject.activeSelf)
                    {
                        target.PrepareHidden();
                        _scenePresentation?.PrepareCards(target.transform);
                    }
                    if (product)
                    {
                        // The original frames stay opaque above the crossfading pages; destinations stay inactive.
                        if (_slotTransfer == null)
                            _slotTransfer = new UIBlockSlotTransfer((RectTransform)transform, groupSlots, selectionSlots);
                        await UniTask.WhenAll(_slotTransfer.PlayAsync(source.Token),
                            SetPanelVisibleAsync(previous, false, source.Token),
                            SetPanelVisibleAsync(target, true, source.Token),
                            _scenePresentation ? _scenePresentation.PlayEntranceAsync(source.Token) : UniTask.CompletedTask);
                        source.Token.ThrowIfCancellationRequested();
                        _slotTransfer.Complete();
                        _slotTransfer.Dispose(); _slotTransfer = null;
                    }
                    else
                    {
                        await _manager.Presentation.SwitchPanelsAsync(previous, target, source.Token);
                        source.Token.ThrowIfCancellationRequested();
                        if (_scenePresentation) await _scenePresentation.PlayEntranceAsync(source.Token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    if (this && _panelCts == source)
                    {
                        _slotTransfer?.Dispose(); _slotTransfer = null;
                        previous.gameObject.SetActive(false);
                        target.SnapVisible();
                        _scenePresentation?.RestoreVisible();
                        Debug.LogException(ex, this);
                    }
                }
                finally
                {
                    if (_panelCts == source) _panelCts = null;
                    source.Dispose();
                }
            }
        }

        private static async UniTask SetPanelVisibleAsync(UIAnimatedElement panel, bool visible, CancellationToken token)
        {
            if (!await panel.SetVisibleAsync(visible, token)) token.ThrowIfCancellationRequested();
        }

        private void OnDisable()
        {
            _viewCts?.Cancel(); _viewCts?.Dispose(); _viewCts = null;
            var source = _panelCts; _panelCts = null; source?.Cancel();
            _slotTransfer?.Dispose(); _slotTransfer = null;
            if (controller) controller.PresentationChanged -= Refresh;
            if (_manager) _manager.ShortButtonPressed -= Continue;
        }
    }
}
