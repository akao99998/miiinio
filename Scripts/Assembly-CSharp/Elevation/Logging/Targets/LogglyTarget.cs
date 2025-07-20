using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Elevation.Logging.Targets
{
	public abstract class LogglyTarget : BufferedTarget
	{
		protected readonly string _url;

		private DateTime _lastRollTime;

		private int _sendRate;

		protected LogglyTarget(string name, LogLevel level, int sendRateSeconds, int maxBufferSize, string logFolder, string token, string tag, params LogFilter[] filters)
			: base(name, level, sendRateSeconds * 500, maxBufferSize, logFolder, filters)
		{
			_lastRollTime = DateTime.UtcNow;
			_sendRate = sendRateSeconds;
			if (tag != null)
			{
				_url = string.Format("https://logs-01.loggly.com/bulk/{0}/tag/{1}", token, tag);
			}
			else
			{
				_url = string.Format("https://logs-01.loggly.com/bulk/{0}", token);
			}
		}

		protected override void PostProcess()
		{
			if ((DateTime.UtcNow - _lastRollTime).TotalSeconds > (double)_sendRate && base.CurrentFileSize > 0)
			{
				RollLogFiles();
			}
		}

		protected override void RollLogFiles()
		{
			base.RollLogFiles();
			_lastRollTime = DateTime.UtcNow;
		}

		protected override void BatchProcess(FileInfo buffer)
		{
			byte[] array;
			using (FileStream fileStream = buffer.OpenRead())
			{
				int num = 0;
				int num2 = (int)fileStream.Length;
				array = new byte[num2];
				while (num2 > 0)
				{
					int num3 = fileStream.Read(array, num, num2);
					if (num3 == 0)
					{
						break;
					}
					num += num3;
					num2 -= num3;
				}
			}
			if (array.Length > 0)
			{
				SendRequest(array);
			}
		}

		protected abstract void SendRequest(byte[] bytes);

		public override void UpdateConfig(Dictionary<string, object> config)
		{
			if (!base.Disposed)
			{
				base.UpdateConfig(config);
				if (config.ContainsKey("sendRateSeconds"))
				{
					_sendRate = Convert.ToInt32(config["sendRateSeconds"]);
					base.TimeoutMillis = _sendRate * 500;
				}
			}
		}

		protected virtual void SerializeProperties(StringBuilder sb, LogEvent logEvent)
		{
		}

		private void SerializeStandardProperties(StringBuilder sb, LogEvent logEvent)
		{
			sb.AppendFormat("\"timestamp\":\"{0}\",", logEvent.Timestamp.ToString("o"));
			sb.AppendFormat("\"logLevel\":\"{0}\",", logEvent.Level.ToString().ToUpper());
			sb.AppendFormat("\"scope\":\"{0}\",", logEvent.Scope);
			if (logEvent.ClassName != null)
			{
				sb.AppendFormat("\"class\":\"{0}\",", logEvent.ClassName);
			}
			if (logEvent.MethodName != null)
			{
				sb.AppendFormat("\"method\":\"{0}\",", logEvent.MethodName);
			}
			if (logEvent.ElapsedTime > 0.0)
			{
				sb.AppendFormat("\"elapsedTime\":{0},", logEvent.ElapsedTime);
			}
			if (logEvent.StackTrace != null)
			{
				sb.AppendFormat("\"stacktrace\":");
				SerializeString(sb, logEvent.StackTrace.ToString());
				sb.Append(',');
			}
			if (logEvent.Exception != null)
			{
				sb.AppendFormat("\"exception\":");
				SerializeString(sb, logEvent.Exception.ToString());
				sb.Append(',');
			}
			sb.AppendFormat("\"message\":\"{0}\"", logEvent.FormattedMessage);
		}

		protected void SerializeString(StringBuilder sb, string str)
		{
			sb.Append('"');
			foreach (char c in str)
			{
				switch (c)
				{
				case '"':
					sb.Append("\\\"");
					continue;
				case '\\':
					sb.Append("\\\\");
					continue;
				case '\b':
					sb.Append("\\b");
					continue;
				case '\f':
					sb.Append("\\f");
					continue;
				case '\n':
					sb.Append("\\n");
					continue;
				case '\r':
					sb.Append("\\r");
					continue;
				case '\t':
					sb.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					sb.Append(c);
					continue;
				}
				sb.Append("\\u");
				sb.Append(num.ToString("x4"));
			}
			sb.Append('"');
		}

		protected override string FormattedLogEvent(LogEvent logEvent)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			stringBuilder.Append('{');
			SerializeProperties(stringBuilder, logEvent);
			SerializeStandardProperties(stringBuilder, logEvent);
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}
	}
}
