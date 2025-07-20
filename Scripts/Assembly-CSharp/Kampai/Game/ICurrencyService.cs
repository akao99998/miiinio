using Kampai.Game.Mtx;

namespace Kampai.Game
{
	public interface ICurrencyService
	{
		string GetPriceWithCurrencyAndFormat(string SKU);

		void RequestPurchase(KampaiPendingTransaction item);

		void PurchaseCanceledCallback(string SKU, uint errorCode);

		void PurchaseDeferredCallback(string SKU);

		void PurchaseSucceededAndValidatedCallback(string SKU);

		void ReceiptValidationCallback(ReceiptValidationResult result);

		void CurrencyDialogClosed(bool success);

		void CurrencyDialogOpened(PendingCurrencyTransaction pendingTransaction);

		void PauseTransactionsHandling();

		void ResumeTransactionsHandling();

		void RestorePurchases();

		void CollectRedemption(ReceiptValidationResult pendingRedemption);

		void RefreshCatalog();

		bool TransactionProcessingEnabled();
	}
}
