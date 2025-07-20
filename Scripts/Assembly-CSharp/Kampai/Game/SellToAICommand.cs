using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SellToAICommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		[Inject]
		public MarketplaceItemSoldSignal marketplaceItemSoldSignal { get; set; }

		[Inject]
		public UpdateStorageItemsSignal updateStorageItemsSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public InterpolateSaleTimeSignal interpolateSaleTimeSignal { get; set; }

		[Inject]
		public Tuple<int, int, int, int> saleParameters { get; set; }

		[Inject]
		public ReportMarketplaceTransactionSignal reportMarketplaceTransactionSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		public override void Execute()
		{
			int item = saleParameters.Item1;
			int startTime = timeService.CurrentTime();
			MarketplaceItemDefinition marketplaceItemDefinition = definitionService.Get<MarketplaceItemDefinition>(item);
			MarketplaceSaleItem marketplaceSaleItem = CreateMarketplaceItem(startTime, marketplaceItemDefinition);
			if (localPersistService.GetData("ZeroSellTime") == "true")
			{
				marketplaceSaleItem.LengthOfSale = 0;
			}
			playerService.Add(marketplaceSaleItem);
			MarketplaceSaleSlot nextAvailableSlot = marketplaceService.GetNextAvailableSlot();
			nextAvailableSlot.itemId = marketplaceSaleItem.ID;
			updateSaleSlot.Dispatch(nextAvailableSlot.ID);
			updateSoldItemsSignal.Dispatch(true);
			timeEventService.AddEvent(marketplaceSaleItem.ID, startTime, marketplaceSaleItem.LengthOfSale, marketplaceItemSoldSignal);
			RemoveItemsFromInventory(marketplaceSaleItem);
			updateStorageItemsSignal.Dispatch();
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(marketplaceItemDefinition.ItemID);
			telemetryService.Send_Telemtry_EVT_MARKETPLACE_ITEM_LISTED(itemDefinition.Description, saleParameters.Item3, saleParameters.Item2, itemDefinition.TaxonomyHighLevel, itemDefinition.TaxonomySpecific, itemDefinition.TaxonomyType, itemDefinition.TaxonomyOther);
			reportMarketplaceTransactionSignal.Dispatch(marketplaceSaleItem);
		}

		private MarketplaceSaleItem CreateMarketplaceItem(int startTime, MarketplaceItemDefinition marketplaceItemDefinition)
		{
			MarketplaceSaleItem marketplaceSaleItem = new MarketplaceSaleItem(marketplaceItemDefinition);
			marketplaceSaleItem.SaleStartTime = startTime;
			marketplaceSaleItem.SalePrice = saleParameters.Item2;
			marketplaceSaleItem.QuantitySold = saleParameters.Item3;
			interpolateSaleTimeSignal.Dispatch(marketplaceSaleItem);
			return marketplaceSaleItem;
		}

		private void RemoveItemsFromInventory(MarketplaceSaleItem marketplaceItem)
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Inputs.Add(new QuantityItem(marketplaceItem.Definition.ItemID, (uint)marketplaceItem.QuantitySold));
			playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null, new TransactionArg
			{
				InstanceId = 314,
				Source = "Marketplace"
			});
		}
	}
}
