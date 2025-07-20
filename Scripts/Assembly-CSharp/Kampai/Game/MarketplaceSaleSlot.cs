using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceSaleSlot : Instance<MarketplaceSaleSlotDefinition>, IComparable<MarketplaceSaleSlot>
	{
		public enum State
		{
			LOCKED = 0,
			UNLOCKED = 1
		}

		public State state { get; set; }

		public int itemId { get; set; }

		public int premiumCost { get; set; }

		public MarketplaceSaleSlot(MarketplaceSaleSlotDefinition definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATE":
				reader.Read();
				state = ReaderUtil.ReadEnum<State>(reader);
				break;
			case "ITEMID":
				reader.Read();
				itemId = Convert.ToInt32(reader.Value);
				break;
			case "PREMIUMCOST":
				reader.Read();
				premiumCost = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
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
			writer.WritePropertyName("state");
			writer.WriteValue((int)state);
			writer.WritePropertyName("itemId");
			writer.WriteValue(itemId);
			writer.WritePropertyName("premiumCost");
			writer.WriteValue(premiumCost);
		}

		public int CompareTo(MarketplaceSaleSlot other)
		{
			if (other == null)
			{
				return -1;
			}
			return other.state - state;
		}
	}
}
