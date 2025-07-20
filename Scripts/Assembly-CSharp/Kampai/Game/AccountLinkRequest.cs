using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class AccountLinkRequest : IFastJSONSerializable
	{
		public string identityType { get; set; }

		public string externalId { get; set; }

		public string credentials { get; set; }

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
			if (externalId != null)
			{
				writer.WritePropertyName("externalId");
				writer.WriteValue(externalId);
			}
			if (credentials != null)
			{
				writer.WritePropertyName("credentials");
				writer.WriteValue(credentials);
			}
		}
	}
}
