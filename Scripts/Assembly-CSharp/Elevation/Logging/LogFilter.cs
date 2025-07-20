using System.Text.RegularExpressions;

namespace Elevation.Logging
{
	public class LogFilter
	{
		private Regex _regex;

		public string Pattern { get; private set; }

		public FilterType Type { get; private set; }

		public bool Inclusive { get; private set; }

		public LogFilter(string pattern, FilterType type, bool inclusive)
		{
			Pattern = pattern;
			Type = type;
			Inclusive = inclusive;
			if (!string.IsNullOrEmpty(pattern))
			{
				_regex = new Regex(pattern, RegexOptions.Singleline);
			}
		}

		public bool IsMatch(LogEvent logEvent)
		{
			if (_regex == null)
			{
				return true;
			}
			string text;
			switch (Type)
			{
			default:
				text = logEvent.FormattedMessage;
				break;
			case FilterType.Scope:
				text = logEvent.Scope.ToString();
				break;
			case FilterType.ClassName:
				text = logEvent.ClassName;
				break;
			case FilterType.MethodName:
				text = logEvent.MethodName;
				break;
			case FilterType.ClassAndMethodNames:
				text = logEvent.ClassAndMethodName;
				break;
			}
			if (text == null)
			{
				return false;
			}
			return _regex.IsMatch(text);
		}
	}
}
