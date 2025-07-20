using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.UI;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ReconcileLevelUnlocksCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		public override void Execute()
		{
			LevelUpDefinition levelUpDefinition = definitionService.Get<LevelUpDefinition>(88888);
			if (playerService.GetUnlockedQuantityOfID(0) == -1)
			{
				TransactionDefinition type = definitionService.Get<TransactionDefinition>(levelUpDefinition.transactionList[(int)playerService.GetQuantity(StaticItem.LEVEL_ID)]);
				awardLevelSignal.Dispatch(type);
				return;
			}
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Outputs = new List<QuantityItem>();
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			for (int i = 0; i <= quantity; i++)
			{
				if (i >= levelUpDefinition.transactionList.Count)
				{
					continue;
				}
				TransactionDefinition transactionDefinition2 = definitionService.Get<TransactionDefinition>(levelUpDefinition.transactionList[i]);
				foreach (QuantityItem output in transactionDefinition2.Outputs)
				{
					UnlockDefinition definition = null;
					if (definitionService.TryGet<UnlockDefinition>(output.ID, out definition))
					{
						int unlockedQuantityOfID = playerService.GetUnlockedQuantityOfID(definition.ReferencedDefinitionID);
						if (unlockedQuantityOfID < (int)output.Quantity)
						{
							transactionDefinition.Outputs.Add(output);
							AddNewUnlockToBuildMenu(definition.ReferencedDefinitionID);
						}
					}
				}
			}
			if (transactionDefinition.Outputs.Count > 0)
			{
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
			}
		}

		public void AddNewUnlockToBuildMenu(int buildingID)
		{
			BuildingDefinition definition;
			if (definitionService.TryGet<BuildingDefinition>(buildingID, out definition))
			{
				switch (definition.Type)
				{
				case BuildingType.BuildingTypeIdentifier.CRAFTING:
				case BuildingType.BuildingTypeIdentifier.DECORATION:
				case BuildingType.BuildingTypeIdentifier.LEISURE:
				case BuildingType.BuildingTypeIdentifier.RESOURCE:
				{
					int storeItemDefinitionIDFromBuildingID = buildMenuService.GetStoreItemDefinitionIDFromBuildingID(buildingID);
					StoreItemDefinition storeItemDefinition = definitionService.Get<StoreItemDefinition>(storeItemDefinitionIDFromBuildingID);
					buildMenuService.AddUncheckedInventoryItem(storeItemDefinition.Type, buildingID);
					break;
				}
				}
			}
		}
	}
}
