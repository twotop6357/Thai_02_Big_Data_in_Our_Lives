namespace Exhibition.Input
{
    public enum ButtonGesture { None, StartSession, ToggleLanguage, ReturnToIdle, ShortPress }

    /// <summary>Allocation-free input state machine driven by unscaled seconds.</summary>
    public sealed class ExhibitionButtonGesture
    {
        public const double ReturnHoldSeconds = 5.0;
        public const double LanguageHoldSeconds = 2.0;
        private bool _wasPressed, _waitForRelease, _holding, _languageToggled, _startedInIdle;
        private double _holdStarted;

        public void Reset(bool waitForRelease = true)
        {
            _wasPressed = false;
            _waitForRelease = waitForRelease;
            _holding = false;
            _languageToggled = false;
        }

        public ButtonGesture Tick(bool pressed, bool isIdle, double now)
        {
            if (_waitForRelease)
            {
                if (!pressed) _waitForRelease = false;
                _wasPressed = pressed;
                return ButtonGesture.None;
            }
            bool pressedThisTick = pressed && !_wasPressed;
            _wasPressed = pressed;
            if (pressedThisTick)
            {
                _holding = true;
                _languageToggled = false;
                _startedInIdle = isIdle;
                _holdStarted = now;
                return ButtonGesture.None;
            }
            if (!_holding) return ButtonGesture.None;
            double held = now - _holdStarted;
            // Also check release duration: a slow frame must not turn a completed hold into a tap.
            if (held >= ReturnHoldSeconds)
            {
                Reset(pressed);
                return ButtonGesture.ReturnToIdle;
            }
            if (_startedInIdle && isIdle && !_languageToggled && held >= LanguageHoldSeconds)
            {
                _languageToggled = true;
                if (!pressed) _holding = false;
                return ButtonGesture.ToggleLanguage;
            }
            if (!pressed)
            {
                _holding = false;
                if (_languageToggled || _startedInIdle != isIdle) return ButtonGesture.None;
                return isIdle ? ButtonGesture.StartSession : ButtonGesture.ShortPress;
            }
            return ButtonGesture.None;
        }
    }
}
