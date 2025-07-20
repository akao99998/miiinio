using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game.Transaction
{
	public static class TransactionDataExtension
	{
		public static int GetInputCount(this TransactionDefinition transactionDefinition)
		{
			return (transactionDefinition != null) ? SafeListCount(transactionDefinition.Inputs) : 0;
		}

		public static int GetOutputCount(this TransactionDefinition transactionDefinition)
		{
			return (transactionDefinition != null) ? SafeListCount(transactionDefinition.Outputs) : 0;
		}

		public static int GetInputCount(this TransactionInstance transactionInstance)
		{
			return (transactionInstance != null) ? SafeListCount(transactionInstance.Inputs) : 0;
		}

		public static int GetOutputCount(this TransactionInstance transactionInstance)
		{
			return (transactionInstance != null) ? SafeListCount(transactionInstance.Outputs) : 0;
		}

		public static QuantityItem GetInputItem(this TransactionInstance transactionInstance, int index)
		{
			return (transactionInstance != null) ? SafeListCount(transactionInstance.Inputs, index) : null;
		}

		public static QuantityItem GetInputItem(this TransactionDefinition transactionDefinition, int index)
		{
			return (transactionDefinition != null) ? SafeListCount(transactionDefinition.Inputs, index) : null;
		}

		public static QuantityItem GetOutputItem(this TransactionInstance transactionInstance, int index)
		{
			return (transactionInstance != null) ? SafeListCount(transactionInstance.Outputs, index) : null;
		}

		public static QuantityItem GetOutputItem(this TransactionDefinition transactionDefinition, int index)
		{
			return (transactionDefinition != null) ? SafeListCount(transactionDefinition.Outputs, index) : null;
		}

		public static QuantityItem GetInputItemId(this TransactionInstance transactionInstance, int itemId)
		{
			return (transactionInstance != null) ? SafeListByItemId(transactionInstance.Inputs, itemId) : null;
		}

		public static QuantityItem GetInputItemId(this TransactionDefinition transactionDefinition, int itemId)
		{
			return (transactionDefinition != null) ? SafeListByItemId(transactionDefinition.Inputs, itemId) : null;
		}

		public static QuantityItem GetOutputItemId(this TransactionInstance transactionInstance, int itemId)
		{
			return (transactionInstance != null) ? SafeListByItemId(transactionInstance.Outputs, itemId) : null;
		}

		public static QuantityItem GetOutputItemId(this TransactionDefinition transactionDefinition, int itemId)
		{
			return (transactionDefinition != null) ? SafeListByItemId(transactionDefinition.Outputs, itemId) : null;
		}

		private static QuantityItem SafeListByItemId(IList<QuantityItem> list, int itemId)
		{
			if (list == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				QuantityItem quantityItem = list[i];
				if (quantityItem != null && quantityItem.ID == itemId)
				{
					return quantityItem;
				}
			}
			return null;
		}

		private static QuantityItem SafeListCount(IList<QuantityItem> list, int index)
		{
			return (list == null) ? null : ((index >= list.Count) ? null : list[index]);
		}

		private static int SafeListCount(IList<QuantityItem> list)
		{
			return (list != null) ? list.Count : 0;
		}
	}
}
