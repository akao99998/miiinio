using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadAnimationToolKitManifestCommand : Command
	{
		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IAssetBundlesService assetBundlesService { get; set; }

		[Inject]
		public ILocalContentService localContentService { get; set; }

		public override void Execute()
		{
			manifestService.GenerateMasterManifest();
			KampaiResources.SetManifestService(manifestService);
			KampaiResources.SetAssetBundlesService(assetBundlesService);
			KampaiResources.SetLocalContentService(localContentService);
		}
	}
}
