using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Splash;
using UnityEngine;

namespace Kampai.Util
{
	public static class KampaiResources
	{
		private sealed class AssetsCache
		{
			private readonly Dictionary<string, Dictionary<Type, UnityEngine.Object>> cache = new Dictionary<string, Dictionary<Type, UnityEngine.Object>>(4096);

			public UnityEngine.Object Get(string name, Type type)
			{
				Dictionary<Type, UnityEngine.Object> value;
				if (name == null || !cache.TryGetValue(name, out value))
				{
					return null;
				}
				UnityEngine.Object value2;
				return (!value.TryGetValue(type, out value2)) ? null : value2;
			}

			public void Clear()
			{
				foreach (KeyValuePair<string, Dictionary<Type, UnityEngine.Object>> item in cache)
				{
					foreach (KeyValuePair<Type, UnityEngine.Object> item2 in item.Value)
					{
						if (!(item2.Value is GameObject))
						{
							Resources.UnloadAsset(item2.Value);
						}
					}
				}
				cache.Clear();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			public void Add(string name, UnityEngine.Object obj, Type type)
			{
				Dictionary<Type, UnityEngine.Object> value;
				if (!cache.TryGetValue(name, out value))
				{
					value = new Dictionary<Type, UnityEngine.Object>(1);
					cache.Add(name, value);
				}
				value.Add(type, obj);
			}
		}

		private static IManifestService manifestService;

		private static IAssetBundlesService assetBundlesService;

		private static ILocalContentService localContentService;

		private static IKampaiLogger logger;

		private static readonly AssetsCache cachedObjects = new AssetsCache();

		public static void SetManifestService(IManifestService service)
		{
			manifestService = service;
		}

		public static void SetAssetBundlesService(IAssetBundlesService service)
		{
			assetBundlesService = service;
		}

		public static void SetLocalContentService(ILocalContentService service)
		{
			localContentService = service;
		}

		public static void SetLogger()
		{
			logger = LogManager.GetClassLogger("KampaiResources") as IKampaiLogger;
		}

		public static void ClearCache()
		{
			cachedObjects.Clear();
		}

		public static bool FileExists(string path)
		{
			return manifestService.GetAssetLocation(path).Length > 0 || localContentService.IsLocalAsset(path);
		}

		public static T Load<T>(string path) where T : class
		{
			object obj = Load(path, typeof(T));
			return obj as T;
		}

		public static UnityEngine.Object Load(string path)
		{
			return Load(path, typeof(UnityEngine.Object));
		}

		public static AsyncOperation LoadAsync(string path, IRoutineRunner routineRunner, Action<UnityEngine.Object> onComplete = null)
		{
			return LoadAsync(path, typeof(UnityEngine.Object), routineRunner, onComplete);
		}

		public static bool FileDownloaded(string path, DLCModel dlcModel)
		{
			string assetLocation = manifestService.GetAssetLocation(path);
			if (assetLocation.Length == 0)
			{
				return localContentService.IsLocalAsset(path);
			}
			int bundleTier = manifestService.GetBundleTier(assetLocation);
			return dlcModel.HighestTierDownloaded >= bundleTier;
		}

		public static IEnumerator LoadAsyncWait(AsyncOperation request, Action<UnityEngine.Object> onComplete, string name, Type type)
		{
			if (request == null)
			{
				yield break;
			}
			yield return request;
			UnityEngine.Object obj = null;
			ResourceRequest resourceRequest = request as ResourceRequest;
			if (resourceRequest != null)
			{
				obj = resourceRequest.asset;
			}
			else
			{
				AssetBundleRequest assetRequest = request as AssetBundleRequest;
				if (assetRequest != null)
				{
					obj = assetRequest.asset;
				}
			}
			if (obj != null)
			{
				cachedObjects.Add(name, obj, type);
				if (onComplete != null)
				{
					onComplete(obj);
				}
			}
		}

