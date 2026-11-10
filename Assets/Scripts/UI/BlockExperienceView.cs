using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Exhibition.Blocks;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    [DisallowMultipleComponent]
    public sealed class BlockExperienceView : MonoBehaviour
    {
        private const string SlotLabelPath = "SlotLabelCover/SlotLabel";
        private const float LabelBounceScale = .18f, LabelBounceSeconds = .55f, BlockShakePixels = 10f, BlockShakeSeconds = .45f, CueElasticity = .6f;
        private const int LabelBounceVibrato = 5, BlockShakeVibrato = 8;
        [SerializeField] private BlockExperienceController controller;
        [SerializeField] private GameObject groupPanel, selectionPanel, groupContinue, selectionContinue;
        [SerializeField] private UIAnimatedElement groupSlot;
        // Purpose order: group, product, first color, second color.
        [SerializeField] private UIAnimatedElement[] selectionSlots;
        private readonly RfidBlock?[] _lastBlocks = new RfidBlock?[RfidReaderSelection.SlotCount];
        private readonly Image[] _selectionArtwork = new Image[RfidReaderSelection.SlotCount];
        private readonly Image[] _selectionFrames = new Image[RfidReaderSelection.SlotCount];
        private Image _groupArtwork, _groupFrame;
        // Baked words stay in the frame PNG, hidden by the label's parent cover so only the words bounce.
        private readonly RectTransform[] _selectionLabels = new RectTransform[RfidReaderSelection.SlotCount];
        private readonly Tween[] _selectionLabelTweens = new Tween[RfidReaderSelection.SlotCount];
        private readonly Tween[] _selectionShakeTweens = new Tween[RfidReaderSelection.SlotCount];
        private RectTransform _groupLabel;
        private Tween _groupLabelTween, _groupShakeTween;
        private readonly BlockAttentionSchedule _attention = new BlockAttentionSchedule();
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
        private bool _autoAdvanceRequested;
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
            _readerHub.Readers.RequireGroupFirst = !_choosingProduct;
            _autoAdvanceRequested = !_choosingProduct && _readerHub.Readers.GroupCount == 1 &&
                _readerHub.Readers.ProductCount + _readerHub.Readers.ColorCount > 0;
            _viewCts = new CancellationTokenSource();
            _groupHint = groupContinue.GetComponent<UIAnimatedElement>();
            _selectionHint = selectionContinue ? selectionContinue.GetComponent<UIAnimatedElement>() : null;
            _groupHintRequested = _selectionHintRequested = false;
            _groupHint?.PrepareHidden(); groupContinue.SetActive(false);
            _selectionHint?.PrepareHidden(); if (selectionContinue) selectionContinue.SetActive(false);
            _groupFrame = groupSlot.GetComponent<Image>();
            _groupArtwork = PrepareArtwork(groupSlot);
            _groupLabel = (RectTransform)groupSlot.transform.Find(SlotLabelPath);
            for (int i = 0; i < RfidReaderSelection.SlotCount; i++)
            {
                _lastBlocks[i] = _readerHub.Readers.DisplayBlock(i);
                _selectionFrames[i] = selectionSlots[i].GetComponent<Image>();
                _selectionArtwork[i] = PrepareArtwork(selectionSlots[i]);
                _selectionLabels[i] = (RectTransform)selectionSlots[i].transform.Find(SlotLabelPath);
            }
            controller.PresentationChanged += Refresh;
            _readerHub.ReaderChanged += OnReaderChanged;
            _readerHub.ArtworkChanged += RefreshArtwork;
            _manager.ShortButtonPressed += Continue;
            Refresh();
        }

        private static Image PrepareArtwork(UIAnimatedElement slot)
        {
            var image = slot.transform.Find("BlockContent").GetComponent<Image>();
            image.preserveAspect = true;
            image.color = Color.white;
            image.rectTransform.anchorMin = Vector2.zero; image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
            return image;
        }

        private void Continue()
        {
            if (!AreRequiredBlocksRecognized || controller.IsGenerating || _manager.IsPresentationLocked) return;
            if (!_choosingProduct)
            {
                if (controller.Selection[BlockSlot.Group] == null) return;
                _autoAdvanceRequested = false;
                _readerHub.Readers.RequireGroupFirst = false;
                _choosingProduct = true;
                _manager.Audio.PlayConfirm();
                Refresh();
            }
            else if (controller.HasGeneratedImage) _manager.TryOpenResult();
            else controller.GenerateImage();
        }

        private void OnReaderChanged(int reader, RfidBlock? previous, RfidBlock? current)
        {
            // Hub rejects blocks while the generation snapshot holds input, so only accepted cards reach here.
            if (current.HasValue && !Nullable.Equals(previous, current) && !_manager.IsTransitioning)
                _manager.Audio.PlaySelected();
            if (_choosingProduct) return;
            var readers = _readerHub.Readers;
            if (readers.GroupCount != 1 || readers.ProductCount + readers.ColorCount == 0)
            {
                _autoAdvanceRequested = false;
                return;
            }
            if (current.HasValue && current.Value.Kind != BlockKind.Group)
                _autoAdvanceRequested = true;
        }

        private void LateUpdate()
        {
            UpdateAttention();
            if (!_autoAdvanceRequested || _choosingProduct) return;
            var readers = _readerHub.Readers;
            if (readers.GroupCount != 1 || readers.ProductCount + readers.ColorCount == 0)
            {
                _autoAdvanceRequested = false;
                return;
            }
            // Defer while placement/scene animation holds input; use the exact button transition path.
            if (!controller.IsReady || controller.IsGenerating || _manager.IsTransitioning || _manager.IsPresentationLocked) return;
            Continue();
        }

        // The cues never lock input: they repeat while a visitor may press the button at any moment.
        private void UpdateAttention()
        {
            var readers = _readerHub.Readers;
            bool active = controller.IsReady && !controller.IsGenerating && _panelCts == null &&
                !_manager.IsTransitioning && !_manager.IsPresentationLocked;
            // The group page only shows the group slot.
            int visible = _choosingProduct ? RfidReaderSelection.SlotCount : RfidReaderSelection.GroupSlot + 1;
            int emptyCount = 0;
            int overfilled = 0;
            for (int i = 0; i < visible; i++)
            {
                if (!readers.DisplayBlock(i).HasValue) emptyCount++;
                if (readers.IsSlotOverfilled(i)) overfilled |= 1 << i;
            }
            _attention.Tick(Time.unscaledDeltaTime, active, readers.Revision, emptyCount > 0, overfilled, out bool playEmpty, out bool playOverfilled);
            if (!playEmpty && !playOverfilled) return;
            // Each empty cue points at one needed block, picked at random among the empty slots.
            int emptyPick = playEmpty ? UnityEngine.Random.Range(0, emptyCount) : -1;
            for (int i = 0; i < visible; i++)
            {
                if (!readers.DisplayBlock(i).HasValue && emptyPick-- == 0)
                {
                    if (_choosingProduct) BounceLabel(_selectionLabels[i], ref _selectionLabelTweens[i]);
                    else BounceLabel(_groupLabel, ref _groupLabelTween);
                }
                if (playOverfilled && (overfilled & 1 << i) != 0)
                {
                    if (_choosingProduct) ShakeBlock(_selectionArtwork[i].rectTransform, ref _selectionShakeTweens[i]);
                    else ShakeBlock(_groupArtwork.rectTransform, ref _groupShakeTween);
                }
            }
        }

        private static void BounceLabel(RectTransform label, ref Tween tween)
        {
            StopBounce(label, ref tween);
            tween = label.DOPunchScale(Vector3.one * LabelBounceScale, LabelBounceSeconds, LabelBounceVibrato, CueElasticity)
                .SetUpdate(true).SetLink(label.gameObject);
        }

        private static void ShakeBlock(RectTransform artwork, ref Tween tween)
        {
            StopShake(artwork, ref tween);
            tween = artwork.DOPunchAnchorPos(new Vector2(BlockShakePixels, 0), BlockShakeSeconds, BlockShakeVibrato, CueElasticity)
                .SetUpdate(true).SetLink(artwork.gameObject);
        }

        private static void StopBounce(RectTransform label, ref Tween tween)
        {
            tween?.Kill(); tween = null;
            if (label) label.localScale = Vector3.one;
        }

        private static void StopShake(RectTransform artwork, ref Tween tween)
        {
            tween?.Kill(); tween = null;
            if (artwork) artwork.anchoredPosition = Vector2.zero;
        }

        // Panels and the group frame move next, so cues must not leave a slot mid-offset.
        private void StopAttentionCues()
        {
            StopBounce(_groupLabel, ref _groupLabelTween);
            if (_groupArtwork) StopShake(_groupArtwork.rectTransform, ref _groupShakeTween);
            for (int i = 0; i < RfidReaderSelection.SlotCount; i++)
            {
                StopBounce(_selectionLabels[i], ref _selectionLabelTweens[i]);
                if (_selectionArtwork[i]) StopShake(_selectionArtwork[i].rectTransform, ref _selectionShakeTweens[i]);
            }
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
            StopAttentionCues();
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
            for (int i = 0; i < RfidReaderSelection.SlotCount; i++)
            {
                var current = _readerHub.Readers.DisplayBlock(i);
                bool placed = current.HasValue && !Nullable.Equals(current, _lastBlocks[i]);
                _lastBlocks[i] = current;
                if (!placed || _manager.IsTransitioning || _panelCts != null) continue;
                // The group panel only has the group slot.
                var slot = groupPanel.activeInHierarchy ? (i == RfidReaderSelection.GroupSlot ? groupSlot : null) : selectionSlots[i];
                if (slot && slot.gameObject.activeInHierarchy)
                    AnimatePlacementAsync(slot, _viewCts.Token).Forget();
            }
        }

        private void RefreshArtwork()
        {
            var language = _manager.CurrentLanguage;
            ShowBlock(_groupFrame, _groupArtwork, _groupLabel, ref _groupLabelTween,
                _readerHub.Artwork.Find(_readerHub.Readers.DisplayBlock(RfidReaderSelection.GroupSlot), language));
            for (int i = 0; i < RfidReaderSelection.SlotCount; i++)
                ShowBlock(_selectionFrames[i], _selectionArtwork[i], _selectionLabels[i], ref _selectionLabelTweens[i],
                    _readerHub.Artwork.Find(_readerHub.Readers.DisplayBlock(i), language));
        }

        // The labelled empty frame is replaced by the block card, so its dashes never show behind it.
        private static void ShowBlock(Image frame, Image artwork, RectTransform label, ref Tween labelTween, Sprite sprite)
        {
            artwork.sprite = sprite; artwork.enabled = sprite;
            frame.enabled = !sprite;
            if (sprite) StopBounce(label, ref labelTween);
            label.parent.gameObject.SetActive(!sprite);
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
                        // The group frame shrinks onto its selection slot above the crossfading pages.
                        if (_slotTransfer == null)
                            _slotTransfer = new UIBlockSlotTransfer((RectTransform)transform, groupSlot, selectionSlots[RfidReaderSelection.GroupSlot]);
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
            StopAttentionCues();
            _viewCts?.Cancel(); _viewCts?.Dispose(); _viewCts = null;
            var source = _panelCts; _panelCts = null; source?.Cancel();
            _slotTransfer?.Dispose(); _slotTransfer = null;
            if (controller) controller.PresentationChanged -= Refresh;
            if (_readerHub) _readerHub.ReaderChanged -= OnReaderChanged;
            if (_readerHub) _readerHub.ArtworkChanged -= RefreshArtwork;
            _autoAdvanceRequested = false;
            if (_manager) _manager.ShortButtonPressed -= Continue;
        }
    }
}
