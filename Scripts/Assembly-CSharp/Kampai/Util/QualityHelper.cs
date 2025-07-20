namespace Kampai.Util
{
	public static class QualityHelper
	{
		public static TargetPerformance getDlcHD(TargetPerformance target)
		{
			switch (target)
			{
			case TargetPerformance.HIGH:
				return TargetPerformance.HIGH;
			case TargetPerformance.MED:
				return TargetPerformance.HIGH;
			case TargetPerformance.LOW:
				return TargetPerformance.MED;
			default:
				return TargetPerformance.VERYLOW;
			}
		}

		public static TargetPerformance getDlcSD(TargetPerformance target)
		{
			switch (target)
			{
			case TargetPerformance.HIGH:
				return TargetPerformance.MED;
			case TargetPerformance.MED:
				return TargetPerformance.MED;
			case TargetPerformance.LOW:
				return TargetPerformance.LOW;
			default:
				return TargetPerformance.VERYLOW;
			}
		}

		public static string getStartingQuality(TargetPerformance target)
		{
			switch (target)
			{
			case TargetPerformance.HIGH:
				return "DLCHDPack";
			case TargetPerformance.MED:
				return "DLCSDPack";
			default:
				return "DLCSDPack";
			}
		}

		public static TargetPerformance getCurrentTarget(TargetPerformance deviceTarget, string Quality)
		{
			if (Quality == "DLCHDPack")
			{
				return getDlcHD(deviceTarget);
			}
			return getDlcSD(deviceTarget);
		}
	}
}
