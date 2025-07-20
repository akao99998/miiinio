using System;
using System.Collections.Generic;
using Elevation.Logging.Targets;

namespace Elevation.Logging
{
	public class LogManager
	{
		private static readonly LogManager _instance = new LogManager();

		private readonly List<ILoggingTarget> _targets;

		private readonly object _targetLock = new object();

		private readonly Dictionary<string, ILogger> _loggers;

		private Factory<ILoggingTarget, Dictionary<string, object>> _targetFactory;

		private Func<LogScope, string, ILogger> _loggerBuilder;

		public static LogManager Instance
		{
			get
			{
				return _instance;
			}
		}

		private LogManager()
		{
			_targets = new List<ILoggingTarget>();
			_loggers = new Dictionary<string, ILogger>(16);
			_targetFactory = new Factory<ILoggingTarget, Dictionary<string, object>>();
			_loggerBuilder = (LogScope scope, string className) => new DefaultLogger(scope, className);
			_targetFactory.Register("file", FileTarget.Build);
			_targetFactory.Register("console", ConsoleTarget.Build);
			_targetFactory.Register("null", (Dictionary<string, object> config) => new NullTarget());
		}

		public void SetConfig(Dictionary<string, object> config)
		{
			if (config == null)
			{
				return;
			}
			List<ILoggingTarget> list = new List<ILoggingTarget>(_targets);
			foreach (string key in config.Keys)
			{
				ILoggingTarget loggingTarget = GetTarget(key);
				Dictionary<string, object> dictionary = config[key] as Dictionary<string, object>;
				if (dictionary == null)
				{
					continue;
				}
				try
				{
					if (loggingTarget == null)
					{
						loggingTarget = CreateTarget(key, dictionary);
						AddTarget(loggingTarget);
					}
					else
					{
						loggingTarget.UpdateConfig(dictionary);
						list.Remove(loggingTarget);
					}
				}
				catch (Exception arg)
				{
					Console.Error.WriteLine("Error processing config for target: {0}.\n{1}", key, arg);
					if (loggingTarget != null)
					{
						RemoveTarget(loggingTarget);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				RemoveTarget(list[i]);
			}
		}

		public void AddTarget(ILoggingTarget target)
		{
			lock (_targetLock)
			{
				_targets.Add(target);
			}
		}

		public void Shutdown()
		{
			lock (_targetLock)
			{
				for (int i = 0; i < _targets.Count; i++)
				{
					ShutdownTarget(_targets[i]);
				}
				_targets.Clear();
			}
		}

		private void ShutdownTarget(ILoggingTarget target)
		{
			IDisposable disposable = target as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		public void WriteToTargets(LogEvent logEvent)
		{
			lock (_targetLock)
			{
				for (int i = 0; i < _targets.Count; i++)
				{
					_targets[i].WriteLogEvent(logEvent);
				}
			}
		}

		public static void RegisterLogger(Func<LogScope, string, ILogger> builder)
		{
			_instance._loggerBuilder = builder;
		}

		public static ILogger GetDefaultLogger()
		{
			return _instance.GetLogger(LogScope.Default, null);
		}

		public static ILogger GetClassLogger(LogScope scope, string className)
		{
			return _instance.GetLogger(scope, className);
		}

		public static ILogger GetClassLogger(string className)
		{
			return _instance.GetLogger(LogScope.Default, className);
		}

		private ILogger GetLogger(LogScope scope, string className)
		{
			string text = scope.ToString();
			string key = string.Join(".", new string[2]
			{
				text,
				(className != null) ? className : string.Empty
			});
			ILogger value;
			if (_loggers.TryGetValue(key, out value))
			{
				return value;
			}
			value = _loggerBuilder(scope, className);
			_loggers[key] = value;
			return value;
		}

		public static void RegisterTarget(string name, Func<Dictionary<string, object>, ILoggingTarget> builder)
		{
			_instance._targetFactory.Register(name, builder);
		}

		private ILoggingTarget GetTarget(string targetName)
		{
			lock (_targetLock)
			{
				for (int i = 0; i < _targets.Count; i++)
				{
					if (string.Compare(_targets[i].Name, targetName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return _targets[i];
					}
				}
			}
			return null;
		}

		private ILoggingTarget CreateTarget(string name, Dictionary<string, object> config)
		{
			if (_targetFactory == null)
			{
				return null;
			}
			return _targetFactory.Create(name, config);
		}

		private void RemoveTarget(ILoggingTarget target)
		{
			lock (_targetLock)
			{
				for (int i = 0; i < _targets.Count; i++)
				{
					if (string.Compare(_targets[i].Name, target.Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						ShutdownTarget(_targets[i]);
						_targets.RemoveAt(i);
						break;
					}
				}
			}
		}
	}
}
