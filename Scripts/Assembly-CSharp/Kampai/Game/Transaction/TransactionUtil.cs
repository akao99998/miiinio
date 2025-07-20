using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.Transaction
{
	public static class TransactionUtil
	{
		public static int GetPremiumCostForTransaction(TransactionDefinition transaction)
		{
			return SumOutputsForStaticItem(transaction, StaticItem.PREMIUM_CURRENCY_ID, true);
		}

		public static int GetPremiumOutputForTransaction(TransactionDefinition transaction)
		{
			return SumOutputsForStaticItem(transaction, StaticItem.PREMIUM_CURRENCY_ID);
		}

		public static int GetGrindOutputForTransaction(TransactionDefinition transaction)
		{
			return SumOutputsForStaticItem(transaction, StaticItem.GRIND_CURRENCY_ID);
		}

		public static int GetXPOutputForTransaction(TransactionDefinition transaction)
		{
			return SumOutputsForStaticItem(transaction, StaticItem.XP_ID);
		}

		public static string GetTransactionItemName(TransactionDefinition transaction, IDefinitionService definitionService)
		{
			if (transaction.Outputs != null)
			{
				foreach (QuantityItem output in transaction.Outputs)
				{
					ItemDefinition definition = null;
					definitionService.TryGet<ItemDefinition>(output.ID, out definition);
					if (definition != null)
					{
						int iD = definition.ID;
						if (iD == 2 || iD == 0 || iD == 1)
						{
							return string.Empty;
						}
						return definition.LocalizedKey;
					}
				}
			}
			else if (transaction.Inputs != null)
			{
				ItemDefinition definition2 = null;
				definitionService.TryGet<ItemDefinition>(transaction.Inputs[0].ID, out definition2);
				if (definition2 != null)
				{
					int iD2 = definition2.ID;
					if (iD2 == 2 || iD2 == 0 || iD2 == 1)
					{
						return string.Empty;
					}
					return definition2.LocalizedKey;
				}
			}
			return string.Empty;
		}

		public static int SumOutputsForStaticItem(TransactionDefinition transaction, StaticItem staticItem, bool inputs = false)
		{
			int num = 0;
			if (transaction != null)
			{
				IList<QuantityItem> list;
				if (inputs)
				{
					IList<QuantityItem> inputs2 = transaction.Inputs;
					list = inputs2;
				}
				else
				{
					list = transaction.Outputs;
				}
				IList<QuantityItem> list2 = list;
				if (list2 != null)
				{
					foreach (QuantityItem item in list2)
					{
						if (item.ID == (int)staticItem)
						{
							num += (int)item.Quantity;
						}
					}
				}
			}
			return num;
		}

		public static int SumInputsForStaticItem(TransactionDefinition transaction, StaticItem staticItem)
		{
			return SumOutputsForStaticItem(transaction, staticItem, true);
		}

		public static bool IsOnlyPremiumInputs(TransactionDefinition def)
		{
			return IsOnlyIDInputs(def, 1);
		}

		public static bool IsOnlyGrindInputs(TransactionDefinition def)
		{
			return IsOnlyIDInputs(def, 0);
		}

		public static bool IsOnlyIDInputs(TransactionDefinition def, int id)
		{
			bool result = false;
			IList<QuantityItem> inputs = def.Inputs;
			if (inputs != null && inputs.Count > 0)
			{
				result = inputs[0].ID == id;
				foreach (QuantityItem item in inputs)
				{
					if (item.ID != id)
					{
						result = false;
					}
				}
			}
			return result;
		}

		public static int ExtractQuantityFromTransaction(TransactionDefinition transactionDefinition, int definitionID)
		{
			return ExtractQuantityFromQuantityItemList(transactionDefinition.Outputs, definitionID);
		}

		public static int ExtractQuantityFromTransaction(TransactionInstance transactionInstance, int definitionID)
		{
			return ExtractQuantityFromQuantityItemList(transactionInstance.Outputs, definitionID);
		}

		private static int ExtractQuantityFromQuantityItemList(IList<QuantityItem> itemList, int definitionID)
		{
			int result = 0;
			if (itemList != null)
			{
				foreach (QuantityItem item in itemList)
				{
					if (item.ID == definitionID)
					{
						result = (int)item.Quantity;
						break;
					}
				}
			}
			return result;
		}

		public static int GetTransactionCurrencyCost(TransactionDefinition transactionDefinition, IDefinitionService definitionService, IPlayerService playerService, StaticItem currencyType)
		{
			int num = 0;
			foreach (QuantityItem output in transactionDefinition.Outputs)
			{
				switch (currencyType)
				{
				case StaticItem.GRIND_CURRENCY_ID:
				{
					Definition definition2 = definitionService.Get(output.ID);
					ItemDefinition itemDefinition2 = definition2 as ItemDefinition;
					if (itemDefinition2 != null)
					{
						num += (int)(itemDefinition2.BaseGrindCost * output.Quantity);
					}
					BuildingDefinition buildingDefinition = definition2 as BuildingDefinition;
					if (buildingDefinition != null)
					{
						ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(output.ID);
						int num2 = byDefinitionId.Count * buildingDefinition.IncrementalCost;
						TransactionDefinition transaction = definitionService.Get<TransactionDefinition>(definitionService.getItemTransactionID(output.ID));
						int num3 = SumInputsForStaticItem(transaction, StaticItem.GRIND_CURRENCY_ID) + num2;
						num += (int)(num3 * output.Quantity);
					}
					break;
				}
				case StaticItem.PREMIUM_CURRENCY_ID:
				{
					Definition definition = definitionService.Get(output.ID);
					ItemDefinition itemDefinition = definition as ItemDefinition;
					if (itemDefinition != null)
					{
						num += (int)(itemDefinition.BasePremiumCost * (float)output.Quantity);
					}
					break;
				}
				}
			}
			return num;
		}
	}
}
