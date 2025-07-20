using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FillOrderErrorResponse
	{
		[JsonProperty(PropertyName = "error", Required = Required.Always)]
		public FillOrderErrorResponseContent error { get; set; }
	}
}
