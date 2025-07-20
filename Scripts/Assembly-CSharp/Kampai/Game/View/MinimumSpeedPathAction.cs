using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class MinimumSpeedPathAction : ConstantSpeedPathAction
	{
		private float MinimumSpeed;

		private float TimeToArrive;

		public MinimumSpeedPathAction(MinionObject obj, IList<Vector3> path, float timeToArrive, float minimumSpeed, IKampaiLogger logger)
			: base(obj, path, minimumSpeed, logger)
		{
			base.obj = obj;
			base.path = new List<Vector3>(path);
			MinimumSpeed = minimumSpeed;
			TimeToArrive = timeToArrive;
		}

		public override float Duration()
		{
			float num = EstimatePathLength();
			float num2 = num / TimeToArrive;
			float result = TimeToArrive;
			if (num2 < MinimumSpeed)
			{
				result = num / MinimumSpeed;
			}
			return result;
		}

		public override void LateUpdate()
		{
			obj.SetAnimBool("isMoving", true);
			obj.SetAnimFloat("speed", base.Speed);
		}
	}
}
