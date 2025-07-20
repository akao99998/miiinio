using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Item : Instance<ItemDefinition>
	{
		public uint Quantity { get; set; }

		public Item(ItemDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "QUANTITY":
				reader.Read();
				Quantity = Convert.ToUInt32(reader.Value);
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
			writer.WritePropertyName("Quantity");
			writer.WriteValue(Quantity);
		}
	}
}
