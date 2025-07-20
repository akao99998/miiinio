using System.Collections.Generic;
using System.Diagnostics;

namespace Elevation.Logging
{
	public class DefaultLogger : ILogger
	{
		private readonly LogScope _scope;

		private readonly string _className;

		private Dictionary<string, Stopwatch> _timers;

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

		public DefaultLogger(LogScope scope, string className)
		{
			_scope = scope;
			_className = className;
		}

		public void Log(LogEvent logEvent)
		{
			if (logEvent.TimerName != null)
			{
				if (_timers == null)
				{
					_timers = new Dictionary<string, Stopwatch>();
				}
				Stopwatch stopwatch;
				if (!logEvent.TimerStopped)
				{
					stopwatch = new Stopwatch();
					stopwatch.Start();
					_timers[logEvent.TimerName] = stopwatch;
				}
				else if (_timers.TryGetValue(logEvent.TimerName, out stopwatch))
				{
					stopwatch.Stop();
					logEvent.ElapsedTime = stopwatch.Elapsed.TotalSeconds;
				}
			}
			if (logEvent.Scope == LogScope.Unknown)
			{
				logEvent.Scope = _scope;
			}
			if (logEvent.ClassName == null && _className != null)
			{
				logEvent.ClassName = _className;
			}
			LogManager.Instance.WriteToTargets(logEvent);
		}
	}
}
