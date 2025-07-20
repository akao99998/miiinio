using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class NamedCharacterDefinitionFastConverter : FastJsonCreationConverter<NamedCharacterDefinition>
	{
		private NamedCharacterType type;

		public override NamedCharacterDefinition ReadJson(JsonReader reader, JsonConverters converters)
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
				type = (NamedCharacterType)(int)Enum.Parse(typeof(NamedCharacterType), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override NamedCharacterDefinition Create()
		{
			switch (type)
			{
			case NamedCharacterType.BOB:
				return new BobCharacterDefinition();
			case NamedCharacterType.VILLAIN:
				return new VillainDefinition();
			case NamedCharacterType.PHIL:
				return new PhilCharacterDefinition();
			case NamedCharacterType.STUART:
				return new StuartCharacterDefinition();
			case NamedCharacterType.KEVIN:
				return new KevinCharacterDefinition();
			case NamedCharacterType.TSM:
				return new TSMCharacterDefinition();
			case NamedCharacterType.SPECIAL_EVENT:
				return new SpecialEventCharacterDefinition();
			default:
				throw new JsonSerializationException(string.Format("Unexpected NamedCharacterDefinition type: {0}", type));
			}
		}
	}
}
