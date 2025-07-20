using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class PurchaseMarketplaceSlotCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PurchaseMarketplaceSlotCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CreateLockedPremiumSlotSignal createPremiumSlotSignal { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		[Inject]
		public RefreshSlotsSignal refreshSlotsSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public int slotId { get; set; }

		public override void Execute()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			int count = playerService.GetByDefinitionId<MarketplaceSaleSlot>(1000008096).Count;
			if (count == 0 || count > marketplaceDefinition.MaxPremiumSlots)
			{
				logger.Error("Invalid number of premium slots");
				return;
			}
			MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(slotId);
			if (byInstanceId.Definition.type != MarketplaceSaleSlotDefinition.SlotType.PREMIUM_UNLOCKABLE)
			{
				logger.Error("Slot is not premium unlockable");
			}
			else
			{
				playerService.ProcessSlotPurchase(byInstanceId.premiumCost, true, count - 1, PurchaseSlotCallback, 1000008096);
			}
		}

		private void PurchaseSlotCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				setPremiumCurrencySignal.Dispatch();
				MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(slotId);
				byInstanceId.state = MarketplaceSaleSlot.State.UNLOCKED;
				updateSaleSlot.Dispatch(byInstanceId.ID);
				globalSFXSignal.Dispatch("Play_button_premium_01");
				displaySignal.Dispatch(19000012, false, new Signal<bool>());
				MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
				int count = playerService.GetByDefinitionId<MarketplaceSaleSlot>(1000008096).Count;
				if (count < marketplaceDefinition.MaxPremiumSlots)
				{
					createPremiumSlotSignal.Dispatch();
					refreshSlotsSignal.Dispatch(true);
				}
			}
		}
	}
}
