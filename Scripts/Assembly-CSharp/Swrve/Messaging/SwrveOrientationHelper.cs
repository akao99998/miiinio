namespace Swrve.Messaging
{
	public static class SwrveOrientationHelper
	{
		public static SwrveOrientation Parse(string orientation)
		{
			if (orientation.ToLower().Equals("portrait"))
			{
				return SwrveOrientation.Portrait;
			}
			if (orientation.ToLower().Equals("both"))
			{
				return SwrveOrientation.Both;
			}
			return SwrveOrientation.Landscape;
		}
	}
}
