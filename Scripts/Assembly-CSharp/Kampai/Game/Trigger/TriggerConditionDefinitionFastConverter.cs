using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class TriggerConditionDefinitionFastConverter : FastJsonCreationConverter<TriggerConditionDefinition>
	{
		private TriggerConditionType.Identifier conditionType;

		private IKampaiLogger logger;

		public TriggerConditionDefinitionFastConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override TriggerConditionDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			conditionType = TriggerConditionType.ReadFromJson(ref reader);
			if (conditionType == TriggerConditionType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, converters);
		}

		public override TriggerConditionDefinition Create()
		{
			return TriggerConditionType.CreateFromIdentifier(conditionType, logger);
		}
	}
}
