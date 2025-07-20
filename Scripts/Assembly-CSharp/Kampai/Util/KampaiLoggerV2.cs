using System.Collections.Generic;
using Elevation.Logging;

namespace Kampai.Util
{
	public class KampaiLoggerV2 : DefaultLogger, IKampaiLogger
	{
		public KampaiLoggerV2(LogScope scope, string className)
			: base(scope, className)
		{
		}

		public bool IsAllowedLevel(KampaiLogLevel level)
		{
			return true;
		}

		public void SetAllowedLevel(int level)
		{
		}

		public void Log(KampaiLogLevel level, string format, params object[] args)
		{
			LogIt(level, format, args);
		}

		public void Log(KampaiLogLevel level, string text)
		{
			LogIt(level, text);
		}

		public void Log(KampaiLogLevel level, bool toScreen, string text)
		{
			LogIt(level, text);
		}

		public void LogNullArgument()
		{
			Log(KampaiLogLevel.Error, "Null arguments");
		}

		public void Verbose(string text)
		{
			Log(KampaiLogLevel.Verbose, text);
		}

		public void Verbose(string format, params object[] args)
		{
			Log(KampaiLogLevel.Verbose, format, args);
		}

		public void Debug(string text)
		{
			Log(KampaiLogLevel.Debug, text);
		}

		public void Debug(string format, params object[] args)
		{
			Log(KampaiLogLevel.Debug, format, args);
		}

		public void Info(string text)
		{
			Log(KampaiLogLevel.Info, text);
		}

		public void Info(string format, params object[] args)
		{
			Log(KampaiLogLevel.Info, format, args);
		}

		public void Warning(string text)
		{
			Log(KampaiLogLevel.Warning, text);
		}

		public void Warning(string format, params object[] args)
		{
			Log(KampaiLogLevel.Warning, format, args);
		}

		public void Error(string text)
		{
			Log(KampaiLogLevel.Error, text);
		}

		public void Error(string format, params object[] args)
		{
			Log(KampaiLogLevel.Error, format, args);
		}

		public void EventStart(string eventName)
		{
			Log(LogEvent.Debug(string.Format("EventStart: {0}", eventName)).StartTimer(eventName));
		}

		public void EventStop(string eventName)
		{
			Log(LogEvent.Debug(string.Format("EventStop: {0}", eventName)).StopTimer(eventName));
		}

		public void LogEventList()
		{
		}

		protected void LogIt(KampaiLogLevel level, string text)
		{
			switch (level)
			{
			case KampaiLogLevel.Info:
				Log(LogEvent.Info(text));
				break;
			case KampaiLogLevel.Debug:
				Log(LogEvent.Debug(text));
				break;
			case KampaiLogLevel.Warning:
				Log(LogEvent.Warn(text));
				break;
			case KampaiLogLevel.Error:
				Log(LogEvent.Error(text));
				break;
			case KampaiLogLevel.Verbose:
				Log(LogEvent.Trace(text));
				break;
			default:
				Log(LogEvent.Error(text));
				break;
			}
		}

		protected void LogIt(KampaiLogLevel level, string format, params object[] args)
		{
			switch (level)
			{
			case KampaiLogLevel.Info:
				Log(LogEvent.Info(format, args));
				break;
			case KampaiLogLevel.Debug:
				Log(LogEvent.Debug(format, args));
				break;
			case KampaiLogLevel.Warning:
				Log(LogEvent.Warn(format, args));
				break;
			case KampaiLogLevel.Error:
				Log(LogEvent.Error(format, args));
				break;
			case KampaiLogLevel.Verbose:
				Log(LogEvent.Trace(format, args));
				break;
			default:
				Log(LogEvent.Error(format, args));
				break;
			}
		}

		public void Fatal(FatalCode code, string format, params object[] args)
		{
			Fatal(code, 0, format, args);
		}

		public void FatalNoThrow(FatalCode code, string format, params object[] args)
		{
			FatalNoThrow(code, 0, format, args);
		}

		public void Fatal(FatalCode code, int referencedId, string format, params object[] args)
		{
			FatalNoThrow(code, referencedId, format, args);
			throw new FatalException(code, referencedId, format, args);
		}

		public void FatalNullArgument(FatalCode code)
		{
			Fatal(code, "Null argument");
		}

		public void Fatal(FatalCode code)
		{
			Fatal(code, code.ToString());
		}

		public void FatalNoThrow(FatalCode code)
		{
			FatalNoThrow(code, 0, code.ToString());
		}

		public void Fatal(FatalCode code, int referencedId)
		{
			Fatal(code, referencedId, string.Empty);
		}

		public void FatalNoThrow(FatalCode code, int referencedId)
		{
			FatalNoThrow(code, referencedId, string.Empty);
		}

		public void FatalNoThrow(FatalCode code, int referencedId, string format, params object[] args)
		{
			string message = string.Format("[ERROR {0}-{1}] {2}", (int)code, referencedId, string.Format(format, args));
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("format", format);
			dictionary.Add("fatalCode", code);
			dictionary.Add("referencedId", referencedId);
			dictionary.Add("params", args);
			Dictionary<string, object> data = dictionary;
			Log(LogEvent.Fatal(message).WithData(data).WithStackTrace());
		}

		public static ILogger BuildingKampaiLogger(LogScope scope, string className)
		{
			return new KampaiLoggerV2(scope, className);
		}
	}
}
