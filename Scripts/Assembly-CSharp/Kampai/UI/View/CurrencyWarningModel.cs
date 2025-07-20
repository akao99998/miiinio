using Kampai.Game;

namespace Kampai.UI.View
{
	public class CurrencyWarningModel
	{
		public bool GrindFromPremium { get; private set; }

		public int Amount { get; private set; }

		public int Cost { get; private set; }

		public StoreItemType Type { get; private set; }

		public PendingCurrencyTransaction PendingTransaction { get; private set; }

		public CurrencyWarningModel(int amount, int cost, StoreItemType type, bool grindFromPremium = false, PendingCurrencyTransaction pendingTransaction = null)
		{
			GrindFromPremium = grindFromPremium;
			Amount = amount;
			Cost = cost;
			Type = type;
			PendingTransaction = pendingTransaction;
		}
	}
}
