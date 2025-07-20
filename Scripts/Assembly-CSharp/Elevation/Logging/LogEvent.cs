using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Elevation.Logging
{
	public class LogEvent
	{
		private string _message;

		private object[] _parameters;

		private string _formattedMessage;

		public LogLevel Level { get; set; }

		public LogScope Scope { get; set; }

		public string ClassName { get; set; }

		public string MethodName { get; set; }

		public string TimerName { get; set; }

		public bool TimerStopped { get; set; }

		public double ElapsedTime { get; set; }

		public Dictionary<string, object> Data { get; set; }

		public StackTrace StackTrace { get; private set; }

		public Exception Exception { get; set; }

		public int FatalCode { get; set; }

		public DateTime Timestamp { get; private set; }

		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				_message = value;
				_formattedMessage = null;
			}
		}

		public object[] Parameters
		{
			get
			{
				return _parameters;
			}
			set
			{
				_parameters = value;
				_formattedMessage = null;
			}
		}

		public string FormattedMessage
		{
			get
			{
				if (_formattedMessage != null)
				{
					return _formattedMessage;
				}
				_formattedMessage = ((_parameters == null) ? _message : string.Format(_message, _parameters));
				return _formattedMessage;
			}
		}

		public string ClassAndMethodName
		{
			get
			{
				if (ClassName == null && MethodName == null)
				{
					return string.Empty;
				}
				if (ClassName == null)
				{
					return MethodName;
				}
				if (MethodName == null)
				{
					return ClassName;
				}
				return string.Join(".", new string[2] { ClassName, MethodName });
			}
		}

		public LogEvent()
		{
			Timestamp = DateTime.UtcNow;
			Level = LogLevel.None;
			Scope = LogScope.Unknown;
			ClassName = null;
			MethodName = null;
			TimerName = null;
			TimerStopped = false;
			ElapsedTime = -1.0;
			Data = null;
			StackTrace = null;
			Exception = null;
		}

		public static LogEvent Trace(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Trace;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Trace(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Trace;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public static LogEvent Debug(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Debug;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Debug(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Debug;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public static LogEvent Info(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Info;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Info(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Info;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public static LogEvent Warn(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Warn;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Warn(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Warn;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public static LogEvent Error(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Error;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Error(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Error;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public static LogEvent Fatal(string message)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Fatal;
			logEvent.Message = message;
			return logEvent;
		}

		public static LogEvent Fatal(string message, params object[] args)
		{
			LogEvent logEvent = new LogEvent();
			logEvent.Level = LogLevel.Fatal;
			logEvent.Message = message;
			logEvent.Parameters = args;
			return logEvent;
		}

		public LogEvent FromScope(LogScope scope)
		{
			Scope = scope;
			return this;
		}

		public LogEvent FromClass(string className)
		{
			ClassName = className;
			return this;
		}

		public LogEvent FromMethod(string methodName)
		{
			MethodName = methodName;
			return this;
		}

		public LogEvent StartTimer(string timerName)
		{
			TimerName = timerName;
			TimerStopped = false;
			return this;
		}

		public LogEvent StopTimer(string timerName)
		{
			TimerName = timerName;
			TimerStopped = true;
			return this;
		}

		public LogEvent WithData(Dictionary<string, object> data)
		{
			Data = data;
			return this;
		}

		public LogEvent WithException(Exception exception)
		{
			Exception = exception;
			return this;
		}

		public LogEvent WithStackTrace()
		{
			StackTrace = new StackTrace(1, true);
			return this;
		}

		public LogEvent WithFatalCode(int code)
		{
			FatalCode = code;
			return this;
		}
	}
}
