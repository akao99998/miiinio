using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class FloatLocation : IEquatable<FloatLocation>
	{
		public float x { get; set; }

		public float y { get; set; }

		[JsonConstructor]
		public FloatLocation(float xPos = 0f, float yPos = 0f)
		{
			x = xPos;
			y = yPos;
		}

		public FloatLocation(Vector3 pos)
		{
			x = pos.x;
			y = pos.z;
		}

		public static float Distance(Location l1, Location l2)
		{
			float num = l2.x - l1.x;
			float num2 = l2.y - l1.y;
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		public override bool Equals(object obj)
		{
			return obj is FloatLocation;
		}

		public bool Equals(FloatLocation obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(FloatLocation))
			{
				return false;
			}
			return Math.Abs(x - obj.x) < 0.001f && Math.Abs(y - obj.y) < 0.001f;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}

		public static explicit operator Vector3(FloatLocation l)
		{
			return new Vector3(l.x, 0f, l.y);
		}
	}
}
