using Newtonsoft.Json;

namespace Kampai.Game
{
	public class AccountLinkErrorResponse
	{
		[JsonProperty(PropertyName = "error", Required = Required.Always)]
		public AccountLinkErrorResponseContent error { get; set; }
	}
}
