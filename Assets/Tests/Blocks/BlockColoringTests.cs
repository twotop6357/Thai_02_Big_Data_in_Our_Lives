using System;
using System.IO;
using System.Threading;
using Exhibition.Blocks;
using NUnit.Framework;
using UnityEngine;

public sealed class BlockColoringTests
{
    private static readonly Color32 Red = new Color32(255, 0, 0, 255);
    private static readonly Color32 Blue = new Color32(0, 0, 255, 255);
    private static readonly Color32 White = new Color32(255, 255, 255, 255);
    private static readonly Color32 Black = new Color32(0, 0, 0, 255);
    private static readonly string Root = Path.Combine(Application.streamingAssetsPath, "BlockGeneration");

    [Test] public void TwoRegionsPreserveBlackLinesAlphaAndUnmappedPixels()
    {
        var original = new[] { White, White, Black, new Color32(128, 128, 128, 77), White };
        var mask = new[] { Red, new Color32(0, 255, 0, 255), Red, Red, Black };
        var output = BlockColorizer.Colorize(original, mask, Red, Blue);
        Assert.AreEqual(Red, output[0]);
        Assert.AreEqual(Blue, output[1]);
        Assert.AreEqual(Black, output[2]);
        Assert.AreEqual(new Color32(128, 0, 0, 77), output[3]);
        Assert.AreEqual(White, output[4]);
        Assert.AreEqual(White, original[0], "Original artwork must remain reusable.");
    }

    [Test] public void WhiteAndBlackSelectionsAndIdenticalColorsAreSupported()
    {
        var mask = new[] { Red, new Color32(0, 255, 0, 255) };
        var source = new[] { White, White };
        CollectionAssert.AreEqual(new[] { White, Black }, BlockColorizer.Colorize(source, mask, White, Black));
        CollectionAssert.AreEqual(new[] { Red, Red }, BlockColorizer.Colorize(source, mask, Red, Red));
    }

    [Test] public void MaskCoverageBlendsWithoutChangingTransparency()
    {
        var source = new[] { new Color32(255, 255, 255, 0), White, White };
        var mask = new[] { Red, new Color32(128, 127, 0, 255), new Color32(255, 0, 0, 0) };
        var output = BlockColorizer.Colorize(source, mask, Red, Blue);
        Assert.AreEqual(0, output[0].a);
        Assert.AreEqual(new Color32(128, 0, 127, 255), output[1]);
        Assert.AreEqual(White, output[2]);
        Assert.AreEqual(new Color32(128, 0, 128, 255),
            BlockColorizer.Colorize(new[] { White }, new[] { new Color32(128, 128, 0, 255) }, Red, Blue)[0]);
    }

    [Test] public void InvalidMasksAndCancellationAreRejected()
    {
        Assert.Throws<ArgumentException>(() => BlockColorizer.Colorize(new[] { White }, Array.Empty<Color32>(), Red, Blue));
        Assert.Throws<ArgumentException>(() => BlockColorizer.Colorize(new[] { White }, new[] { White }, Red, Blue));
        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => BlockColorizer.Colorize(new[] { White }, new[] { Red }, Red, Blue, cts.Token));
        }
    }

    private static BlockColoringCatalog Load(out BlockCatalog blocks)
    {
        blocks = JsonUtility.FromJson<BlockCatalog>(File.ReadAllText(Path.Combine(Root, "BlockCatalog.json")));
        return JsonUtility.FromJson<BlockColoringCatalog>(File.ReadAllText(Path.Combine(Root, "BlockColoring.json")));
    }

    [Test] public void EveryAgeProductHasItsOwnTemplateAndPalette()
    {
        var coloring = Load(out var blocks);
        coloring.Validate(blocks);
        var paths = new System.Collections.Generic.HashSet<string>();
        foreach (var group in blocks.groups)
        foreach (var product in blocks.products)
        {
            var template = coloring.Find(group.id, product.id);
            Assert.IsTrue(paths.Add(template.baseImage), "Age variants must have separate artwork.");
        }
        foreach (var color in blocks.colors) Assert.AreEqual(255, coloring.GetColor(color.id).a);
    }

    [Test] public void MissingDuplicateAndUnsafeTemplatesFailValidation()
    {
        var coloring = Load(out var blocks);
        coloring.templates[0].baseImage = "Coloring/../outside.png";
        Assert.Throws<ArgumentException>(() => coloring.Validate(blocks));
        coloring = Load(out blocks);
        coloring.templates[0] = coloring.templates[1];
        Assert.Throws<ArgumentException>(() => coloring.Validate(blocks));
        coloring = Load(out blocks);
        Array.Resize(ref coloring.templates, coloring.templates.Length-1);
        Assert.Throws<ArgumentException>(() => coloring.Validate(blocks));
        coloring = Load(out blocks);
        coloring.colors[0].hex = "#FF000080";
        Assert.Throws<ArgumentException>(() => coloring.Validate(blocks));
    }

    [Test] public void AllBundledArtworkHasMatchingMasksTwoRegionsAndPreservedLines()
    {
        var coloring = Load(out _);
        foreach (var template in coloring.templates)
        {
            var source = new Texture2D(2, 2);
            var mask = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(source.LoadImage(File.ReadAllBytes(Path.Combine(Root, template.baseImage))));
                Assert.IsTrue(mask.LoadImage(File.ReadAllBytes(Path.Combine(Root, template.maskImage))));
                Assert.AreEqual(source.width, mask.width);
                Assert.AreEqual(source.height, mask.height);
                var original = source.GetPixels32();
                var regions = mask.GetPixels32();
                var colored = BlockColorizer.Colorize(original, regions, Red, Blue);
                int first = 0, second = 0, lines = 0, transparent = 0;
                for (int i = 0; i < original.Length; i++)
                {
                    Assert.AreEqual(original[i].a, colored[i].a);
                    if (original[i].a == 0) { transparent++; continue; }
                    if (original[i].r == 0 && original[i].g == 0 && original[i].b == 0)
                    { Assert.AreEqual(original[i], colored[i]); lines++; }
                    if (original[i].Equals(White) && regions[i].Equals(Red)) first++;
                    if (original[i].Equals(White) && regions[i].g == 255 && regions[i].r == 0) second++;
                }
                Assert.Greater(first, 20, template.baseImage);
                Assert.Greater(second, 20, template.baseImage);
                Assert.Greater(lines, 20, template.baseImage);
                Assert.Greater(transparent, 20, template.baseImage);
            }
            finally { UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(mask); }
        }
    }
}
