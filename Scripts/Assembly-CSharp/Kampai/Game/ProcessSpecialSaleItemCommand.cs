using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class ProcessSpecialSaleItemCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public CleanupDebrisSignal cleanupDebrisSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public TransactionUpdateData update { get; set; }

		public override void Execute()
		{
			if (update == null || update.Outputs == null)
			{
				return;
			}
			foreach (QuantityItem output in update.Outputs)
			{
				switch (output.ID)
				{
				case 700:
					UnlockAllLandExpansions();
					break;
				case 701:
					ClearAllDebris();
					break;
				case 702:
					UpgradeStorageLevel((int)output.Quantity);
					break;
				}
			}
		}

		private void UnlockAllLandExpansions()
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Outputs = new List<QuantityItem>();
			transactionDefinition.ID = int.MaxValue;
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			IList<QuantityItem> outputs = transactionDefinition.Outputs;
			foreach (LandExpansionBuilding allExpansionBuilding in landExpansionService.GetAllExpansionBuildings())
			{
				LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(allExpansionBuilding.ExpansionID);
				QuantityItem item = new QuantityItem(expansionConfig.ID, 1u);
				if (!outputs.Contains(item) && !byInstanceId.HasPurchased(expansionConfig.expansionId))
				{
					outputs.Add(item);
				}
			}
			QuantityItemTriggerRewardDefinition quantityItemTriggerRewardDefinition = new QuantityItemTriggerRewardDefinition();
			quantityItemTriggerRewardDefinition.transaction = transactionDefinition.ToInstance();
			quantityItemTriggerRewardDefinition.RewardPlayer(gameContext);
		}

		private void ClearAllDebris()
		{
			List<DebrisBuilding> instancesByType = playerService.GetInstancesByType<DebrisBuilding>();
			foreach (DebrisBuilding item in instancesByType)
			{
				cleanupDebrisSignal.Dispatch(item.ID, false);
			}
		}

		private void UpgradeStorageLevel(int times)
		{
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
			byInstanceId.CurrentStorageBuildingLevel += times;
			int count = byInstanceId.Definition.StorageUpgrades.Count;
			byInstanceId.CurrentStorageBuildingLevel = ((byInstanceId.CurrentStorageBuildingLevel >= count) ? (count - 1) : byInstanceId.CurrentStorageBuildingLevel);
			setStorageSignal.Dispatch();
		}
	}
}
