using UnityEngine;

namespace Kampai.Game.View
{
	public class BridgeBuildingObject : BuildingObject
	{
		public override bool CanFadeGFX()
		{
			return false;
		}

		public override bool CanFadeSFX()
		{
			return false;
		}

		protected override Vector3 GetZoomCenterPosition()
		{
			Vector3 zoomCenterPosition = base.GetZoomCenterPosition();
			zoomCenterPosition.y = 0f;
			return zoomCenterPosition;
		}
	}
}
