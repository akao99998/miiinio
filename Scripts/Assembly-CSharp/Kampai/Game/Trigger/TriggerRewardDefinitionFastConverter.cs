using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class TriggerRewardDefinitionFastConverter : FastJsonCreationConverter<TriggerRewardDefinition>
	{
		private TriggerRewardType.Identifier rewardType;

		private IKampaiLogger logger;

		public TriggerRewardDefinitionFastConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override TriggerRewardDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			rewardType = TriggerRewardType.ReadFromJson(ref reader);
			if (rewardType == TriggerRewardType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, converters);
		}

		public override TriggerRewardDefinition Create()
		{
			return TriggerRewardType.CreateFromIdentifier(rewardType, logger);
		}
	}
}
