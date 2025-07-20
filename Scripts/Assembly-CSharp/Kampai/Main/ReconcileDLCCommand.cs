using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class ReconcileDLCCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ReconcileDLCCommand") as IKampaiLogger;

		[Inject]
		public bool purge { get; set; }

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public DLCModel model { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		public override void Execute()
		{
			TimeProfiler.StartSection("reconcile dlc");
			List<BundleInfo> bundles = manifestService.GetBundles();
			List<BundleInfo> list = new List<BundleInfo>();
			ulong num = 0uL;
			int playerDLCTier = dlcService.GetPlayerDLCTier();
			int num2 = playerDLCTier;
			foreach (BundleInfo item in bundles)
			{
				string name = item.name;
				if (IsValidBundle(item))
				{
					continue;
				}
				if (playerDLCTier >= item.tier)
				{
					list.Add(item);
					num += item.size;
					if (num2 >= item.tier)
					{
						num2 = item.tier - 1;
					}
					if (purge && BundleExists(name))
					{
						File.Delete(GetBundlePath(name));
					}
				}
				else
				{
					logger.Debug("Unable to download: " + name + "    Tier is too high for this user");
				}
			}
			if (purge && Directory.Exists(GameConstants.DLC_PATH))
			{
				string[] files = Directory.GetFiles(GameConstants.DLC_PATH);
				foreach (string text in files)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					if (!text.Equals(".DS_Store") && !manifestService.ContainsBundle(fileNameWithoutExtension))
					{
						File.Delete(text);
					}
				}
			}
			model.NeededBundles = list;
			model.HighestTierDownloaded = num2;
			model.TotalSize = num;
			logger.Debug("ReconcileDLCCommand BundlesNeeded: {0} {1} Mb", list.Count, (double)num / 1024.0 / 1024.0);
			TimeProfiler.EndSection("reconcile dlc");
		}

		private string GetBundlePath(string name)
		{
			return Path.Combine(GameConstants.DLC_PATH, name + ".unity3d");
		}

		private bool BundleExists(string name)
		{
			return File.Exists(GetBundlePath(name));
		}

		private bool IsValidBundle(BundleInfo info)
		{
			if (info.isStreamable)
			{
				return true;
			}
			if (!BundleExists(info.name))
			{
				return false;
			}
			string bundlePath = GetBundlePath(info.name);
			ulong length = (ulong)new FileInfo(bundlePath).Length;
			if (length != info.size)
			{
				logger.Error("SIZE CHECK FAILED: {0} {1}!={2} failed", info.name, length, info.size);
				return false;
			}
			return true;
		}
	}
}
