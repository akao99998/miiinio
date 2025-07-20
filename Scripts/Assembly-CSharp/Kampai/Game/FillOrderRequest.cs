using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FillOrderRequest : IFastJSONSerializable
	{
		[JsonProperty("orderId")]
		public int OrderID { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			writer.WritePropertyName("orderId");
			writer.WriteValue(OrderID);
		}
	}
}
