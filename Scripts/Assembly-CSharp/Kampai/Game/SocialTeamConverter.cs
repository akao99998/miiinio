using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class SocialTeamConverter : CustomCreationConverter<SocialTeam>
	{
		private IDefinitionService definitionService;

		private TimedSocialEventDefinition def;

		public SocialTeamConverter(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				JProperty jProperty = jObject.Property("socialEventId");
				int id = jProperty.Value.Value<int>();
				def = definitionService.Get<TimedSocialEventDefinition>(id);
				reader = jObject.CreateReader();
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override SocialTeam Create(Type objectType)
		{
			return new SocialTeam(def);
		}
	}
}
