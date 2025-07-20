using System;
using UnityEngine;

namespace Kampai.UI
{
	public class HudElementToAvoid : IEquatable<HudElementToAvoid>
	{
		public GameObject GameObject { get; private set; }

		public bool IsCircleShape { get; private set; }

		public HudElementToAvoid(GameObject gameObject, bool isCircleShape = false)
		{
			GameObject = gameObject;
			IsCircleShape = isCircleShape;
		}

		public bool Contains(GameObject gameObject)
		{
			if (gameObject == null || GameObject == null)
			{
				return false;
			}
			return GameObject.GetInstanceID().Equals(gameObject.GetInstanceID());
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}
			HudElementToAvoid hudElementToAvoid = (HudElementToAvoid)obj;
			if (hudElementToAvoid == null)
			{
				return false;
			}
			return Equals(hudElementToAvoid);
		}

		public bool Equals(HudElementToAvoid other)
		{
			if (other == null)
			{
				return false;
			}
			return Contains(other.GameObject);
		}

		public override int GetHashCode()
		{
			return GameObject.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("[HudElementToAvoid: gameObjectName: {0} gameObjectId: {1} isCircleShape: {2}]", GameObject.name, GameObject.GetInstanceID(), IsCircleShape);
		}
	}
}
