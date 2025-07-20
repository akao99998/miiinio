namespace Kampai.Util
{
	public interface IKampaiLogger
	{
		void Log(KampaiLogLevel level, string format, params object[] args);

		void Log(KampaiLogLevel level, string text);

		void Log(KampaiLogLevel level, bool toScreen, string text);

		void LogNullArgument();

		void Verbose(string text);

		void Verbose(string format, params object[] args);

		void Debug(string text);

		void Debug(string format, params object[] args);

		void Info(string text);

		void Info(string format, params object[] args);

		void Warning(string text);

		void Warning(string format, params object[] args);

		void Error(string text);

		void Error(string format, params object[] args);

		void Fatal(FatalCode code);

		void Fatal(FatalCode code, int referencedId);

		void Fatal(FatalCode code, string format, params object[] args);

		void Fatal(FatalCode code, int referencedId, string format, params object[] args);

		void FatalNullArgument(FatalCode code);

		void FatalNoThrow(FatalCode code);

		void FatalNoThrow(FatalCode code, int referencedId);

		void FatalNoThrow(FatalCode code, string format, params object[] args);

		void FatalNoThrow(FatalCode code, int referencedId, string format, params object[] args);

		void SetAllowedLevel(int level);

		bool IsAllowedLevel(KampaiLogLevel level);

		void EventStart(string eventName);

		void EventStop(string eventName);

		void LogEventList();
	}
}
