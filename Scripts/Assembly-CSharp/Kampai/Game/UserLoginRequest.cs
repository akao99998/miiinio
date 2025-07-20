using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UserLoginRequest : IFastJSONSerializable
	{
		[JsonProperty("identityId")]
		public string IdentityID { get; set; }

		[JsonProperty("userId")]
		public string UserID { get; set; }

		[JsonProperty("anonymousSecret")]
		public string AnonymousSecret { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			if (IdentityID != null)
			{
				writer.WritePropertyName("identityId");
				writer.WriteValue(IdentityID);
			}
			if (UserID != null)
			{
				writer.WritePropertyName("userId");
				writer.WriteValue(UserID);
			}
			if (AnonymousSecret != null)
			{
				writer.WritePropertyName("anonymousSecret");
				writer.WriteValue(AnonymousSecret);
			}
		}
	}
}
