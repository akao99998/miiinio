using Elevation.Logging;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialInitSuccessCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialInitSuccessCommand") as IKampaiLogger;

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public LinkAccountSignal linkAccountSignal { get; set; }

		[Inject]
		public IClientHealthService clientHealth { get; set; }

		[Inject]
		public DisplayHindsightContentSignal displayHindsightContentSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public UpdateMarketplaceSlotStateSignal updateMarketplaceSlotSignal { get; set; }

		public override void Execute()
		{
			SocialServices type = socialService.type;
			switch (type)
			{
			case SocialServices.FACEBOOK:
				clientHealth.MarkMeterEvent("External.Facebook.Login");
				break;
			case SocialServices.GAMECENTER:
				clientHealth.MarkMeterEvent("External.GameCenter.Login");
				break;
			case SocialServices.GOOGLEPLAY:
				clientHealth.MarkMeterEvent("External.Google.Login");
				break;
			}
			logger.Debug("In {0} Init Success", type.ToString());
			if (userSessionService.UserSession != null)
			{
				updateMarketplaceSlotSignal.Dispatch();
				CheckLoggedIn(type);
				if (!localPersistanceService.GetData("HindsightTriggeredAtGameLaunch").Equals("True"))
				{
					displayHindsightContentSignal.Dispatch(HindsightCampaign.Scope.game_launch);
					localPersistanceService.PutData("HindsightTriggeredAtGameLaunch", "True");
				}
			}
		}

		private void CheckLoggedIn(SocialServices socialType)
		{
			bool flag = false;
			if (!socialService.isLoggedIn)
			{
				return;
			}
			if (string.IsNullOrEmpty(socialService.LoginSource))
			{
				socialService.SendLoginTelemetry("Automatic");
			}
			else
			{
				socialService.SendLoginTelemetry(socialService.LoginSource);
			}
			logger.Debug("{0} Logged into looking into links", socialType.ToString());
			foreach (UserIdentity socialIdentity in userSessionService.UserSession.SocialIdentities)
			{
				if (socialIdentity.ExternalID == socialService.userID)
				{
					return;
				}
			}
			foreach (UserIdentity socialIdentity2 in userSessionService.UserSession.SocialIdentities)
			{
				if (socialIdentity2.Type.ToString().ToLower().Equals(socialType.ToString().ToLower()))
				{
					flag = true;
					if (socialIdentity2.ExternalID != socialService.userID)
					{
						LinkAccount(socialType);
					}
					return;
				}
			}
			if (!flag)
			{
				LinkAccount(socialType);
			}
		}

		private void LinkAccount(SocialServices socialType)
		{
			logger.Debug("Calling link from {0} Init", socialType.ToString());
			linkAccountSignal.Dispatch(socialService, false);
		}
	}
}
