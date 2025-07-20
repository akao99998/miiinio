using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game.Trigger
{
	public class TriggerInstanceConverter : CustomCreationConverter<TriggerInstance>
	{
		private IDefinitionService definitionService;

		private Definition def;

		public TriggerInstanceConverter(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = ((jObject.Property("def") != null) ? jObject.Property("def") : jObject.Property("Definition"));
			if (jProperty == null)
			{
				return null;
			}
			int id = jProperty.Value.Value<int>();
			def = null;
			if (!definitionService.TryGet<Definition>(id, out def))
			{
				return null;
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override TriggerInstance Create(Type objectType)
		{
			if (def == null)
			{
				return null;
			}
			IBuilder<TriggerInstance> builder = def as IBuilder<TriggerInstance>;
			object result;
			if (builder != null)
			{
				TriggerInstance triggerInstance = builder.Build();
				result = triggerInstance;
			}
			else
			{
				result = null;
			}
			return (TriggerInstance)result;
		}
	}
}
