using System;
using System.Collections.Generic;

namespace Kampai.Game
{
	public class DCNContent
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string Type { get; set; }

		public string Mime_Type { get; set; }

		public DateTime Created_At { get; set; }

		public DateTime Updated_At { get; set; }

		public DateTime Expires_In { get; set; }

		public IDictionary<string, string> Urls { get; set; }

		public bool Featured { get; set; }
	}
}
