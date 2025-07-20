using System;
using UnityEngine;

namespace Kampai.Util
{
	public struct Point : IEquatable<Point>
	{
		public int x;

		public int y;

		public Vector3 XYProjection
		{
			get
			{
				return new Vector3(x, y, 0f);
			}
			set
			{
				x = Mathf.RoundToInt(value.x);
				y = Mathf.RoundToInt(value.y);
			}
		}

		public Vector3 XZProjection
		{
			get
			{
				return new Vector3(x, 0f, y);
			}
			set
			{
				x = Mathf.RoundToInt(value.x);
				y = Mathf.RoundToInt(value.z);
			}
		}

		public Point(int xVal, int yVal)
		{
			x = xVal;
			y = yVal;
		}

		public Point(float xVal, float yVal)
		{
			x = (int)xVal;
			y = (int)yVal;
		}

		public static Point FromVector3(Vector3 pos)
		{
			return new Point(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
		}

		public static float Distance(Point a, Point b)
		{
			int num = b.x - a.x;
			int num2 = b.y - a.y;
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		public static float DistanceSquared(Point a, Point b)
		{
			int num = b.x - a.x;
			int num2 = b.y - a.y;
			return num * num + num2 * num2;
		}

		public static int RoundedDistance(Point a, Point b)
		{
			int num = b.x - a.x;
			int num2 = b.y - a.y;
			return Mathf.RoundToInt(Mathf.Sqrt(num * num + num2 * num2));
		}

		public bool Equals(Point other)
		{
			return other.x == x && other.y == y;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			Point point = (Point)obj;
			return point.x == x && point.y == y;
		}

		public override int GetHashCode()
		{
			return x ^ y;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", x, y);
		}

		public static implicit operator Vector2(Point p)
		{
			return new Vector2(p.x, p.y);
		}

		public static bool operator ==(Point a, Point b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Point a, Point b)
		{
			return !a.Equals(b);
		}
	}
}
