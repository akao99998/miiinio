using UnityEngine;

namespace Kampai.Util
{
	public class EditorDeviceInspector : IDeviceInspector
	{
		public bool IsSupported(RuntimePlatform platform)
		{
			return false;
		}

		public TargetPerformance CaluclateTargetPerformance(DeviceInformation device)
		{
			return TargetPerformance.HIGH;
		}

		public int GetTargetFrameRate(DeviceInformation device)
		{
			return 60;
		}
	}
}
