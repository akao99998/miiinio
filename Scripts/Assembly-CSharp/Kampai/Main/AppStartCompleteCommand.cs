using Elevation.Logging;
using Kampai.Game;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class AppStartCompleteCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("AppStartCompleteCommand") as IKampaiLogger;

		[Inject]
		public DisplayHindsightContentSignal displayHindsightContentSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public GameLoadedModel gameLoadedModel { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public DeviceInformation deviceInformation { get; set; }

		public override void Execute()
		{
			currencyService.RefreshCatalog();
			localPersistanceService.PutData("ExternalLinkOpened", "False");
			if (!localPersistanceService.GetData("SocialInProgress").Equals("True"))
			{
				displayHindsightContentSignal.Dispatch(HindsightCampaign.Scope.game_launch);
				localPersistanceService.PutData("HindsightTriggeredAtGameLaunch", "True");
			}
			else
			{
				localPersistanceService.PutData("HindsightTriggeredAtGameLaunch", "False");
			}
			showAllWayFindersSignal.Dispatch();
			gameLoadedModel.gameLoaded = true;
			Application.targetFrameRate = new DeviceCapabilities().GetTargetFrameRate(logger, Application.platform, deviceInformation);
			long num = TimeUtil.CurrentTimeMillis() - Native.GetAppStartupTime();
			logger.Log(KampaiLogLevel.Error, true, string.Format("App Started: {0}", ((float)num / 1000f).ToString()));
			gameLoadedModel.coldStartTime = num;
		}
	}
}
