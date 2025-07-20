using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToSeek : SteeringBehaviour
	{
		public Vector3 Target;

		public override int Priority
		{
			get
			{
				return 80;
			}
		}

		public override Vector3 Force
		{
			get
			{
				Vector3 vector = Target - agent.Position;
				return vector - agent.Velocity;
			}
		}
	}
}
