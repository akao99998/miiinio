using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BuildingCooldownCompleteCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			if (byInstanceId != null)
			{
				buildingChangeStateSignal.Dispatch(buildingID, BuildingState.Idle);
			}
		}
	}
}
