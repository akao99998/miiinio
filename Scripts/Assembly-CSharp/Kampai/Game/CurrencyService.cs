using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Mtx;
using Kampai.Util;

namespace Kampai.Game
{
	public abstract class CurrencyService : ICurrencyService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CurrencyService") as IKampaiLogger;

		protected Stack<PendingCurrencyTransaction> pendingTransactions = new Stack<PendingCurrencyTransaction>();

		[Inject]
		public FinishPremiumPurchaseSignal finishPremiumPurchaseSignal { get; set; }

		[Inject]
		public CancelPremiumPurchaseSignal cancelPremiumPurchaseSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public abstract string GetPriceWithCurrencyAndFormat(string SKU);

		public abstract void RequestPurchase(KampaiPendingTransaction item);

		public abstract void ReceiptValidationCallback(ReceiptValidationResult result);

		public abstract void CollectRedemption(ReceiptValidationResult pendingRedemption);

		public abstract void PauseTransactionsHandling();

		public abstract void ResumeTransactionsHandling();

		public abstract void RestorePurchases();

		public void PurchaseCanceledCallback(string SKU, uint errorCode)
		{
			logger.Debug("[NCS] PurchaseCanceledCallback(): sku {0}, errorCode = {1}", SKU, errorCode);
			cancelPremiumPurchaseSignal.Dispatch(SKU, errorCode);
			CurrencyDialogClosed(false);
		}

		public void PurchaseSucceededAndValidatedCallback(string SKU)
		{
			finishPremiumPurchaseSignal.Dispatch(SKU);
		}

		public void PurchaseDeferredCallback(string SKU)
		{
			logger.Debug("[NCS] PurchaseDeferredCallback(): sku {0}", SKU);
			CurrencyDialogClosed(false);
		}

		public void CurrencyDialogClosed(bool success)
		{
			if (pendingTransactions.Count > 0)
			{
				PendingCurrencyTransaction pendingCurrencyTransaction = pendingTransactions.Pop();
				pendingCurrencyTransaction.ParentSuccess = success;
				pendingCurrencyTransaction.GetCallback()(pendingCurrencyTransaction);
			}
		}

		public void CurrencyDialogOpened(PendingCurrencyTransaction pendingTransaction)
		{
			pendingTransactions.Push(pendingTransaction);
		}

		public virtual void RefreshCatalog()
		{
		}

		public virtual bool TransactionProcessingEnabled()
		{
			return playerService.GetQuantity(StaticItem.LEVEL_ID) >= 1;
		}
	}
}
