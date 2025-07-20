using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class RestoreLeisureBuildingCommand : Command
	{
		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeStateSignal { get; set; }

		[Inject]
		public StartLeisurePartyPointsSignal startLeisurePartyPointsSignal { get; set; }

		[Inject]
		public LeisureBuilding building { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		public override void Execute()
		{
			if (building.State == BuildingState.Harvestable)
			{
				harvestSignal.Dispatch(building.ID);
				return;
			}
			BuildingState buildingState = BuildingState.Inactive;
			int num = timeService.CurrentTime() - building.UTCLastTaskingTimeStarted;
			if (num > building.Definition.LeisureTimeDuration)
			{
				if (building.UTCLastTaskingTimeStarted != 0)
				{
					startLeisurePartyPointsSignal.Dispatch(building.ID);
					buildingState = BuildingState.Harvestable;
				}
				else
				{
					buildingState = BuildingState.Idle;
				}
			}
			else
			{
				buildingState = BuildingState.Working;
			}
			changeStateSignal.Dispatch(building.ID, buildingState);
		}
	}
}
