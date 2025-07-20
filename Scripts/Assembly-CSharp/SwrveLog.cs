public static class SwrveLog
{
	public enum SwrveLogType
	{
		Info = 0,
		Warning = 1,
		Error = 2
	}

	public delegate void SwrveLogEventHandler(SwrveLogType type, object message, string tag);

	public static bool Verbose = true;

	public static SwrveLogEventHandler OnLog;

	public static void Log(object message)
	{
		Log(message, "activity");
	}

	public static void LogWarning(object message)
	{
		LogWarning(message, "activity");
	}

	public static void LogError(object message)
	{
		LogError(message, "activity");
	}

	public static void Log(object message, string tag)
	{
		if (Verbose && OnLog != null)
		{
			OnLog(SwrveLogType.Info, message, tag);
		}
	}

	public static void LogWarning(object message, string tag)
	{
		if (Verbose && OnLog != null)
		{
			OnLog(SwrveLogType.Warning, message, tag);
		}
	}

	public static void LogError(object message, string tag)
	{
		if (OnLog != null)
		{
			OnLog(SwrveLogType.Error, message, tag);
		}
	}
}
