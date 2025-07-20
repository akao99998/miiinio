using System.Collections.Generic;

namespace Elevation.Logging.Targets
{
	public class NullTarget : ILoggingTarget
	{
		public string Name { get; private set; }

		public LogLevel Level { get; private set; }

		public NullTarget()
		{
			Name = "Null";
			Level = LogLevel.None;
		}

		public void WriteLogEvent(LogEvent logEvent)
		{
		}

		public void Flush()
		{
		}

		public bool IsEnabled(LogEvent logEvent)
		{
			return false;
		}

		public void UpdateConfig(Dictionary<string, object> config)
		{
		}
	}
}
