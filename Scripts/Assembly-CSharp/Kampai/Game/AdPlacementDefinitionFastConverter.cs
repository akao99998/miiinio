using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class AdPlacementDefinitionFastConverter : FastJsonCreationConverter<AdPlacementDefinition>
	{
		private RewardedAdType rewardType;

		public override AdPlacementDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				string value = jProperty.Value.ToString();
				rewardType = (RewardedAdType)(int)Enum.Parse(typeof(RewardedAdType), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override AdPlacementDefinition Create()
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
				throw new JsonSerializationException(string.Format("Unexpected advertisement type: {0}", rewardType));
			}
		}
	}
}