		public static AsyncOperation LoadAsync(string path, Type type, IRoutineRunner routineRunner, Action<UnityEngine.Object> onComplete = null)
		{
			UnityEngine.Object @object = cachedObjects.Get(path, type);
			if (@object != null)
			{
				if (onComplete != null)
				{
					onComplete(@object);
				}
				return null;
			}
			AsyncOperation asyncOperation = null;
			string assetLocation = manifestService.GetAssetLocation(path);
			if (assetLocation.Length == 0)
			{
				UnityEngine.Object obj = Load(path, type);
				if (onComplete != null)
				{
					onComplete(obj);
				}
				return null;
			}
			AssetBundle assetBundle;
			if (assetBundlesService.IsSharedBundle(assetLocation))
			{
				assetBundle = assetBundlesService.GetSharedBundle(assetLocation);
				if (assetBundle == null)
				{
					logger.Debug(string.Format("assetBundlesService.GetSharedBundle({0}) returned a null bundle", assetLocation));
				}
			}
			else
			{
				assetBundle = assetBundlesService.GetDLCBundle(assetLocation);
				if (assetBundle == null)
				{
					logger.Debug(string.Format("assetBundlesService.GetDLCBundle({0}) returned a null bundle", assetLocation));
				}
			}
			if (assetBundle != null)
			{
				asyncOperation = assetBundle.LoadAssetAsync(path, type);
			}
			if (asyncOperation != null)
			{
				routineRunner.StartCoroutine(LoadAsyncWait(asyncOperation, onComplete, path, type));
			}
			else
			{
				logger.Error("KampaiResources.Load is returning NULL for object " + path);
			}
			return asyncOperation;
		}

		public static bool IsAssetTierGated(string asset)
		{
			return manifestService.IsAssetTierTooHigh(asset);
		}

		public static UnityEngine.Object Load(string path, Type type)
		{
			UnityEngine.Object @object = cachedObjects.Get(path, type);
			if (null != @object)
			{
				return @object;
			}
			TimeProfiler.StartAssetLoadSection(path);
			UnityEngine.Object object2 = null;
			string assetLocation = manifestService.GetAssetLocation(path);
			bool flag = assetLocation.Length > 0 && manifestService.IsBundleTierTooHigh(assetLocation);
			if (assetLocation.Length == 0)
			{
				if (localContentService.IsLocalAsset(path))
				{
					string assetPath = localContentService.GetAssetPath(path);
					object2 = Resources.Load(assetPath, type);
					if (object2 == null)
					{
						logger.Error(string.Format("Resources.Load( {0}, {1}) returned a null value", assetPath, type.ToString()));
					}
				}
				else
				{
					logger.Error(string.Format("Unable to find bundle for '{0}'. This should only be an issue if you see it on device.", path));
				}
			}
			else if (!flag)
			{
				AssetBundle assetBundle;
				if (assetBundlesService.IsSharedBundle(assetLocation))
				{
					assetBundle = assetBundlesService.GetSharedBundle(assetLocation);
					if (assetBundle == null)
					{
						logger.Error(string.Format("assetBundlesService.GetSharedBundle({0}) returned a null bundle", assetLocation));
					}
				}
				else
				{
					assetBundle = assetBundlesService.GetDLCBundle(assetLocation);
					if (assetBundle == null)
					{
						logger.Error(string.Format("assetBundlesService.GetDLCBundle({0}) returned a null bundle", assetLocation));
					}
				}
				if (null != assetBundle)
				{
					object2 = assetBundle.LoadAsset(path, type);
				}
			}
			if (null != object2)
			{
				cachedObjects.Add(path, object2, type);
			}
			else if (!flag)
			{
				logger.Error("KampaiResources.Load is returning NULL for object '{0}'", path);
			}
			else
			{
				logger.Info("Asset '{0}' is not available for the current tier", path);
			}
			TimeProfiler.EndAssetLoadSection();
			return object2;
		}
	}
}
