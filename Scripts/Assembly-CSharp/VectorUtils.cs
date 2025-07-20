using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class VectorUtils
{
	public static Vector3 ZeroY(Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	public static Vector3 ZeroXZ(Vector3 vector)
	{
		return new Vector3(0f, vector.y, 0f);
	}

	public static Vector3 ZeroZ(Vector3 vector)
	{
		return new Vector3(vector.x, vector.y, 0f);
	}

	public static string PathToString(IEnumerable<Vector3> path)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Vector3 item in path)
		{
			stringBuilder.Append(item).Append(" ");
		}
		return stringBuilder.ToString();
	}
}
