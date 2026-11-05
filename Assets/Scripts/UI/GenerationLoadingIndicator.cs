using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    /// <summary>Rounded pastel ring; rotates using unscaled time without rebuilding its mesh.</summary>
    public sealed class GenerationLoadingIndicator : MonoBehaviour
    {
        private RectTransform _rotor;
        public bool IsLoading => gameObject.activeSelf;
        public float Rotation => _rotor ? _rotor.localEulerAngles.z : 0;

        public static GenerationLoadingIndicator Create(RectTransform image)
        {
            var root = new GameObject("GenerationLoading", typeof(RectTransform));
            var rect = (RectTransform)root.transform; rect.SetParent(image.parent, false);
            rect.anchorMin = image.anchorMin; rect.anchorMax = image.anchorMax; rect.pivot = image.pivot;
            rect.anchoredPosition3D = image.anchoredPosition3D; rect.sizeDelta = image.sizeDelta;
            rect.localRotation = image.localRotation; rect.localScale = image.localScale;
            var indicator = root.AddComponent<GenerationLoadingIndicator>();
            var arc = AddRing(rect, "RotatingArc"); arc.ArcDegrees = 275; arc.FadeTail = true;
            arc.color = new Color(.94f, .48f, .64f, 1); indicator._rotor = arc.rectTransform;
            root.SetActive(false);
            return indicator;
        }

        private static LoadingRingGraphic AddRing(RectTransform parent, string name)
        {
            var node = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer)); var rect = (RectTransform)node.transform;
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(132, 132);
            var ring = node.AddComponent<LoadingRingGraphic>(); ring.raycastTarget = false;
            return ring;
        }

        public void SetLoading(bool loading)
        {
            if (gameObject.activeSelf == loading) return;
            if (_rotor) _rotor.localRotation = Quaternion.identity;
            gameObject.SetActive(loading);
            if (loading) transform.SetAsLastSibling();
        }
        private void Update() { if (_rotor) _rotor.Rotate(0, 0, -170 * Time.unscaledDeltaTime); }
    }
}
