using System;
using UnityEngine;

namespace Swrve.Helpers
{
	public class UnityWwwHelper
	{
		public static WwwDeducedError DeduceWwwError(WWW request)
		{
			if (request.responseHeaders.Count > 0)
			{
				string value = null;
				foreach (string key in request.responseHeaders.Keys)
				{
					if (string.Equals(key, "X-Swrve-Error", StringComparison.OrdinalIgnoreCase))
					{
						request.responseHeaders.TryGetValue(key, out value);
						break;
					}
				}
				if (value != null)
				{
					SwrveLog.LogError("Request response headers [\"X-Swrve-Error\"]: " + value + " at " + request.url);
					return WwwDeducedError.ApplicationErrorHeader;
				}
			}
			if (!string.IsNullOrEmpty(request.error))
			{
				SwrveLog.LogError("Request error: " + request.error + " in " + request.url);
				return WwwDeducedError.NetworkError;
			}
			return WwwDeducedError.NoError;
		}
	}
}
