using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ChangeMinionCostumeCommand : Command
	{
		[Inject]
		public MinionObject minionObject { get; set; }

		[Inject]
		public CostumeItemDefinition newCostume { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		public override void Execute()
		{
			SkinnedMeshRenderer[] componentsInChildren = minionObject.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].transform.parent = null;
				componentsInChildren[i].enabled = false;
				Object.Destroy(componentsInChildren[i].gameObject);
			}
			string targetLOD = dlcService.GetDownloadQualityLevel().ToUpper();
			SkinnedMeshAggregator.AddSubModels(newCostume.MeshList, minionObject.transform, targetLOD);
			minionObject.RefreshRenderers();
			minionBuilder.RebuildMinion(minionObject.gameObject);
		}
	}
}
