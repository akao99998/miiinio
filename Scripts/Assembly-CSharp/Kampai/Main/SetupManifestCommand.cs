using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class SetupManifestCommand : Command
	{
		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IAssetBundlesService assetBundlesService { get; set; }

		[Inject]
		public ILocalContentService localContentService { get; set; }

		public override void Execute()
		{
			TimeProfiler.StartSection("read manifest");
			manifestService.GenerateMasterManifest();
			TimeProfiler.EndSection("read manifest");
			KampaiResources.SetManifestService(manifestService);
			KampaiResources.SetAssetBundlesService(assetBundlesService);
			KampaiResources.SetLocalContentService(localContentService);
		}
	}
}
