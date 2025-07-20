using Elevation.Logging;
using UnityEngine;

namespace Kampai.Util
{
	public class TestLogger : Elevation.Logging.ILogger, IKampaiLogger
	{
		public bool OutputToDebugLog;

		private readonly LogScope _scope;

		private readonly string _className;

		public LogScope Scope
		{
			get
			{
				return _scope;
			}
		}

		public string ClassName
		{
			get
			{
				return _className;
			}
		}

		public TestLogger()
		{
			_scope = LogScope.Default;
			_className = "Test";
		}

		public TestLogger EnableOutput()
		{
			OutputToDebugLog = true;
			return this;
		}

		public void Log(KampaiLogLevel level, string format, params object[] args)
		{
			if (OutputToDebugLog)
			{
				UnityEngine.Debug.Log(level.ToString() + ": " + string.Format(format, args));
			}
		}

		public void Log(KampaiLogLevel level, string text)
		{
			if (OutputToDebugLog)
			{
				UnityEngine.Debug.Log(level.ToString() + ": " + text);
			}
		}

		public void Log(KampaiLogLevel level, bool toScreen, string text)
		{
			Log(level, text);
		}

		public void Log(LogEvent logEvent)
		{
			if (logEvent != null && OutputToDebugLog)
			{
				switch (logEvent.Level)
				{
				case LogLevel.Trace:
				case LogLevel.Debug:
				case LogLevel.Info:
				case LogLevel.None:
					UnityEngine.Debug.Log(logEvent.Message);
					break;
				case LogLevel.Warn:
					UnityEngine.Debug.LogWarning(logEvent.Message);
					break;
				case LogLevel.Error:
					UnityEngine.Debug.LogError(logEvent.Message);
					break;
				case LogLevel.Fatal:
					UnityEngine.Debug.LogError("<color=blue>FATAL:</color>" + logEvent.Message);
					break;
				default:
					UnityEngine.Debug.LogError(logEvent.Message);
					break;
				}
			}
		}

		public void LogNullArgument()
		{
			Log(KampaiLogLevel.Error, "Null argument");
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

		public void Fatal(FatalCode code, string format, params object[] args)
		{
			Log(KampaiLogLevel.Error, "FATAL " + code.ToString() + string.Format(format, args));
		}

		public void Fatal(FatalCode code, int referenceId, string format, params object[] args)
		{
			Log(KampaiLogLevel.Error, "FATAL " + code.ToString() + " - " + referenceId + " " + string.Format(format, args));
		}

		public void FatalNullArgument(FatalCode code)
		{
			Log(KampaiLogLevel.Error, "FATAL " + code.ToString() + " Null argument");
		}

		public void Fatal(FatalCode code)
		{
			Log(KampaiLogLevel.Error, "FATAL " + code);
		}

		public void FatalNoThrow(FatalCode code)
		{
			Fatal(code);
		}

		public void FatalNoThrow(FatalCode code, int referencedId)
		{
			Fatal(code, referencedId);
		}

		public void FatalNoThrow(FatalCode code, string format, params object[] args)
		{
			Fatal(code, format, args);
		}

		public void FatalNoThrow(FatalCode code, int referencedId, string format, params object[] args)
		{
			Fatal(code, referencedId, format, args);
		}

		public void SetAllowedLevel(int level)
		{
		}

		public bool IsAllowedLevel(KampaiLogLevel level)
		{
			return true;
		}

		public virtual void Fatal(FatalCode code, int referencedId)
		{
			Log(KampaiLogLevel.Error, "FATAL " + code.ToString() + ":" + referencedId);
		}

		public void EventStart(string eventName)
		{
		}

		public void EventStop(string eventName)
		{
		}

		public void LogEventList()
		{
		}

		public static Elevation.Logging.ILogger BuildingTestLogger(LogScope scope, string className)
		{
			return new TestLogger();
		}
	}
}
