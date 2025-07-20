using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Achievement : Instance<AchievementDefinition>
	{
		public int Progress { get; set; }

		public Achievement(AchievementDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PROGRESS":
				reader.Read();
				Progress = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("Progress");
			writer.WriteValue(Progress);
		}
	}
}
