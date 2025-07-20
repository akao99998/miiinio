using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ConstructionCompleteCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public MasterPlanComponentCompleteSignal componentCompleteSignal { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			if (byInstanceId != null)
			{
				if (byInstanceId.Definition.ConstructionTime > 0 || byInstanceId is MasterPlanComponentBuilding)
				{
					buildingChangeStateSignal.Dispatch(buildingID, BuildingState.Complete);
				}
				else
				{
					buildingChangeStateSignal.Dispatch(buildingID, BuildingState.Idle);
				}
			}
			componentCompleteSignal.Dispatch(buildingID);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Construction);
		}
	}
}
