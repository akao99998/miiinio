using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class CameraCinematicMoveToBuildingCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public float moveTime { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public CameraCinematicZoomSignal autoZoomSignal { get; set; }

		[Inject]
		public CameraCinematicPanSignal autoPanSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public GameObject mainCameraGO { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(buildingID);
			Vector3 position = buildingObject.transform.position;
			ScreenPosition screenPosition = byInstanceId.Definition.ScreenPosition;
			if (screenPosition == null)
			{
				screenPosition = new ScreenPosition();
			}
			float zoom = screenPosition.zoom;
			if (screenPosition.x != -1f || screenPosition.z != -1f)
			{
				Vector3 vector = cameraUtils.GroundPlaneRaycast(screenPosition.x, screenPosition.z);
				Vector3 vector2 = position - vector;
				Vector3 position2 = mainCameraGO.transform.position;
				Vector3 first = new Vector3(position2.x + vector2.x, position2.y, position2.z + vector2.z);
				CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null);
				autoPanSignal.Dispatch(Tuple.Create(first, moveTime), cameraMovementSettings, new Boxed<Building>(cameraMovementSettings.building), new Boxed<Quest>(cameraMovementSettings.quest));
				if (zoom > 0f)
				{
					autoZoomSignal.Dispatch(Tuple.Create(zoom, moveTime));
				}
			}
		}
	}
}
