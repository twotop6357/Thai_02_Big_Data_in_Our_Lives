using System;
using System.Threading;
using Exhibition.Blocks;
using NUnit.Framework;
using UnityEngine;

public sealed class GeneratedImageFramingTests
{
    private static readonly Color32 Ink = new Color32(95, 176, 227, 255);

    private static Color32[] Rectangle(int x, int y, int width, int height)
    {
        var pixels = new Color32[480 * 480];
        for (int row = y; row < y + height; row++)
        for (int col = x; col < x + width; col++) pixels[row * 480 + col] = Ink;
        return pixels;
    }

    private static RectInt Bounds(Color32[] pixels)
    {
        int left = 480, right = -1, bottom = 480, top = -1;
        for (int y = 0; y < 480; y++)
        for (int x = 0; x < 480; x++)
            if (pixels[y * 480 + x].a >= GeneratedImageFraming.VisibleAlpha)
            {
                left = Math.Min(left, x); right = Math.Max(right, x);
                bottom = Math.Min(bottom, y); top = Math.Max(top, y);
            }
        return new RectInt(left, bottom, right - left + 1, top - bottom + 1);
    }

    [TestCase(3, 11, 200, 100)]
    [TestCase(220, 350, 201, 71)]
    [TestCase(390, 25, 60, 350)]
    public void OffCenterProductsAreCenteredWithoutUpscalingOrChangingColors(int x, int y, int w, int h)
    {
        var source = Rectangle(x, y, w, h);
        var output = GeneratedImageFraming.Center(source, 480, 480);
        var bounds = Bounds(output);
        Assert.AreEqual(w, bounds.width); Assert.AreEqual(h, bounds.height);
        Assert.That(bounds.center.x, Is.EqualTo(240).Within(.5));
        Assert.That(bounds.center.y, Is.EqualTo(240).Within(.5));
        Assert.AreEqual(Ink, output[bounds.yMin * 480 + bounds.xMin]);
        Assert.AreEqual(Ink, source[y * 480 + x]);
        Assert.AreEqual(0, output[0].a);
    }

    [Test] public void LargeArtworkShrinksProportionallyWithTransparentMargin()
    {
        var output = GeneratedImageFraming.Center(Rectangle(0, 25, 480, 240), 480, 480);
        Assert.AreEqual(new RectInt(24, 132, 432, 216), Bounds(output));
        Assert.AreEqual(480 * 480, output.Length);
    }

    [Test] public void NearTransparentBorderNoiseDoesNotDisplaceTheProduct()
    {
        var source = Rectangle(10, 40, 160, 120);
        source[479 * 480 + 479] = new Color32(255, 255, 255, 1);
        var output = GeneratedImageFraming.Center(source, 480, 480);
        Assert.AreEqual(new RectInt(160, 180, 160, 120), Bounds(output));
        Assert.AreEqual(0, output[479 * 480 + 479].a);
    }

    [Test] public void ShrinkingCannotBlendInvisibleBackgroundRgbIntoEdges()
    {
        var source = Rectangle(0, 0, 480, 2);
        for (int x = 1; x < 479; x++) source[480 + x] = new Color32(255, 0, 0, 0);
        var output = GeneratedImageFraming.Center(source, 480, 480);
        foreach (var pixel in output)
            if (pixel.a > 0) { Assert.AreEqual(Ink.r, pixel.r); Assert.AreEqual(Ink.g, pixel.g); Assert.AreEqual(Ink.b, pixel.b); }
    }

    [Test] public void EmptyOrInvalidImagesFailInsteadOfDisplayingBlankResults()
    {
        Assert.Throws<InvalidOperationException>(() => GeneratedImageFraming.Center(new Color32[480 * 480], 480, 480));
        Assert.Throws<ArgumentException>(() => GeneratedImageFraming.Center(null, 480, 480));
        Assert.Throws<ArgumentException>(() => GeneratedImageFraming.Center(new Color32[2], 480, 480));
    }

    [Test] public void StrictDownloadRejectsClippedArtworkBeforeCenteringCanHideIt()
    {
        foreach (var source in new[] { Rectangle(0, 40, 80, 80), Rectangle(400, 40, 80, 80),
                     Rectangle(40, 0, 80, 80), Rectangle(40, 400, 80, 80) })
            Assert.Throws<GeneratedImageQualityException>(() => GeneratedImageFraming.Center(source, 480, 480, requireComplete: true));
        Assert.DoesNotThrow(() => GeneratedImageFraming.Center(Rectangle(20, 40, 80, 80), 480, 480, requireComplete: true));
    }

    [Test] public void StrictDownloadRejectsSubstantialDetachedPartsButKeepsMinorSpecks()
    {
        var pixels = Rectangle(60, 100, 100, 100);
        for (int y = 100; y < 160; y++)
        for (int x = 300; x < 360; x++) pixels[y * 480 + x] = Ink;
        Assert.Throws<GeneratedImageQualityException>(() => GeneratedImageFraming.Center(pixels, 480, 480, requireComplete: true));
        Assert.DoesNotThrow(() => GeneratedImageFraming.Center(Rectangle(60, 100, 100, 100), 480, 480, requireComplete: true));
        pixels = Rectangle(60, 100, 100, 100);
        pixels[460 * 480 + 460] = Ink;
        Assert.DoesNotThrow(() => GeneratedImageFraming.Center(pixels, 480, 480, requireComplete: true));
    }

    [Test] public void StrictDownloadRejectsFaintOutputWithoutMutatingTheSource()
    {
        var pixels = Rectangle(60, 100, 100, 100);
        for (int i = 0; i < pixels.Length; i++) if (pixels[i].a > 0) pixels[i].a = 30;
        Assert.Throws<GeneratedImageQualityException>(() => GeneratedImageFraming.Center(pixels, 480, 480, requireComplete: true));
        Assert.AreEqual(30, pixels[100 * 480 + 60].a);
    }

    [Test] public void CancelledFramingDoesNotReturnAnImage()
    {
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => GeneratedImageFraming.Center(Rectangle(0, 0, 50, 50), 480, 480, cancel.Token));
    }

    [Test] public void RepeatedFramingDoesNotChangeAlreadyCenteredArtwork()
    {
        var first = GeneratedImageFraming.Center(Rectangle(0, 0, 480, 240), 480, 480);
        CollectionAssert.AreEqual(first, GeneratedImageFraming.Center(first, 480, 480));
    }

    [Test] public void ExtraObjectsAreRemovedWithoutChangingSourceOrMainProductColors()
    {
        var source = Rectangle(30, 70, 100, 80);
        for (int y = 300; y < 320; y++)
        for (int x = 350; x < 370; x++) source[y * 480 + x] = new Color32(236, 104, 104, 255);
        var output = GeneratedImageFraming.Center(source, 480, 480);
        Assert.AreEqual(new RectInt(190, 200, 100, 80), Bounds(output));
        Assert.AreEqual(255, source[300 * 480 + 350].a);
        foreach (var pixel in output) if (pixel.a != 0) Assert.AreEqual(Ink, pixel);
    }

    [Test] public void ThinDiagonalConnectionsRetainAllPartsOfOneProduct()
    {
        var source = Rectangle(100, 100, 12, 12);
        for (int i = 0; i < 20; i++) source[(112 + i) * 480 + 112 + i] = Ink;
        for (int y = 132; y < 144; y++)
        for (int x = 132; x < 144; x++) source[y * 480 + x] = Ink;
        var output = GeneratedImageFraming.Center(source, 480, 480);
        Assert.AreEqual(new RectInt(218, 218, 44, 44), Bounds(output));
    }
}
