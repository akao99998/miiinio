using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class Location : IEquatable<Location>
	{
		public int x { get; set; }

		public int y { get; set; }

		[JsonConstructor]
		public Location(int xPos = 0, int yPos = 0)
		{
			x = xPos;
			y = yPos;
		}

		public Location(Vector3 pos)
		{
			x = Mathf.RoundToInt(pos.x);
			y = Mathf.RoundToInt(pos.z);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(x, 0f, y);
		}

		public static float Distance(Location l1, Location l2)
		{
			int num = l2.x - l1.x;
			int num2 = l2.y - l1.y;
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		public override bool Equals(object obj)
		{
			Location location = obj as Location;
			if (location == null)
			{
				return false;
			}
			return Equals(location);
		}

		public bool Equals(Location other)
		{
			return x == other.x && y == other.y;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}

		public static explicit operator Vector3(Location l)
		{
			return new Vector3(l.x, 0f, l.y);
		}
	}
}
