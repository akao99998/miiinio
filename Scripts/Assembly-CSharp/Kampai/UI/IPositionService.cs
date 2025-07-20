using UnityEngine;

namespace Kampai.UI
{
	public interface IPositionService
	{
		Vector2 GetUIAnchorRatioPosition(Vector3 worldPosition);

		Vector2 GetUIAnchorRatioPosition(int buildingInstanceID);

		PositionData GetPositionData(Vector3 worldPosition);

		SnappablePositionData GetSnappablePositionData(PositionData normalPositionData, ViewportBoundary boundary, bool avoidHudElements = false);

		void AddHUDElementToAvoid(GameObject toAppend, bool isCircleShape = false);

		void RemoveHUDElementToAvoid(GameObject toRemove);
	}
}
