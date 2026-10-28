using System;
using System.Threading;
using UnityEngine;

namespace Exhibition.Blocks
{
    public static class BlockColorizer
    {
        // Mask R = primary coverage, G = secondary coverage, black = unchanged.
        // Multiply the source (white fill + black lines) so antialiased lines/shading survive.
        // Pure array work: safe on a worker thread. Source and mask are never modified.
        public static Color32[] Colorize(Color32[] source, Color32[] mask, Color32 primary,
            Color32 secondary, CancellationToken token = default)
        {
            if (source == null || mask == null || source.Length == 0 || source.Length != mask.Length)
                throw new ArgumentException("Base image and color mask must have the same non-empty dimensions.");
            token.ThrowIfCancellationRequested();
            var output = new Color32[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();
                var pixel = source[i];
                var region = mask[i];
                int coverage = region.r + region.g;
                // Allow one byte of rounding at an antialiased red/green boundary.
                if (coverage > 256)
                    throw new ArgumentException("Primary and secondary mask coverage must not overlap.");
                float denominator = 255f * Math.Max(255, coverage);
                float first = region.r * region.a / denominator;
                float second = region.g * region.a / denominator;
                float unchanged = 1f - first - second;
                output[i] = new Color32(
                    Channel(pixel.r, unchanged, first, second, primary.r, secondary.r),
                    Channel(pixel.g, unchanged, first, second, primary.g, secondary.g),
                    Channel(pixel.b, unchanged, first, second, primary.b, secondary.b), pixel.a);
            }
            token.ThrowIfCancellationRequested();
            return output;
        }

        private static byte Channel(byte value, float unchanged, float first, float second, byte primary, byte secondary)
            => (byte)Math.Min(255, Math.Max(0, (int)(value * (unchanged + first * primary / 255f + second * secondary / 255f) + .5f)));
    }
}
