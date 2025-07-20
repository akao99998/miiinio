using System.Diagnostics;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.context.api;

namespace Kampai.Util
{
	public class KampaiLogger : IKampaiLogger
	{
		private KampaiLogLevel allowedLevel;

		[Inject]
		public LogToScreenSignal logToScreenSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(BaseElement.CONTEXT)]
		public ICrossContextCapable baseContext { get; set; }

		public virtual bool IsAllowedLevel(KampaiLogLevel level)
		{
			return allowedLevel <= level;
		}

		public virtual void SetAllowedLevel(int level)
		{
			allowedLevel = (KampaiLogLevel)level;
			Log(KampaiLogLevel.Info, "Set log level: {0}", allowedLevel);
		}

		public virtual void Log(KampaiLogLevel level, string format, params object[] args)
		{
			string text = string.Format(format, args);
			LogIt(level, text);
		}

		public virtual void Log(KampaiLogLevel level, string text)
		{
			LogIt(level, text);
		}

		public virtual void Log(KampaiLogLevel level, bool toScreen, string text)
		{
			if (toScreen)
			{
				logToScreenSignal.Dispatch(text);
			}
			LogIt(level, text);
		}

		public virtual void LogNullArgument()
		{
			Log(KampaiLogLevel.Error, "Null arguments");
		}

		public virtual void Verbose(string text)
		{
			Log(KampaiLogLevel.Verbose, text);
		}

		public virtual void Verbose(string format, params object[] args)
		{
			Log(KampaiLogLevel.Verbose, format, args);
		}

		public virtual void Debug(string text)
		{
			Log(KampaiLogLevel.Debug, text);
		}

		public virtual void Debug(string format, params object[] args)
		{
			Log(KampaiLogLevel.Debug, format, args);
		}

		public virtual void Info(string text)
		{
			Log(KampaiLogLevel.Info, text);
		}

		public virtual void Info(string format, params object[] args)
		{
			Log(KampaiLogLevel.Info, format, args);
		}

		public virtual void Warning(string text)
		{
			Log(KampaiLogLevel.Warning, text);
		}

		public virtual void Warning(string format, params object[] args)
		{
			Log(KampaiLogLevel.Warning, format, args);
		}

		public virtual void Error(string text)
		{
			Log(KampaiLogLevel.Error, text);
		}

		public virtual void Error(string format, params object[] args)
		{
			Log(KampaiLogLevel.Error, format, args);
		}

		public void EventStart(string eventName)
		{
			LogIt(KampaiLogLevel.Debug, string.Format("EventStart: {0}", eventName));
		}

		public void EventStop(string eventName)
		{
			LogIt(KampaiLogLevel.Debug, string.Format("EventStop: {0}", eventName));
		}

		public void LogEventList()
		{
		}

		protected virtual void LogIt(KampaiLogLevel level, string text, bool isFatal = false)
		{
			if (IsAllowedLevel(level))
			{
				switch (level)
				{
				case KampaiLogLevel.Info:
					Native.LogInfo(text);
					break;
				case KampaiLogLevel.Debug:
					Native.LogDebug(text);
					break;
				case KampaiLogLevel.Warning:
					Native.LogWarning(text);
					break;
				case KampaiLogLevel.Error:
					Native.LogError(text);
					break;
				default:
					Native.LogVerbose(text);
					break;
				}
			}
		}

		public void Fatal(FatalCode code, string format, params object[] args)
		{
			Fatal(code, 0, format, args);
		}

		public void FatalNoThrow(FatalCode code, string format, params object[] args)
		{
			FatalNoThrow(code, 0, format, args);
		}

		public virtual void FatalNoThrow(FatalCode code, int referencedId, string format, params object[] args)
		{
			string text = string.Format("[ERROR {0}-{1}] {2}", (int)code, referencedId, string.Format(format, args));
			string text2 = new StackTrace(1, true).ToString();
			LogIt(KampaiLogLevel.Error, text, true);
			LogIt(KampaiLogLevel.Error, text2, true);
			string code2 = string.Format("{0}-{1}", (int)code, referencedId);
			string title;
			string @string;
			if (GracefulErrors.IsGracefulError(code))
			{
				GracefulMessage gracefulError = GracefulErrors.GetGracefulError(code);
				title = localService.GetString(gracefulError.Title);
				@string = localService.GetString(gracefulError.Description, args);
			}
			else if (code.IsNetworkError())
			{
				title = string.Format(localService.GetString("FatalNetworkTitle"));
				@string = localService.GetString("FatalNetworkMessage");
			}
			else
			{
				title = string.Format(localService.GetString("FatalTitle"));
				@string = localService.GetString("FatalMessage");
			}
			string playerID = string.Empty;
			IPlayerService instance = baseContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance != null && instance.IsPlayerInitialized())
			{
				playerID = instance.ID.ToString();
			}
			FatalView.SetFatalText(code2, @string, title, playerID);
			baseContext.injectionBinder.GetInstance<ReInitializeGameSignal>().Dispatch("Fatal");
		}

		public void Fatal(FatalCode code, int referencedId, string format, params object[] args)
		{
			FatalNoThrow(code, referencedId, format, args);
			throw new FatalException(code, referencedId, format, args);
		}

		public void FatalNullArgument(FatalCode code)
		{
			Fatal(code, "Null argument");
		}

		public void Fatal(FatalCode code)
		{
			Fatal(code, code.ToString());
		}

		public void FatalNoThrow(FatalCode code)
		{
			FatalNoThrow(code, 0, code.ToString());
		}

		public void Fatal(FatalCode code, int referencedId)
		{
			Fatal(code, referencedId, string.Empty);
		}

		public void FatalNoThrow(FatalCode code, int referencedId)
		{
			FatalNoThrow(code, referencedId, string.Empty);
		}
	}
}
