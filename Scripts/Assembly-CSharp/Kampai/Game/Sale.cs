using System;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Sale : Instance<SalePackDefinition>
	{
		public bool isDynamicSaleDefinition { get; set; }

		public int UTCUserStartTime { get; set; }

		public int Impressions { get; set; }

		public int UTCLastImpressionTime { get; set; }

		public bool Purchased { get; set; }

		public bool Started { get; set; }

		public bool Finished { get; set; }

		public bool Viewed { get; set; }

		public Sale(SalePackDefinition def)
			: base(def)
		{
			if (def.Type == SalePackType.Upsell)
			{
				isDynamicSaleDefinition = true;
			}
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ISDYNAMICSALEDEFINITION":
				reader.Read();
				isDynamicSaleDefinition = Convert.ToBoolean(reader.Value);
				break;
			case "UTCUSERSTARTTIME":
				reader.Read();
				UTCUserStartTime = Convert.ToInt32(reader.Value);
				break;
			case "IMPRESSIONS":
				reader.Read();
				Impressions = Convert.ToInt32(reader.Value);
				break;
			case "UTCLASTIMPRESSIONTIME":
				reader.Read();
				UTCLastImpressionTime = Convert.ToInt32(reader.Value);
				break;
			case "PURCHASED":
				reader.Read();
				Purchased = Convert.ToBoolean(reader.Value);
				break;
			case "STARTED":
				reader.Read();
				Started = Convert.ToBoolean(reader.Value);
				break;
			case "FINISHED":
				reader.Read();
				Finished = Convert.ToBoolean(reader.Value);
				break;
			case "VIEWED":
				reader.Read();
				Viewed = Convert.ToBoolean(reader.Value);
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
			writer.WritePropertyName("ID");
			writer.WriteValue(ID);
			writer.WritePropertyName("Definition");
			if (base.Definition.Type == SalePackType.Upsell)
			{
				base.Definition.Serialize(writer);
			}
			else
			{
				writer.WriteValue(base.Definition.ID);
			}
			writer.WritePropertyName("isDynamicSaleDefinition");
			writer.WriteValue(isDynamicSaleDefinition);
			writer.WritePropertyName("UTCUserStartTime");
			writer.WriteValue(UTCUserStartTime);
			writer.WritePropertyName("Impressions");
			writer.WriteValue(Impressions);
			writer.WritePropertyName("UTCLastImpressionTime");
			writer.WriteValue(UTCLastImpressionTime);
			writer.WritePropertyName("Purchased");
			writer.WriteValue(Purchased);
			writer.WritePropertyName("Started");
			writer.WriteValue(Started);
			writer.WritePropertyName("Finished");
			writer.WriteValue(Finished);
			writer.WritePropertyName("Viewed");
			writer.WriteValue(Viewed);
		}
	}
}
