using Kampai.Game.Transaction;

namespace Kampai.Common
{
	public interface IIapTelemetryService
	{
		void SendInAppPurchaseEventOnPurchaseComplete(IapTelemetryEvent iapTelemetryEvent);

		void SendInAppPurchaseEventOnProductDelivery(string sku, TransactionDefinition reward);
	}
}
