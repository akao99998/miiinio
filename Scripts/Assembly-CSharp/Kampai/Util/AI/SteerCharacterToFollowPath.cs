using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerCharacterToFollowPath : SteerToAvoidCollisions
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SteerCharacterToFollowPath") as IKampaiLogger;

		public float Threshold = 0.25f;

		public float FinalThreshold = 0.1f;

		private KampaiQueue<Vector3> targetQueue;

		private Vector3 currentTarget;

		private CharacterObject obj;

		[Inject]
		public CharacterArrivedAtDestinationSignal arrivedSignal { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

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

		protected override void Start()
		{
			base.Start();
			obj = GetComponentInParent<CharacterObject>();
		}

		private void OnEnable()
		{
			if (agent == null)
			{
				agent = GetComponent<Agent>();
			}
			if (obj == null)
			{
				obj = GetComponentInParent<CharacterObject>();
			}
		}

		protected override Vector3 CalculateForces()
		{
			if (agent == null || obj == null)
			{
				logger.Error("MISSING Agent/Character in SteerCharacterToFollowPath");
				return Vector3.zero;
			}
			if (targetQueue == null)
			{
				return Vector3.zero;
			}
			if (targetQueue.Count == 0)
			{
				agent.UpdateInterval = 1;
			}
			else
			{
				agent.UpdateInterval = 3;
			}
			Vector3 position = agent.Position;
			float maxForce = agent.MaxForce;
			Vector3 vector = currentTarget - position;
			float magnitude = vector.magnitude;
			float num = ((targetQueue.Count != 0) ? Threshold : FinalThreshold);
			if (magnitude > num)
			{
				if (magnitude > 1.5f)
				{
					IList<Vector3> list = pathFinder.FindPath(position, currentTarget, Modifier);
					if (list != null)
					{
						List<Vector3> list2 = new List<Vector3>(list);
						if (list2.Count > 2)
						{
							for (int num2 = list2.Count - 1; num2 > 0; num2--)
							{
								targetQueue.AddFirst(list2[num2]);
							}
							do
							{
								currentTarget = targetQueue.Dequeue();
								vector = currentTarget - position;
								magnitude = vector.magnitude;
							}
							while (magnitude < Threshold && targetQueue.Count > 0);
							return vector / magnitude * maxForce;
						}
					}
				}
				Vector3 vector2 = vector / magnitude;
				float num3 = Vector3.Dot(vector2, agent.Forward);
				if (num3 < 0.4f)
				{
					return (vector2 - agent.Velocity) * maxForce;
				}
				return vector2 * maxForce;
			}
			if (arrivedSignal != null && obj != null && targetQueue.Count == 0)
			{
				arrivedSignal.Dispatch(obj.ID);
				base.enabled = false;
				targetQueue = null;
				return Vector3.zero;
			}
			if (targetQueue.Count > 0)
			{
				currentTarget = targetQueue.Dequeue();
				vector = currentTarget - position;
				magnitude = vector.magnitude;
				num = ((targetQueue.Count != 0) ? Threshold : FinalThreshold);
				if (magnitude > num)
				{
					return vector / magnitude * maxForce;
				}
				return Vector3.zero;
			}
			logger.Error("INVALID targetQueue");
			targetQueue = null;
			return Vector3.zero;
		}

		public void SetTarget(Vector3 target)
		{
			IList<Vector3> list = pathFinder.FindPath(agent.Transform.position, target, Modifier);
			if (list == null)
			{
				logger.Error("Unable to path {0} to {1} ({2})", agent.gameObject.name, target, Modifier);
				return;
			}
			targetQueue = new KampaiQueue<Vector3>(list);
			if (targetQueue != null && targetQueue.Count > 0)
			{
				float magnitude;
				do
				{
					currentTarget = targetQueue.Dequeue();
					magnitude = (currentTarget - agent.Position).magnitude;
				}
				while (magnitude < Threshold && targetQueue.Count > 0);
			}
		}

		private void OnDrawGizmos()
		{
			if (!base.enabled || targetQueue == null)
			{
				return;
			}
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(currentTarget, 0.1f);
			Gizmos.DrawLine(agent.Position, currentTarget);
			Vector3 from = currentTarget;
			Gizmos.color = Color.blue;
			foreach (Vector3 item in targetQueue)
			{
				Gizmos.DrawLine(from, item);
				from = item;
				Gizmos.DrawSphere(item, 0.1f);
			}
		}
	}
}
