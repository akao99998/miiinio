using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CameraInstanceFocusCommand : Command
	{
		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public Vector3 focusPosition { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public ToggleMinionRendererSignal toggleMinionSignal { get; set; }

		public override void Execute()
		{
			showHiddenBuildingsSignal.Dispatch();
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			FadeMaterialBlock fadeMaterialBlock = component.gameObject.GetComponent<FadeMaterialBlock>();
			if (fadeMaterialBlock == null)
			{
				fadeMaterialBlock = component.gameObject.AddComponent<FadeMaterialBlock>();
			}
			LinkedList<ActionableObject> linkedList = new LinkedList<ActionableObject>();
			LinkedList<ActionableObject> linkedList2 = new LinkedList<ActionableObject>();
			component.GetOccludingObjects(focusPosition, buildingId, linkedList, linkedList2);
			TaskableBuildingObject taskableBuildingObject = null;
			LeisureBuildingObjectView leisureBuildingObjectView = null;
			List<Renderer> list = new List<Renderer>();
			foreach (ActionableObject item in linkedList)
			{
				if (item.ID != buildingId && item.CanFadeGFX() && item.CanFadeSFX())
				{
					list.AddRange(item.gameObject.GetComponentsInChildren<Renderer>());
					item.gfxFaded = true;
					taskableBuildingObject = item as TaskableBuildingObject;
					leisureBuildingObjectView = item as LeisureBuildingObjectView;
					if (taskableBuildingObject != null)
					{
						taskableBuildingObject.FadeMinions(toggleMinionSignal, false);
					}
					else if (leisureBuildingObjectView != null)
					{
						leisureBuildingObjectView.FadeMinions(toggleMinionSignal, false);
					}
					item.FadeSFX(0.5f, false);
				}
			}
			if (fadeMaterialBlock != null && list.Count > 0)
			{
				fadeMaterialBlock.StartFade(false, 0.5f, list);
			}
			foreach (ActionableObject item2 in linkedList2)
			{
				if (item2.ID != buildingId)
				{
					item2.FadeSFX(0.5f, false);
				}
			}
		}
	}
}
