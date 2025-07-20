namespace Kampai.Game.Mtx
{
	public interface IMtxReceiptValidationService
	{
		void AddPendingReceipt(string sku, string nimbleTransactionId, string platformStoreTransactionId, IMtxReceipt receipt);

		void ValidatePendingReceipt();

		void ValidationResultCallback(ReceiptValidationResult result);

		void RemovePendingReceipt(string nimbleTransactionId);

		bool HasPendingReceipts();
	}
}
