using System.Collections.Generic;
using Kampai.Game;
using Kampai.UI.View;

namespace Kampai.UI
{
	public interface IBuildMenuService
	{
		void SetStoreUnlockChecked();

		void AddNewUnlockedItem(StoreItemType type, int buildingDefinitionID);

		bool RemoveNewUnlockedItem(StoreItemType type, int buildingDefinitionID);

		void AddUncheckedInventoryItem(StoreItemType type, int buildingDefinitionID);

		void RemoveUncheckedInventoryItem(StoreItemType type, int buildingDefinitionID);

		void ClearTab(StoreItemType type);

		void RetoreBuidMenuState(Dictionary<StoreItemType, List<StoreButtonView>> buttonViews);

		void ClearAllNewUnlockItems();

		void UpdateNewUnlockList(Dictionary<StoreItemType, List<StoreButtonView>> buttonViews, bool updateBuildMenuButton = true, bool updateBadge = true);

		int GetStoreItemDefinitionIDFromBuildingID(int buildingID);

		bool ShouldRenderStoreDef(StoreItemDefinition storeDef);

		bool ShowingAChild(List<StoreButtonView> children, bool notifyShouldBeRendered = true);

		void CompleteBuildMenuUpdate(BuildingType.BuildingTypeIdentifier buildingDefType, int buildingDefinitionID);
	}
}
