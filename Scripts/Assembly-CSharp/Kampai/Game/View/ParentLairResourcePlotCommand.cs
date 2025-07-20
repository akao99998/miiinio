using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class ParentLairResourcePlotCommand : Command
	{
		[Inject]
		public VillainLairResourcePlot resourcePlot { get; set; }

		[Inject]
		public GameObject childInstance { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void Execute()
		{
			int iD = resourcePlot.ID;
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			VillainLairResourcePlotObjectView villainLairResourcePlotObjectView = component.GetBuildingObject(iD) as VillainLairResourcePlotObjectView;
			if (villainLairResourcePlotObjectView != null)
			{
				GameObject gameObject = villainLairResourcePlotObjectView.gameObject;
				childInstance.transform.SetParent(gameObject.transform);
				MinionObject mo = null;
				if (resourcePlot.MinionIsTaskedToBuilding())
				{
					MinionManagerView component2 = minionManager.GetComponent<MinionManagerView>();
					mo = component2.Get(resourcePlot.MinionIDInBuilding);
				}
				villainLairResourcePlotObjectView.UpdateRoutes(childInstance, mo);
				villainLairResourcePlotObjectView.UpdateRenderers();
			}
		}
	}
}
