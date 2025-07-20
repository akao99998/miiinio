using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class AdPlacementDefinitionConverter : CustomCreationConverter<AdPlacementDefinition>
	{
		private RewardedAdType rewardType;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("type") != null)
			{
				string value = jObject.Property("type").Value.ToString();
				rewardType = (RewardedAdType)(int)Enum.Parse(typeof(RewardedAdType), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override AdPlacementDefinition Create(Type objectType)
		{
			switch (rewardType)
			{
			case RewardedAdType.OnTheGlassDailyReward:
				return new OnTheGlassDailyRewardDefinition();
			case RewardedAdType.CraftingRushReward:
				return new CraftingRushRewardDefinition();
			case RewardedAdType.MarketplaceRefreshRushReward:
				return new MarketplaceRefreshRushRewardDefinition();
			case RewardedAdType.MissingResourcesReward:
				return new MissingResourcesRewardDefinition();
			case RewardedAdType.Offerwall:
				return new OfferwallPlacementDefinition();
			case RewardedAdType.OrderboardFillOrderReward:
				return new OrderboardFillOrderRewardDefinition();
			case RewardedAdType.Quest2xReward:
				return new Quest2xRewardDefinition();
			default:
				return null;
			}
		}
	}
}
