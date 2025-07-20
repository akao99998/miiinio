using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	public class TriggerDefinitionFastConverter : FastJsonCreationConverter<TriggerDefinition>
	{
		private TriggerDefinitionType.Identifier triggerType;

		private IKampaiLogger logger;

		public TriggerDefinitionFastConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override TriggerDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			triggerType = TriggerDefinitionType.ReadFromJson(ref reader);
			if (triggerType == TriggerDefinitionType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, converters);
		}

		public override TriggerDefinition Create()
		{
			return TriggerDefinitionType.CreateDefinitionFromIdentifier(triggerType, logger);
		}
	}
}
