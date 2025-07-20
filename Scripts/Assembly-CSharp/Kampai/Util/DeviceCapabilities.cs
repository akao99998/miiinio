using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class DeviceCapabilities
	{
		private static bool isTablet;

		public TargetPerformance GetTargetPerformance(IKampaiLogger logger, RuntimePlatform platform, DeviceInformation info)
		{
			List<IDeviceInspector> list = new List<IDeviceInspector>();
			list.Add(new AndroidDeviceInspector());
			list.Add(new IOSDeviceInspector(logger));
			list.Add(new EditorDeviceInspector());
			List<IDeviceInspector> list2 = list;
			foreach (IDeviceInspector item in list2)
			{
				if (item.IsSupported(platform))
				{
					return item.CaluclateTargetPerformance(info);
				}
			}
			logger.Log(KampaiLogLevel.Info, "Unknown platform {0}; defaulting to low LOD", platform.ToString());
			return TargetPerformance.LOW;
		}

		public int GetTargetFrameRate(IKampaiLogger logger, RuntimePlatform platform, DeviceInformation info)
		{
			List<IDeviceInspector> list = new List<IDeviceInspector>();
			list.Add(new AndroidDeviceInspector());
			list.Add(new IOSDeviceInspector(logger));
			list.Add(new EditorDeviceInspector());
			List<IDeviceInspector> list2 = list;
			foreach (IDeviceInspector item in list2)
			{
				if (item.IsSupported(platform))
				{
					return item.GetTargetFrameRate(info);
				}
			}
			logger.Log(KampaiLogLevel.Info, "Unknown platform {0}; defaulting to 30 FPS", platform.ToString());
			return 30;
		}

		public static bool IsTablet()
		{
			return isTablet;
		}

		public static void Initialize()
		{
			float num = 6f;
			float num2 = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
			float num3 = num2 / Screen.dpi;
			if (num3 > num)
			{
				isTablet = true;
			}
			else
			{
				isTablet = false;
			}
		}
	}
}
