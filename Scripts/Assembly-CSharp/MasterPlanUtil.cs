using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;

public static class MasterPlanUtil
{
	public static IList<QuantityItem> GetMissingItemList(MasterPlanComponentTask task)
	{
		IList<QuantityItem> list = new List<QuantityItem>();
		switch (task.Definition.Type)
		{
		case MasterPlanComponentTaskType.Deliver:
		case MasterPlanComponentTaskType.Collect:
		{
			QuantityItem item3 = new QuantityItem(task.Definition.requiredItemId, task.remainingQuantity);
			list.Add(item3);
			break;
		}
		case MasterPlanComponentTaskType.PlayMiniGame:
		{
			QuantityItem item2 = new QuantityItem(186, task.remainingQuantity);
			list.Add(item2);
			break;
		}
		case MasterPlanComponentTaskType.EarnPartyPoints:
		case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
		case MasterPlanComponentTaskType.EarnMignettePartyPoints:
		{
			QuantityItem item = new QuantityItem(2, task.remainingQuantity);
			list.Add(item);
			break;
		}
		}
		return list;
	}
}
