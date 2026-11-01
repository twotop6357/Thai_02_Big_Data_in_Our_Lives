using Exhibition.Input;
using NUnit.Framework;

public class ExhibitionButtonGestureTests
{
    private ExhibitionButtonGesture _button;
    [SetUp] public void SetUp() => _button = new ExhibitionButtonGesture();

    [Test] public void TitleTapStartsOnReleaseWithoutLanguageSelection()
    {
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 0));
        Assert.AreEqual(ButtonGesture.StartSession, _button.Tick(false, true, .1));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 3));
    }

    [Test] public void TitleHoldTogglesAtTwoSecondsOnlyOnceAndNeverStarts()
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 1.999));
        Assert.AreEqual(ButtonGesture.ToggleLanguage, _button.Tick(true, true, 2));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 4));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 4.1));
    }

    [TestCase(true)] [TestCase(false)]
    public void FiveSecondHoldReturnsHomeFromAnyScreenOnce(bool idle)
    {
        _button.Tick(true, idle, 0);
        _button.Tick(true, idle, 2);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, idle, 4.999));
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, idle, 5));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, idle, 10));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, idle, 11));
    }

    [TestCase(2, ButtonGesture.ToggleLanguage)]
    [TestCase(5, ButtonGesture.ReturnToIdle)]
    public void TitleReleaseOnThresholdIsNotTap(double release, ButtonGesture expected)
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(expected, _button.Tick(false, true, release));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, release + 1));
    }

    [Test] public void NonTitleTwoSecondHoldDoesNotSwitchLanguage()
    {
        _button.Tick(true, false, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 2));
        Assert.AreEqual(ButtonGesture.ShortPress, _button.Tick(false, false, 2.1));
    }

    [Test] public void ShortPressOutsideTitleFiresOnlyOnRelease()
    {
        _button.Tick(true, false, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 1));
        Assert.AreEqual(ButtonGesture.ShortPress, _button.Tick(false, false, 1.2));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, false, 2));
    }

    [Test] public void FiveSecondReleaseAfterSlowFrameDoesNotAdvance()
    {
        _button.Tick(true, false, 0);
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(false, false, 5));
    }

    [Test] public void ReleasedHoldsDoNotAccumulate()
    {
        _button.Tick(true, false, 0);
        _button.Tick(false, false, 4);
        _button.Tick(true, false, 4.1);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, false, 5));
        Assert.AreEqual(ButtonGesture.ReturnToIdle, _button.Tick(true, false, 9.1));
    }

    [Test] public void ResetRequiresFreshPressAndCancelsPendingTap()
    {
        _button.Tick(true, true, 0);
        _button.Reset();
        Assert.AreEqual(ButtonGesture.None, _button.Tick(true, true, 6));
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, true, 7));
        _button.Tick(true, true, 8);
        Assert.AreEqual(ButtonGesture.StartSession, _button.Tick(false, true, 8.1));
    }

    [Test] public void TwoTapsHaveNoSpecialLanguageMeaning()
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.StartSession, _button.Tick(false, true, .1));
        _button.Reset();
        _button.Tick(false, true, .2);
        _button.Tick(true, true, .3);
        Assert.AreEqual(ButtonGesture.StartSession, _button.Tick(false, true, .4));
    }

    [Test] public void ScreenChangeDuringPressDoesNotAdvanceNextScreen()
    {
        _button.Tick(true, true, 0);
        Assert.AreEqual(ButtonGesture.None, _button.Tick(false, false, 1));
    }
}
