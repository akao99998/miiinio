using UnityEngine;

namespace Kampai.UI
{
	public class SnappablePositionData
	{
		public Vector3 ClampedWorldPositionInUI;

		public Vector3 ClampedViewportPosition;

		public Vector3 WorldPositionInUI;

		public Vector3 ViewportPosition;

		public Vector3 ViewportDirectionFromCenter;

		public bool IsVisible;

		public SnappablePositionData(Vector3 worldPositionInUI, Vector3 clampedWorldPositionInUI, Vector3 viewportPosition, Vector3 clampedViewportPosition)
		{
			WorldPositionInUI = worldPositionInUI;
			ViewportDirectionFromCenter = viewportPosition - new Vector3(0.5f, 0.5f, viewportPosition.z);
			if (viewportPosition.z < 0f)
			{
				viewportPosition.x *= -1f;
				viewportPosition.y *= -1f;
			}
			ViewportPosition = viewportPosition;
			ClampedWorldPositionInUI = clampedWorldPositionInUI;
			ClampedViewportPosition = clampedViewportPosition;
			IsVisible = ViewportPosition == clampedViewportPosition;
		}
	}
}
