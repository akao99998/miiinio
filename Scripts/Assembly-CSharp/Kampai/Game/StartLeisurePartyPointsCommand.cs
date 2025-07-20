using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartLeisurePartyPointsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartLeisurePartyPointsCommand") as IKampaiLogger;

		private LeisureBuilding building;

		private LeisureBuildingDefintiion definition;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateChangeSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public HarvestReadySignal harvestSignal { get; set; }

		[Inject]
		public int buildingId { get; set; }

		public override void Execute()
		{
			building = playerService.GetByInstanceId<LeisureBuilding>(buildingId);
			if (building == null)
			{
				logger.Error("Invalid leisure building with id {0}", buildingId);
				return;
			}
			definition = building.Definition;
			if (definition.PartyPointsReward <= 0)
			{
				logger.Error("Invalid party points reward");
				return;
			}
			buildingStateChangeSignal.Dispatch(building.ID, BuildingState.Working);
			timeEventService.AddEvent(building.ID, building.UTCLastTaskingTimeStarted, building.Definition.LeisureTimeDuration, harvestSignal, TimeEventType.LeisureBuff);
		}
	}
}
