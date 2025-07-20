using System.Collections.Generic;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class HighlightLandExpansionCommand : Command
	{
		[Inject]
		public int expansionID { get; set; }

		[Inject]
		public bool enabled { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			List<LandExpansionBuilding> list = landExpansionService.GetBuildingsByExpansionID(expansionID) as List<LandExpansionBuilding>;
			foreach (LandExpansionBuilding item in list)
			{
				LandExpansionBuildingObject landExpansionBuildingObject = component.GetBuildingObject(item.ID) as LandExpansionBuildingObject;
				if (landExpansionBuildingObject != null)
				{
					landExpansionBuildingObject.Highlight(enabled);
				}
			}
		}
	}
}
