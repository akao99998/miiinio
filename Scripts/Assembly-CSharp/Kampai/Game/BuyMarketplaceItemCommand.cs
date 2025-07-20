using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class BuyMarketplaceItemCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("BuyMarketplaceItemCommand") as IKampaiLogger;

		private TransactionDefinition transaction;

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public MarketplaceBuyItem marketplaceItem { get; set; }

		[Inject]
		public int slotIndex { get; set; }

		[Inject]
		public UpdateBuySlotSignal updateBuySlot { get; set; }

		[Inject]
		public UpdateStorageItemsSignal updateStorageItemsSignal { get; set; }

		[Inject]
		public ReportMarketplaceTransactionSignal reportMarketplaceTransactionSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		public override void Execute()
		{
			if (marketplaceItem == null)
			{
				logger.Error(string.Format("Can't load marketplace buy item for slot index {0}", slotIndex));
				return;
			}
			transaction = new TransactionDefinition();
			transaction.Inputs = new List<QuantityItem>();
			transaction.Inputs.Add(new QuantityItem(0, (uint)marketplaceItem.BuyPrice));
			transaction.Outputs = new List<QuantityItem>();
			transaction.Outputs.Add(new QuantityItem(marketplaceItem.Definition.ItemID, (uint)marketplaceItem.BuyQuantity));
			playerService.RunEntireTransaction(transaction, TransactionTarget.MARKETPLACE, TransactionCallback, NewTransactionArg());
		}

		private TransactionArg NewTransactionArg()
		{
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.InstanceId = 314;
			transactionArg.Source = "Marketplace";
			return transactionArg;
		}

		private void TransactionCallback(PendingCurrencyTransaction pendingTransaction)
		{
			if (pendingTransaction.Success)
			{
				marketplaceItem.BoughtFlag = true;
				updateStorageItemsSignal.Dispatch();
				setGrindCurrencySignal.Dispatch();
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Complete);
				updateBuySlot.Dispatch(slotIndex, true);
				reportMarketplaceTransactionSignal.Dispatch(marketplaceItem);
				displaySignal.Dispatch(19000010, false, new Signal<bool>());
				return;
			}
			if (pendingTransaction.ParentSuccess)
			{
				playerService.RunEntireTransaction(transaction, TransactionTarget.MARKETPLACE, TransactionCallback, NewTransactionArg());
				return;
			}
			logger.Info(string.Format("Marketplace buy item transaction failed for itemID {0} in slot {1}", marketplaceItem.ID, slotIndex));
			if (pendingTransaction.FailReason == CurrencyTransactionFailReason.STORAGE)
			{
				updateBuySlot.Dispatch(slotIndex, false);
				StorageBuilding storageBuilding = GetStorageBuilding();
				if (storageBuilding.CurrentStorageBuildingLevel == storageBuilding.Definition.StorageUpgrades.Count - 1)
				{
					string @string = localizationService.GetString("MaxStorageCapacityReached");
					popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
				}
			}
		}

		private StorageBuilding GetStorageBuilding()
		{
			StorageBuilding result = null;
			using (IEnumerator<StorageBuilding> enumerator = playerService.GetByDefinitionId<StorageBuilding>(3018).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StorageBuilding current = enumerator.Current;
					result = current;
				}
			}
			return result;
		}
	}
}
