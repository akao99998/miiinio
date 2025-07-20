using UnityEngine;

namespace Kampai.UI
{
	public class PositionData
	{
		public Vector3 WorldPositionInUI;

		public Vector3 ViewportPosition;

		public Vector3 ViewportDirectionFromCenter;

		public bool IsVisible;

		public PositionData(Vector3 worldPositionInUI, Vector3 viewportPosition)
		{
			WorldPositionInUI = worldPositionInUI;
			ViewportDirectionFromCenter = viewportPosition - new Vector3(0.5f, 0.5f, viewportPosition.z);
			if (viewportPosition.z < 0f)
			{
				viewportPosition.x *= -1f;
				viewportPosition.y *= -1f;
			}
			ViewportPosition = viewportPosition;
			IsVisible = viewportPosition.x >= 0f && viewportPosition.x <= 1f && viewportPosition.y >= 0f && viewportPosition.y <= 1f;
		}

		public PositionData(SnappablePositionData data)
		{
			WorldPositionInUI = data.WorldPositionInUI;
			ViewportPosition = data.ViewportPosition;
			ViewportDirectionFromCenter = data.ViewportDirectionFromCenter;
			IsVisible = data.IsVisible;
		}
	}
}
