using System;
using System.IO;
using Exhibition.Blocks;
using NUnit.Framework;
using UnityEngine;

public sealed class BlockArtworkDocumentTests
{
    [TestCase("en")]
    [TestCase("th")]
    public void SuppliedLanguageDocumentsResolveAllTwentyImages(string language)
    {
        string root = Path.Combine(Application.streamingAssetsPath, "BlockArtwork");
        var document = JsonUtility.FromJson<BlockArtworkDocument>(File.ReadAllText(Path.Combine(root, "blocks_" + language + ".json")));
        document.Validate(language); Assert.AreEqual(20, document.blocks.Length);
        foreach (var entry in document.blocks) Assert.IsTrue(File.Exists(Path.Combine(root, entry.image)), entry.Key);
    }

    [TestCase("../image.png")]
    [TestCase("Images/../image.png")]
    [TestCase("/image.png")]
    [TestCase("C:/image.png")]
    [TestCase("https://example.com/image.png")]
    [TestCase("Images\\image.png")]
    [TestCase("Images//image.png")]
    [TestCase("Images/image.exe")]
    [TestCase("")]
    public void ImagePathsCannotEscapeTheLocalArtworkFolder(string path)
    {
        Assert.Throws<ArgumentException>(() => BlockArtworkDocument.ValidateImagePath(path));
    }

    [Test] public void DuplicateIdsAndMismatchedLanguageAreRejected()
    {
        var entry = new BlockArtworkPath { kind = "group", id = "child", image = "Images/en/ChildBlock_Eng.png" };
        var document = new BlockArtworkDocument { schemaVersion = 1, language = "en", blocks = new[] { entry, entry } };
        Assert.Throws<ArgumentException>(() => document.Validate("en"));
        document.blocks = new[] { entry };
        Assert.Throws<ArgumentException>(() => document.Validate("th"));
        entry.kind = "unsupported";
        Assert.Throws<ArgumentException>(() => document.Validate("en"));
    }
}
