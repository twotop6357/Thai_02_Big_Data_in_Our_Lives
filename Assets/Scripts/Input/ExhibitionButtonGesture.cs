namespace Exhibition.Input
{
    public enum ButtonGesture { None, SelectThai, SelectEnglish, ReturnToIdle }

    /// <summary>Allocation-free state machine. Time is supplied in unscaled seconds.</summary>
    public sealed class ExhibitionButtonGesture
    {
        public const double ReturnHoldSeconds = 5.0;
        public double DoublePressIntervalSeconds { get; set; } = 0.4;
        private bool _wasPressed, _waitForRelease, _pendingSingle, _holding;
        private double _singleDeadline, _holdStarted;

        public void Reset(bool waitForRelease = true)
        {
            _wasPressed = false;
            _waitForRelease = waitForRelease;
            _pendingSingle = false;
            _holding = false;
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
            if (isIdle)
            {
                // An exact-boundary second press takes priority over single-press expiry.
                if (_pendingSingle)
                {
                    if (pressedThisTick && now <= _singleDeadline)
                    {
                        Reset();
                        return ButtonGesture.SelectEnglish;
                    }
                    if (now >= _singleDeadline)
                    {
                        Reset();
                        return ButtonGesture.SelectThai;
                    }
                }
                else if (pressedThisTick)
                {
                    _pendingSingle = true;
                    _singleDeadline = now + DoublePressIntervalSeconds;
                }
                return ButtonGesture.None;
            }
            if (!pressed) _holding = false;
            else if (pressedThisTick)
            {
                _holding = true;
                _holdStarted = now;
            }
            else if (_holding && now - _holdStarted >= ReturnHoldSeconds)
            {
                Reset();
                return ButtonGesture.ReturnToIdle;
            }
            return ButtonGesture.None;
        }
    }
}
