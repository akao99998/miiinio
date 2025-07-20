using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.Controller
{
	public class AppQuitCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("AppQuitCommand") as IKampaiLogger;

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public LogClientMetricsSignal logClientMetricsSignal { get; set; }

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			logger.EventStart("AppQuitCommand.Execute");
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
			telemetryService.Send_Telemetry_EVT_USER_DATA_AT_APP_CLOSE();
			clientHealthService.MarkTimerEvent("AppFlow.Quit", Time.time);
			logClientMetricsSignal.Dispatch(false);
			MediaClient.Stop();
			logger.EventStop("AppQuitCommand.Execute");
		}
	}
}
