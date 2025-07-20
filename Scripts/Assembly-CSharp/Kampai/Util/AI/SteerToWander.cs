using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToWander : SteeringBehaviour
	{
		private float wanderScalar;

		public override int Priority
		{
			get
			{
				return 100;
			}
		}

		public override Vector3 Force
		{
			get
			{
				float num = 12f * Time.deltaTime;
				wanderScalar += (Random.value * 2f - 1f) * num;
				wanderScalar = Mathf.Clamp(wanderScalar, -1f, 1f);
				return agent.Right * wanderScalar;
			}
		}
	}
}
