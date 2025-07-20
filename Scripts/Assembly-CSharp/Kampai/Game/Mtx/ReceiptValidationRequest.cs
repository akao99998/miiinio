using Newtonsoft.Json;

namespace Kampai.Game.Mtx
{
	public class ReceiptValidationRequest
	{
		public string sku;

		[JsonProperty("mtxTransactionId")]
		public string nimbleTransactionId;

		public string platformStoreTransactionId = string.Empty;

		public IMtxReceipt receipt;

		public ReceiptValidationRequest(string sku, string nimbleTransactionId, string platformStoreTransactionId, IMtxReceipt receipt)
		{
			this.sku = sku;
			this.nimbleTransactionId = nimbleTransactionId;
			this.platformStoreTransactionId = platformStoreTransactionId;
			this.receipt = receipt;
		}
	}
}
