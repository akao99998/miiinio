using Newtonsoft.Json;

namespace Kampai.Game
{
	public class UserIdentity
	{
		[JsonProperty("id")]
		public string ID { get; set; }

		[JsonProperty("externalId")]
		public string ExternalID { get; set; }

		[JsonProperty("userId")]
		public string UserID { get; set; }

		[JsonProperty("type")]
		public IdentityType Type { get; set; }
	}
}
