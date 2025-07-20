using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class UpdateMarketplaceSlotStateCommand : Command
	{
		[Inject(SocialServices.FACEBOOK)]
		public ISocialService socialService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			if (socialService.type != 0)
			{
				return;
			}
			MarketplaceSaleSlot.State state = ((socialService.isLoggedIn || marketplaceService.DebugFacebook) ? MarketplaceSaleSlot.State.UNLOCKED : MarketplaceSaleSlot.State.LOCKED);
			ICollection<MarketplaceSaleSlot> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleSlot>(1000008095);
			foreach (MarketplaceSaleSlot item in byDefinitionId)
			{
				item.state = state;
			}
		}
	}
}
