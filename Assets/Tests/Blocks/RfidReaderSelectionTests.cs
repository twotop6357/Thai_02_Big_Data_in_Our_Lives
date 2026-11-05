using System;
using Exhibition.Blocks;
using NUnit.Framework;

public sealed class RfidReaderSelectionTests
{
    [Test] public void FrozenSnapshotIgnoresRemovalReplacementMoveAndClearWithoutChangingRevision()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Group()); state.Set(1, Product()); state.Set(2, Color()); state.Set(3, Color("blue"));
        int revision = state.Revision;
        using (state.FreezeInput())
        {
            for (int i = 0; i < 4; i++) Assert.IsFalse(state.Set(i, null));
            Assert.IsFalse(state.CanRecognize(1, Product("shoes")));
            Assert.IsFalse(state.Set(1, Product("shoes")));
            Assert.IsFalse(state.Move(0, 1)); Assert.IsFalse(state.Clear());
            Assert.AreEqual(revision, state.Revision); Assert.IsTrue(state.HasRequiredComposition);
            Assert.AreEqual("child", state.GroupId); Assert.AreEqual("bags", state.ProductId);
            Assert.AreEqual("red", state.Color1Id); Assert.AreEqual("blue", state.Color2Id);
        }
        Assert.IsFalse(state.IsInputLocked); Assert.AreEqual(revision, state.Revision);
        Assert.AreEqual(4, state.OccupiedCount, "Ignored input is not replayed when the snapshot is released.");
        Assert.IsTrue(state.Set(2, null), "A fresh signal after generation is accepted.");
    }

    [Test] public void DisposingAnOldLeaseTwiceCannotUnlockANewerGeneration()
    {
        var state = new RfidReaderSelection(); state.Set(0, Group());
        var old = state.FreezeInput(); old.Dispose(); var current = state.FreezeInput(); old.Dispose();
        Assert.IsTrue(state.IsInputLocked); Assert.IsFalse(state.Set(0, null));
        current.Dispose(); Assert.IsFalse(state.IsInputLocked); Assert.IsTrue(state.Set(0, null));
    }

    [Test] public void HomeCanClearTheSnapshotWithoutReplayingIgnoredInput()
    {
        var state = new RfidReaderSelection(); state.Set(0, Group());
        using (state.FreezeInput())
        {
            state.Set(0, Product()); Assert.IsTrue(state.Clear(true));
            Assert.AreEqual(0, state.OccupiedCount); Assert.IsTrue(state.IsInputLocked);
        }
        Assert.AreEqual(0, state.OccupiedCount); Assert.IsFalse(state.IsInputLocked);
    }

    [TestCase(BlockKind.Product, "bags")]
    [TestCase(BlockKind.Color, "red")]
    public void GroupFirstIgnoresEarlierNonGroupAndDoesNotReplayIt(BlockKind kind, string id)
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true };
        Assert.IsFalse(state.CanRecognize(0, new RfidBlock(kind, id)));
        Assert.IsFalse(state.Set(0, new RfidBlock(kind, id)));
        Assert.AreEqual(0, state.Revision);
        state.Set(3, Group());
        Assert.IsNull(state[0]); Assert.AreEqual(1, state.OccupiedCount);
        Assert.IsTrue(state.Set(0, new RfidBlock(kind, id)));
    }

    [TestCase("child")]
    [TestCase("student")]
    [TestCase("working_adult")]
    [TestCase("senior")]
    public void EveryAgeGroupAllowsSubsequentProductAndColor(string id)
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true };
        Assert.IsTrue(state.Set(2, Group(id)));
        Assert.IsTrue(state.Set(0, Product())); Assert.IsTrue(state.Set(1, Color()));
        Assert.AreEqual(id, state.GroupId); Assert.AreEqual(3, state.OccupiedCount);
    }

    [Test] public void TwoGroupsDoNotAcceptNonGroupUntilOnlyOneRemains()
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true };
        state.Set(0, Group()); state.Set(1, Group("senior"));
        Assert.IsFalse(state.Set(2, Product())); Assert.IsFalse(state.Set(3, Color()));
        Assert.AreEqual(2, state.OccupiedCount); Assert.IsNull(state.GroupId);
        state.Set(1, null); Assert.IsTrue(state.Set(2, Product()));
    }

    [Test] public void ReplacingOnlyGroupWithNonGroupRemovesItWithoutAcceptingReplacement()
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true }; state.Set(1, Group());
        Assert.IsFalse(state.CanRecognize(1, Product()));
        Assert.IsTrue(state.Set(1, Product()));
        Assert.IsNull(state[1]); Assert.AreEqual(0, state.OccupiedCount);
    }

    [Test] public void MovingProductOntoOnlyGroupCannotBypassGroupFirst()
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true };
        state.Set(0, Group()); state.Set(1, Product());
        Assert.IsTrue(state.Move(1, 0));
        Assert.IsNull(state[0]); Assert.IsNull(state[1]); Assert.AreEqual(0, state.OccupiedCount);
    }

    [Test] public void ProductStageStillAcceptsNonGroupAfterGroupRemoval()
    {
        var state = new RfidReaderSelection { RequireGroupFirst = true };
        state.Set(2, Group()); state.RequireGroupFirst = false; state.Set(2, null);
        Assert.IsTrue(state.Set(0, Product())); Assert.IsTrue(state.Set(3, Color()));
        Assert.AreEqual(0, state.GroupCount); Assert.AreEqual(2, state.OccupiedCount);
    }

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

    [Test] public void ColorsFillColorSlotsInArrivalOrderRegardlessOfReader()
    {
        var state = new RfidReaderSelection();
        state.Set(3, Color("red")); state.Set(1, Group()); state.Set(2, Product()); state.Set(0, Color("blue"));
        Assert.IsTrue(state.HasRequiredComposition);
        Assert.AreEqual("red", state.Color1Id); Assert.AreEqual("blue", state.Color2Id);
        Assert.AreEqual("red", state.DisplayBlock(RfidReaderSelection.FirstColorSlot).Value.Id);
        Assert.AreEqual("blue", state.DisplayBlock(RfidReaderSelection.SecondColorSlot).Value.Id);
    }

    [Test] public void EveryReaderArrangementShowsEachBlockInItsPurposeSlot()
    {
        int[][] orders = { new[] { 0, 1, 2, 3 }, new[] { 3, 2, 1, 0 }, new[] { 2, 0, 3, 1 }, new[] { 1, 3, 0, 2 } };
        foreach (var r in orders)
        {
            var state = new RfidReaderSelection();
            state.Set(r[0], Color("red")); state.Set(r[1], Product()); state.Set(r[2], Group()); state.Set(r[3], Color("blue"));
            Assert.AreEqual(Group(), state.DisplayBlock(RfidReaderSelection.GroupSlot));
            Assert.AreEqual(Product(), state.DisplayBlock(RfidReaderSelection.ProductSlot));
            Assert.AreEqual(Color("red"), state.DisplayBlock(RfidReaderSelection.FirstColorSlot));
            Assert.AreEqual(Color("blue"), state.DisplayBlock(RfidReaderSelection.SecondColorSlot));
        }
    }

    [Test] public void RemovingAColorLeavesTheOtherInPlaceAndTheGapIsRefilled()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Color("red")); state.Set(1, Color("blue"));
        state.Set(0, null);
        Assert.IsNull(state.DisplayBlock(RfidReaderSelection.FirstColorSlot));
        Assert.AreEqual(Color("blue"), state.DisplayBlock(RfidReaderSelection.SecondColorSlot));
        Assert.AreEqual("blue", state.Color1Id, "A single remaining color is still the primary color.");
        Assert.IsNull(state.Color2Id);
        state.Set(3, Color("green"));
        Assert.AreEqual(Color("green"), state.DisplayBlock(RfidReaderSelection.FirstColorSlot));
        Assert.AreEqual(Color("blue"), state.DisplayBlock(RfidReaderSelection.SecondColorSlot));
        Assert.AreEqual("green", state.Color1Id); Assert.AreEqual("blue", state.Color2Id);
    }

    [Test] public void ReplacingABlockOnTheSameReaderKeepsItsSlot()
    {
        var state = new RfidReaderSelection();
        state.Set(2, Color("red")); state.Set(1, Color("blue"));
        state.Set(1, Color("green"));
        Assert.AreEqual(Color("red"), state.DisplayBlock(RfidReaderSelection.FirstColorSlot));
        Assert.AreEqual(Color("green"), state.DisplayBlock(RfidReaderSelection.SecondColorSlot));
    }

    [Test] public void ExtraBlocksWaitForASlotAndKeepTheCompositionInvalid()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Product()); state.Set(1, Product("shoes")); state.Set(2, Group()); state.Set(3, Color());
        Assert.AreEqual(Product(), state.DisplayBlock(RfidReaderSelection.ProductSlot));
        Assert.IsNull(state.ProductId, "Two products are never treated as one selection.");
        state.Set(0, null);
        Assert.AreEqual(Product("shoes"), state.DisplayBlock(RfidReaderSelection.ProductSlot));
        Assert.AreEqual("shoes", state.ProductId);
    }

    [Test] public void MovingBetweenReadersAndClearingKeepSlotsConsistent()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Group()); state.Set(1, Color("red")); state.Set(2, Color("blue"));
        Assert.IsTrue(state.Move(1, 3));
        Assert.AreEqual(Color("red"), state.DisplayBlock(RfidReaderSelection.FirstColorSlot));
        Assert.AreEqual(Color("blue"), state.DisplayBlock(RfidReaderSelection.SecondColorSlot));
        Assert.IsTrue(state.Clear());
        for (int slot = 0; slot < RfidReaderSelection.SlotCount; slot++) Assert.IsNull(state.DisplayBlock(slot));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.DisplayBlock(4));
    }

    [Test] public void OverfilledSlotsFollowEachKindsCapacity()
    {
        var state = new RfidReaderSelection();
        state.Set(0, Group("student")); state.Set(1, Color("red")); state.Set(2, Color("blue"));
        for (int slot = 0; slot < RfidReaderSelection.SlotCount; slot++) Assert.IsFalse(state.IsSlotOverfilled(slot));
        state.Set(3, Group("senior"));
        Assert.IsTrue(state.IsSlotOverfilled(RfidReaderSelection.GroupSlot));
        Assert.IsFalse(state.IsSlotOverfilled(RfidReaderSelection.ProductSlot));
        Assert.IsFalse(state.IsSlotOverfilled(RfidReaderSelection.FirstColorSlot), "Two colors are the requirement, not an excess.");
        state.Set(3, Color("green"));
        Assert.IsFalse(state.IsSlotOverfilled(RfidReaderSelection.GroupSlot));
        Assert.IsTrue(state.IsSlotOverfilled(RfidReaderSelection.FirstColorSlot));
        Assert.IsTrue(state.IsSlotOverfilled(RfidReaderSelection.SecondColorSlot));
        state.Set(3, Product()); state.Set(1, Product("shoes"));
        Assert.IsTrue(state.IsSlotOverfilled(RfidReaderSelection.ProductSlot));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.IsSlotOverfilled(4));
    }

    [Test] public void EmptyCueWaitsThreeQuietSecondsAndRestartsOnAnyChange()
    {
        var schedule = new BlockAttentionSchedule();
        Assert.IsFalse(Tick(schedule, 2.9f, true, 1, true, 0).empty);
        Assert.IsFalse(Tick(schedule, 2.9f, true, 2, true, 0).empty, "A block change restarts the wait.");
        Assert.IsTrue(Tick(schedule, .2f, true, 2, true, 0).empty);
        Assert.IsFalse(Tick(schedule, 2.9f, true, 2, true, 0).empty);
        Assert.IsTrue(Tick(schedule, .2f, true, 2, true, 0).empty, "The cue repeats while nothing is placed.");
        Assert.IsFalse(Tick(schedule, 5f, true, 2, false, 0).empty, "Nothing to place, no cue.");
        Assert.IsFalse(Tick(schedule, 2.9f, true, 2, true, 0).empty, "A slot becoming empty waits the full interval.");
        Assert.IsFalse(Tick(schedule, 2.9f, false, 2, true, 0).empty);
        Assert.IsFalse(Tick(schedule, 2.9f, true, 2, true, 0).empty, "A screen animation restarts the wait.");
    }

    [Test] public void OverfilledCuePlaysImmediatelyThenEveryThreeSecondsUntilResolved()
    {
        var schedule = new BlockAttentionSchedule();
        Assert.IsFalse(Tick(schedule, .1f, true, 1, false, 0).overfilled);
        Assert.IsTrue(Tick(schedule, .1f, true, 2, false, 1).overfilled, "Plays as soon as the extra block appears.");
        Assert.IsFalse(Tick(schedule, 2.9f, true, 2, false, 1).overfilled);
        Assert.IsTrue(Tick(schedule, .2f, true, 2, false, 1).overfilled);
        Assert.IsTrue(Tick(schedule, .1f, true, 3, false, 1 | 2).overfilled, "A newly overfilled slot does not wait.");
        Assert.IsFalse(Tick(schedule, .1f, true, 4, false, 2).overfilled, "A resolved slot does not retrigger the rest.");
        Assert.IsFalse(Tick(schedule, 5f, true, 5, false, 0).overfilled);
        Assert.IsTrue(Tick(schedule, .1f, true, 6, false, 1).overfilled, "Overfilling again plays again.");
    }

    [Test] public void OverfillDuringAScreenAnimationPlaysWhenItEnds()
    {
        var schedule = new BlockAttentionSchedule();
        Assert.IsFalse(Tick(schedule, 1f, false, 1, false, 1).overfilled);
        Assert.IsTrue(Tick(schedule, .1f, true, 1, false, 1).overfilled);
        Assert.IsFalse(Tick(schedule, 9f, false, 1, false, 1).overfilled, "The interval does not advance while held.");
        Assert.IsFalse(Tick(schedule, 2.9f, true, 1, false, 1).overfilled);
    }

    private static (bool empty, bool overfilled) Tick(BlockAttentionSchedule schedule, float seconds, bool active, int revision, bool anyEmpty, int overfilled)
    {
        schedule.Tick(seconds, active, revision, anyEmpty, overfilled, out bool playEmpty, out bool playOverfilled);
        return (playEmpty, playOverfilled);
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
