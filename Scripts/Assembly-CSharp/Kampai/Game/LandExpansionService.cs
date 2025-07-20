using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public class LandExpansionService : ILandExpansionService
	{
		private Dictionary<int, LandExpansionBuilding> byID = new Dictionary<int, LandExpansionBuilding>();

		private Dictionary<int, List<LandExpansionBuilding>> expansionMap = new Dictionary<int, List<LandExpansionBuilding>>();

		private Dictionary<int, List<GameObject>> flowerMap = new Dictionary<int, List<GameObject>>();

		private List<LandExpansionBuilding> flowerList = new List<LandExpansionBuilding>();

		private Dictionary<int, DebrisBuilding> debrisLookupMap = new Dictionary<int, DebrisBuilding>();

		private Dictionary<int, Building> aspirationalLookupMap = new Dictionary<int, Building>();

		private List<int> allExpansionIDs = new List<int>();

		private Dictionary<int, GameObject> forSaleSignMap = new Dictionary<int, GameObject>();

		public void AddBuilding(LandExpansionBuilding building)
		{
			AddToAllExpansionIDs(building.ExpansionID);
			AddToExpansionMap(building.ExpansionID, building);
			AddToIDMap(building);
		}

		public void TrackDebris(int debrisDefId, DebrisBuilding building)
		{
			debrisLookupMap.Add(debrisDefId, building);
		}

		public DebrisBuilding GetDebris(int debrisDefId)
		{
			return debrisLookupMap[debrisDefId];
		}

		public void TrackAspirationalBuilding(int aspirationalBuildingID, Building building)
		{
			if (!aspirationalLookupMap.ContainsKey(aspirationalBuildingID))
			{
				aspirationalLookupMap.Add(aspirationalBuildingID, building);
			}
		}

		public Building GetAspirationalBuilding(int aspirationalBuildingID)
		{
			Building value = null;
			aspirationalLookupMap.TryGetValue(aspirationalBuildingID, out value);
			return value;
		}

		public IList<Building> GetAllAspirationalBuildings()
		{
			return new List<Building>(aspirationalLookupMap.Values);
		}

		public IList<LandExpansionBuilding> GetAllExpansionBuildings()
		{
			return new List<LandExpansionBuilding>(byID.Values);
		}

		public IList<LandExpansionBuilding> GetBuildingsByExpansionID(int expansionID)
		{
			if (!expansionMap.ContainsKey(expansionID))
			{
				return new List<LandExpansionBuilding>();
			}
			return expansionMap[expansionID];
		}

		public LandExpansionBuilding GetBuildingByInstanceID(int builidngID)
		{
			return byID[builidngID];
		}

		public int GetExpansionByItemID(int itemID)
		{
			return byID[itemID].ExpansionID;
		}

		public int GetLandExpansionCount()
		{
			return allExpansionIDs.Count;
		}

		private void AddToIDMap(LandExpansionBuilding building)
		{
			byID[building.ID] = building;
		}

		private void AddToExpansionMap(int expansionID, LandExpansionBuilding building)
		{
			if (!expansionMap.ContainsKey(expansionID))
			{
				expansionMap[expansionID] = new List<LandExpansionBuilding>();
			}
			expansionMap[expansionID].Add(building);
		}

		private void AddToAllExpansionIDs(int expansionID)
		{
			if (!allExpansionIDs.Contains(expansionID))
			{
				allExpansionIDs.Add(expansionID);
			}
		}

		public IList<int> GetAllExpansionIDs()
		{
			return allExpansionIDs;
		}

		public void AddForSaleSign(int expansionID, GameObject sign)
		{
			if (!forSaleSignMap.ContainsKey(expansionID))
			{
				forSaleSignMap[expansionID] = sign;
			}
		}

		public bool HasForSaleSign(int expansionID)
		{
			return forSaleSignMap.ContainsKey(expansionID);
		}

		public GameObject GetForSaleSign(int expansionID)
		{
			if (!forSaleSignMap.ContainsKey(expansionID))
			{
				return null;
			}
			return forSaleSignMap[expansionID];
		}

		public void RemoveForSaleSign(int expansionID)
		{
			GameObject forSaleSign = GetForSaleSign(expansionID);
			if (!(forSaleSign == null))
			{
				Object.Destroy(forSaleSign);
				forSaleSignMap.Remove(expansionID);
			}
		}

		public void TrackFlower(LandExpansionBuilding building)
		{
			if (!flowerList.Contains(building))
			{
				flowerList.Add(building);
			}
		}

		public IList<LandExpansionBuilding> GetTrackedFlowers()
		{
			return flowerList;
		}

		public void AddToFlowerMap(int expansionID, GameObject flowerObject)
		{
			if (!flowerMap.ContainsKey(expansionID))
			{
				flowerMap[expansionID] = new List<GameObject>();
			}
			flowerMap[expansionID].Add(flowerObject);
		}

		public IList<GameObject> GetFlowersByExpansionID(int expansionID)
		{
			if (flowerMap.ContainsKey(expansionID))
			{
				return flowerMap[expansionID];
			}
			return null;
		}

		public void RemoveFlowersByExpansionID(int expansionID)
		{
			if (!flowerMap.ContainsKey(expansionID))
			{
				return;
			}
			foreach (GameObject item in flowerMap[expansionID])
			{
				Object.Destroy(item);
			}
			flowerMap[expansionID] = null;
		}

		public bool IsLevelGated(int expansionID, int playerLevel)
		{
			if (!expansionMap.ContainsKey(expansionID))
			{
				return false;
			}
			int minimumLevel = expansionMap[expansionID][0].MinimumLevel;
			return minimumLevel > playerLevel;
		}

		public bool ShouldUnlockThislevel(int expansionID, int playerLevel)
		{
			int minimumLevel = expansionMap[expansionID][0].MinimumLevel;
			return minimumLevel == playerLevel;
		}
	}
}
