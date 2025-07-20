using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class NamedCharacterDefinitionConverter : CustomCreationConverter<NamedCharacterDefinition>
	{
		private NamedCharacterType type;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("type") != null)
			{
				string value = jObject.Property("type").Value.ToString();
				type = (NamedCharacterType)(int)Enum.Parse(typeof(NamedCharacterType), value);
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override NamedCharacterDefinition Create(Type objectType)
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
				return null;
			}
		}
	}
}
