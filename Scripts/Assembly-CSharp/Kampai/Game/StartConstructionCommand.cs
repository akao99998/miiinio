using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartConstructionCommand : Command
	{
		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public bool restoreTimer { get; set; }

		[Inject]
		public ConstructionCompleteSignal constructionCompleteSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public DisplayWorldProgressSignal worldProgressSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingId);
			BuildingDefinition definition = byInstanceId.Definition;
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Construction);
			if (definition.ConstructionTime <= 0)
			{
				constructionCompleteSignal.Dispatch(buildingId);
				return;
			}
			int startTime = ((!restoreTimer) ? timeService.CurrentTime() : byInstanceId.StateStartTime);
			int num = definition.ConstructionTime;
			if (definition.IncrementalConstructionTime > 0)
			{
				num += (byInstanceId.BuildingNumber - 1) * definition.IncrementalConstructionTime;
			}
			ProgressBarSettings progressBarSettings = new ProgressBarSettings(buildingId, constructionCompleteSignal, startTime, num);
			worldProgressSignal.Dispatch(progressBarSettings);
			timeEventService.AddEvent(buildingId, progressBarSettings.StartTime, progressBarSettings.Duration, constructionCompleteSignal);
			if (byInstanceId.State != BuildingState.Construction)
			{
				buildingChangeStateSignal.Dispatch(buildingId, BuildingState.Construction);
			}
		}
	}
}
