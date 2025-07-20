using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialInitFailureCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialInitFailureCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public DisplayHindsightContentSignal displayHindsightContentSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public override void Execute()
		{
			logger.Log(KampaiLogLevel.Error, socialService.type.ToString() + " Init Failed");
			if (!localPersistanceService.GetData("HindsightTriggeredAtGameLaunch").Equals("True"))
			{
				displayHindsightContentSignal.Dispatch(HindsightCampaign.Scope.game_launch);
				localPersistanceService.PutData("HindsightTriggeredAtGameLaunch", "True");
			}
		}
	}
}
