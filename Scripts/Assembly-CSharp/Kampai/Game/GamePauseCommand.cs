using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GamePauseCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GamePauseCommand") as IKampaiLogger;

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public LogClientMetricsSignal clientMetricsSignal { get; set; }

		[Inject]
		public ReengageNotificationSignal reengageNotificationSignal { get; set; }

		[Inject]
		public ScheduleJobsCompleteNotificationsSignal scheduleJobsCompleteNotificationsSignal { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public CancelBuildingMovementSignal cancelBuildingMovementSignal { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IBackgroundDownloadDlcService backgroundDownloadDlcService { get; set; }

		public override void Execute()
		{
			logger.EventStart("GamePauseCommand.Execute");
			Native.LogInfo(string.Format("AppPause, GamePauseCommand, realtimeSinceStartup: {0}", timeService.RealtimeSinceStartup()));
			playerDurationService.OnAppPause();
			reengageNotificationSignal.Dispatch();
			scheduleJobsCompleteNotificationsSignal.Dispatch();
			currencyService.PauseTransactionsHandling();
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
			cancelBuildingMovementSignal.Dispatch(false);
			clientVersion.RemoveOverrideVersion();
			StopBackgroundDownloadDlc();
			clientHealthService.MarkTimerEvent("AppFlow.Pause", Time.time);
			clientMetricsSignal.Dispatch(false);
			telemetryService.Send_Telemetry_EVT_USER_DATA_AT_APP_CLOSE();
			logger.EventStop("GamePauseCommand.Execute");
		}

		private void StopBackgroundDownloadDlc()
		{
			if (!backgroundDownloadDlcService.Stopped)
			{
				backgroundDownloadDlcService.Stop();
			}
		}
	}
}
