using System;
using System.IO;
using Exhibition.Blocks;
using NUnit.Framework;
using UnityEngine;

public sealed class BlockSelectionTests
{
    private BlockCatalog _catalog;
    [SetUp] public void Load()
    {
        _catalog = JsonUtility.FromJson<BlockCatalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,
            "BlockGeneration/BlockCatalog.json")));
        _catalog.Validate();
    }

    [Test] public void CatalogMatchesExhibitionBlocks()
    {
        Assert.AreEqual(4, _catalog.groups.Length);
        Assert.AreEqual(10, _catalog.products.Length);
        Assert.AreEqual(6, _catalog.colors.Length);
        Assert.AreEqual(3, _catalog.equipmentVariants.Length);
        Assert.Throws<ArgumentException>(() => _catalog.Find(BlockSlot.Product, "equipment"));
        Assert.Throws<ArgumentException>(() => _catalog.Find(BlockSlot.Product, "phone_cases"));
        CollectionAssert.AreEquivalent(new[] { "black", "white", "blue", "red", "green", "yellow" },
            Array.ConvertAll(_catalog.colors, c => c.id));
    }

    [Test] public void JournalSettingsHaveNoGenerationServerDependency()
    {
        var settings = JsonUtility.FromJson<BlockSettings>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,
            "BlockGeneration/BlockSettings.json")));
        Assert.DoesNotThrow(settings.Validate);
        settings.maxJournalFiles = 1;
        Assert.Throws<ArgumentException>(settings.Validate);
        settings.maxJournalFiles = 30;
        settings.maxJournalMegabytes = 0;
        Assert.Throws<ArgumentException>(settings.Validate);
    }

    private BlockSelection Complete(string product = "bags")
    {
        var selection = new BlockSelection();
        selection.Set(_catalog, BlockSlot.Group, "child");
        selection.Set(_catalog, BlockSlot.Product, product);
        selection.Set(_catalog, BlockSlot.Color1, "red");
        selection.Set(_catalog, BlockSlot.Color2, "blue");
        return selection;
    }

    [Test] public void RemovingEitherColorInvalidatesCompleteSelection()
    {
        var selection = Complete();
        Assert.IsTrue(selection.Complete);
        selection.Remove(BlockSlot.Color1);
        Assert.IsFalse(selection.Complete);
        Assert.AreEqual("blue", selection[BlockSlot.Color2]);
        Assert.Throws<InvalidOperationException>(() => selection.CreatePrompt(_catalog, 0));
    }

    [Test] public void UnsupportedBlockIsRejectedWithoutChangingRevision()
    {
        var selection = Complete();
        int before = selection.Revision;
        Assert.Throws<ArgumentException>(() => selection.Set(_catalog, BlockSlot.Product, "invalid"));
        Assert.AreEqual(before, selection.Revision);
        Assert.AreEqual("bags", selection[BlockSlot.Product]);
    }

    [Test] public void RepeatedRecognitionAndRemovalAreIdempotent()
    {
        var selection = Complete();
        int before = selection.Revision;
        selection.Set(_catalog, BlockSlot.Group, "child");
        Assert.AreEqual(before, selection.Revision);
        selection.Remove(BlockSlot.Group);
        selection.Remove(BlockSlot.Group);
        Assert.AreEqual(before + 1, selection.Revision);
    }

    [Test] public void ImageLessProductsCannotBeSelected()
    {
        Assert.Throws<ArgumentException>(() => Complete("equipment"));
        Assert.Throws<ArgumentException>(() => Complete("phone_cases"));
    }

    [Test] public void AllCombinationsProduceSeparateColorPartsAndClearSession()
    {
        var selection = new BlockSelection();
        foreach (var group in _catalog.groups)
        foreach (var product in _catalog.products)
        foreach (var first in _catalog.colors)
        foreach (var second in _catalog.colors)
        {
            selection.Set(_catalog, BlockSlot.Group, group.id);
            selection.Set(_catalog, BlockSlot.Product, product.id);
            selection.Set(_catalog, BlockSlot.Color1, first.id);
            selection.Set(_catalog, BlockSlot.Color2, second.id);
            var prompt = selection.CreatePrompt(_catalog, 0);
            var design = product.ForGroup(group.id);
            Assert.IsNotNull(design, product.id + "/" + group.id);
            StringAssert.StartsWith(design.prompt, prompt.positive);
            StringAssert.Contains(first.prompt, prompt.primaryFragment);
            StringAssert.Contains(design.primaryPart, prompt.primaryFragment);
            StringAssert.Contains(first.hex, prompt.primaryFragment);
            StringAssert.Contains(second.prompt, prompt.secondaryFragment);
            StringAssert.Contains(design.secondaryPart, prompt.secondaryFragment);
            StringAssert.Contains(second.hex, prompt.secondaryFragment);
            StringAssert.Contains(first.hex, prompt.primaryReference);
            StringAssert.Contains(second.hex, prompt.secondaryReference);
            Assert.AreEqual(first.hex, prompt.primaryHex);
            Assert.AreEqual(second.hex, prompt.secondaryHex);
            StringAssert.Contains(_catalog.negativePrompt, prompt.negative);
            if (!string.IsNullOrWhiteSpace(product.negativePrompt)) StringAssert.Contains(product.negativePrompt, prompt.negative);
        }
        selection.Clear();
        Assert.IsFalse(selection.Complete);
        foreach (BlockSlot slot in Enum.GetValues(typeof(BlockSlot))) Assert.IsNull(selection[slot]);
    }

    [Test] public void InvalidResourceColorIsRejectedBeforeGeneration()
    {
        _catalog.colors[0].hex = "#GG0000";
        Assert.Throws<ArgumentException>(_catalog.Validate);
        _catalog.colors[0].hex = null;
        Assert.Throws<ArgumentException>(_catalog.Validate);
    }

    [Test] public void EveryProductHasFourDistinctAudienceSubjects()
    {
        foreach (var product in _catalog.products)
        {
            var subjects = new System.Collections.Generic.HashSet<string>();
            foreach (var group in _catalog.groups)
                Assert.IsTrue(subjects.Add(product.ForGroup(group.id).prompt), product.id);
            Assert.AreEqual(4, subjects.Count);
        }
        var hats = _catalog.Find(BlockSlot.Product, "hats");
        StringAssert.Contains("kindergarten", hats.ForGroup("child").prompt);
        StringAssert.Contains("beanie", hats.ForGroup("student").prompt);
        StringAssert.Contains("baseball cap", hats.ForGroup("working_adult").prompt);
        StringAssert.Contains("fedora", hats.ForGroup("senior").prompt);
        StringAssert.DoesNotContain("brim", hats.ForGroup("student").secondaryPart);
    }

    [Test] public void IncompleteDuplicateOrUnknownAgeDesignIsRejected()
    {
        var product = _catalog.products[0];
        var original = product.ageVariants;
        product.ageVariants = new[] { original[0] };
        Assert.Throws<ArgumentException>(_catalog.Validate);
        Assert.Throws<ArgumentException>(() => product.ForGroup("senior"));
        product.ageVariants = new[] { original[0], original[0], original[2], original[3] };
        Assert.Throws<ArgumentException>(_catalog.Validate);
        product.ageVariants = original;
        original[0].groupId = "unknown";
        Assert.Throws<ArgumentException>(_catalog.Validate);
    }

    [Test] public void ShoePoseReferenceCannotUseTraversalOrStrongFullLengthControl()
    {
        var shoe = _catalog.Find(BlockSlot.Product, "shoes");
        // The fast runtime no longer uses a guide; the optional reference contract still rejects unsafe values.
        shoe.poseGuide = "guide_shoes_pose_3q_v1.png";
        shoe.poseStrength = .45f; shoe.poseEndPercent = .5f;
        shoe.poseLowThreshold = .2f; shoe.poseHighThreshold = .5f;
        var prompt = Complete("shoes").CreatePrompt(_catalog, 0);
        Assert.AreEqual(shoe.poseGuide, prompt.poseGuide);
        Assert.AreEqual(shoe.poseLowThreshold, prompt.poseLowThreshold);
        Assert.AreEqual(shoe.poseHighThreshold, prompt.poseHighThreshold);
        float high = shoe.poseHighThreshold;
        shoe.poseHighThreshold = float.NaN;
        Assert.Throws<ArgumentException>(_catalog.Validate);
        shoe.poseHighThreshold = shoe.poseLowThreshold;
        Assert.Throws<ArgumentException>(_catalog.Validate);
        shoe.poseHighThreshold = high;
        shoe.poseGuide = "../shoe.png";
        Assert.Throws<ArgumentException>(_catalog.Validate);
        shoe.poseGuide = "guide_shoes_pose_3q_v1.png";
        shoe.poseStrength = 1.4f;
        Assert.Throws<ArgumentException>(_catalog.Validate);
        shoe.poseStrength = .55f; shoe.poseEndPercent = 1f;
        Assert.Throws<ArgumentException>(_catalog.Validate);
        shoe.poseEndPercent = float.NaN;
        Assert.Throws<ArgumentException>(_catalog.Validate);
    }
}
