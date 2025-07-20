using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FillOrderErrorResponseContentDetails
	{
		[JsonProperty("orderId")]
		public string OrderID { get; set; }

		[JsonProperty("userId")]
		public string UserID { get; set; }
	}
}
