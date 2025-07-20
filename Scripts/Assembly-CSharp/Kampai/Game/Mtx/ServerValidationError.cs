using Newtonsoft.Json;

namespace Kampai.Game.Mtx
{
	public class ServerValidationError
	{
		public enum Code
		{
			RECEIPT_DUPLICATE = 11,
			RECEIPT_INVALID = 12,
			VALIDATION_UNAVAILABLE = 13
		}

		public Code code;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string description;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string message;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string exceptionDetails;
	}
}
