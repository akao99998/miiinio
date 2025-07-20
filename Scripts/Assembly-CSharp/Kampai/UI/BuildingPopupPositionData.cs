using UnityEngine;

namespace Kampai.UI
{
	public class BuildingPopupPositionData
	{
		public Vector2 StartPosition;

		public Vector2 EndPosition;

		public BuildingPopupPositionData(Vector2 startPosition, Vector2 endPosition)
		{
			StartPosition = startPosition;
			EndPosition = endPosition;
		}
	}
}
