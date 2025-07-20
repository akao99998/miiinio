using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ShowCraftingBuildingMenuCommand : Command
	{
		[Inject]
		public Building building { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject BuildingManager { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = BuildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(building.ID);
			if (buildingObject != null)
			{
				openBuildingMenuSignal.Dispatch(buildingObject, building);
			}
		}
	}
}
