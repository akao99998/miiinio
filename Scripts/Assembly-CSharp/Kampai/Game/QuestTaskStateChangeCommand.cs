using System.Collections.Generic;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class QuestTaskStateChangeCommand : Command
	{
		[Inject]
		public UpdateQuestWorldIconsSignal updateWorldIconSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public UpdateProceduralQuestPanelSignal updateSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public Quest quest { get; set; }

		[Inject]
		public int stepIndex { get; set; }

		[Inject]
		public QuestStepState previousState { get; set; }

		public override void Execute()
		{
			QuestStepState state = quest.Steps[stepIndex].state;
			int iD = quest.GetActiveDefinition().ID;
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(iD);
			QuestDefinition definition = questControllerByDefinitionID.Definition;
			if (previousState == QuestStepState.Notstarted && definition.QuestVersion != -1)
			{
				telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_STARTED_EAL(questService.GetEventName(quest.GetActiveDefinition().LocalizedKey), TelemetryAchievementType.QuestStep, string.Empty);
			}
			if (previousState == QuestStepState.Ready && state == QuestStepState.Inprogress)
			{
				updateWorldIconSignal.Dispatch(quest);
			}
			switch (state)
			{
			case QuestStepState.Ready:
				OnStepReady(definition);
				break;
			case QuestStepState.WaitComplete:
				questService.UnlockMinionParty(iD);
				break;
			case QuestStepState.Complete:
				OnStepComplete(questControllerByDefinitionID, iD);
				break;
			case QuestStepState.RunningCompleteScript:
				break;
			}
		}

		private void OnStepReady(QuestDefinition questDefinition)
		{
			IList<QuestStepDefinition> questSteps = questDefinition.QuestSteps;
			if (questDefinition.SurfaceType == QuestSurfaceType.ProcedurallyGenerated && questSteps != null && questSteps.Count > 0 && questSteps[0].Type != QuestStepType.OrderBoard)
			{
				updateSignal.Dispatch(quest.ID);
			}
			updateWorldIconSignal.Dispatch(quest);
		}

		private void OnStepComplete(IQuestController questController, int definitionID)
		{
			QuestStepDefinition questStepDefinition = quest.GetActiveDefinition().QuestSteps[stepIndex];
			displayPlayerTrainingSignal.Dispatch(questStepDefinition.QuestStepCompletePlayerTrainingCategoryItemId, false, new Signal<bool>());
			questController.CheckAndUpdateQuestCompleteState();
			if (questController.Definition.QuestVersion != -1)
			{
				telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(questService.GetEventName(quest.GetActiveDefinition().LocalizedKey), TelemetryAchievementType.QuestStep, 0, string.Empty);
			}
			questService.UnlockMinionParty(definitionID);
		}
	}
}
