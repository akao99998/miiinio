using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Common
{
	public interface IManifestService
	{
		void GenerateMasterManifest();

		string GetAssetLocation(string asset);

		string GetBundleLocation(string bundle);

		string GetBundleOriginalName(string bundle);

		string GetBundleByOriginalName(string name);

		int GetBundleTier(string bundle);

		List<BundleInfo> GetBundles();

		string GetDLCURL();

		IList<string> GetSharedBundles();

		IList<string> GetShaderBundles();

		IList<string> GetAudioBundles();

		IList<string> GetAssetsInBundle(string bundle);

		bool ContainsBundle(string name);

		IList<BundleInfo> GetUnstreamablePackagedBundlesList();

		bool IsStreamingBundle(string bundle);

		bool IsBundleTierTooHigh(string bundle);

		bool IsAssetTierTooHigh(string asset);
	}
}
