using System;
using System.Collections.Generic;
using Elevation.Logging;
using Elevation.Logging.Targets;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.Main
{
	public class KampaiFatalTarget : ILoggingTarget
	{
		private ICrossContextCapable _baseContext;

		private ILocalizationService _localService;

		private LogClientMetricsSignal _clientMetricsSignal;

		public string Name { get; set; }

		public LogLevel Level { get; set; }

		public KampaiFatalTarget(ICrossContextCapable baseContext, ILocalizationService localService, LogClientMetricsSignal clientMetricsSignal, string name)
		{
			Name = name;
			Level = LogLevel.Fatal;
			_baseContext = baseContext;
			_localService = localService;
			_clientMetricsSignal = clientMetricsSignal;
		}

		public void WriteLogEvent(LogEvent logEvent)
		{
			if (IsEnabled(logEvent))
			{
				Dictionary<string, object> data = logEvent.Data;
				FatalCode code = (FatalCode)(int)data["fatalCode"];
				int subcode = (int)data["referencedId"];
				object[] args = data["params"] as object[];
				string format = data["format"] as string;
				SendFatalTelemetry(code, subcode, format, args);
				ShowFatalView(code, subcode, args);
			}
		}

		private void ShowFatalView(FatalCode code, int subcode, params object[] args)
		{
			string code2 = string.Format("{0}-{1}", (int)code, subcode);
			string title;
			string @string;
			if (GracefulErrors.IsGracefulError(code))
			{
				GracefulMessage gracefulError = GracefulErrors.GetGracefulError(code);
				title = _localService.GetString(gracefulError.Title);
				@string = _localService.GetString(gracefulError.Description, args);
			}
			else if (code.IsNetworkError())
			{
				title = string.Format(_localService.GetString("FatalNetworkTitle"));
				@string = _localService.GetString("FatalNetworkMessage");
			}
			else
			{
				title = string.Format(_localService.GetString("FatalTitle"));
				@string = _localService.GetString("FatalMessage");
			}
			string playerID = string.Empty;
			IPlayerService instance = _baseContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance != null && instance.IsPlayerInitialized())
			{
				playerID = instance.ID.ToString();
			}
			FatalView.SetFatalText(code2, @string, title, playerID);
			_baseContext.injectionBinder.GetInstance<ReInitializeGameSignal>().Dispatch("Fatal");
		}

		private void SendFatalTelemetry(FatalCode code, int subcode, string format, params object[] args)
		{
			bool flag = false;
			try
			{
				string eventName = string.Format("AppFlow.Fatal.{0}", code);
				IClientHealthService instance = _baseContext.injectionBinder.CrossContextBinder.GetInstance<IClientHealthService>();
				if (instance != null)
				{
					instance.MarkMeterEvent(eventName);
					_clientMetricsSignal.Dispatch(true);
					flag = true;
				}
			}
			catch (Exception ex)
			{
				Native.LogError(string.Format("Unable to report fatal code metric: {0}", ex.Message));
				flag = false;
			}
			string text = Enum.GetName(typeof(FatalCode), code);
			if (string.IsNullOrEmpty(text))
			{
				text = "UNKNOWN";
			}
			string nameOfError = string.Format("{0}-{1}-{2}", text, (int)code, subcode);
			string errorDetails = (string.IsNullOrEmpty(format) ? string.Empty : ((args == null) ? string.Format("{0}, Userfacing: {1}", format, flag) : string.Format("{0}, {1}, Userfacing: {2}", format, args, flag)));
			try
			{
				ITelemetryService instance2 = _baseContext.injectionBinder.CrossContextBinder.GetInstance<ITelemetryService>();
				if (instance2 != null)
				{
					if (code.IsNetworkError())
					{
						instance2.Send_Telemetry_EVT_GAME_ERROR_CONNECTIVITY(nameOfError, errorDetails, true);
					}
					else
					{
						instance2.Send_Telemetry_EVT_GAME_ERROR_GAMEPLAY(nameOfError, errorDetails, true);
					}
				}
			}
			catch (Exception ex2)
			{
				Native.LogError(string.Format("Unable to log telemetry fatal code: {0}", ex2.Message));
			}
		}

		public void Flush()
		{
		}

		public bool IsEnabled(LogEvent logEvent)
		{
			return Level <= logEvent.Level;
		}

		public void UpdateConfig(Dictionary<string, object> config)
		{
		}
	}
}
