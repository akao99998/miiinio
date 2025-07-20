using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CollectMarketplaceSaleCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CollectMarketplaceSaleCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public int slotId { get; set; }

		[Inject]
		public TransactionArg arg { get; set; }

		public override void Execute()
		{
			MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(slotId);
			if (byInstanceId == null)
			{
				return;
			}
			MarketplaceSaleItem byInstanceId2 = playerService.GetByInstanceId<MarketplaceSaleItem>(byInstanceId.itemId);
			if (byInstanceId2 == null)
			{
				return;
			}
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(byInstanceId2.Definition.TransactionID);
			TransactionDefinition transactionDefinition2 = transactionDefinition.CopyTransaction();
			foreach (QuantityItem output in transactionDefinition2.Outputs)
			{
				output.Quantity *= (uint)byInstanceId2.SalePrice;
			}
			playerService.RunEntireTransaction(transactionDefinition2, TransactionTarget.MARKETPLACE, TransactionCallback, arg);
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(slotId);
				if (byInstanceId != null)
				{
					MarketplaceSaleItem byInstanceId2 = playerService.GetByInstanceId<MarketplaceSaleItem>(byInstanceId.itemId);
					playerService.Remove(byInstanceId2);
					byInstanceId.itemId = 0;
					updateSaleSlot.Dispatch(byInstanceId.ID);
					updateSoldItemsSignal.Dispatch(true);
				}
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Failed to collect sold item reward");
			}
		}
	}
}
