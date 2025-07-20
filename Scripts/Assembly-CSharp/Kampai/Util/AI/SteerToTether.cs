using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToTether : SteeringBehaviour
	{
		public Vector3 Tether;

		public float MaxDist = 15f;

		public override int Priority
		{
			get
			{
				return 90;
			}
		}

		public override Vector3 Force
		{
			get
			{
				Vector3 result = Vector3.zero;
				Vector3 vector = Tether - agent.Position;
				if (vector.sqrMagnitude > MaxDist * MaxDist)
				{
					result = (vector + agent.Velocity) * 0.5f;
				}
				return result;
			}
		}
	}
}
