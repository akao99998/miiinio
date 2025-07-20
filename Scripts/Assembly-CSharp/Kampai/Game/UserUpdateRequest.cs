using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UserUpdateRequest : IFastJSONSerializable
	{
		[JsonProperty("synergyId")]
		public string SynergyID { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			if (SynergyID != null)
			{
				writer.WritePropertyName("synergyId");
				writer.WriteValue(SynergyID);
			}
		}
	}
}
