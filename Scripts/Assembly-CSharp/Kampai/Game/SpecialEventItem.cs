using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class SpecialEventItem : Item
	{
		public bool HasEnded { get; set; }

		public SpecialEventItem(SpecialEventItemDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HASENDED":
				reader.Read();
				HasEnded = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("HasEnded");
			writer.WriteValue(HasEnded);
		}
	}
}
