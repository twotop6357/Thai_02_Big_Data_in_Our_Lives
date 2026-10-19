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
        Assert.AreEqual(12, _catalog.products.Length);
        Assert.AreEqual(6, _catalog.colors.Length);
        Assert.AreEqual(3, _catalog.equipmentVariants.Length);
        Assert.AreEqual("Equipment", _catalog.Find(BlockSlot.Product, "equipment").label);
        CollectionAssert.AreEquivalent(new[] { "black", "white", "blue", "red", "green", "yellow" },
            Array.ConvertAll(_catalog.colors, c => c.id));
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

    [Test] public void EquipmentResolvesToExactlyOneHiddenVariant()
    {
        var selection = Complete("equipment");
        for (int i = 0; i < 3; i++)
        {
            var prompt = selection.CreatePrompt(_catalog, i);
            Assert.AreEqual(_catalog.equipmentVariants[i].id, prompt.resolvedProduct);
            Assert.AreEqual(_catalog.equipmentVariants[i].prompt, prompt.productFragment);
            StringAssert.DoesNotContain("(", _catalog.Find(BlockSlot.Product, "equipment").label);
        }
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
            StringAssert.Contains(first.prompt + " " + product.primaryPart, prompt.primaryFragment);
            StringAssert.Contains(second.prompt + " " + product.secondaryPart, prompt.secondaryFragment);
        }
        selection.Clear();
        Assert.IsFalse(selection.Complete);
        foreach (BlockSlot slot in Enum.GetValues(typeof(BlockSlot))) Assert.IsNull(selection[slot]);
    }
}
