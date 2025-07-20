using Kampai.Util;

namespace Kampai.Game
{
	public class PurchaseAwarePlayerService : PlayerService, IPurchaseRecorder
	{
		public void AddPurchasedCurrency(bool isPremium, uint quantity)
		{
			if (quantity != 0)
			{
				if (isPremium)
				{
					player.AlterQuantity(StaticItem.PREMIUM_PURCHASED_ID, (int)quantity);
				}
				else
				{
					player.AlterQuantity(StaticItem.GRIND_PURCHASED_ID, (int)quantity);
				}
			}
		}

		public bool CurrencySpent(bool isPremium, uint quantity)
		{
			if (quantity != 0)
			{
				if (isPremium)
				{
					return CurrencySpent(StaticItem.PREMIUM_PURCHASED_ID, quantity);
				}
				return CurrencySpent(StaticItem.GRIND_PURCHASED_ID, quantity);
			}
			return false;
		}

		private bool CurrencySpent(StaticItem item, uint quantity)
		{
			uint quantity2 = player.GetQuantity(item);
			if (quantity2 != 0)
			{
				int amount = (int)((quantity2 < quantity) ? (0 - quantity2) : (0 - quantity));
				player.AlterQuantity(item, amount);
				return true;
			}
			return false;
		}

		public static bool PurchasedCurrencyUsed(IKampaiLogger logger, IPlayerService playerService, bool isPremium, uint quantity)
		{
			IPurchaseRecorder purchaseRecorder = playerService as IPurchaseRecorder;
			if (purchaseRecorder == null)
			{
				logger.Error("Purchase recorder not available");
				return false;
			}
			return purchaseRecorder.CurrencySpent(isPremium, quantity);
		}
	}
}
