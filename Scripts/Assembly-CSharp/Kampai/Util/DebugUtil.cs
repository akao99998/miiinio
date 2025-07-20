using System;
using UnityEngine;

namespace Kampai.Util
{
	public static class DebugUtil
	{
		public static void DrawXZCircle(Vector3 center, float radius)
		{
			DrawXZCircle(center, radius, 18, Color.white);
		}

		public static void DrawXZCircle(Vector3 center, float radius, Color color)
		{
			DrawXZCircle(center, radius, 18, color);
		}

		public static void DrawXZCircle(Vector3 center, float radius, int segments, Color color)
		{
			Vector3 start = center;
			start.x += radius * Mathf.Cos(0f);
			start.z += radius * Mathf.Sin(0f);
			int num = 360 / segments;
			for (int i = num; i <= 360; i += num)
			{
				Vector3 vector = center;
				vector.x += radius * Mathf.Cos((float)Math.PI / 180f * (float)i);
				vector.z += radius * Mathf.Sin((float)Math.PI / 180f * (float)i);
				Debug.DrawLine(start, vector, color);
				start = vector;
			}
		}
	}
}
