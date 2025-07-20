using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class GoToNextQuestStateCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GoToNextQuestStateCommand") as IKampaiLogger;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IQuestScriptService questScriptService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public StartTimedQuestSignal startTimedQuestSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal showQuestPanelSignal { get; set; }

		[Inject]
		public int questDefinitionID { get; set; }

		public override void Execute()
		{
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(questDefinitionID);
			if (questControllerByDefinitionID != null)
			{
				GotoNextQuestState(questControllerByDefinitionID);
				return;
			}
			logger.Error("Quest Controller with def Id {0} doesn't exist in quest map", questDefinitionID);
		}

		public void GotoNextQuestState(IQuestController questController)
		{
			switch (questController.State)
			{
			case QuestState.Notstarted:
				EventsForQuestStart(questController);
				CheckRunningStartScriptState(questController);
				break;
			case QuestState.RunningStartScript:
				CheckRunningTasksState(questController);
				break;
			case QuestState.RunningTasks:
				CheckRunningCompleteScriptState(questController);
				break;
			case QuestState.RunningCompleteScript:
				CheckHarvestableState(questController);
				break;
			case QuestState.Harvestable:
				questController.GoToQuestState(QuestState.Complete);
				break;
			}
		}

		private void CheckRunningStartScriptState(IQuestController questController)
		{
			if (!questScriptService.HasScript(questController.ID, true))
			{
				CheckRunningTasksState(questController);
			}
			else
			{
				questController.GoToQuestState(QuestState.RunningStartScript);
			}
		}

		private void CheckRunningTasksState(IQuestController questController)
		{
			if (questController.StepCount == 0)
			{
				CheckRunningCompleteScriptState(questController);
				return;
			}
			questController.GoToQuestState(QuestState.RunningTasks);
			if (questController.Definition.SurfaceType != QuestSurfaceType.ProcedurallyGenerated)
			{
				showQuestPanelSignal.Dispatch(questController.ID);
			}
		}

		private void CheckRunningCompleteScriptState(IQuestController questController)
		{
			if (!questScriptService.HasScript(questController.ID, false))
			{
				CheckHarvestableState(questController);
			}
			else
			{
				questController.GoToQuestState(QuestState.RunningCompleteScript);
			}
		}

		private void CheckHarvestableState(IQuestController questController)
		{
			if (questController.Definition.GetReward(definitionService) != null)
			{
				questController.GoToQuestState(QuestState.Harvestable);
			}
			else
			{
				questController.GoToQuestState(QuestState.Complete);
			}
		}

		private void EventsForQuestStart(IQuestController questController)
		{
			string questGiver = string.Empty;
			QuestDefinition definition = questController.Definition;
			if (definition.SurfaceType == QuestSurfaceType.Character)
			{
				Prestige prestige = prestigeService.GetPrestige(definition.SurfaceID, false);
				if (prestige != null)
				{
					questGiver = prestige.Definition.LocalizedKey;
				}
			}
			if (definition.QuestVersion != -1)
			{
				telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_STARTED_EAL(questService.GetEventName(definition.LocalizedKey), TelemetryAchievementType.Quest, questGiver);
			}
			if (questController.Definition.SurfaceType == QuestSurfaceType.TimedEvent)
			{
				startTimedQuestSignal.Dispatch(questController.ID);
			}
		}
	}
}
