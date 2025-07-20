using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kampai.Game.Trigger
{
	public class TriggerDefinitionConverter : CustomCreationConverter<TriggerDefinition>
	{
		private TriggerDefinitionType.Identifier triggerType;

		private IKampaiLogger logger;

		public TriggerDefinitionConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			triggerType = TriggerDefinitionType.ReadFromJson(ref reader);
			if (triggerType == TriggerDefinitionType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override TriggerDefinition Create(Type objectType)
		{
			return TriggerDefinitionType.CreateDefinitionFromIdentifier(triggerType, logger);
		}
	}
}
