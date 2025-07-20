using Kampai.UI;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MasterPlanSelectComponentCommand : Command
	{
		[Inject]
		public MasterPlanDefinition masterPlanDefinition { get; set; }

		[Inject]
		public int componentIndex { get; set; }

		[Inject]
		public bool showPlan { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllMessageDialogsSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		[Inject]
		public EnableOneVillainLairColliderSignal enableOneVillainLairColliderSignal { get; set; }

		public override void Execute()
		{
			closeAllMessageDialogsSignal.Dispatch();
			int num = 0;
			for (int i = 0; i < masterPlanDefinition.ComponentDefinitionIDs.Count; i++)
			{
				int definitionId = masterPlanDefinition.ComponentDefinitionIDs[i];
				int type = masterPlanDefinition.CompBuildingDefinitionIDs[i];
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(definitionId);
				if (i == componentIndex)
				{
					if (firstInstanceByDefinitionId.State == MasterPlanComponentState.NotStarted)
					{
						firstInstanceByDefinitionId.State = MasterPlanComponentState.InProgress;
					}
					num = ((firstInstanceByDefinitionId.State < MasterPlanComponentState.TasksCollected) ? firstInstanceByDefinitionId.ID : 711);
					masterPlanService.SelectMasterPlanComponent(firstInstanceByDefinitionId);
					ghostService.DisplayComponentMarkedAsInProgress(firstInstanceByDefinitionId);
					enableOneVillainLairColliderSignal.Dispatch(false, type);
				}
			}
			if (num != 0 || showPlan)
			{
				MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
				CreateQuestPanel((!showPlan) ? num : currentMasterPlan.ID);
			}
		}

		private void CreateQuestPanel(int id)
		{
			gameContext.injectionBinder.GetInstance<CancelBuildingMovementSignal>().Dispatch(false);
			showHUDSignal.Dispatch(true);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_QuestPanel");
			iGUICommand.skrimScreen = "QuestPanelSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.singleSkrimClose = true;
			iGUICommand.Args.Add(id);
			guiService.Execute(iGUICommand);
		}
	}
}
