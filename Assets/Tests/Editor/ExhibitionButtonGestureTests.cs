using Exhibition.Input;
using NUnit.Framework;

public class ExhibitionButtonGestureTests
{
    private ExhibitionButtonGesture _button;
    [SetUp]
    public void SetUp() => _button = new ExhibitionButtonGesture { DoublePressIntervalSeconds = 0.5 };

    [Test]
    public void SinglePressWaitsThenSelectsThai()
    {
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 0));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 0.1));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 0.49));
        Assert.AreEqual(ButtonGesture.SelectThai, _button.Tick(false, true, 0.5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 1));
    }

    [TestCase(0.2)]
    [TestCase(0.5)]
    public void TwoDistinctPressesWithinWindowSelectEnglish(double secondPress)
    {
        _button.Tick(true, true, 0);
        _button.Tick(false, true, 0.1);
        Assert.AreEqual(ButtonGesture.SelectEnglish, _button.Tick(true, true, secondPress));
    }

    [Test]
    public void LateSecondPressDoesNotSelectEnglish()
    {
        _button.Tick(true, true, 0);
        _button.Tick(false, true, 0.1);
        Assert.AreEqual(ButtonGesture.SelectThai, _button.Tick(true, true, 0.51));
    }

    [Test]
    public void HeldIdleButtonCannotBecomeReturnUntilReleased()
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.SelectThai, _button.Tick(true, true, 0.5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 10));
        _button.Tick(false, false, 10.1);
        _button.Tick(true, false, 11);
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, false, 16));
    }

    [Test]
    public void ReturnRequiresFiveContinuousSecondsAndOnlyFiresOnce()
    {
        _button.Tick(true, false, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 4.99));
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, false, 5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 10));
    }

    [Test]
    public void ReleaseCancelsHoldInsteadOfAccumulatingTime()
    {
        _button.Tick(true, false, 0);
        _button.Tick(false, false, 4);
        _button.Tick(true, false, 4.1);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 5));
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, false, 9.1));
    }

    [Test]
    public void ResetCancelsPendingSelectionAndRequiresRelease()
    {
        _button.Tick(true, true, 0);
        _button.Reset();
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 1));
        _button.Tick(false, true, 2);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 3));
        _button.Tick(true, true, 4);
        Assert.AreEqual(ButtonGesture.SelectThai, _button.Tick(false, true, 4.5));
    }

    [Test]
    public void ConfiguredIntervalChangesDeadline()
    {
        _button.DoublePressIntervalSeconds = 1;
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 0.5));
        Assert.AreEqual(ButtonGesture.SelectEnglish, _button.Tick(true, true, 0.8));
    }

    [Test]
    public void ShortPressOutsideIdleFiresOnReleaseOnce()
    {
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 0));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 1));
        Assert.AreEqual(ButtonGesture.ShortPress, _button.Tick(false, false, 1.2));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, false, 2));
    }

    [Test]
    public void CompletedReturnHoldDoesNotAlsoFireShortPress()
    {
        _button.Tick(true, false, 0);
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, false, 5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, false, 5.1));
    }

    [Test]
    public void ReleaseCarriedOverFromIdleIsNotShortPress()
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.SelectThai, _button.Tick(true, true, 0.5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, false, 1));
    }
}
