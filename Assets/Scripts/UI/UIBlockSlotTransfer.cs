using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Exhibition.UI
{
    /// <summary>Moves the existing four frames above both panels, then hands off at identical corners.</summary>
    internal sealed class UIBlockSlotTransfer : IDisposable
    {
        private sealed class Slot
        {
            public UIAnimatedElement Source, Target;
            public RectTransform Rect;
            public Transform Parent;
            public int Sibling;
            public Vector2 AnchorMin, AnchorMax, Pivot, Size;
            public Vector3 Position, Scale;
            public Quaternion Rotation;
            public Vector2 FromCenter, FromSize, ToCenter, ToSize;
            public bool TargetActive;
        }

        private readonly Slot[] _slots = new Slot[4];
        private RectTransform _layer;
        private bool _completed;

        public UIBlockSlotTransfer(RectTransform root, UIAnimatedElement[] from, UIAnimatedElement[] to)
        {
            if (from == null || to == null || from.Length != 4 || to.Length != 4)
                throw new InvalidOperationException("Assign all four source and destination block frames.");
            for (int i = 0; i < 4; i++)
                if (!from[i] || !to[i]) throw new InvalidOperationException("A block transfer frame is missing.");

            _layer = new GameObject("BlockSlotTransfer", typeof(RectTransform)).GetComponent<RectTransform>();
            _layer.SetParent(root, false);
            _layer.anchorMin = Vector2.zero; _layer.anchorMax = Vector2.one;
            _layer.offsetMin = _layer.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            // Capture every sibling index before removing any frame from its original board.
            for (int i = 0; i < 4; i++)
            {
                var rect = (RectTransform)from[i].transform;
                var slot = new Slot { Source = from[i], Target = to[i], Rect = rect, Parent = rect.parent,
                    Sibling = rect.GetSiblingIndex(), AnchorMin = rect.anchorMin, AnchorMax = rect.anchorMax,
                    Pivot = rect.pivot, Size = rect.sizeDelta, Position = rect.anchoredPosition3D,
                    Scale = rect.localScale, Rotation = rect.localRotation, TargetActive = to[i].gameObject.activeSelf };
                Measure(rect, out slot.FromCenter, out slot.FromSize);
                Measure((RectTransform)to[i].transform, out slot.ToCenter, out slot.ToSize);
                _slots[i] = slot;
            }
            foreach (var slot in _slots)
            {
                slot.Target.PrepareHidden();
                slot.Target.gameObject.SetActive(false);
                slot.Source.SnapVisible();
                slot.Rect.SetParent(_layer, false);
                slot.Rect.anchorMin = slot.Rect.anchorMax = new Vector2(.5f, .5f);
                slot.Rect.pivot = new Vector2(.5f, .5f);
                slot.Rect.localRotation = Quaternion.identity;
                slot.Rect.localScale = Vector3.one;
                slot.Rect.anchoredPosition3D = slot.FromCenter;
                slot.Rect.sizeDelta = slot.FromSize;
            }
        }

        private void Measure(RectTransform rect, out Vector2 center, out Vector2 size)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            Vector2 bottomLeft = _layer.InverseTransformPoint(corners[0]);
            Vector2 topRight = _layer.InverseTransformPoint(corners[2]);
            center = (bottomLeft + topRight) * .5f;
            size = topRight - bottomLeft;
        }

        public async UniTask PlayAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var sequence = DOTween.Sequence().SetUpdate(true);
            foreach (var slot in _slots)
            {
                sequence.Join(slot.Rect.DOAnchorPos(slot.ToCenter, UIMotion.PanelSeconds)
                    .SetEase(Ease.InOutSine));
                sequence.Join(slot.Rect.DOSizeDelta(slot.ToSize, UIMotion.PanelSeconds)
                    .SetEase(Ease.InOutSine));
            }
            await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);
        }

        public void Complete() => _completed = true;

        public void Dispose()
        {
            if (!_layer) return;
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                if (slot.Source && slot.Parent)
                {
                    slot.Rect.SetParent(slot.Parent, false);
                    slot.Rect.SetSiblingIndex(slot.Sibling);
                    slot.Rect.anchorMin = slot.AnchorMin; slot.Rect.anchorMax = slot.AnchorMax;
                    slot.Rect.pivot = slot.Pivot; slot.Rect.sizeDelta = slot.Size;
                    slot.Rect.anchoredPosition3D = slot.Position;
                    slot.Rect.localRotation = slot.Rotation; slot.Rect.localScale = slot.Scale;
                    if (slot.Source.gameObject.activeInHierarchy) slot.Source.SnapVisible();
                    else slot.Source.PrepareHidden();
                }
                if (!slot.Target) continue;
                if (_completed) slot.Target.SnapVisible();
                else
                {
                    slot.Target.PrepareHidden();
                    slot.Target.gameObject.SetActive(slot.TargetActive);
                }
            }
            UnityEngine.Object.Destroy(_layer.gameObject);
            _layer = null;
        }
    }
}
