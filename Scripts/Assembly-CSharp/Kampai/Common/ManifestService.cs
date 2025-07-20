using System;
using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Common
{
	public class ManifestService : IManifestService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ManifestService") as IKampaiLogger;

		private Dictionary<string, string> assetManifest = new Dictionary<string, string>();

		private Dictionary<string, string> bundleManifest = new Dictionary<string, string>();

		private Dictionary<string, BundleInfo> bundleInfoMap = new Dictionary<string, BundleInfo>();

		private Dictionary<string, BundleInfo> originalNameToBundleInfoMap = new Dictionary<string, BundleInfo>();

		private List<string> sharedBundles = new List<string>();

		private List<string> shaderBundles = new List<string>();

		private List<string> audioBundles = new List<string>();

		private List<BundleInfo> bundles = new List<BundleInfo>();

		private HashSet<string> bundleNames = new HashSet<string>();

		private List<BundleInfo> unstreamablePackagedBundles = new List<BundleInfo>();

		private Dictionary<string, List<string>> bundleAssetsMap = new Dictionary<string, List<string>>();

		private string dlcURL;

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		public void GenerateMasterManifest()
		{
			Clear();
			if (!File.Exists(GameConstants.RESOURCE_MANIFEST_PATH) || new FileInfo(GameConstants.RESOURCE_MANIFEST_PATH).Length == 0L)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 1, "Read empty manifest");
			}
			ManifestObject manifestObject;
			try
			{
				manifestObject = FastJSONDeserializer.DeserializeFromFile<ManifestObject>(GameConstants.RESOURCE_MANIFEST_PATH);
			}
			catch (JsonReaderException ex)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 2, ex.ToString());
			}
			catch (JsonSerializationException ex2)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 2, ex2.ToString());
			}
			if (manifestObject == null)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 2, "Load null manifest");
			}
			assetManifest = manifestObject.assets;
			if (assetManifest == null)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 3, "Null assets in manifest");
			}
			if (manifestObject.bundles == null)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 4, "Null bundles in manifest");
			}
			foreach (BundleInfo bundle in manifestObject.bundles)
			{
				BundleInfo bundleInfo = AddBundle(bundle);
				bundleNames.Add(bundleInfo.name);
				if (bundle.shaders)
				{
					shaderBundles.Add(bundleInfo.name);
				}
				else if (bundle.audio)
				{
					audioBundles.Add(bundleInfo.name);
				}
				else if (bundle.shared)
				{
					sharedBundles.Add(bundleInfo.name);
				}
			}
			dlcURL = manifestObject.baseURL;
			if (dlcURL == null)
			{
				throw new FatalException(FatalCode.GS_ERROR_BAD_MANIFEST, 5, "Null dlcURL in manifest");
			}
			buildBundledAssetsLookup();
		}

		private BundleInfo AddBundle(BundleInfo bundle)
		{
			BundleInfo packagedAssetBundleInfo = dlcModel.GetPackagedAssetBundleInfo(bundle.originalName);
			bool flag = packagedAssetBundleInfo != null && packagedAssetBundleInfo.sum == bundle.sum && packagedAssetBundleInfo.size == bundle.size;
			bool flag2 = flag && packagedAssetBundleInfo.isStreamable;
			string value = ((!flag2) ? GameConstants.DLC_PATH : ((!bundle.audio) ? GameConstants.PRE_INSTALLED_DLC_PATH : GameConstants.PRE_INSTALLED_FMOD_PATH));
			BundleInfo bundleInfo = ((!flag) ? bundle : packagedAssetBundleInfo);
			bundleManifest.Add(bundleInfo.name, value);
			bundleInfoMap.Add(bundleInfo.name, bundleInfo);
			if (flag && !packagedAssetBundleInfo.isStreamable)
			{
				unstreamablePackagedBundles.Add(packagedAssetBundleInfo);
			}
			bundles.Add(bundleInfo);
			logger.Info("ManifestService added bundle: '{0}' (packaged: {1}, streamable: {2})", bundle.originalName, flag, flag2);
			return bundleInfo;
		}

		public string GetAssetLocation(string asset)
		{
			if (!assetManifest.ContainsKey(asset))
			{
				return string.Empty;
			}
			return assetManifest[asset];
		}

		public string GetBundleLocation(string bundle)
		{
			if (!bundleManifest.ContainsKey(bundle))
			{
				logger.Error("Unable to find bundle: {0}", bundle);
				return string.Empty;
			}
			return bundleManifest[bundle];
		}

		public string GetBundleOriginalName(string bundle)
		{
			if (!bundleInfoMap.ContainsKey(bundle))
			{
				logger.Error("Unable to find bundle: {0}", bundle);
				return string.Empty;
			}
			return bundleInfoMap[bundle].originalName;
		}

		public int GetBundleTier(string bundle)
		{
			if (!bundleInfoMap.ContainsKey(bundle))
			{
				logger.Error("Unable to find bundle: {0}", bundle);
				return int.MaxValue;
			}
			return bundleInfoMap[bundle].tier;
		}

		public List<BundleInfo> GetBundles()
		{
			return bundles;
		}

		public string GetDLCURL()
		{
			return dlcURL;
		}

		public IList<string> GetSharedBundles()
		{
			return sharedBundles;
		}

		public IList<string> GetShaderBundles()
		{
			return shaderBundles;
		}

		public IList<string> GetAudioBundles()
		{
			return audioBundles;
		}

		public bool ContainsBundle(string name)
		{
			return bundleNames.Contains(name);
		}

		public IList<string> GetAssetsInBundle(string bundle)
		{
			if (!bundleAssetsMap.ContainsKey(bundle))
			{
				return new List<string>();
			}
			return bundleAssetsMap[bundle];
		}

		private void buildBundledAssetsLookup()
		{
			foreach (KeyValuePair<string, string> item in assetManifest)
			{
				string key = item.Key;
				string value = item.Value;
				if (!bundleAssetsMap.ContainsKey(value))
				{
					bundleAssetsMap.Add(value, new List<string>());
				}
				bundleAssetsMap[value].Add(key);
			}
		}

		private void Clear()
		{
			bundleManifest.Clear();
			bundleInfoMap.Clear();
			sharedBundles.Clear();
			shaderBundles.Clear();
			audioBundles.Clear();
			bundleNames.Clear();
			originalNameToBundleInfoMap.Clear();
		}

		public string GetBundleByOriginalName(string name)
		{
			BundleInfo value;
			if (originalNameToBundleInfoMap.TryGetValue(name, out value))
			{
				return value.name;
			}
			foreach (KeyValuePair<string, BundleInfo> item in bundleInfoMap)
			{
				if (item.Value.originalName.StartsWith(name, StringComparison.Ordinal))
				{
					originalNameToBundleInfoMap.Add(name, item.Value);
					return item.Key;
				}
			}
			return string.Empty;
		}

		public IList<BundleInfo> GetUnstreamablePackagedBundlesList()
		{
			return unstreamablePackagedBundles;
		}

		public bool IsStreamingBundle(string bundle)
		{
			BundleInfo value;
			if (bundleInfoMap.TryGetValue(bundle, out value))
			{
				return value.isStreamable;
			}
			return false;
		}

		public bool IsBundleTierTooHigh(string bundle)
		{
			BundleInfo value;
			return bundleInfoMap.TryGetValue(bundle, out value) && value.tier > dlcService.GetPlayerDLCTier();
		}

		public bool IsAssetTierTooHigh(string asset)
		{
			return IsBundleTierTooHigh(GetAssetLocation(asset));
		}
	}
}
