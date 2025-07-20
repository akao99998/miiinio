using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CameraAutoMoveToBuildingCommand : Command
	{
		[Inject]
		public Building building { get; set; }

		[Inject]
		public PanInstructions panInstructions { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingDefSignal autoMoveSignal { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public CameraAutoMoveSignal cameraAutoMoveSignal { get; set; }

		public override void Execute()
		{
			int iD = building.ID;
			bool flag = false;
			showHiddenBuildingsSignal.Dispatch();
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(iD);
			if (buildingObject == null)
			{
				ScaffoldingBuildingObject scaffoldingBuildingObject = component.GetScaffoldingBuildingObject(iD);
				if (scaffoldingBuildingObject == null)
				{
					return;
				}
				flag = true;
				buildingObject = scaffoldingBuildingObject;
			}
			Vector3 vector = ((!(building is TikiBarBuilding) && !(building is VillainLairEntranceBuilding)) ? buildingObject.ZoomCenter : buildingObject.transform.position);
			if (flag || building.State == BuildingState.Construction || building.State == BuildingState.Complete)
			{
				ScreenPosition screenPosition = new ScreenPosition();
				Boxed<Vector3> offset = panInstructions.Offset;
				Boxed<float> zoomDistance = panInstructions.ZoomDistance;
				if (zoomDistance != null)
				{
					screenPosition.zoom = zoomDistance.Value;
				}
				Vector3 type = ((offset != null) ? (offset.Value + vector) : vector);
				cameraAutoMoveSignal.Dispatch(type, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), false);
			}
			else
			{
				autoMoveSignal.Dispatch(building.Definition, vector, panInstructions);
			}
		}
	}
}
