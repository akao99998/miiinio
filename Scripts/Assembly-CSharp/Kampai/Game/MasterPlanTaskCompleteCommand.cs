using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MasterPlanTaskCompleteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MasterPlanTaskCompleteCommand") as IKampaiLogger;

		[Inject]
		public MasterPlanComponent component { get; set; }

		[Inject]
		public int taskIndex { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal fxSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal updateStorageItemsSignal { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		[Inject]
		public UpdateQuestWorldIconsSignal updateQuestWorldIconsSignal { get; set; }

		public override void Execute()
		{
			MasterPlanComponentTask masterPlanComponentTask = component.tasks[taskIndex];
			if (masterPlanComponentTask.isHarvestable)
			{
				if (masterPlanComponentTask.Definition.Type == MasterPlanComponentTaskType.Deliver)
				{
					TransactionDefinition transactionDefinition = new TransactionDefinition();
					transactionDefinition.Inputs = new List<QuantityItem>();
					transactionDefinition.Outputs = new List<QuantityItem>();
					transactionDefinition.Inputs.Add(new QuantityItem(masterPlanComponentTask.Definition.requiredItemId, masterPlanComponentTask.Definition.requiredQuantity));
					TransactionArg transactionArg = new TransactionArg();
					transactionArg.IsFromQuestSource = 3;
					playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.AUTOMATIC, TaskCompleteTransactionCallback, transactionArg);
				}
				else
				{
					TaskComplete();
				}
			}
		}

		private void TaskCompleteTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				TaskComplete();
			}
		}

		private void TaskComplete()
		{
			MasterPlanComponentTask masterPlanComponentTask = component.tasks[taskIndex];
			masterPlanComponentTask.isComplete = true;
			fxSignal.Dispatch("Play_completePartQuest_01");
			updateStorageItemsSignal.Dispatch();
			bool flag = true;
			foreach (MasterPlanComponentTask task in component.tasks)
			{
				if (!task.isComplete)
				{
					flag = false;
				}
			}
			if (flag && component.State == MasterPlanComponentState.InProgress)
			{
				component.State = MasterPlanComponentState.TasksComplete;
			}
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(component.ID);
			if (questByInstanceId != null)
			{
				updateQuestWorldIconsSignal.Dispatch(questByInstanceId);
				SendTelemetry(masterPlanComponentTask);
			}
		}

		private void SendTelemetry(MasterPlanComponentTask task)
		{
			string requiredItem = string.Empty;
			switch (task.Definition.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
			case MasterPlanComponentTaskType.Collect:
				requiredItem = definitionService.Get<ItemDefinition>(task.Definition.requiredItemId).LocalizedKey;
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				requiredItem = "Complete Orders";
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
				requiredItem = ((task.Definition.requiredItemId != 0) ? definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey : "Any Mini Game");
				break;
			case MasterPlanComponentTaskType.MiniGameScore:
				requiredItem = definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey;
				break;
			case MasterPlanComponentTaskType.EarnPartyPoints:
				requiredItem = ((task.Definition.requiredItemId != 0) ? definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey : "Any Party Points");
				break;
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
				requiredItem = ((task.Definition.requiredItemId != 0) ? definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey : "Distractivity Party Points");
				break;
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
				requiredItem = ((task.Definition.requiredItemId != 0) ? definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey : "Mini Game Party Points");
				break;
			case MasterPlanComponentTaskType.EarnSandDollars:
				requiredItem = ((task.Definition.requiredItemId != 0) ? definitionService.Get<Definition>(task.Definition.requiredItemId).LocalizedKey : "Any Sand Dollars");
				break;
			default:
				logger.Warning("No task type of {0} defined for telemetry", task.Definition.Type);
				break;
			}
			string taxonomySpecific = definitionService.Get<MasterPlanComponentBuildingDefinition>(component.buildingDefID).TaxonomySpecific;
			telemetryService.Send_Telemetry_EVT_MASTER_PLAN_TASK_COMPLETE(taxonomySpecific, task.Definition.Type.ToString(), requiredItem, (int)task.Definition.requiredQuantity);
		}
	}
}
