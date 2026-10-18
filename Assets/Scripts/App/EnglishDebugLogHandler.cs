using System;
using UnityEngine;

namespace App
{
    /// <summary>Translate the package's legacy Reporter warning without editing PackageCache.</summary>
    public sealed class EnglishDebugLogHandler : ILogHandler
    {
        private const string LegacyReporterWarning = "[Reporter] Log Content가 null입니다. 기본 스타일로 대체합니다.";
        private const string EnglishReporterWarning = "[Reporter] Log Content is null. Falling back to the default style.";
        private readonly ILogHandler _inner;

        private EnglishDebugLogHandler(ILogHandler inner) => _inner = inner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Install()
        {
            // Also idempotent when entering Play with Domain Reload disabled.
            if (!(Debug.unityLogger.logHandler is EnglishDebugLogHandler))
                Debug.unityLogger.logHandler = new EnglishDebugLogHandler(Debug.unityLogger.logHandler);
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (format == LegacyReporterWarning) format = EnglishReporterWarning;
            object[] forwarded = args;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (!(args[i] is string text) || text != LegacyReporterWarning) continue;
                    // Never mutate a caller-owned array; ordinary logs allocate no extra array here.
                    if (ReferenceEquals(forwarded, args)) forwarded = (object[])args.Clone();
                    forwarded[i] = EnglishReporterWarning;
                }
            }
            _inner.LogFormat(logType, context, format, forwarded);
        }

        public void LogException(Exception exception, UnityEngine.Object context) => _inner.LogException(exception, context);
    }
}
