using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Util;

public static class RewardUtil
{
	public static TransactionDefinition GetRewardTransaction(IDefinitionService definitionService, IPlayerService playerService, int playerLevel = -1)
	{
		LevelUpDefinition levelUpDefinition = definitionService.Get<LevelUpDefinition>(88888);
		int num = ((playerLevel >= 0) ? playerLevel : ((int)playerService.GetQuantity(StaticItem.LEVEL_ID)));
		TransactionDefinition transactionDefinition;
		if (num < levelUpDefinition.transactionList.Count)
		{
			transactionDefinition = definitionService.Get<TransactionDefinition>(levelUpDefinition.transactionList[num]);
		}
		else
		{
			transactionDefinition = new TransactionDefinition();
			transactionDefinition.Outputs = new List<QuantityItem>();
			transactionDefinition.Outputs.Add(new QuantityItem(1, 2u));
			transactionDefinition.Outputs.Add(new QuantityItem(21, 1u));
		}
		return transactionDefinition;
	}

	public static TransactionDefinition GetPartyTransaction(IDefinitionService definitionService, IPlayerService playerService, int playerLevel = -1)
	{
		int num = ((playerLevel >= 0) ? playerLevel : ((int)playerService.GetQuantity(StaticItem.LEVEL_ID)));
		LevelFunTable levelFunTable = definitionService.Get<LevelFunTable>();
		if (num >= levelFunTable.partiesNeededList.Count)
		{
			num = levelFunTable.partiesNeededList.Count - 1;
		}
		int num2 = (int)playerService.GetQuantity(StaticItem.LEVEL_PARTY_INDEX_ID);
		PartyUpDefinition partyUpDefinition = levelFunTable.partiesNeededList[num];
		if (num2 >= partyUpDefinition.PointsNeeded.Count)
		{
			num2 = partyUpDefinition.PointsNeeded.Count - 1;
		}
		TransactionDefinition transactionDefinition = partyUpDefinition.PartyTransaction.CopyTransaction();
		for (int i = 0; i < transactionDefinition.Outputs.Count; i++)
		{
			float num3 = partyUpDefinition.Multiplier * (float)num2;
			QuantityItem quantityItem = transactionDefinition.Outputs[i];
			quantityItem.Quantity += (uint)(num3 * (float)quantityItem.Quantity);
		}
		return transactionDefinition;
	}

	public static List<RewardQuantity> GetRewardQuantityFromTransaction(TransactionDefinition transaction, IDefinitionService definitionService, IPlayerService playerService)
	{
		List<RewardQuantity> list = new List<RewardQuantity>();
		foreach (QuantityItem output in transaction.Outputs)
		{
			if (output.ID == 0 || output.ID == 1 || output.ID == 5 || output.ID == 21)
			{
				list.Add(new RewardQuantity(output.ID, (int)output.Quantity, false, true));
			}
			else
			{
				if (output.ID == 6 || output.ID == 9 || output.ID == 1000012555)
				{
					continue;
				}
				bool isNew = false;
				UnlockDefinition definition;
				if (definitionService.TryGet<UnlockDefinition>(output.ID, out definition))
				{
					int unlockedQuantityOfID = playerService.GetUnlockedQuantityOfID(definition.ReferencedDefinitionID);
					if (!definition.Delta && unlockedQuantityOfID >= definition.UnlockedQuantity * (int)output.Quantity)
					{
						continue;
					}
					if (unlockedQuantityOfID == 0)
					{
						isNew = true;
					}
				}
				list.Add(new RewardQuantity(output.ID, (int)output.Quantity, isNew, false));
			}
		}
		return list;
	}
}
