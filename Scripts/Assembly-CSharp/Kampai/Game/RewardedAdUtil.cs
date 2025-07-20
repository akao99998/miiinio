using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	public static class RewardedAdUtil
	{
		public static bool GetFirstItemDefintion(IList<QuantityItem> rewards, out ItemDefinition itemDefinition, out int rewardAmount, IDefinitionService definitionService)
		{
			if (rewards != null && rewards.Count > 0)
			{
				QuantityItem quantityItem = rewards[0];
				if (definitionService.TryGet<ItemDefinition>(quantityItem.ID, out itemDefinition))
				{
					rewardAmount = (int)quantityItem.Quantity;
					return true;
				}
			}
			itemDefinition = null;
			rewardAmount = 0;
			return false;
		}
	}
}
