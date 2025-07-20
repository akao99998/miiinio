using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using strange.extensions.injector.api;

namespace Kampai.Util
{
	public static class BuildingPacksHelper
	{
		public static bool UpdateTransactionUnlocksList(TransactionInstance transaction, IInjectionBinder binder)
		{
			bool result = false;
			IDefinitionService instance = binder.GetInstance<IDefinitionService>();
			IPlayerService instance2 = binder.GetInstance<IPlayerService>();
			List<UnlockDefinition> all = instance.GetAll<UnlockDefinition>();
			List<QuantityItem> list = new List<QuantityItem>();
			IList<QuantityItem> outputs = transaction.Outputs;
			foreach (QuantityItem item in outputs)
			{
				BuildingDefinition definition;
				if (!instance.TryGet<BuildingDefinition>(item.ID, out definition))
				{
					continue;
				}
				result = true;
				foreach (UnlockDefinition item2 in all)
				{
					if (item2.ReferencedDefinitionID != definition.ID)
					{
						continue;
					}
					bool flag = false;
					foreach (QuantityItem item3 in list)
					{
						if (item3.ID == item2.ID)
						{
							item3.Quantity += item.Quantity;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						uint unlockedQuantityOfID = (uint)instance2.GetUnlockedQuantityOfID(item.ID);
						uint quantity = ((!item2.Delta) ? (unlockedQuantityOfID + item.Quantity) : item.Quantity);
						list.Add(new QuantityItem(item2.ID, quantity));
					}
				}
			}
			foreach (QuantityItem item4 in list)
			{
				bool flag2 = false;
				foreach (QuantityItem item5 in outputs)
				{
					if (item5.ID == item4.ID)
					{
						item5.Quantity = item4.Quantity;
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					outputs.Add(item4);
				}
			}
			return result;
		}
	}
}
