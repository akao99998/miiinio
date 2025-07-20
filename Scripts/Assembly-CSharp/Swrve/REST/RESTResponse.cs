using System.Collections.Generic;
using Swrve.Helpers;

namespace Swrve.REST
{
	public class RESTResponse
	{
		public readonly string Body;

		public readonly WwwDeducedError Error;

		public readonly Dictionary<string, string> Headers;

		public RESTResponse(string body)
		{
			Body = body;
		}

		public RESTResponse(string body, Dictionary<string, string> headers)
			: this(body)
		{
			Headers = headers;
		}

		public RESTResponse(WwwDeducedError error)
		{
			Error = error;
		}
	}
}
