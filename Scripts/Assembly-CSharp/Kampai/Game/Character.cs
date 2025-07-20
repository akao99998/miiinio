using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class Character<T> : Instance<T>, Character, Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : Definition
	{
		public string Name { get; set; }

		protected Character(T definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NAME":
				reader.Read();
				Name = ReaderUtil.ReadString(reader, converters);
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
			if (Name != null)
			{
				writer.WritePropertyName("Name");
				writer.WriteValue(Name);
			}
		}
	}
	public interface Character : Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		string Name { get; set; }
	}
}
