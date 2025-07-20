using Kampai.Game;
using UnityEngine;

public static class MinionUtil
{
	public static int RushCost(int minionInstanceID, IPlayerService playerService, ITimeEventService timeEventService, IDefinitionService definitionService)
	{
		Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionInstanceID);
		if (byInstanceId.State != MinionState.Tasking && byInstanceId.State != MinionState.Leisure)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		RushActionType rushActionType = RushActionType.HARVESTING;
		Building byInstanceId2 = playerService.GetByInstanceId<Building>(byInstanceId.BuildingID);
		switch (byInstanceId.State)
		{
		case MinionState.Leisure:
		{
			LeisureBuilding leisureBuilding = byInstanceId2 as LeisureBuilding;
			num = leisureBuilding.Definition.LeisureTimeDuration;
			num2 = ((!timeEventService.HasEventID(leisureBuilding.ID)) ? num : timeEventService.GetTimeRemaining(leisureBuilding.ID));
			rushActionType = RushActionType.LEISURE;
			break;
		}
		case MinionState.Tasking:
		{
			VillainLairResourcePlot villainLairResourcePlot = byInstanceId2 as VillainLairResourcePlot;
			ResourceBuilding resourceBuilding = byInstanceId2 as ResourceBuilding;
			if (villainLairResourcePlot != null)
			{
				num = villainLairResourcePlot.parentLair.Definition.SecondsToHarvest;
				num2 = ((!timeEventService.HasEventID(villainLairResourcePlot.ID)) ? num : timeEventService.GetTimeRemaining(villainLairResourcePlot.ID));
			}
			else
			{
				if (resourceBuilding == null)
				{
					return -1;
				}
				num = BuildingUtil.GetHarvestTimeForTaskableBuilding(resourceBuilding, definitionService);
				num2 = timeEventService.GetTimeRemaining(minionInstanceID);
			}
			rushActionType = RushActionType.HARVESTING;
			break;
		}
		default:
			return -1;
		}
		return timeEventService.CalculateRushCostForTimer(Mathf.Min(num2, num), rushActionType);
	}
}
