using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceBuyItem : Instance<MarketplaceItemDefinition>
	{
		public int BuyQuantity { get; set; }

		public int BuyPrice { get; set; }

		public bool BoughtFlag { get; set; }

		public MarketplaceBuyItem(MarketplaceItemDefinition definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUYQUANTITY":
				reader.Read();
				BuyQuantity = Convert.ToInt32(reader.Value);
				break;
			case "BUYPRICE":
				reader.Read();
				BuyPrice = Convert.ToInt32(reader.Value);
				break;
			case "BOUGHTFLAG":
				reader.Read();
				BoughtFlag = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("BuyQuantity");
			writer.WriteValue(BuyQuantity);
			writer.WritePropertyName("BuyPrice");
			writer.WriteValue(BuyPrice);
			writer.WritePropertyName("BoughtFlag");
			writer.WriteValue(BoughtFlag);
		}
	}
}
