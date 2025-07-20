using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kampai.Game.Trigger
{
	public class TriggerConditionDefinitionConverter : CustomCreationConverter<TriggerConditionDefinition>
	{
		private TriggerConditionType.Identifier conditionType;

		private IKampaiLogger logger;

		public TriggerConditionDefinitionConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			conditionType = TriggerConditionType.ReadFromJson(ref reader);
			if (conditionType == TriggerConditionType.Identifier.Unknown)
			{
				return null;
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override TriggerConditionDefinition Create(Type objectType)
		{
			return TriggerConditionType.CreateFromIdentifier(conditionType, logger);
		}
	}
}
