using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MarketplaceSaleItem : Instance<MarketplaceItemDefinition>, IComparable<MarketplaceSaleItem>
	{
		public enum State
		{
			PENDING = 0,
			SOLD = 1
		}

		public State state { get; set; }

		public int QuantitySold { get; set; }

		public int SalePrice { get; set; }

		public int SaleStartTime { get; set; }

		public int LengthOfSale { get; set; }

		public MarketplaceSaleItem(MarketplaceItemDefinition definition)
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
			case "QUANTITYSOLD":
				reader.Read();
				QuantitySold = Convert.ToInt32(reader.Value);
				break;
			case "SALEPRICE":
				reader.Read();
				SalePrice = Convert.ToInt32(reader.Value);
				break;
			case "SALESTARTTIME":
				reader.Read();
				SaleStartTime = Convert.ToInt32(reader.Value);
				break;
			case "LENGTHOFSALE":
				reader.Read();
				LengthOfSale = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("QuantitySold");
			writer.WriteValue(QuantitySold);
			writer.WritePropertyName("SalePrice");
			writer.WriteValue(SalePrice);
			writer.WritePropertyName("SaleStartTime");
			writer.WriteValue(SaleStartTime);
			writer.WritePropertyName("LengthOfSale");
			writer.WriteValue(LengthOfSale);
		}

		public int CompareTo(MarketplaceSaleItem other)
		{
			if (other == null)
			{
				return -1;
			}
			return other.state - state;
		}
	}
}
