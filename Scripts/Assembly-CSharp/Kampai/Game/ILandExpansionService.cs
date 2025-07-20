using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public interface ILandExpansionService
	{
		void AddBuilding(LandExpansionBuilding building);

		void TrackDebris(int debrisDefId, DebrisBuilding building);

		DebrisBuilding GetDebris(int debrisDefId);

		void TrackAspirationalBuilding(int aspirationalBuildingID, Building building);

		Building GetAspirationalBuilding(int aspirationalBuildingID);

		IList<Building> GetAllAspirationalBuildings();

		IList<LandExpansionBuilding> GetAllExpansionBuildings();

		IList<LandExpansionBuilding> GetBuildingsByExpansionID(int expansionID);

		LandExpansionBuilding GetBuildingByInstanceID(int builidngID);

		int GetExpansionByItemID(int itemID);

		IList<int> GetAllExpansionIDs();

		void AddForSaleSign(int expansionID, GameObject sign);

		bool HasForSaleSign(int expansionID);

		void RemoveForSaleSign(int expansionID);

		GameObject GetForSaleSign(int expansionID);

		void TrackFlower(LandExpansionBuilding building);

		IList<LandExpansionBuilding> GetTrackedFlowers();

		void AddToFlowerMap(int expansionID, GameObject flowerObject);

		IList<GameObject> GetFlowersByExpansionID(int expansionID);

		void RemoveFlowersByExpansionID(int expansionID);

		int GetLandExpansionCount();

		bool IsLevelGated(int expansionID, int playerLevel);

		bool ShouldUnlockThislevel(int expansionID, int playerLevel);
	}
}
