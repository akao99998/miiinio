using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kampai.Game.Trigger
{
	public class TriggerRewardDefinitionConverter : CustomCreationConverter<TriggerRewardDefinition>
	{
		private TriggerRewardType.Identifier rewardType;

		private IKampaiLogger logger;

		public TriggerRewardDefinitionConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			rewardType = TriggerRewardType.ReadFromJson(ref reader);
			if (rewardType == TriggerRewardType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override TriggerRewardDefinition Create(Type objectType)
		{
			return TriggerRewardType.CreateFromIdentifier(rewardType, logger);
		}
	}
}
