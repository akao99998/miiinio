using Kampai.Game;

namespace Kampai.UI
{
	public interface ICurrencyStoreService
	{
		void Initialize();

		void MarkCategoryAsViewed(int categoryDefinitionID);

		void MarkCategoryAsViewed(CurrencyStoreCategoryDefinition currencyStoreCategoryDef);

		int GetBadgeCount(CurrencyStoreCategoryDefinition currencyStoreCategoryDef);

		CurrencyStorePackDefinition GetCurrencyStorePackDefinition(int packDefinitionId);

		bool ShouldPackBeVisuallyLocked(CurrencyStorePackDefinition currencyStorePackDefinition);

		bool HasPurchasedEnough(CurrencyStorePackDefinition currencyStorePackDefinition);

		bool IsValidCurrencyItem(int storeItemDefinitionID, StoreCategoryType type, bool countInLocked = true);
	}
}
