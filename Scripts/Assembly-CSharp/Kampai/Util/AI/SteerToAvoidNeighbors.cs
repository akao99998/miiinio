using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToAvoidNeighbors : SteeringBehaviour
	{
		public override int Priority
		{
			get
			{
				return 10;
			}
		}

		public override Vector3 Force
		{
			get
			{
				return CalculateForces();
			}
		}

		private Vector3 CalculateForces()
		{
			ICollection<Agent> collection = Agent.Agents.WithinRange(agent.Position, 4f * agent.Radius);
			if (collection.Count < 2)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			foreach (Agent item in collection)
			{
				if (item == agent)
				{
					continue;
				}
				Vector3 lhs = item.Position - agent.Position;
				float num = Vector3.Dot(lhs, agent.Forward);
				float num2 = Vector3.Dot(lhs, agent.Right);
				if (num > 0f)
				{
					if (num2 >= 0f)
					{
						zero += agent.Right * (0f - agent.MaxForce) - agent.Forward;
					}
					else
					{
						zero += agent.Right * agent.MaxForce - agent.Forward;
					}
				}
			}
			return zero * agent.MaxForce;
		}
	}
}
