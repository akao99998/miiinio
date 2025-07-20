using Kampai.Game.Transaction;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CollectTSMQuestTaskRewardCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public Quest quest { get; set; }

		[Inject]
		public UpdateTSMQuestTaskSignal updateTSMQuestTaskSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageCapacitySignal { get; set; }

		[Inject]
		public GoToNextQuestStateSignal goToNextQuestStateSignal { get; set; }

		public override void Execute()
		{
			DynamicQuestDefinition dynamicQuestDefinition = quest.GetActiveDefinition() as DynamicQuestDefinition;
			if (dynamicQuestDefinition != null)
			{
				TransactionDefinition reward = dynamicQuestDefinition.GetReward(definitionService);
				if (reward != null)
				{
					TransactionArg transactionArg = new TransactionArg();
					TaskTransactionArgument taskTransactionArgument = new TaskTransactionArgument();
					taskTransactionArgument.DropStep = dynamicQuestDefinition.DropStep;
					transactionArg.Add(taskTransactionArgument);
					transactionArg.InstanceId = quest.QuestIconTrackedInstanceId;
					transactionArg.Source = "TSMTrigger";
					playerService.RunEntireTransaction(reward, TransactionTarget.TASK_COMPLETE, transactionCallback, transactionArg);
				}
			}
		}

		private void transactionCallback(PendingCurrencyTransaction pendingTransaction)
		{
			if (pendingTransaction.Success)
			{
				goToNextQuestStateSignal.Dispatch(quest.GetActiveDefinition().ID);
				updateTSMQuestTaskSignal.Dispatch(quest, true);
				setStorageCapacitySignal.Dispatch();
			}
		}
	}
}
