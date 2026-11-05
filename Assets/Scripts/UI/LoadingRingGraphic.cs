using UnityEngine;
using UnityEngine.UI;

namespace Exhibition.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LoadingRingGraphic : MaskableGraphic
    {
        public float InnerRadius = 41, OuterRadius = 50, ArcDegrees = 360;
        public bool FadeTail;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); const int segments = 96;
            Vector2 center = rectTransform.rect.center;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments, angle = (90 - ArcDegrees * t) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Color shade = color; if (FadeTail) shade.a *= Mathf.SmoothStep(0, 1, t);
                mesh.AddVert(center + direction * InnerRadius, shade, Vector2.zero);
                mesh.AddVert(center + direction * OuterRadius, shade, Vector2.zero);
                if (i == 0) continue;
                int v = i * 2; mesh.AddTriangle(v - 2, v - 1, v + 1); mesh.AddTriangle(v - 2, v + 1, v);
            }
            if (!FadeTail) return;
            // Circular end cap keeps the leading edge as soft as the surrounding UI frames.
            float endAngle = (90 - ArcDegrees) * Mathf.Deg2Rad;
            var cap = center + new Vector2(Mathf.Cos(endAngle), Mathf.Sin(endAngle)) * ((InnerRadius + OuterRadius) * .5f);
            int start = mesh.currentVertCount; Color head = color;
            mesh.AddVert(cap, head, Vector2.zero);
            for (int i = 0; i <= 20; i++)
            {
                float angle = i * Mathf.PI * 2 / 20;
                mesh.AddVert(cap + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ((OuterRadius - InnerRadius) * .5f), head, Vector2.zero);
                if (i > 0) mesh.AddTriangle(start, start + i, start + i + 1);
            }
        }
    }
}
