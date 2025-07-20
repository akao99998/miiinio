using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DetermineLairUICommand : Command
	{
		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownAlertSignal displayAlertUISignal { get; set; }

		[Inject]
		public MasterPlanSelectComponentSignal selectComponentSignal { get; set; }

		[Inject]
		public DisplayMasterPlanSelectComponentSimpleSignal displaySelectSimpleSignal { get; set; }

		[Inject]
		public DisplayMasterPlanIntroDialogSignal displayMasterPlanIntroDialogSignal { get; set; }

		[Inject]
		public QuestHarvestableSignal questHarvestSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			questService.UpdateMasterPlanQuestLine();
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			MasterPlanDefinition definition = currentMasterPlan.Definition;
			MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(definition.BuildingDefID);
			if (currentMasterPlan.cooldownUTCStartTime > 0)
			{
				displayAlertUISignal.Dispatch(currentMasterPlan);
			}
			else if (!currentMasterPlan.introHasBeenDisplayed)
			{
				displayMasterPlanIntroDialogSignal.Dispatch();
			}
			else
			{
				bool allComplete;
				int selectedIndex;
				if (SelectOrHarvest(definition, out allComplete, out selectedIndex))
				{
					return;
				}
				if (allComplete && (firstInstanceByDefinitionId == null || firstInstanceByDefinitionId.State == BuildingState.Idle))
				{
					if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State == BuildingState.Idle)
					{
						Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(currentMasterPlan.ID);
						questHarvestSignal.Dispatch(questByInstanceId);
					}
					else
					{
						selectComponentSignal.Dispatch(definition, -1, true);
					}
				}
				else if (!allComplete || firstInstanceByDefinitionId == null)
				{
					if (selectedIndex != -1)
					{
						selectComponentSignal.Dispatch(definition, selectedIndex, false);
					}
					else
					{
						displaySelectSimpleSignal.Dispatch();
					}
				}
			}
		}

		private bool SelectOrHarvest(MasterPlanDefinition planDef, out bool allComplete, out int selectedIndex)
		{
			allComplete = true;
			selectedIndex = -1;
			for (int i = 0; i < planDef.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(planDef.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId != null)
				{
					if (firstInstanceByDefinitionId.State == MasterPlanComponentState.InProgress || firstInstanceByDefinitionId.State == MasterPlanComponentState.Scaffolding || firstInstanceByDefinitionId.State == MasterPlanComponentState.TasksCollected)
					{
						selectedIndex = i;
					}
					else
					{
						if (firstInstanceByDefinitionId.State == MasterPlanComponentState.TasksComplete)
						{
							Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(firstInstanceByDefinitionId.ID);
							questHarvestSignal.Dispatch(questByInstanceId);
							return true;
						}
						if (firstInstanceByDefinitionId.State == MasterPlanComponentState.Built)
						{
							Quest questByInstanceId2 = masterPlanQuestService.GetQuestByInstanceId(711);
							questHarvestSignal.Dispatch(questByInstanceId2);
							return true;
						}
					}
					if (firstInstanceByDefinitionId.State != MasterPlanComponentState.Complete)
					{
						allComplete = false;
					}
					continue;
				}
				displaySelectSimpleSignal.Dispatch();
				return true;
			}
			return false;
		}
	}
}
