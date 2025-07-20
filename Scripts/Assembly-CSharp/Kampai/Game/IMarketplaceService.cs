namespace Kampai.Game
{
	public interface IMarketplaceService
	{
		bool isServerKillSwitchEnabled { get; }

		bool IsDebugMode { get; set; }

		float DebugMultiplier { get; set; }

		int DebugSelectedItem { get; set; }

		bool DebugFacebook { get; set; }

		bool GetItemDefinitionByItemID(int itemID, out MarketplaceItemDefinition itemDefinition);

		MarketplaceSaleSlot GetSlotByItem(MarketplaceSaleItem item);

		MarketplaceSaleSlot GetNextAvailableSlot();

		int GetSlotIndex(MarketplaceSaleSlot slot);

		bool AreThereValidItemsInStorage();

		bool IsUnlocked();

		bool AreThereSoldItems();

		bool IsSlotVisible(MarketplaceSaleSlot slot);

		bool AreTherePendingItems();

		void SetMinStrikePrice(int itemID, int price);

		void SetMaxStrikePrice(int itemID, int price);
	}
}
