using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Sticker : Instance<StickerDefinition>
	{
		public int UTCTimeEarned { get; set; }

		public bool isNew { get; set; }

		public Sticker(StickerDefinition def)
			: base(def)
		{
			isNew = true;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					isNew = Convert.ToBoolean(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "UTCTIMEEARNED":
				reader.Read();
				UTCTimeEarned = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
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
			writer.WritePropertyName("UTCTimeEarned");
			writer.WriteValue(UTCTimeEarned);
			writer.WritePropertyName("isNew");
			writer.WriteValue(isNew);
		}
	}
}
