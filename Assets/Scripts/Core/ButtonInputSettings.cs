using System;

namespace Core
{
    [Serializable]
    public sealed class ButtonInputSettings
    {
        public const float DefaultDoublePressIntervalSeconds = 0.4f;
        public float buttonDoublePressIntervalSeconds = DefaultDoublePressIntervalSeconds;
        public bool HasValidInterval => !float.IsNaN(buttonDoublePressIntervalSeconds)
            && !float.IsInfinity(buttonDoublePressIntervalSeconds)
            && buttonDoublePressIntervalSeconds >= 0.1f && buttonDoublePressIntervalSeconds <= 2f;
    }

    public enum SessionLanguage { Thai, English }
}
