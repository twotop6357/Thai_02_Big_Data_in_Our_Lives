using System;
using System.Threading;
using UnityEngine;
namespace UnityEngine {
    public struct Color32 {
        public byte r,g,b,a;
        public Color32(byte red,byte green,byte blue,byte alpha) {r=red;g=green;b=blue;a=alpha;}
    }
}
namespace Exhibition.Blocks
{
    /// <summary>Centers visible RGBA artwork in a square canvas. Pure array work for a worker thread.</summary>
    public static class GeneratedImageFraming
    {
        public const int CanvasSize = 480;
        public const int Margin = 24;
        public const byte VisibleAlpha = 8;

        public static Color32[] Center(Color32[] source, int width, int height, CancellationToken token = default)
        {
            if (width < 1 || height < 1 || width > 2048 || height > 2048 ||
                source == null || source.Length != width * height)
                throw new ArgumentException("Invalid generated image pixel dimensions.");
            token.ThrowIfCancellationRequested();
            source = MainProduct(source, width, height, token);
            int left = width, bottom = height, right = -1, top = -1;
            for (int y = 0; y < height; y++)
            {
                token.ThrowIfCancellationRequested();
                for (int x = 0; x < width; x++)
                {
                    if (source[y * width + x].a < VisibleAlpha) continue;
                    left = Math.Min(left, x); right = Math.Max(right, x);
                    bottom = Math.Min(bottom, y); top = Math.Max(top, y);
                }
            }
            if (right < left) throw new InvalidOperationException("Generated image contains no visible product.");
            int boundsWidth = right - left + 1, boundsHeight = top - bottom + 1;
            // Preserve detail: translate smaller images directly; only shrink when a margin is needed.
            double scale = Math.Min(1.0, (CanvasSize - 2.0 * Margin) / Math.Max(boundsWidth, boundsHeight));
            int targetWidth = Math.Max(1, (int)Math.Round(boundsWidth * scale));
            int targetHeight = Math.Max(1, (int)Math.Round(boundsHeight * scale));
            int offsetX = (CanvasSize - targetWidth) / 2, offsetY = (CanvasSize - targetHeight) / 2;
            var output = new Color32[CanvasSize * CanvasSize];
            for (int y = 0; y < targetHeight; y++)
            {
                token.ThrowIfCancellationRequested();
                double sy = bottom + (y + .5) * boundsHeight / targetHeight - .5;
                int y0 = Math.Max(bottom, (int)Math.Floor(sy)), y1 = Math.Min(top, y0 + 1);
                double fy = Math.Max(0, sy - y0);
                for (int x = 0; x < targetWidth; x++)
                {
                    double sx = left + (x + .5) * boundsWidth / targetWidth - .5;
                    int x0 = Math.Max(left, (int)Math.Floor(sx)), x1 = Math.Min(right, x0 + 1);
                    double fx = Math.Max(0, sx - x0);
                    output[(offsetY + y) * CanvasSize + offsetX + x] = Blend(
                        source[y0 * width + x0], source[y0 * width + x1],
                        source[y1 * width + x0], source[y1 * width + x1], fx, fy);
                }
            }
            token.ThrowIfCancellationRequested();
            return output;
        }

        private static Color32[] MainProduct(Color32[] source, int width, int height, CancellationToken token)
        {
            // Ignore separate decorations/extra products. Eight-neighbor connectivity retains thin diagonal parts.
            var labels = new int[source.Length];
            var queue = new int[source.Length];
            int label = 0, largest = 0, largestCount = 0;
            for (int start = 0; start < source.Length; start++)
            {
                if (start % width == 0) token.ThrowIfCancellationRequested();
                if (source[start].a < VisibleAlpha || labels[start] != 0) continue;
                label++;
                int count = 1;
                queue[0] = start; labels[start] = label;
                for (int head = 0; head < count; head++)
                {
                    if ((head & 4095) == 0) token.ThrowIfCancellationRequested();
                    int point = queue[head], x = point % width, y = point / width;
                    for (int ny = Math.Max(0, y - 1); ny <= Math.Min(height - 1, y + 1); ny++)
                    for (int nx = Math.Max(0, x - 1); nx <= Math.Min(width - 1, x + 1); nx++)
                    {
                        int next = ny * width + nx;
                        if (labels[next] != 0 || source[next].a < VisibleAlpha) continue;
                        labels[next] = label; queue[count++] = next;
                    }
                }
                if (count > largestCount) { largest = label; largestCount = count; }
            }
            if (largestCount == 0) throw new InvalidOperationException("Generated image contains no visible product.");
            var selected = new Color32[source.Length];
            for (int i = 0; i < selected.Length; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();
                if (labels[i] == largest) selected[i] = source[i];
            }
            return selected;
        }

        private static Color32 Blend(Color32 a, Color32 b, Color32 c, Color32 d, double x, double y)
        {
            // Interpolate premultiplied channels so transparent RGB cannot create dark/colored fringes.
            double wa = (1 - x) * (1 - y) * a.a, wb = x * (1 - y) * b.a;
            double wc = (1 - x) * y * c.a, wd = x * y * d.a;
            double alpha = wa + wb + wc + wd;
            if (alpha < .5) return default;
            return new Color32((byte)Math.Round((a.r * wa + b.r * wb + c.r * wc + d.r * wd) / alpha),
                (byte)Math.Round((a.g * wa + b.g * wb + c.g * wc + d.g * wd) / alpha),
                (byte)Math.Round((a.b * wa + b.b * wb + c.b * wc + d.b * wd) / alpha), (byte)Math.Round(alpha));
        }
    }
}

public static class AgeGalleryFrameHost {
    public static void Run(string input,string output) {
        var bytes=System.IO.File.ReadAllBytes(input);
        if(bytes.Length!=480*480*4) throw new Exception("Unexpected RGBA byte length.");
        var pixels=new Color32[480*480];
        for(int i=0;i<pixels.Length;i++) pixels[i]=new Color32(bytes[i*4],bytes[i*4+1],bytes[i*4+2],bytes[i*4+3]);
        var framed=Exhibition.Blocks.GeneratedImageFraming.Center(pixels,480,480);
        for(int i=0;i<framed.Length;i++) {bytes[i*4]=framed[i].r;bytes[i*4+1]=framed[i].g;bytes[i*4+2]=framed[i].b;bytes[i*4+3]=framed[i].a;}
        System.IO.File.WriteAllBytes(output,bytes);
    }
}
