using System.Collections.Generic;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RelocateTaskedMinionsCommand : Command
	{
		[Inject]
		public Building building { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			BuildingManagerView component2 = buildingManager.GetComponent<BuildingManagerView>();
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			TaskableBuildingObject taskableBuildingObject = null;
			if (taskableBuilding != null)
			{
				taskableBuildingObject = component2.GetBuildingObject(building.ID) as TaskableBuildingObject;
			}
			LeisureBuilding leisureBuilding = building as LeisureBuilding;
			LeisureBuildingObjectView leisureBuildingObjectView = null;
			if (leisureBuilding != null)
			{
				leisureBuildingObjectView = component2.GetBuildingObject(building.ID) as LeisureBuildingObjectView;
			}
			IList<int> list3;
			if (taskableBuilding == null)
			{
				IList<int> list2;
				IList<int> list;
				if (leisureBuilding == null)
				{
					list = null;
					list2 = list;
				}
				else
				{
					list2 = leisureBuilding.MinionList;
				}
				list = list2;
				list3 = list;
			}
			else
			{
				list3 = taskableBuilding.MinionList;
			}
			IList<int> list4 = list3;
			if (list4 == null)
			{
				return;
			}
			for (int i = 0; i < list4.Count; i++)
			{
				MinionObject characterObject = component.Get(list4[i]);
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.MoveToRoutingPosition(characterObject, i);
				}
				if (leisureBuildingObjectView != null)
				{
					leisureBuildingObjectView.MoveToRoutingPosition(characterObject, i);
				}
			}
		}
	}
}
