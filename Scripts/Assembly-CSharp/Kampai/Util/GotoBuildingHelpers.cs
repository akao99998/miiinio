using System;
using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.Util
{
	internal static class GotoBuildingHelpers
	{
		public static Building GetSuitableBuilding(ICollection<Building> playerBuildings)
		{
			Building building = null;
			foreach (Building playerBuilding in playerBuildings)
			{
				if (building == null)
				{
					building = playerBuilding;
				}
				if (playerBuilding.State != BuildingState.Inventory)
				{
					ResourceBuilding resourceBuilding = playerBuilding as ResourceBuilding;
					if (resourceBuilding != null && !resourceBuilding.AreAllMinionSlotsFilled())
					{
						return playerBuilding;
					}
					CraftingBuilding craftingBuilding = playerBuilding as CraftingBuilding;
					if (craftingBuilding != null && craftingBuilding.Slots > craftingBuilding.RecipeInQueue.Count)
					{
						return playerBuilding;
					}
					VillainLairResourcePlot villainLairResourcePlot = playerBuilding as VillainLairResourcePlot;
					if (villainLairResourcePlot != null)
					{
						return GetSuitableResourcePlot(playerBuildings);
					}
				}
			}
			return building;
		}

		public static VillainLairResourcePlot GetSuitableResourcePlot(ICollection<Building> playerPlotBuildings)
		{
			IList<Building> list = (IList<Building>)playerPlotBuildings;
			IList<int> list2 = new List<int>();
			IList<int> list3 = new List<int>();
			for (int i = 0; i < list.Count; i++)
			{
				VillainLairResourcePlot villainLairResourcePlot = list[i] as VillainLairResourcePlot;
				BuildingState state = villainLairResourcePlot.State;
				if (state != BuildingState.Inaccessible)
				{
					list2.Add(i);
					if (state == BuildingState.Idle)
					{
						list3.Add(i);
					}
				}
			}
			Random random = new Random();
			if (list3.Count > 0)
			{
				return list[list3[random.Next(0, list3.Count)]] as VillainLairResourcePlot;
			}
			if (list2.Count > 0)
			{
				return list[list2[random.Next(0, list2.Count)]] as VillainLairResourcePlot;
			}
			return list[random.Next(0, list.Count)] as VillainLairResourcePlot;
		}

		public static int GetSuitableMignette(IPlayerService playerService, IDefinitionService definitionService)
		{
			int num = 0;
			List<int> list = new List<int>();
			List<MignetteBuilding> list2 = new List<MignetteBuilding>();
			Random random = new Random();
			IList<AspirationalBuildingDefinition> all = definitionService.GetAll<AspirationalBuildingDefinition>();
			int i = 0;
			for (int count = all.Count; i < count; i++)
			{
				AspirationalBuildingDefinition aspirationalBuildingDefinition = all[i];
				Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(aspirationalBuildingDefinition.BuildingDefinitionID);
				if (firstInstanceByDefinitionId == null)
				{
					list.Add(aspirationalBuildingDefinition.BuildingDefinitionID);
					continue;
				}
				MignetteBuilding mignetteBuilding = firstInstanceByDefinitionId as MignetteBuilding;
				if (mignetteBuilding != null)
				{
					list2.Add(mignetteBuilding);
				}
			}
			if (list2.Count == 1)
			{
				return list2[0].Definition.ID;
			}
			List<int> list3 = new List<int>();
			foreach (MignetteBuilding item in list2)
			{
				if (item.State != BuildingState.Cooldown)
				{
					list3.Add(item.Definition.ID);
				}
			}
			if (list3.Count > 0)
			{
				return list3[random.Next(0, list3.Count)];
			}
			if (list2.Count > 0)
			{
				return list2[random.Next(0, list2.Count)].Definition.ID;
			}
			return list[random.Next(0, list.Count)];
		}

		public static bool BuildingMenuIsAccessible(Building building)
		{
			BuildingState state = building.State;
			return state != 0 && state != BuildingState.Complete && state != BuildingState.Construction;
		}

		public static bool BuildingLivesInsideLair(Building building)
		{
			if (building is VillainLairResourcePlot || building is MasterPlanComponentBuilding)
			{
				return true;
			}
			return false;
		}

		public static bool BuildingLivesInsideLair(BuildingDefinition buildingDef)
		{
			if (buildingDef.Type == BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT || buildingDef.Type == BuildingType.BuildingTypeIdentifier.MASTER_COMPONENT)
			{
				return true;
			}
			return false;
		}
	}
}
