using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public static class Bresenham
	{
		public static IList<Point> Line(Vector3 start, Vector3 dir, float dist)
		{
			Vector3 xZProjection = start + dir * dist;
			Point start2 = default(Point);
			start2.XZProjection = start;
			Point end = default(Point);
			end.XZProjection = xZProjection;
			return Line(start2, end);
		}

		public static IList<Point> Line(Point start, Point end)
		{
			List<Point> list = new List<Point>();
			bool flag = Mathf.Abs(end.y - start.y) > Mathf.Abs(end.x - start.x);
			if (flag)
			{
				Swap(ref start.x, ref start.y);
				Swap(ref end.x, ref end.y);
			}
			bool flag2 = false;
			if (start.x > end.x)
			{
				Swap(ref start.x, ref end.x);
				Swap(ref start.y, ref end.y);
				flag2 = true;
			}
			int num = end.x - start.x;
			int num2 = Mathf.Abs(end.y - start.y);
			int num3 = num / 2;
			int num4 = ((start.y < end.y) ? 1 : (-1));
			int num5 = start.y;
			for (int i = start.x; i <= end.x; i++)
			{
				if (flag)
				{
					list.Add(new Point(num5, i));
				}
				else
				{
					list.Add(new Point(i, num5));
				}
				num3 -= num2;
				if (num3 < 0)
				{
					num5 += num4;
					num3 += num;
				}
			}
			if (flag2)
			{
				list.Reverse();
			}
			return list;
		}

		private static void Swap(ref int lhs, ref int rhs)
		{
			int num = lhs;
			lhs = rhs;
			rhs = num;
		}
	}
}
