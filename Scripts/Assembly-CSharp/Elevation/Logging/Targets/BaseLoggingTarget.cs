using System;
using System.Collections.Generic;
using System.Text;

namespace Elevation.Logging.Targets
{
	public abstract class BaseLoggingTarget : ILoggingTarget
	{
		public string Name { get; protected set; }

		public LogLevel Level { get; protected set; }

		protected BaseLoggingTarget(string name, LogLevel level)
		{
			Name = name;
			Level = level;
		}

		public abstract void WriteLogEvent(LogEvent logEvent);

		public virtual void Flush()
		{
		}

		public virtual bool IsEnabled(LogEvent logEvent)
		{
			if (logEvent != null)
			{
				return Level <= logEvent.Level;
			}
			return false;
		}

		public virtual void UpdateConfig(Dictionary<string, object> config)
		{
			if (config.ContainsKey("level"))
			{
				Level = (LogLevel)(int)Enum.Parse(typeof(LogLevel), config["level"].ToString(), true);
			}
		}

		protected virtual string FormattedLogEvent(LogEvent logEvent)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			stringBuilder.AppendFormat("{0} {1,-5} [{2}] {3}", logEvent.Timestamp, logEvent.Level, logEvent.Scope, logEvent.ClassName);
			if (logEvent.MethodName != null)
			{
				stringBuilder.AppendFormat(".{0}", logEvent.MethodName);
			}
			stringBuilder.AppendFormat(": {0}", logEvent.FormattedMessage);
			if (logEvent.ElapsedTime > 0.0)
			{
				stringBuilder.AppendFormat(" Elapsed: {0:0.000}s", logEvent.ElapsedTime);
			}
			if (logEvent.Data != null)
			{
				stringBuilder.AppendFormat(" Data: {0}", logEvent.Data);
			}
			if (logEvent.StackTrace != null)
			{
				stringBuilder.AppendFormat("\nStackTrace: {0}", logEvent.StackTrace.ToString());
			}
			if (logEvent.Exception != null)
			{
				stringBuilder.AppendFormat("\nException: {0}", logEvent.Exception.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
