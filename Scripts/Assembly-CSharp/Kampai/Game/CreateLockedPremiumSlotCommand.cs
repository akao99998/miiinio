using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateLockedPremiumSlotCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CreateMarketplaceSlotSignal createMarketplaceSlotSignal { get; set; }

		public override void Execute()
		{
			bool flag = false;
			ICollection<MarketplaceSaleSlot> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleSlot>(1000008096);
			foreach (MarketplaceSaleSlot item in byDefinitionId)
			{
				if (item.state == MarketplaceSaleSlot.State.LOCKED)
				{
					flag = true;
				}
			}
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			if (!flag && byDefinitionId.Count < marketplaceDefinition.MaxPremiumSlots)
			{
				createMarketplaceSlotSignal.Dispatch(MarketplaceSaleSlotDefinition.SlotType.PREMIUM_UNLOCKABLE);
			}
		}
	}
}
