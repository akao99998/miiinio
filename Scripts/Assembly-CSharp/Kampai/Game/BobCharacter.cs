using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class BobCharacter : FrolicCharacter<BobCharacterDefinition>
	{
		public bool HasShownExpansionNarrative { get; set; }

		public BobCharacter(BobCharacterDefinition def)
			: base(def)
		{
			Name = "Bob";
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HASSHOWNEXPANSIONNARRATIVE":
				reader.Read();
				HasShownExpansionNarrative = Convert.ToBoolean(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			writer.WritePropertyName("HasShownExpansionNarrative");
			writer.WriteValue(HasShownExpansionNarrative);
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			return go.AddComponent<BobView>();
		}
	}
}
