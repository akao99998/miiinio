using UnityEngine;

namespace Kampai.Util
{
	public class AndroidDeviceInspector : IDeviceInspector
	{
		public bool IsSupported(RuntimePlatform platform)
		{
			return platform == RuntimePlatform.Android;
		}

		public TargetPerformance CaluclateTargetPerformance(DeviceInformation device)
		{
			int ram = device.ram;
			if (ram > 1536)
			{
				return TargetPerformance.HIGH;
			}
			if (ram > 1024)
			{
				return TargetPerformance.MED;
			}
			if (ram > 768)
			{
				return TargetPerformance.LOW;
			}
			return TargetPerformance.VERYLOW;
		}

		public int GetTargetFrameRate(DeviceInformation device)
		{
			return 30;
		}
	}
}
