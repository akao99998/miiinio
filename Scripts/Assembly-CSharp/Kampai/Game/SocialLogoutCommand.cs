using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialLogoutCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialLogoutCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public UpdateMarketplaceSlotStateSignal updateSlotStateSignal { get; set; }

		public override void Execute()
		{
			SocialServices type = socialService.type;
			logger.Debug("Social Logout Command Called With {0}", type.ToString());
			socialService.Logout();
			switch (type)
			{
			case SocialServices.FACEBOOK:
			{
				string string2 = localService.GetString("fbLogoutSuccess");
				popupMessageSignal.Dispatch(string2, PopupMessageType.NORMAL);
				updateSlotStateSignal.Dispatch();
				break;
			}
			case SocialServices.GOOGLEPLAY:
			{
				string @string = localService.GetString("googleplaylogoutsuccess");
				popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
				break;
			}
			}
		}
	}
}
