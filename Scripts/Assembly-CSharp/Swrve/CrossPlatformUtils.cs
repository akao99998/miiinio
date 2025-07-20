using System.Collections.Generic;
using UnityEngine;

namespace Swrve
{
	public static class CrossPlatformUtils
	{
		public static WWW MakeWWW(string url, byte[] encodedData, Dictionary<string, string> headers)
		{
			return new WWW(url, encodedData, headers);
		}
	}
}
