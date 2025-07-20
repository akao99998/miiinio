using System.Collections.Generic;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MasterPlanRushTaskCommand : Command
	{
		[Inject]
		public MasterPlanComponent component { get; set; }

		[Inject]
		public int taskIndex { get; set; }

		[Inject]
		public MasterPlanTaskCompleteSignal taskCompleteSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void Execute()
		{
			MasterPlanComponentTask masterPlanComponentTask = component.tasks[taskIndex];
			MasterPlanComponentTaskType type = masterPlanComponentTask.Definition.Type;
			if (type != MasterPlanComponentTaskType.CompleteOrders && type != MasterPlanComponentTaskType.EarnSandDollars && type != MasterPlanComponentTaskType.MiniGameScore)
			{
				IList<QuantityItem> missingItemList = MasterPlanUtil.GetMissingItemList(masterPlanComponentTask);
				int rushCost = playerService.CalculateRushCost(missingItemList);
				if (masterPlanComponentTask.Definition.Type == MasterPlanComponentTaskType.Deliver)
				{
					playerService.ProcessRush(rushCost, missingItemList, true, "MasterPlan", RushTransactionCallback, true);
				}
				else
				{
					playerService.ProcessRush(rushCost, null, true, "MasterPlan", RushTransactionCallback, true);
				}
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				MasterPlanComponentTask masterPlanComponentTask = component.tasks[taskIndex];
				masterPlanComponentTask.earnedQuantity = masterPlanComponentTask.Definition.requiredQuantity;
				taskCompleteSignal.Dispatch(component, taskIndex);
				setPremiumCurrencySignal.Dispatch();
				soundFXSignal.Dispatch("Play_button_premium_01");
			}
		}
	}
}
