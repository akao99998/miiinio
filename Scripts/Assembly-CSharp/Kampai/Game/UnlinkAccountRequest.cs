using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UnlinkAccountRequest : IFastJSONSerializable
	{
		public string identityType { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			if (identityType != null)
			{
				writer.WritePropertyName("identityType");
				writer.WriteValue(identityType);
			}
		}
	}
}
