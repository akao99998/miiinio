using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ShowHiddenBuildingCommand : Command
	{
		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public ToggleMinionRendererSignal toggleMinionSignal { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			ICollection<ActionableObject> fadedObjects = component.GetFadedObjects();
			FadeMaterialBlock component2 = component.gameObject.GetComponent<FadeMaterialBlock>();
			TaskableBuildingObject taskableBuildingObject = null;
			LeisureBuildingObjectView leisureBuildingObjectView = null;
			List<Renderer> list = new List<Renderer>();
			foreach (ActionableObject item in fadedObjects)
			{
				if (item.gfxFaded && item.CanFadeGFX())
				{
					list.AddRange(item.gameObject.GetComponentsInChildren<Renderer>());
					item.gfxFaded = false;
					taskableBuildingObject = item as TaskableBuildingObject;
					leisureBuildingObjectView = item as LeisureBuildingObjectView;
					if (taskableBuildingObject != null)
					{
						taskableBuildingObject.FadeMinions(toggleMinionSignal, true);
					}
					else if (leisureBuildingObjectView != null)
					{
						leisureBuildingObjectView.FadeMinions(toggleMinionSignal, true);
					}
				}
				if (item.CanFadeSFX())
				{
					item.FadeSFX(0.5f, true);
				}
			}
			if (component2 != null && list.Count > 0)
			{
				component2.StartFade(true, 0.5f, list);
			}
		}
	}
}
