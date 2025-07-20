using System.Collections.Generic;

namespace Elevation.Logging.Targets
{
	public interface ILoggingTarget
	{
		string Name { get; }

		LogLevel Level { get; }

		void WriteLogEvent(LogEvent logEvent);

		void Flush();

		bool IsEnabled(LogEvent logEvent);

		void UpdateConfig(Dictionary<string, object> config);
	}
}
