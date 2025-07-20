using System;

namespace Kampai.Game
{
	public class DCNToken
	{
		public string Token { get; set; }

		public DateTime Expires_In { get; set; }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Token))
			{
				return false;
			}
			if (Expires_In <= DateTime.Now)
			{
				return false;
			}
			return true;
		}
	}
}
