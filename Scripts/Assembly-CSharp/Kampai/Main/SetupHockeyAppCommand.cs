using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class SetupHockeyAppCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("SetupHockeyAppCommand") as IKampaiLogger;

		private string HockeyAppId = GameConstants.StaticConfig.HOCKEY_APP_ID;

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject]
		public ILocalPersistanceService persistanceService { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			logger.EventStart("SetupHockeyAppCommand.Execute");
			GameObject gameObject = new GameObject("HockeyApp");
			gameObject.transform.parent = managers.transform;
			gameObject.SetActive(false);
			string userId = persistanceService.GetData("UserID");
			gameObject.name = "HockeyAppUnityAndroid";
			HockeyAppAndroid hockeyAppAndroid = gameObject.AddComponent<HockeyAppAndroid>();
			hockeyAppAndroid.packageID = Native.BundleIdentifier;
			hockeyAppAndroid.exceptionLogging = true;
			hockeyAppAndroid.appID = HockeyAppId;
			hockeyAppAndroid.userId = userId;
			hockeyAppAndroid.crashReportCallback = delegate
			{
				clientHealthService.MarkMeterEvent("AppFlow.Crash");
			};
			hockeyAppAndroid.autoUpload = true;
			hockeyAppAndroid.telemetryService = telemetryService;
			gameObject.SetActive(true);
			logger.EventStop("SetupHockeyAppCommand.Execute");
		}
	}
}
