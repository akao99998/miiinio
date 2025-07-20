using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelMarketplaceSaleCommand : Command
	{
		private MarketplaceSaleItem item;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public UpdateStorageItemsSignal updateStorageItemsSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlotSignal { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public int instanceId { get; set; }

		public override void Execute()
		{
			item = playerService.GetByInstanceId<MarketplaceSaleItem>(instanceId);
			if (item != null)
			{
				int saleCancellationCost = definitionService.Get<MarketplaceDefinition>().SaleCancellationCost;
				playerService.ProcessSaleCancel(saleCancellationCost, TransactionCallback);
			}
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				setPremiumCurrencySignal.Dispatch();
				MarketplaceSaleSlot slotByItem = marketplaceService.GetSlotByItem(item);
				playerService.Remove(item);
				timeEventService.RemoveEvent(instanceId);
				updateStorageItemsSignal.Dispatch();
				updateSaleSlotSignal.Dispatch(slotByItem.ID);
				updateSoldItemsSignal.Dispatch(true);
			}
		}
	}
}
