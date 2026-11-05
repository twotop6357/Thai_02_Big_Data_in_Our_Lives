using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Exhibition.UI
{
    /// <summary>Moves the group frame above both panels and shrinks it onto its selection slot, then hands off at identical corners.</summary>
    internal sealed class UIBlockSlotTransfer : IDisposable
    {
        private readonly UIAnimatedElement _source, _target;
        private readonly RectTransform _rect;
        private readonly Transform _parent;
        private readonly int _sibling;
        private readonly Vector2 _anchorMin, _anchorMax, _pivot, _size;
        private readonly Vector3 _position, _scale;
        private readonly Quaternion _rotation;
        private readonly Vector2 _toCenter, _toSize;
        private readonly bool _targetActive;
        private RectTransform _layer;
        private bool _completed;

        public UIBlockSlotTransfer(RectTransform root, UIAnimatedElement from, UIAnimatedElement to)
        {
            if (!from || !to) throw new InvalidOperationException("Assign the source and destination block frames.");
            _source = from; _target = to;
            _layer = new GameObject("BlockSlotTransfer", typeof(RectTransform)).GetComponent<RectTransform>();
            _layer.SetParent(root, false);
            _layer.anchorMin = Vector2.zero; _layer.anchorMax = Vector2.one;
            _layer.offsetMin = _layer.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();

            _rect = (RectTransform)from.transform;
            _parent = _rect.parent; _sibling = _rect.GetSiblingIndex();
            _anchorMin = _rect.anchorMin; _anchorMax = _rect.anchorMax; _pivot = _rect.pivot; _size = _rect.sizeDelta;
            _position = _rect.anchoredPosition3D; _scale = _rect.localScale; _rotation = _rect.localRotation;
            _targetActive = to.gameObject.activeSelf;
            Measure(_rect, out var fromCenter, out var fromSize);
            Measure((RectTransform)to.transform, out _toCenter, out _toSize);

            to.PrepareHidden();
            to.gameObject.SetActive(false);
            from.SnapVisible();
            _rect.SetParent(_layer, false);
            _rect.anchorMin = _rect.anchorMax = new Vector2(.5f, .5f);
            _rect.pivot = new Vector2(.5f, .5f);
            _rect.localRotation = Quaternion.identity;
            _rect.localScale = Vector3.one;
            _rect.anchoredPosition3D = fromCenter;
            _rect.sizeDelta = fromSize;
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
            sequence.Join(_rect.DOAnchorPos(_toCenter, UIMotion.PanelSeconds).SetEase(Ease.InOutSine));
            sequence.Join(_rect.DOSizeDelta(_toSize, UIMotion.PanelSeconds).SetEase(Ease.InOutSine));
            await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);
        }

        public void Complete() => _completed = true;

        public void Dispose()
        {
            if (!_layer) return;
            if (_source && _parent)
            {
                _rect.SetParent(_parent, false);
                _rect.SetSiblingIndex(_sibling);
                _rect.anchorMin = _anchorMin; _rect.anchorMax = _anchorMax;
                _rect.pivot = _pivot; _rect.sizeDelta = _size;
                _rect.anchoredPosition3D = _position;
                _rect.localRotation = _rotation; _rect.localScale = _scale;
                if (_source.gameObject.activeInHierarchy) _source.SnapVisible();
                else _source.PrepareHidden();
            }
            if (_target)
            {
                if (_completed) _target.SnapVisible();
                else
                {
                    _target.PrepareHidden();
                    _target.gameObject.SetActive(_targetActive);
                }
            }
            UnityEngine.Object.Destroy(_layer.gameObject);
            _layer = null;
        }
    }
}
