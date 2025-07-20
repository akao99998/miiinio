using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class PurchaseSalePackCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PurchaseSalePackCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public StartPremiumPurchaseSignal startPremiumPurchaseSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreHighlightItemSignal { get; set; }

		[Inject]
		public InsufficientInputsSignal insufficientInputsSignal { get; set; }

		[Inject]
		public CollectRedemptionSignal collectRedemptionSignal { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public FinishPurchasingSalePackSignal finishPurchasingSalePackSignal { get; set; }

		[Inject]
		public RemoveSalePackSignal removeSalePackSignal { get; set; }

		[Inject]
		public CloseHUDSignal closeSignal { get; set; }

		[Inject]
		public int packDefinitionId { get; set; }

		public override void Execute()
		{
			logger.Debug("In Purchase Sale Command");
			PackDefinition packDefinition = definitionService.Get<PackDefinition>(packDefinitionId);
			if (packDefinition == null)
			{
				logger.Error("Unable to find the sale definition with id: {0}", packDefinitionId);
				return;
			}
			if (!packDefinition.DisableDynamicUnlock)
			{
				IncrementBuildingCount(packDefinition);
			}
			SalePackDefinition salePackDefinition = packDefinition as SalePackDefinition;
			if (PackUtil.HasPurchasedEnough(packDefinition, playerService))
			{
				logger.Error("Sale for definition id {0} already purchased.", packDefinitionId);
				if (salePackDefinition != null)
				{
					RemoveUpsellFromHUD(salePackDefinition);
				}
				return;
			}
			closeSignal.Dispatch(true);
			if (!NetworkUtil.IsConnected())
			{
				popupMessageSignal.Dispatch(localService.GetString("NoInternetConnection"), PopupMessageType.NORMAL);
			}
			else if (salePackDefinition != null && salePackDefinition.Type == SalePackType.Redeemable)
			{
				logger.Info("Attempting to collect Redeemable sku");
				collectRedemptionSignal.Dispatch();
			}
			else if (!string.IsNullOrEmpty(packDefinition.SKU) && packDefinition.TransactionType == UpsellTransactionType.Cash)
			{
				KampaiPendingTransaction kampaiPendingTransaction = new KampaiPendingTransaction();
				kampaiPendingTransaction.ExternalIdentifier = packDefinition.SKU;
				kampaiPendingTransaction.StoreItemDefinitionId = packDefinition.ID;
				kampaiPendingTransaction.TransactionInstance = packDefinition.TransactionDefinition;
				kampaiPendingTransaction.UTCTimeCreated = timeService.CurrentTime();
				startPremiumPurchaseSignal.Dispatch(kampaiPendingTransaction);
			}
			else if (packDefinition.TransactionDefinition != null)
			{
				PerformTransaction(packDefinition);
			}
		}

		private void PerformTransaction(PackDefinition definition)
		{
			TransactionDefinition transactionDefinition = definition.TransactionDefinition.ToDefinition().CopyTransaction();
			if (definition.TransactionType == UpsellTransactionType.GrindDiscount)
			{
				uint quantity = (uint)((float)TransactionUtil.GetTransactionCurrencyCost(transactionDefinition, definitionService, playerService, StaticItem.GRIND_CURRENCY_ID) * definition.getDiscountRate());
				QuantityItem item = new QuantityItem(0, quantity);
				transactionDefinition.Inputs.Add(item);
			}
			else if (definition.TransactionType == UpsellTransactionType.PremiumDiscount)
			{
				uint quantity2 = (uint)((float)TransactionUtil.GetTransactionCurrencyCost(transactionDefinition, definitionService, playerService, StaticItem.PREMIUM_CURRENCY_ID) * definition.getDiscountRate());
				QuantityItem item2 = new QuantityItem(1, quantity2);
				transactionDefinition.Inputs.Add(item2);
			}
			if (playerService.VerifyTransaction(transactionDefinition))
			{
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.CURRENCY, TransactionCallback, new TransactionArg("Upsell"));
				return;
			}
			PendingCurrencyTransaction pendingCurrencyTransaction = new PendingCurrencyTransaction(transactionDefinition, false, 0, null, null, InsufficientInputsCallback);
			currencyService.CurrencyDialogOpened(pendingCurrencyTransaction);
			insufficientInputsSignal.Dispatch(pendingCurrencyTransaction, true);
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				return;
			}
			PackDefinition packDefinition = definitionService.Get<PackDefinition>(packDefinitionId);
			if (packDefinition == null)
			{
				logger.Error("Unable to find the sale definition with id: {0}", packDefinitionId);
				return;
			}
			finishPurchasingSalePackSignal.Dispatch(packDefinitionId);
			foreach (QuantityItem output in packDefinition.TransactionDefinition.Outputs)
			{
				BuildingDefinition definition;
				if (definitionService.TryGet<BuildingDefinition>(output.ID, out definition))
				{
					updateStoreButtonsSignal.Dispatch(false);
					openStoreHighlightItemSignal.Dispatch(definition.ID, true);
					break;
				}
			}
		}

		private void IncrementBuildingCount(PackDefinition definition)
		{
			BuildingPacksHelper.UpdateTransactionUnlocksList(definition.TransactionDefinition, base.injectionBinder);
		}

		private void InsufficientInputsCallback(PendingCurrencyTransaction pct)
		{
		}

		private void RemoveUpsellFromHUD(SalePackDefinition salePackDefinition)
		{
			if (salePackDefinition == null)
			{
				logger.Error("Unable to find the sale definition with id: {0}", packDefinitionId);
				return;
			}
			Sale firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sale>(packDefinitionId);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Error("Sale instance not found for definition id {0}", packDefinitionId);
			}
			else
			{
				removeSalePackSignal.Dispatch(firstInstanceByDefinitionId.ID);
			}
		}
	}
}
