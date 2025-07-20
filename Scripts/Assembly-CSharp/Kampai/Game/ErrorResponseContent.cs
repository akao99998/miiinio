using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ErrorResponseContent
	{
		[JsonProperty("code")]
		public int Code { get; set; }

		[JsonProperty("responseCode")]
		public int ResponseCode { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("exceptionDetails")]
		public string ExceptionDetails { get; set; }
	}
}
