using System.Collections.Generic;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StartTSMQuestTaskCommand : Command
	{
		[Inject]
		public Quest quest { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PanAndOpenModalSignal panAndOpenSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreSignal { get; set; }

		[Inject]
		public GoToNextQuestStateSignal goToNextQuestStateSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public RemoveQuestWorldIconSignal removeQuestWorldIconSignal { get; set; }

		public override void Execute()
		{
			int iD = quest.GetActiveDefinition().ID;
			IQuestController questControllerByDefinitionID = questService.GetQuestControllerByDefinitionID(iD);
			if (questControllerByDefinitionID.State == QuestState.Notstarted)
			{
				goToNextQuestStateSignal.Dispatch(iD);
			}
			if (questControllerByDefinitionID.StepCount > 0)
			{
				IQuestStepController stepController = questControllerByDefinitionID.GetStepController(0);
				if (stepController.StepState == QuestStepState.Notstarted)
				{
					stepController.GoToNextState();
				}
				if (questControllerByDefinitionID.StepCount == 1)
				{
					removeQuestWorldIconSignal.Dispatch(quest);
				}
			}
			IList<Instance> list = null;
			int num = 0;
			switch (quest.GetActiveDefinition().QuestSteps[0].Type)
			{
			default:
				return;
			case QuestStepType.OrderBoard:
				list = playerService.GetInstancesByDefinition<BlackMarketBoardDefinition>();
				num = 3022;
				break;
			case QuestStepType.MinionTask:
				num = quest.GetActiveDefinition().QuestSteps[0].ItemDefinitionID;
				list = playerService.GetInstancesByDefinitionID(num);
				break;
			case QuestStepType.BridgeRepair:
				return;
			}
			if (list.Count > 0)
			{
				Building building = FindPlacedBuilding(list);
				if (building != null)
				{
					panAndOpenSignal.Dispatch(list[0].ID, false);
				}
				else
				{
					uiContext.injectionBinder.GetInstance<CloseAllOtherMenuSignal>().Dispatch(null);
				}
			}
			else if (num != 0)
			{
				openStoreSignal.Dispatch(num, true);
			}
		}

		private Building FindPlacedBuilding(IList<Instance> instances)
		{
			foreach (Instance instance in instances)
			{
				Building building = instance as Building;
				if (building != null && building.State != BuildingState.Inventory)
				{
					return building;
				}
			}
			return null;
		}
	}
}
