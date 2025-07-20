using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceRefreshTimer : Instance<MarketplaceRefreshTimerDefinition>
	{
		public int UTCStartTime { get; set; }

		public MarketplaceRefreshTimer(MarketplaceRefreshTimerDefinition def)
			: base(def)
		{
			UTCStartTime = 0;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UTCSTARTTIME":
				reader.Read();
				UTCStartTime = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("UTCStartTime");
			writer.WriteValue(UTCStartTime);
		}
	}
}
