using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kampai.Util
{
	public class IOSDeviceInspector : IDeviceInspector
	{
		private sealed class DeviceIdentifier
		{
			public string family;

			public float majorMinor;
		}

		private IKampaiLogger logger;

		private Dictionary<string, float> min = new Dictionary<string, float>
		{
			{ "iPhone", 4.1f },
			{ "iPad", 2.1f },
			{ "iPod", 5.1f }
		};

		private Dictionary<string, float> med = new Dictionary<string, float>
		{
			{ "iPhone", 5.1f },
			{ "iPad", 3.4f },
			{ "iPod", 6.1f }
		};

		private Dictionary<string, float> high = new Dictionary<string, float>
		{
			{ "iPhone", 6.1f },
			{ "iPad", 4.1f },
			{ "iPod", 7.1f }
		};

		private Dictionary<string, float> xhigh = new Dictionary<string, float>
		{
			{ "iPhone", 8.1f },
			{ "iPad", 5.3f },
			{ "iPod", 8.1f }
		};

		public IOSDeviceInspector(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public bool IsSupported(RuntimePlatform platform)
		{
			return platform == RuntimePlatform.IPhonePlayer;
		}

		public TargetPerformance CaluclateTargetPerformance(DeviceInformation device)
		{
			DeviceIdentifier deviceIdentifier = ParseDeviceInfo(device);
			if (deviceIdentifier.family != null)
			{
				if (!min.ContainsKey(deviceIdentifier.family))
				{
					return NotFound(string.Format("Unknown family: {0} - {1}", deviceIdentifier.family, deviceIdentifier.majorMinor.ToString()));
				}
				if (deviceIdentifier.majorMinor < min[deviceIdentifier.family])
				{
					return TargetPerformance.UNSUPPORTED;
				}
				if (deviceIdentifier.majorMinor < med[deviceIdentifier.family])
				{
					return TargetPerformance.LOW;
				}
				if (deviceIdentifier.majorMinor < high[deviceIdentifier.family])
				{
					return TargetPerformance.MED;
				}
				return TargetPerformance.HIGH;
			}
			return NotFound(string.Format("Unrecognized device model format: {0}", device.model));
		}

		public int GetTargetFrameRate(DeviceInformation device)
		{
			DeviceIdentifier deviceIdentifier = ParseDeviceInfo(device);
			if (deviceIdentifier.family != null && xhigh.ContainsKey(deviceIdentifier.family) && deviceIdentifier.majorMinor >= xhigh[deviceIdentifier.family])
			{
				return 60;
			}
			return 30;
		}

		private TargetPerformance NotFound(string message)
		{
			logger.Log(KampaiLogLevel.Warning, "{0} - Defaulting to high performance", message);
			return TargetPerformance.HIGH;
		}

		private DeviceIdentifier ParseDeviceInfo(DeviceInformation device)
		{
			DeviceIdentifier deviceIdentifier = new DeviceIdentifier();
			deviceIdentifier.family = null;
			MatchCollection matchCollection = Regex.Matches(device.model, "([a-zA-Z]+)(\\d+),(\\d+)");
			if (matchCollection != null && matchCollection.Count == 1)
			{
				IEnumerator enumerator = matchCollection.GetEnumerator();
				enumerator.MoveNext();
				Match match = enumerator.Current as Match;
				GroupCollection groups = match.Groups;
				deviceIdentifier.family = groups[1].Value;
				int num = Convert.ToInt32(groups[2].Value);
				int num2 = Convert.ToInt32(groups[3].Value);
				deviceIdentifier.majorMinor = (float)num + (float)num2 * 0.1f;
			}
			return deviceIdentifier;
		}
	}
}
