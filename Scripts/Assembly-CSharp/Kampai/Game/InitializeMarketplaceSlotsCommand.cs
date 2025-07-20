using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class InitializeMarketplaceSlotsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("InitializeMarketplaceSlotsCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CreateMarketplaceSlotSignal createMarketplaceSlotSignal { get; set; }

		[Inject]
		public CreateLockedPremiumSlotSignal createLockedPremiumSlotSignal { get; set; }

		[Inject]
		public UpdateMarketplaceSlotStateSignal updateSlotStatesSignal { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			int num = marketplaceDefinition.StandardSlots + Convert.ToInt32(playerService.GetQuantity(StaticItem.MARKETPLACE_ADDITIONAL_SALE_SLOTS_ID));
			if (marketplaceDefinition == null)
			{
				logger.Warning("MarketplaceDefinition is null");
				return;
			}
			if (coppaService.Restricted())
			{
				num += marketplaceDefinition.FacebookSlots;
			}
			CreateSlots(MarketplaceSaleSlotDefinition.SlotType.DEFAULT, 1000008094, num);
			if (!coppaService.Restricted())
			{
				CreateSlots(MarketplaceSaleSlotDefinition.SlotType.FACEBOOK_UNLOCKABLE, 1000008095, marketplaceDefinition.FacebookSlots);
			}
			if (!coppaService.Restricted())
			{
				createLockedPremiumSlotSignal.Dispatch();
			}
			updateSlotStatesSignal.Dispatch();
			logger.Debug("InitializeMarketplaceSlotsCommand: Marketplace slots created.");
		}

		private void CreateSlots(MarketplaceSaleSlotDefinition.SlotType slotType, int slotDefinitionId, int slotCount)
		{
			ICollection<MarketplaceSaleSlot> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleSlot>(slotDefinitionId);
			while (byDefinitionId.Count < slotCount)
			{
				createMarketplaceSlotSignal.Dispatch(slotType);
				byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleSlot>(slotDefinitionId);
			}
		}
	}
}
