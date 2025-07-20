using System;

namespace Kampai.Game
{
	public struct BuildingZoomSettings
	{
		public ZoomType ZoomType { get; private set; }

		public BuildingZoomType ZoomBuildingType { get; private set; }

		public Action OnComplete { get; private set; }

		public bool EnableCamera { get; private set; }

		public BuildingZoomSettings(ZoomType zoomType, BuildingZoomType zoomBuildingType, Action onComplete = null, bool enableCamera = true)
		{
			ZoomType = zoomType;
			ZoomBuildingType = zoomBuildingType;
			OnComplete = onComplete;
			EnableCamera = enableCamera;
		}
	}
}
