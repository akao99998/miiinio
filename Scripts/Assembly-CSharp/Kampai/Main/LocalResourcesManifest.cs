using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Main
{
	public class LocalResourcesManifest
	{
		public List<string> bundledAssets { get; set; }

		public List<string> streamingAssets { get; set; }

		public List<string> audioBanks { get; set; }

		public List<LODAsset> separatedAssets { get; set; }
	}
}
