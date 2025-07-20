namespace Kampai.Game
{
	public class FillOrderErrorResponseContent
	{
		public int code { get; set; }

		public int responseCode { get; set; }

		public string description { get; set; }

		public string message { get; set; }

		public FillOrderErrorResponseContentDetails details { get; set; }

		public string exceptionDetails { get; set; }
	}
}
