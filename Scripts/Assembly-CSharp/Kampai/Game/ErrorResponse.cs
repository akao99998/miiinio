using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ErrorResponse
	{
		[JsonProperty(PropertyName = "error", Required = Required.Always)]
		public ErrorResponseContent Error { get; set; }
	}
}
