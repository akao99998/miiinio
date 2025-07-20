using UnityEngine;

namespace Kampai.Util
{
	public static class ScreenUtils
	{
		public static void ToggleAutoRotation(bool isEnabled)
		{
			if (isEnabled)
			{
				isEnabled = Native.AutorotationIsOSAllowed();
			}
			ScreenOrientation orientation = Screen.orientation;
			Screen.autorotateToLandscapeLeft = isEnabled || orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeLeft;
			Screen.autorotateToLandscapeRight = isEnabled || orientation == ScreenOrientation.LandscapeRight;
		}
	}
}
