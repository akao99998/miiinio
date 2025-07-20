using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using Swrve;
using Swrve.Messaging;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class SetupSwrveCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupSwrveCommand") as IKampaiLogger;

		private static bool swrveVerboseLogEnabled = true;

		[Inject]
		public string kampaiUserID { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ISwrveService swrveService { get; set; }

		[Inject]
		public ABTestSignal abTestSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ABTestResourcesUpdatedSignal abTestResourcesUpdatedSignal { get; set; }

		public override void Execute()
		{
			if (configurationsService.isKillSwitchOn(KillSwitch.SWRVE))
			{
				logger.Info("Swrve is disabled by kill switch.");
				MoveForwardWithoutSwrve();
				return;
			}
			if (!telemetryService.SharingUsageEnabled())
			{
				logger.Info("Swrve is disabled because sharing usage is disabled");
				MoveForwardWithoutSwrve();
				return;
			}
			abTestSignal.AddOnce(SetSWRVEData);
			playerService.SWRVEGroup = ABTestModel.definitionVariants;
			GameObject gameObject = new GameObject("SwrvePrefab");
			gameObject.transform.parent = managers.transform;
			logger.Debug("SetupSwrveCommand:Execute swrveObject added to scene");
			Init(gameObject);
			swrveService.UpdateResources();
			telemetryService.AddTelemetrySender(swrveService);
			telemetryService.AddIapTelemetryService(swrveService);
			swrveService.SharingUsage(telemetryService.SharingUsageEnabled());
			swrveService.COPPACompliance();
			logger.Debug("SetupSwrveCommand.Execute SWRVEGroup: {0}", playerService.SWRVEGroup);
		}

		private void SetSWRVEData(ABTestCommand.GameMetaData dataArgs)
		{
			playerService.SWRVEGroup = dataArgs.definitionVariants;
		}

		private void MoveForwardWithoutSwrve()
		{
			abTestResourcesUpdatedSignal.Dispatch(false);
		}

		private void Init(GameObject swrveObject)
		{
			SwrveComponent swrveComponent = swrveObject.AddComponent<SwrveComponent>();
			logger.Debug("Swrve: swrveAppIdType: {0}", GameConstants.StaticConfig.ENVIRONMENT);
			int result;
			if (!int.TryParse(GameConstants.StaticConfig.SWRVE_APP_ID, out result))
			{
				logger.Error("Invalid GameConstants.StaticConfig.SWRVE_APP_ID");
			}
			swrveComponent.FlushEventsOnApplicationQuit = true;
			swrveComponent.InitialiseOnStart = false;
			SwrveConfig config = swrveComponent.Config;
			config.PushNotificationEnabled = false;
			config.UserId = kampaiUserID;
			config.Orientation = SwrveOrientation.Landscape;
			config.AutoDownloadCampaignsAndResources = false;
			InitSwrveLog();
			swrveComponent.Init(result, GameConstants.StaticConfig.SWRVE_API_KEY);
			logger.Debug("SetupSwrveCommand:Init SWRVE GameObject initialized");
		}

		private void InitSwrveLog()
		{
			SwrveLog.Verbose = swrveVerboseLogEnabled;
			SwrveLog.OnLog = OnSwrveLog;
		}

		private void OnSwrveLog(SwrveLog.SwrveLogType type, object message, string tag)
		{
			logger.Log(Convert(type), "Swrve: [{0}] {1}", tag, message);
		}

		private static KampaiLogLevel Convert(SwrveLog.SwrveLogType type)
		{
			switch (type)
			{
			case SwrveLog.SwrveLogType.Info:
				return KampaiLogLevel.Info;
			case SwrveLog.SwrveLogType.Warning:
				return KampaiLogLevel.Warning;
			case SwrveLog.SwrveLogType.Error:
				return KampaiLogLevel.Error;
			default:
				return KampaiLogLevel.Debug;
			}
		}
	}
}
