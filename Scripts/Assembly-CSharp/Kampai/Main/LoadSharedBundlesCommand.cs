using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class LoadSharedBundlesCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("LoadSharedBundlesCommand") as IKampaiLogger;

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IAssetBundlesService assetBundlesService { get; set; }

		public override void Execute()
		{
			logger.Debug("Start Loading Shared Bundles");
			TimeProfiler.StartSection("loading shared bundles");
			foreach (string shaderBundle in manifestService.GetShaderBundles())
			{
				assetBundlesService.LoadSharedBundle(shaderBundle);
			}
			TimeProfiler.EndSection("loading shared bundles");
		}
	}
}
