using UnityEngine;

namespace Kampai.UI
{
	public struct ViewportBoundary
	{
		public static readonly ViewportBoundary FULLSCREEN = new ViewportBoundary(0f, 0f, 1f, 1f);

		public float Bottom { get; private set; }

		public float Left { get; private set; }

		public float Top { get; private set; }

		public float Right { get; private set; }

		public Vector2 Center { get; private set; }

		public ViewportBoundary(float bottom, float left, float top, float right)
		{
			Bottom = Mathf.Clamp01(bottom);
			Left = Mathf.Clamp01(left);
			Top = Mathf.Clamp01(top);
			Right = Mathf.Clamp01(right);
			Center = new Vector2(left + (left + Right) / 2f, bottom + (bottom + top) / 2f);
		}

		public bool Contains(Vector2 point)
		{
			return Contains(point, 0f);
		}

		public bool Contains(Vector2 point, float tolerance)
		{
			return point.x >= Left - tolerance && point.x <= Right + tolerance && point.y >= Bottom - tolerance && point.y <= Top + tolerance;
		}
	}
}
