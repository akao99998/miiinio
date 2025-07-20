using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PanAndOpenModalCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public bool bypassModal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		[Inject]
		public OpenVillainLairPortalBuildingSignal openPortalSignal { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			if (byInstanceId == null)
			{
				return;
			}
			VillainLairResourcePlot villainLairResourcePlot = byInstanceId as VillainLairResourcePlot;
			if (villainLairResourcePlot != null)
			{
				VillainLair parentLair = villainLairResourcePlot.parentLair;
				VillainLairEntranceBuildingObject villainLairEntranceBuildingObject = component.GetBuildingObject(parentLair.portalInstanceID) as VillainLairEntranceBuildingObject;
				if (!(villainLairEntranceBuildingObject == null))
				{
					VillainLairEntranceBuilding byInstanceId2 = playerService.GetByInstanceId<VillainLairEntranceBuilding>(parentLair.portalInstanceID);
					if (byInstanceId2 != null)
					{
						openPortalSignal.Dispatch(byInstanceId2, villainLairEntranceBuildingObject);
					}
				}
				return;
			}
			BuildingObject buildingObject = component.GetBuildingObject(buildingID);
			if (!(buildingObject == null))
			{
				Vector3 zero = Vector3.zero;
				zero = buildingObject.ZoomCenter;
				bool flag = !GotoBuildingHelpers.BuildingMenuIsAccessible(byInstanceId);
				if (!flag)
				{
					ScaffoldingBuildingObject scaffoldingBuildingObject = buildingObject as ScaffoldingBuildingObject;
					flag = scaffoldingBuildingObject != null;
				}
				ProcessRegularBuilding(zero, flag, byInstanceId);
			}
		}

		private void ProcessRegularBuilding(Vector3 buildingPos, bool menuInaccessible, Building building)
		{
			ScreenPosition screenPosition = new ScreenPosition();
			if (building.Definition.ScreenPosition != null)
			{
				screenPosition = screenPosition.Clone(building.Definition.ScreenPosition);
			}
			CameraMovementSettings.Settings settings = ((!menuInaccessible) ? CameraMovementSettings.Settings.ShowMenu : CameraMovementSettings.Settings.None);
			autoMoveSignal.Dispatch(buildingPos, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(settings, building, null, bypassModal), false);
		}
	}
}
