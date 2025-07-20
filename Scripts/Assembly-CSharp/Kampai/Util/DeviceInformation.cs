using UnityEngine;

namespace Kampai.Util
{
	public class DeviceInformation
	{
		public string model;

		public int processorCount;

		public int graphicsShaderLevel;

		public int vram;

		public int ram;

		public int screenWidth;

		public int screenHeight;

		public int screenRefresh;

		public DeviceInformation()
		{
			Resolution currentResolution = Screen.currentResolution;
			screenWidth = currentResolution.width;
			screenHeight = currentResolution.height;
			screenRefresh = currentResolution.refreshRate;
			model = SystemInfo.deviceModel;
			processorCount = SystemInfo.processorCount;
			ram = SystemInfo.systemMemorySize;
			vram = SystemInfo.graphicsMemorySize;
			graphicsShaderLevel = SystemInfo.graphicsShaderLevel;
		}

		public bool IsSamsung()
		{
			return model.ToUpper().Contains("SAMSUNG");
		}
	}
}
