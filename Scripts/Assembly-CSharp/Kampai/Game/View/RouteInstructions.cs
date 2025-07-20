using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game.View
{
	public struct RouteInstructions
	{
		public MinionObject minion;

		public IList<Vector3> Path;

		public float Rotation;

		public Building TargetBuilding;

		public int StartTime;
	}
}
