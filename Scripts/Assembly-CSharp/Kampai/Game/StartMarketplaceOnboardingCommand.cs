using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartMarketplaceOnboardingCommand : Command
	{
		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public DisplayFloatingTextSignal displayFloatingTextSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void Execute()
		{
			string @string = localizationService.GetString("MarketplaceOnboarding");
			FloatingTextSettings type = new FloatingTextSettings(buildingId, @string, 130f);
			displayFloatingTextSignal.Dispatch(type);
		}
	}
}
