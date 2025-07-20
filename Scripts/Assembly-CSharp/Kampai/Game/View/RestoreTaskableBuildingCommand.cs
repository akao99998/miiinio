using System;
using System.Collections;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RestoreTaskableBuildingCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RestoreTaskableBuildingCommand") as IKampaiLogger;

		[Inject]
		public TaskableBuilding building { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeStateSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public override void Execute()
		{
			logger.Debug("Restoring a Tasking Building");
			BuildingState newState = BuildingState.Inactive;
			if (building is MignetteBuilding)
			{
				newState = BuildingState.Idle;
			}
			else
			{
				ResourceBuilding resourceBuilding = building as ResourceBuilding;
				if (building.GetMinionsInBuilding() > 0)
				{
					newState = BuildingState.Working;
				}
				else if (resourceBuilding != null && resourceBuilding.GetTotalHarvests() > 0)
				{
					newState = BuildingState.Harvestable;
					harvestSignal.Dispatch(building.ID);
				}
			}
			if (newState != 0)
			{
				routineRunner.StartCoroutine(WaitAFrame(delegate
				{
					changeStateSignal.Dispatch(building.ID, newState);
				}));
			}
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}
	}
}
