namespace Elevation.Logging
{
	public interface ILogger
	{
		LogScope Scope { get; }

		string ClassName { get; }

		void Log(LogEvent logEvent);
	}
}
