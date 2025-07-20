using Kampai.Game;

namespace Kampai.Common
{
	internal static class RewardedAdTelemetryUtil
	{
		public static string GetSurfaceLocation(AdPlacementName placementName)
		{
			return placementName.ToString();
		}

		public static string GetRewardType(AdPlacementName placementName, ItemDefinition reward)
		{
			if (reward == null)
			{
				return "null reward";
			}
			switch (placementName)
			{
			case AdPlacementName.ORDERBOARD:
				return "Buyout";
			case AdPlacementName.CRAFTING:
			case AdPlacementName.MARKETPLACE:
				return "Rush";
			case AdPlacementName.QUEST:
				return "Double Reward";
			default:
				return GetDefaultRewardType(reward);
			}
		}

		public static string GetDefaultRewardType(ItemDefinition itemDefinition)
		{
			switch (itemDefinition.ID)
			{
			case 0:
				return "Grind";
			case 1:
				return "Premium";
			case 50:
				return "Token";
			default:
				switch (itemDefinition.TaxonomySpecific)
				{
				case "Base Resource":
					return "Base Resource";
				case "Decoration":
					return "Deco";
				case "Craftable":
					return "Crafts";
				default:
					if (itemDefinition.TaxonomyHighLevel == "Drop")
					{
						return "Drops";
					}
					return itemDefinition.TaxonomySpecific;
				}
			}
		}
	}
}
