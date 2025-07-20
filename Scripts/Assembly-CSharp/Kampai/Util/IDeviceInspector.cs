using UnityEngine;

namespace Kampai.Util
{
	public interface IDeviceInspector
	{
		bool IsSupported(RuntimePlatform platform);

		TargetPerformance CaluclateTargetPerformance(DeviceInformation device);

		int GetTargetFrameRate(DeviceInformation device);
	}
}
