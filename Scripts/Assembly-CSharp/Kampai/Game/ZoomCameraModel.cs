using UnityEngine;

namespace Kampai.Game
{
	public class ZoomCameraModel : IZoomCameraModel
	{
		public bool ZoomedIn { get; set; }

		public bool ZoomInProgress { get; set; }

		public BuildingZoomType LastZoomBuildingType { get; set; }

		public int PreviousCameraBehavior { get; set; }

		public Vector3 PreviousCameraPosition { get; set; }

		public Vector3 PreviousCameraRotation { get; set; }

		public float PreviousCameraFieldOfView { get; set; }

		public float LastResourceZoomRotation { get; set; }

		public Vector3 GetZoomedCameraPosition(ZoomableBuilding building)
		{
			ZoomableBuildingDefinition zoomableDefinition = building.ZoomableDefinition;
			Vector3 zoomOffset = zoomableDefinition.zoomOffset;
			Location location = building.Location;
			return new Vector3(location.x, 0f, location.y) + zoomOffset;
		}

		public Quaternion GetZoomedCameraRotation(ZoomableBuilding building)
		{
			return Quaternion.Euler(building.ZoomableDefinition.zoomEulers);
		}

		public float GetZoomedFOV(ZoomableBuilding building)
		{
			ZoomableBuildingDefinition zoomableDefinition = building.ZoomableDefinition;
			return zoomableDefinition.zoomFOV;
		}
	}
}
