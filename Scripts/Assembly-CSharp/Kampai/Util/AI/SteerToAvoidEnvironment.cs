using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerToAvoidEnvironment : SteeringBehaviour
	{
		private int modifier = 4;

		private Queue<Point> points = new Queue<Point>();

		[Inject]
		public Environment environment { get; set; }

		protected virtual Color DebugColor
		{
			get
			{
				return Color.white;
			}
		}

		protected virtual float Strength
		{
			get
			{
				return agent.MaxForce * 0.5f;
			}
		}

		protected virtual float SideDist
		{
			get
			{
				return 0.5f;
			}
		}

		public virtual int Modifier
		{
			get
			{
				return modifier;
			}
			set
			{
				modifier = value;
			}
		}

		public override int Priority
		{
			get
			{
				return 2;
			}
		}

		public override Vector3 Force
		{
			get
			{
				return CalculateForces();
			}
		}

		protected virtual Vector3 CalculateForces()
		{
			Point point = Point.FromVector3(agent.Position);
			if (Obstacle(point.x, point.y))
			{
				points.Clear();
				environment.GetClosestGridSquare(point.x, point.y, 1, points, modifier);
				return (points.Dequeue().XZProjection - agent.Position) * Strength;
			}
			Point hitPoint = Point.FromVector3(agent.Position - 0.25f * agent.Right);
			Point hitPoint2 = Point.FromVector3(agent.Position - 0.25f * agent.Right);
			if (Obstacle(hitPoint.x, hitPoint.y))
			{
				return Hit(1f, hitPoint);
			}
			if (Obstacle(hitPoint2.x, hitPoint2.y))
			{
				return Hit(-1f, hitPoint2);
			}
			Vector3 vector = SideDist * agent.Radius * agent.Right;
			Point point2 = Point.FromVector3(agent.Position - vector + agent.Forward);
			Point point3 = Point.FromVector3(agent.Position + vector + agent.Forward);
			if (point2 == point)
			{
				point2 = Point.FromVector3(agent.Position - vector + agent.Forward * 1.5f);
			}
			if (point3 == point)
			{
				point3 = Point.FromVector3(agent.Position + vector + agent.Forward * 1.5f);
			}
			if (Obstacle(point2.x, point2.y))
			{
				return Hit(1f, point2);
			}
			if (Obstacle(point3.x, point3.y))
			{
				return Hit(-1f, point3);
			}
			return Vector3.zero;
		}

		private Vector3 Hit(float hint, Point hitPoint)
		{
			return agent.Right * hint * Strength;
		}

		protected virtual bool Obstacle(int x, int y)
		{
			return !environment.CompareModifiers(x, y, modifier) || environment.Definition.IsWater(x, y);
		}
	}
}
