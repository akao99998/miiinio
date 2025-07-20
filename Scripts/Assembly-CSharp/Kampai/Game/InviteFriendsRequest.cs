using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class InviteFriendsRequest : IFastJSONSerializable
	{
		[JsonProperty("identityType")]
		public IdentityType IdentityType { get; set; }

		[JsonProperty("externalIds")]
		public IList<string> ExternalIds { get; set; }

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			writer.WritePropertyName("identityType");
			writer.WriteValue((int)IdentityType);
			if (ExternalIds == null)
			{
				return;
			}
			writer.WritePropertyName("externalIds");
			writer.WriteStartArray();
			IEnumerator<string> enumerator = ExternalIds.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					writer.WriteValue(current);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndArray();
		}
	}
}
