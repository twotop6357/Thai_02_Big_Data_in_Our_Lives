namespace Exhibition.Blocks
{
    /// <summary>
    /// Decides when the placement cues play. Empty slots wait a full interval without any block change.
    /// Overfilled slots play at once when they become overfilled, then once per interval while it lasts.
    /// </summary>
    public sealed class BlockAttentionSchedule
    {
        public const float IntervalSeconds = 3f;
        private float _emptyElapsed, _overfilledElapsed;
        private int _overfilledSlots, _revision = -1;

        public void Tick(float deltaSeconds, bool active, int revision, bool anyEmpty, int overfilledSlots,
            out bool playEmptyCue, out bool playOverfilledCue)
        {
            playEmptyCue = playOverfilledCue = false;
            if (revision != _revision) { _revision = revision; _emptyElapsed = 0; }
            if (!active)
            {
                // The wait restarts after a screen animation. An overfill noticed meanwhile still plays on release.
                _emptyElapsed = 0;
                return;
            }
            if (!anyEmpty) _emptyElapsed = 0;
            else if ((_emptyElapsed += deltaSeconds) >= IntervalSeconds)
            {
                _emptyElapsed = 0;
                playEmptyCue = true;
            }
            bool newlyOverfilled = (overfilledSlots & ~_overfilledSlots) != 0;
            _overfilledSlots = overfilledSlots;
            if (overfilledSlots == 0) _overfilledElapsed = 0;
            else if (newlyOverfilled || (_overfilledElapsed += deltaSeconds) >= IntervalSeconds)
            {
                _overfilledElapsed = 0;
                playOverfilledCue = true;
            }
        }
    }
}
