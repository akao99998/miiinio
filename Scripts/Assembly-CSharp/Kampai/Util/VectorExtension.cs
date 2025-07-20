using UnityEngine;

namespace Kampai.Util
{
	public static class VectorExtension
	{
		public static Vector3 Truncate(this Vector3 v, float maxLength)
		{
			float num = maxLength * maxLength;
			float sqrMagnitude = v.sqrMagnitude;
			if (sqrMagnitude <= num)
			{
				return v;
			}
			if ((double)sqrMagnitude <= 1E-08)
			{
				return Vector3.zero;
			}
			return v * (maxLength / Mathf.Sqrt(sqrMagnitude));
		}
	}
}
