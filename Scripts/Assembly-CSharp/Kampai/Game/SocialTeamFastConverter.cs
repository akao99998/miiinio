using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class SocialTeamFastConverter : FastJsonCreationConverter<SocialTeam>
	{
		private IDefinitionService definitionService;

		private TimedSocialEventDefinition def;

		public SocialTeamFastConverter(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public override SocialTeam ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				JProperty jProperty = jObject.Property("socialEventId");
				int id = jProperty.Value.Value<int>();
				def = definitionService.Get<TimedSocialEventDefinition>(id);
				reader = jObject.CreateReader();
			}
			return base.ReadJson(reader, converters);
		}

		public override SocialTeam Create()
		{
			return new SocialTeam(def);
		}
	}
}
