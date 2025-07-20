using UnityEngine;

namespace Kampai.Main
{
	public interface IAssetBundlesService
	{
		bool IsSharedBundle(string bundleName);

		void LoadSharedBundle(string bundleName);

		AssetBundle GetSharedBundle(string bundleName);

		AssetBundle GetDLCBundle(string bundleName);

		void UnloadSharedBundles();

		void UnloadDLCBundles();
	}
}
