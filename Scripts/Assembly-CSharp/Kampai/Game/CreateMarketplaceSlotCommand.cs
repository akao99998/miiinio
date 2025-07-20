using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateMarketplaceSlotCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreateMarketplaceSlotCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public MarketplaceSaleSlotDefinition.SlotType slotType { get; set; }

		public override void Execute()
		{
			int num = (int)(1000008094 + slotType);
			MarketplaceSaleSlotDefinition marketplaceSaleSlotDefinition = definitionService.Get<MarketplaceSaleSlotDefinition>(num);
			if (marketplaceSaleSlotDefinition == null)
			{
				logger.Error("Unable to get marketplace slot definition: {0}", num);
				return;
			}
			MarketplaceSaleSlot marketplaceSaleSlot = new MarketplaceSaleSlot(marketplaceSaleSlotDefinition);
			marketplaceSaleSlot.state = MarketplaceSaleSlot.State.UNLOCKED;
			if (slotType == MarketplaceSaleSlotDefinition.SlotType.FACEBOOK_UNLOCKABLE && !facebookService.isLoggedIn)
			{
				marketplaceSaleSlot.state = MarketplaceSaleSlot.State.LOCKED;
			}
			else if (slotType == MarketplaceSaleSlotDefinition.SlotType.PREMIUM_UNLOCKABLE)
			{
				marketplaceSaleSlot.state = MarketplaceSaleSlot.State.LOCKED;
				marketplaceSaleSlot.premiumCost = MarketplaceUtil.GetPremiumSlotCost(definitionService, playerService);
			}
			playerService.Add(marketplaceSaleSlot);
		}
	}
}
