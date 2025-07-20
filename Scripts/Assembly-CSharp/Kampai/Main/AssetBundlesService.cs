using System;
using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Main
{
	public class AssetBundlesService : IAssetBundlesService
	{
		public const string BUNDLE_DEPENDENCY_MANIFEST_NAME = "Raw_Bundle_Dependency_Manifest";

		private IKampaiLogger logger = LogManager.GetClassLogger("AssetBundlesService") as IKampaiLogger;

		private Dictionary<string, AssetBundle> loadedSharedBundles = new Dictionary<string, AssetBundle>();

		private Dictionary<string, AssetBundle> loadedDLCBundles = new Dictionary<string, AssetBundle>();

		private Dictionary<string, List<string>> dependencyManifest;

		[Inject]
		public IManifestService manifestService { get; set; }

		private void loadDependenciesManifest()
		{
			string bundleByOriginalName = manifestService.GetBundleByOriginalName("Raw_Bundle_Dependency_Manifest");
			if (string.IsNullOrEmpty(bundleByOriginalName))
			{
				logger.Fatal(FatalCode.DLC_DEPENDENCY_MANIFEST_ERROR, 1);
			}
			string bundlePath = GetBundlePath(bundleByOriginalName);
			try
			{
				using (FileStream stream = File.OpenRead(bundlePath))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						using (JsonTextReader reader2 = new JsonTextReader(reader))
						{
							JsonSerializer jsonSerializer = JsonSerializer.Create(null);
							dependencyManifest = jsonSerializer.Deserialize<Dictionary<string, List<string>>>(reader2);
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Fatal(FatalCode.DLC_DEPENDENCY_MANIFEST_ERROR, 2, "Failed to load '{0}': {1}'", bundleByOriginalName, ex.ToString());
			}
			if (dependencyManifest == null)
			{
				logger.Fatal(FatalCode.DLC_DEPENDENCY_MANIFEST_ERROR, 3);
			}
		}

		private string GetBundlePath(string bundleName)
		{
			string bundleLocation = manifestService.GetBundleLocation(bundleName);
			if (bundleLocation.Length == 0)
			{
				logger.Error("Unable to find bundle: {0}", bundleName);
			}
			return Path.Combine(bundleLocation, bundleName + ".unity3d");
		}

		public bool IsSharedBundle(string bundleName)
		{
			return loadedSharedBundles.ContainsKey(bundleName);
		}

		public void LoadSharedBundle(string bundleName)
		{
			LoadAssetBundleWithDependencies(bundleName, true);
		}

		public AssetBundle GetSharedBundle(string bundleName)
		{
			return loadedSharedBundles[bundleName];
		}

		private AssetBundle LoadAssetBundleWithDependencies(string bundleName, bool isShared)
		{
			if (dependencyManifest == null)
			{
				loadDependenciesManifest();
			}
			AssetBundle loadedBundle = GetLoadedBundle(bundleName);
			if (loadedBundle != null)
			{
				return loadedBundle;
			}
			string bundleOriginalName = manifestService.GetBundleOriginalName(bundleName);
			List<string> value;
			if (dependencyManifest.TryGetValue(bundleOriginalName, out value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					string bundleByOriginalName = manifestService.GetBundleByOriginalName(value[i]);
					AssetBundle loadedBundle2 = GetLoadedBundle(bundleByOriginalName);
					if (loadedBundle2 == null)
					{
						LoadAssetBundle(bundleByOriginalName, true);
					}
				}
			}
			return LoadAssetBundle(bundleName, isShared);
		}

		private AssetBundle GetLoadedBundle(string bundleName)
		{
			AssetBundle value;
			if (loadedSharedBundles.TryGetValue(bundleName, out value))
			{
				return value;
			}
			if (loadedDLCBundles.TryGetValue(bundleName, out value))
			{
				return value;
			}
			return null;
		}

		private AssetBundle LoadAssetBundle(string bundleName, bool isDependency)
		{
			logger.Info("Loading bundle: '{0}'", manifestService.GetBundleOriginalName(bundleName));
			string bundlePath = GetBundlePath(bundleName);
			if (!File.Exists(bundlePath))
			{
				logger.Error("Content bundle '{0}' was not found. ('{1}')", manifestService.GetBundleOriginalName(bundleName), bundlePath);
				return null;
			}
			AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
			if (null == assetBundle)
			{
				logger.Error("Failed to load bundle '{0}'.", manifestService.GetBundleOriginalName(bundleName));
				return null;
			}
			if (isDependency)
			{
				loadedSharedBundles.Add(bundleName, assetBundle);
			}
			else
			{
				loadedDLCBundles.Add(bundleName, assetBundle);
			}
			return assetBundle;
		}

		public AssetBundle GetDLCBundle(string bundleName)
		{
			return LoadAssetBundleWithDependencies(bundleName, false);
		}

		public void UnloadSharedBundles()
		{
			TimeProfiler.StartSection("unloading shared bundles");
			foreach (AssetBundle value in loadedSharedBundles.Values)
			{
				value.Unload(false);
			}
			loadedSharedBundles.Clear();
			TimeProfiler.EndSection("unloading shared bundles");
		}

		public void UnloadDLCBundles()
		{
			TimeProfiler.StartSection("unload dlc bundles");
			foreach (AssetBundle value in loadedDLCBundles.Values)
			{
				value.Unload(false);
			}
			loadedDLCBundles.Clear();
			TimeProfiler.EndSection("unload dlc bundles");
		}
	}
}
