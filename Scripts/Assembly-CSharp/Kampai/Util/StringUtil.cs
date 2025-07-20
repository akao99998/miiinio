using UnityEngine;

namespace Kampai.Util
{
	public static class StringUtil
	{
		public static string UnifiedPlatformName(RuntimePlatform platform)
		{
			switch (platform)
			{
			case RuntimePlatform.Android:
				return "android";
			case RuntimePlatform.IPhonePlayer:
				return "iOS";
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				return "editor";
			default:
				return null;
			}
		}
	}
}
