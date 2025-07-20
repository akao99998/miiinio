using System;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Splash
{
	public class DLCModel
	{
		private bool _allow;

		public IList<BundleInfo> NeededBundles { get; set; }

		public ulong TotalSize { get; set; }

		public Queue<IRequest> PendingRequests { get; set; }

		public List<IRequest> RunningRequests { get; set; }

		public float LastNetworkFailureTime { get; set; }

		public bool AllowDownloadOnMobileNetwork
		{
			get
			{
				return _allow;
			}
			set
			{
				_allow = value;
				PlayerPrefs.SetInt("PermitMobileData", _allow ? 1 : 0);
			}
		}

		public int HighestTierDownloaded { get; set; }

		public bool ShouldLaunchDownloadAgain { get; set; }

		public bool ShouldLoadAudio { get; set; }

		public bool NextDownloadShouldLoadAudio { get; set; }

		public Queue<string> DownloadedAudioBundles { get; set; }

		public bool UdpEnabled { get; set; }

		public long DownloadedTotalSize { get; set; }

		public DateTime DownloadStartTime { get; set; }

		public List<BundleInfo> PackagedAssetBundles { get; private set; }

		public DLCModel()
		{
			DownloadedAudioBundles = new Queue<string>(10);
			if (PlayerPrefs.HasKey("PermitMobileData"))
			{
				AllowDownloadOnMobileNetwork = PlayerPrefs.GetInt("PermitMobileData") > 0;
			}
			TextAsset textAsset = Resources.Load<TextAsset>("PackagedAssetBundlesManifest");
			if (null == textAsset)
			{
				PackagedAssetBundles = new List<BundleInfo>();
				return;
			}
			PackagedAssetBundles = JsonConvert.DeserializeObject<List<BundleInfo>>(textAsset.text);
			Resources.UnloadAsset(textAsset);
		}

		public BundleInfo GetPackagedAssetBundleInfo(string originalBundleName)
		{
			for (int i = 0; i < PackagedAssetBundles.Count; i++)
			{
				BundleInfo bundleInfo = PackagedAssetBundles[i];
				if (bundleInfo.originalName == originalBundleName)
				{
					return bundleInfo;
				}
			}
			return null;
		}
	}
}
