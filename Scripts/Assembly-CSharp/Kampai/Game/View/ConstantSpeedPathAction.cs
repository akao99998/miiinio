using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class ConstantSpeedPathAction : PathAction
	{
		public float Speed { get; private set; }

		public ConstantSpeedPathAction(ActionableObject obj, IList<Vector3> path, float speed, IKampaiLogger logger)
			: base(obj, path, 1f, logger)
		{
			base.obj = obj;
			base.path = new List<Vector3>(path);
			Speed = speed;
		}

		public override float Duration()
		{
			return EstimatePathLength() / Speed;
		}

		protected float EstimatePathLength()
		{
			float num = 0f;
			for (int i = 0; i < path.Count - 1; i++)
			{
				num += Vector3.Distance(path[i], path[i + 1]);
			}
			return num;
		}

		public override void LateUpdate()
		{
			obj.SetAnimBool("isMoving", true);
			obj.SetAnimFloat("speed", Speed);
		}
	}
}
