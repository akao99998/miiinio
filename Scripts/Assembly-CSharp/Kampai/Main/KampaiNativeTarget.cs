using System.Collections.Generic;
using Elevation.Logging;
using Elevation.Logging.Targets;
using Kampai.Util;

namespace Kampai.Main
{
	public class KampaiNativeTarget : AsyncLoggingTarget
	{
		public KampaiNativeTarget(LogLevel level)
			: base("Kampai.Native", level)
		{
		}

		protected override void Write(LogEvent logEvent)
		{
			if (logEvent != null)
			{
				switch (logEvent.Level)
				{
				case LogLevel.Trace:
				case LogLevel.None:
					Native.LogVerbose(FormattedLogEvent(logEvent));
					break;
				case LogLevel.Info:
					Native.LogInfo(FormattedLogEvent(logEvent));
					break;
				case LogLevel.Debug:
					Native.LogDebug(FormattedLogEvent(logEvent));
					break;
				case LogLevel.Warn:
					Native.LogWarning(FormattedLogEvent(logEvent));
					break;
				case LogLevel.Error:
				case LogLevel.Fatal:
					Native.LogError(FormattedLogEvent(logEvent));
					break;
				default:
					Native.LogError(FormattedLogEvent(logEvent));
					break;
				}
			}
		}

		public static KampaiNativeTarget Build(Dictionary<string, object> config)
		{
			KampaiNativeTarget kampaiNativeTarget = new KampaiNativeTarget(LogLevel.Trace);
			kampaiNativeTarget.UpdateConfig(config);
			return kampaiNativeTarget;
		}
	}
}
