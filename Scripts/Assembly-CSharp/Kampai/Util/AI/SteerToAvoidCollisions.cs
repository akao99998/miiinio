using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToAvoidCollisions : SteerToAvoidEnvironment
	{
		public override int Priority
		{
			get
			{
				return 1;
			}
		}

		protected override Color DebugColor
		{
			get
			{
				return Color.yellow;
			}
		}

		protected override float Strength
		{
			get
			{
				return agent.MaxForce;
			}
		}

		protected override float SideDist
		{
			get
			{
				return 1f;
			}
		}

		public override Vector3 Force
		{
			get
			{
				Vector3 vector = CalculateForcesFromNeighbors();
				Vector3 vector2 = CalculateForces();
				float num = Vector3.Dot(vector, vector2);
				if (num < 0f)
				{
					return -agent.Velocity * agent.MaxForce;
				}
				return vector + vector2;
			}
		}

		protected Vector3 CalculateForcesFromNeighbors()
		{
			List<Agent> list = Agent.Agents.WithinRange(agent.Position, 4f * agent.Radius);
			if (list.Count < 2)
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			List<Agent>.Enumerator enumerator = list.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Agent current = enumerator.Current;
					if (current == agent)
					{
						continue;
					}
					Vector3 lhs = current.Position - agent.Position;
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
			}
			finally
			{
				enumerator.Dispose();
			}
			return zero * agent.MaxForce;
		}

		protected override bool Obstacle(int x, int y)
		{
			return !base.environment.IsWalkable(x, y) && base.environment.IsOccupied(x, y);
		}
	}
}
