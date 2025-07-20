using UnityEngine;

namespace Kampai.Game
{
	public interface IZoomCameraModel
	{
		bool ZoomedIn { get; set; }

		bool ZoomInProgress { get; set; }

		BuildingZoomType LastZoomBuildingType { get; set; }

		int PreviousCameraBehavior { get; set; }

		Vector3 PreviousCameraPosition { get; set; }

		Vector3 PreviousCameraRotation { get; set; }

		float PreviousCameraFieldOfView { get; set; }

		float LastResourceZoomRotation { get; set; }

		Vector3 GetZoomedCameraPosition(ZoomableBuilding building);

		Quaternion GetZoomedCameraRotation(ZoomableBuilding building);

		float GetZoomedFOV(ZoomableBuilding building);
	}
}
