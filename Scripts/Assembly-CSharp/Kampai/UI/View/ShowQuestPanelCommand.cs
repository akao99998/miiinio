using Kampai.Game;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class ShowQuestPanelCommand : Command
	{
		[Inject]
		public int questInstanceID { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeState { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void Execute()
		{
			IQuestController questControllerByInstanceID = questService.GetQuestControllerByInstanceID(questInstanceID);
			QuestState state = questControllerByInstanceID.State;
			if (state == QuestState.RunningTasks || state == QuestState.RunningCompleteScript || state == QuestState.Harvestable)
			{
				CreateQuestPanel();
			}
			if (state == QuestState.Notstarted || state == QuestState.RunningStartScript)
			{
				for (int i = 0; i < questControllerByInstanceID.StepCount; i++)
				{
					IQuestStepController stepController = questControllerByInstanceID.GetStepController(i);
					if (stepController.StepType == QuestStepType.BridgeRepair)
					{
						changeState.Dispatch(stepController.StepInstanceTrackedID, BuildingState.Working);
					}
				}
				gameContext.injectionBinder.GetInstance<GoToNextQuestStateSignal>().Dispatch(questControllerByInstanceID.Definition.ID);
			}
			questControllerByInstanceID.UpdateTask(QuestStepType.Delivery);
			questControllerByInstanceID.UpdateTask(QuestStepType.ThrowParty);
			questControllerByInstanceID.UpdateTask(QuestStepType.Harvest);
		}

		private void CreateQuestPanel()
		{
			gameContext.injectionBinder.GetInstance<CancelBuildingMovementSignal>().Dispatch(false);
			showHUDSignal.Dispatch(true);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_QuestPanel");
			iGUICommand.skrimScreen = "QuestPanelSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.singleSkrimClose = true;
			iGUICommand.Args.Add(questInstanceID);
			guiService.Execute(iGUICommand);
		}
	}
}
