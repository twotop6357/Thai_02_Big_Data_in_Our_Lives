using System;
using Exhibition.Blocks;
using NUnit.Framework;

public sealed class RfidReaderSelectionTests
{
    private static RfidBlock Group(string id = "child") => new RfidBlock(BlockKind.Group, id);
    private static RfidBlock Product(string id = "bags") => new RfidBlock(BlockKind.Product, id);
    private static RfidBlock Color(string id = "red") => new RfidBlock(BlockKind.Color, id);

    [Test] public void EveryTypeArrangementRequiresExactlyOneGroupOneProductTwoColors()
    {
        int valid = 0;
        for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++)
        for (int c = 0; c < 4; c++) for (int d = 0; d < 4; d++)
        {
            var state = new RfidReaderSelection(); var kinds = new[] { a, b, c, d };
            int groups = 0, products = 0, colors = 0;
            for (int i = 0; i < 4; i++)
            {
                if (kinds[i] == 0) continue;
                if (kinds[i] == 1) { state.Set(i, Group()); groups++; }
                if (kinds[i] == 2) { state.Set(i, Product()); products++; }
                if (kinds[i] == 3) { state.Set(i, Color()); colors++; }
            }
            bool expected = groups == 1 && products == 1 && colors == 2;
            Assert.AreEqual(expected, state.HasRequiredComposition, string.Join(",", kinds));
            if (expected) valid++;
        }
        Assert.AreEqual(12, valid, "Every physical position accepts any block kind.");
    }

    [Test] public void DuplicateGroupDoesNotPretendThereIsOneSelectedGroup()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Group()); state.Set(1, Group("senior")); state.Set(2, Product()); state.Set(3, Color());
        Assert.AreEqual(4, state.OccupiedCount);
        Assert.IsNull(state.GroupId); Assert.IsFalse(state.HasRequiredComposition);
        state.Set(1, Color("blue"));
        Assert.IsTrue(state.HasRequiredComposition); Assert.AreEqual("child", state.GroupId);
    }

    [Test] public void ColorsUseReaderOrderRatherThanFixedColorSlots()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Color("blue")); state.Set(1, Group()); state.Set(2, Product()); state.Set(3, Color("red"));
        Assert.IsTrue(state.HasRequiredComposition);
        Assert.AreEqual("blue", state.Color1Id); Assert.AreEqual("red", state.Color2Id);
        state.Set(0, null);
        Assert.IsFalse(state.HasRequiredComposition); Assert.AreEqual("red", state.Color1Id); Assert.IsNull(state.Color2Id);
    }

    [Test] public void ReaderRemovalAndRepeatedRecognitionAreIndependentAndIdempotent()
    {
        var state = new RfidReaderSelection(); state.Set(2, Product());
        int revision = state.Revision;
        Assert.IsFalse(state.Set(2, Product())); Assert.AreEqual(revision, state.Revision);
        state.Set(0, Group()); state.Set(3, Color()); state.Set(2, null);
        Assert.IsNull(state[1]); Assert.IsNull(state[2]); Assert.AreEqual("child", state[0].Value.Id);
        Assert.AreEqual("red", state[3].Value.Id);
    }

    [Test] public void MoveReplacesOnlyTheDestinationAndEmptiesOnlyTheSource()
    {
        var state = new RfidReaderSelection(); state.Set(0, Group()); state.Set(1, Product()); state.Set(3, Color());
        state.Move(0, 1);
        Assert.IsNull(state[0]); Assert.AreEqual("child", state[1].Value.Id);
        Assert.AreEqual(0, state.ProductCount); Assert.AreEqual(1, state.GroupCount);
        Assert.AreEqual("red", state[3].Value.Id); Assert.IsFalse(state.HasRequiredComposition);
        Assert.IsFalse(state.Move(1, 1)); Assert.IsFalse(state.Move(0, 2));
    }

    [Test] public void ClearingAndInvalidIndicesNeverLeaveStaleOccupancy()
    {
        var state = new RfidReaderSelection(); state.Set(0, Group()); state.Clear();
        Assert.AreEqual(0, state.OccupiedCount); Assert.IsFalse(state.Clear());
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Set(-1, Group()));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Move(0, 4));
        Assert.Throws<ArgumentException>(() => state.Set(0, default(RfidBlock)));
        Assert.AreEqual(0, state.OccupiedCount);
    }
}
