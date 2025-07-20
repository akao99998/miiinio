using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupFlowersCommand : Command
	{
		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject(GameElement.FLOWER_PARENT)]
		public GameObject parent { get; set; }

		public override void Execute()
		{
			foreach (LandExpansionBuilding trackedFlower in landExpansionService.GetTrackedFlowers())
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>(trackedFlower.GetPrefab()));
				gameObject.transform.parent = parent.transform;
				gameObject.transform.position = new Vector3(trackedFlower.Location.x, 0f, trackedFlower.Location.y);
				landExpansionService.AddToFlowerMap(trackedFlower.ExpansionID, gameObject);
			}
		}
	}
}
