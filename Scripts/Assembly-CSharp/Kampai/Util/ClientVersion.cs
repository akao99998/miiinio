using System;
using Elevation.Logging;
using UnityEngine;

namespace Kampai.Util
{
	public class ClientVersion : IClientVersion
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ClientVersion") as IKampaiLogger;

		private string clientVersion;

		[Inject]
		public ILocalPersistanceService localPersistance { get; set; }

		public string GetClientVersion()
		{
			if (clientVersion == null)
			{
				if (localPersistance.HasKey("OverrideVersion"))
				{
					clientVersion = localPersistance.GetDataInt("OverrideVersion").ToString();
					logger.Warning("Overriding client version: {0}", clientVersion);
					logger.Warning(string.Format("Stack trace: {0}", Environment.StackTrace));
				}
				else
				{
					clientVersion = Native.BundleVersion.Split('.')[2];
				}
			}
			return clientVersion;
		}

		public string GetClientPlatform()
		{
			string text = "unknown";
			return "android";
		}

		public string GetClientDeviceType()
		{
			return SystemInfo.deviceModel;
		}

		public void RemoveOverrideVersion()
		{
			if (localPersistance.HasKey("OverrideVersion"))
			{
				switch (localPersistance.GetData("OverrideVersionPersistState"))
				{
				case "keep":
					logger.Info("Keeping the override client version for the next session.");
					localPersistance.PutData("OverrideVersionPersistState", "remove");
					break;
				default:
					logger.Info("Removing the override client version.");
					localPersistance.DeleteKey("OverrideVersionPersistState");
					localPersistance.DeleteKey("OverrideVersion");
					break;
				}
			}
		}
	}
}
