using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DeliverTaskItemCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DeliverTaskItemCommand") as IKampaiLogger;

		private TransactionDefinition currentTransactionDefinition;

		private Quest quest;

		private int questStepIndex;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public UpdateProceduralQuestPanelSignal updateSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public Tuple<int, int> tuple { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageCapacitySignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		public override void Execute()
		{
			int item = tuple.Item1;
			questStepIndex = tuple.Item2;
			quest = playerService.GetByInstanceId<Quest>(item);
			if (quest == null)
			{
				logger.Error("Quest instance doesn't exist for id {0}", item);
				return;
			}
			QuestDefinition activeDefinition = quest.GetActiveDefinition();
			QuestStepDefinition questStepDefinition = activeDefinition.QuestSteps[questStepIndex];
			QuestStep questStep = quest.Steps[questStepIndex];
			if (questStep.state == QuestStepState.WaitComplete)
			{
				quest.AutoGrantReward = true;
				int iD = activeDefinition.ID;
				IQuestStepController questStepController = questService.GetQuestStepController(iD, questStepIndex);
				questStepController.GoToNextState();
				if (quest.state == QuestState.RunningTasks)
				{
					globalSFXSignal.Dispatch("Play_completePartQuest_01");
				}
			}
			else if (questStepDefinition.Type == QuestStepType.Delivery || questStepDefinition.Type == QuestStepType.Harvest)
			{
				HandleDeliveryAndHarvestTask(questStepDefinition, activeDefinition);
			}
		}

		private void HandleDeliveryAndHarvestTask(QuestStepDefinition questStepDefinition, QuestDefinition questDefinition)
		{
			currentTransactionDefinition = new TransactionDefinition();
			currentTransactionDefinition.ID = quest.ID;
			currentTransactionDefinition.Inputs = new List<QuantityItem>();
			currentTransactionDefinition.Inputs.Add(new QuantityItem(questStepDefinition.ItemDefinitionID, (uint)quest.GetActiveDefinition().QuestSteps[questStepIndex].ItemAmount));
			IList<QuantityItem> missingItemListFromTransaction = playerService.GetMissingItemListFromTransaction(currentTransactionDefinition);
			if (missingItemListFromTransaction.Count != 0 && questDefinition.SurfaceType != QuestSurfaceType.ProcedurallyGenerated)
			{
				int rushCost = playerService.CalculateRushCost(missingItemListFromTransaction);
				playerService.ProcessRush(rushCost, missingItemListFromTransaction, true, RushItemCallBack, true);
				return;
			}
			if (questStepDefinition.Type == QuestStepType.Harvest)
			{
				currentTransactionDefinition.Outputs = new List<QuantityItem>();
				currentTransactionDefinition.Outputs.Add(new QuantityItem(questStepDefinition.ItemDefinitionID, (uint)quest.GetActiveDefinition().QuestSteps[questStepIndex].ItemAmount));
			}
			RunActualTransaction();
		}

		private void RushItemCallBack(PendingCurrencyTransaction pendingTransaction)
		{
			if (pendingTransaction.Success)
			{
				globalSFXSignal.Dispatch("Play_button_premium_01");
				RunActualTransaction();
			}
		}

		private void RunActualTransaction()
		{
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.IsFromQuestSource = ((!quest.IsDynamic()) ? 1 : 2);
			playerService.RunEntireTransaction(currentTransactionDefinition, TransactionTarget.INGREDIENT, transactionCallback, transactionArg);
		}

		private void transactionCallback(PendingCurrencyTransaction pendingTransaction)
		{
			if (!pendingTransaction.Success)
			{
				return;
			}
			setGrindCurrencySignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
			IList<QuantityItem> inputs = pendingTransaction.GetInputs();
			if (inputs.Count >= 1)
			{
				QuestStepDefinition questStepDefinition = quest.GetActiveDefinition().QuestSteps[questStepIndex];
				if (questStepDefinition.ItemDefinitionID == inputs[0].ID)
				{
					quest.AutoGrantReward = true;
					int iD = quest.GetActiveDefinition().ID;
					IQuestStepController questStepController = questService.GetQuestStepController(iD, questStepIndex);
					questStepController.GoToNextState(true);
				}
				if (questStepDefinition.Type == QuestStepType.Delivery)
				{
					questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Delivery);
				}
				else
				{
					questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest);
				}
				if (quest.state == QuestState.RunningTasks)
				{
					globalSFXSignal.Dispatch("Play_completePartQuest_01");
				}
				updateSignal.Dispatch(quest.ID);
				setStorageCapacitySignal.Dispatch();
			}
		}
	}
}
