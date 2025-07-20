using System;
using System.Collections.Generic;

namespace Elevation.Logging.Targets
{
	public class ConsoleTarget : AsyncLoggingTarget
	{
		public ConsoleTarget(LogLevel level, params LogFilter[] filters)
			: base("Console", level, filters)
		{
		}

		protected override void Write(LogEvent logEvent)
		{
			Console.Error.WriteLine(FormattedLogEvent(logEvent));
		}

		public static ConsoleTarget Build(Dictionary<string, object> config)
		{
			ConsoleTarget consoleTarget = new ConsoleTarget(LogLevel.None);
			consoleTarget.UpdateConfig(config);
			return consoleTarget;
		}
	}
}
