using System;
using System.Collections.Generic;
using System.Threading;
using Elevation.Collections.Generic;

namespace Elevation.Logging.Targets
{
	public abstract class AsyncLoggingTarget : BaseLoggingTarget, IDisposable
	{
		private readonly LockFreeQueue<LogEvent> _events;

		private List<LogFilter> _filters;

		private readonly Thread _worker;

		private volatile bool _flushQueue;

		private volatile bool _isRunning;

		public int TimeoutMillis { get; protected set; }

		protected bool Disposed { get; private set; }

		protected AsyncLoggingTarget(string name, LogLevel level, params LogFilter[] filters)
			: base(name, level)
		{
			TimeoutMillis = -1;
			if (filters != null && filters.Length > 0)
			{
				_filters = new List<LogFilter>(filters);
			}
			_events = new LockFreeQueue<LogEvent>();
			_flushQueue = false;
			_isRunning = true;
			_worker = new Thread(AsyncProcess)
			{
				Name = string.Format("{0}_thread", Name)
			};
			_worker.Start();
		}

		public override void WriteLogEvent(LogEvent logEvent)
		{
			if (!Disposed)
			{
				_events.Enqueue(logEvent);
			}
		}

		public override void Flush()
		{
			if (!Disposed && !_flushQueue)
			{
				_flushQueue = true;
			}
		}

		public override bool IsEnabled(LogEvent logEvent)
		{
			if (Disposed)
			{
				return false;
			}
			List<LogFilter> filters = _filters;
			if (filters != null)
			{
				for (int i = 0; i < filters.Count; i++)
				{
					if (filters[i].IsMatch(logEvent))
					{
						return filters[i].Inclusive;
					}
				}
			}
			return Level <= logEvent.Level;
		}

		protected virtual void SetFilters(List<LogFilter> filters)
		{
			if (!Disposed)
			{
				_filters = filters;
			}
		}

		protected void ClearFilters()
		{
			if (!Disposed)
			{
				_filters = null;
			}
		}

		public override void UpdateConfig(Dictionary<string, object> config)
		{
			if (Disposed)
			{
				return;
			}
			base.UpdateConfig(config);
			ClearFilters();
			if (!config.ContainsKey("filters"))
			{
				return;
			}
			IEnumerable<object> enumerable = config["filters"] as IEnumerable<object>;
			if (enumerable == null)
			{
				return;
			}
			List<LogFilter> list = new List<LogFilter>();
			foreach (object item in enumerable)
			{
				Dictionary<string, object> dictionary = item as Dictionary<string, object>;
				if (dictionary != null)
				{
					string pattern = string.Empty;
					FilterType type = FilterType.Message;
					bool inclusive = true;
					if (dictionary.ContainsKey("pattern") && !string.IsNullOrEmpty(dictionary["pattern"].ToString()))
					{
						pattern = dictionary["pattern"] as string;
					}
					if (dictionary.ContainsKey("type") && !string.IsNullOrEmpty(dictionary["type"].ToString()))
					{
						type = (FilterType)(int)Enum.Parse(typeof(FilterType), dictionary["type"].ToString(), true);
					}
					if (dictionary.ContainsKey("inclusive") && !string.IsNullOrEmpty(dictionary["inclusive"].ToString()))
					{
						inclusive = dictionary["inclusive"].ToString().ToLower().Equals("true");
					}
					list.Add(new LogFilter(pattern, type, inclusive));
				}
			}
			if (list.Count > 0)
			{
				SetFilters(list);
			}
		}

		protected abstract void Write(LogEvent logEvent);

		protected virtual void AsyncProcess()
		{
			while (_isRunning)
			{
				LogEvent logEvent;
				if (_flushQueue)
				{
					while ((logEvent = _events.Dequeue()) != null)
					{
						if (IsEnabled(logEvent))
						{
							Write(logEvent);
						}
					}
					_flushQueue = false;
				}
				logEvent = _events.Dequeue();
				if (logEvent != null && IsEnabled(logEvent))
				{
					Write(logEvent);
				}
				PostProcess();
				Thread.Sleep(1);
			}
		}

		protected virtual void PostProcess()
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed && disposing && _isRunning)
			{
				_isRunning = false;
				_worker.Join();
				Disposed = true;
			}
		}
	}
}
