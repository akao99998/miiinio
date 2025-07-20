using System;
using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class Agent : MonoBehaviour, SpatiallySortable
	{
		public const int DEFAULT_UPDATE_INTERVAL = 3;

		[SerializeField]
		protected float mass = 1f;

		[SerializeField]
		protected float radius = 0.5f;

		[SerializeField]
		protected float maxForce = 8f;

		[SerializeField]
		protected float maxSpeed = 2f;

		private float speed;

		private Vector3 forward = Vector3.forward;

		private Vector3 right = Vector3.right;

		protected SteeringBehaviour[] steeringBehaviours;

		private float stuckTime;

		private static List<Agent> agentList = new List<Agent>();

		public static KDTree<Agent> Agents;

		private static int lastUpdateFrame = -1;

		private Queue<Point> points = new Queue<Point>();

		private Vector3 PrevForce = Vector3.zero;

		private int updateInterval = 3;

		private int InstanceID;

		public float Mass
		{
			get
			{
				return mass;
			}
			set
			{
				mass = value;
			}
		}

		public float Radius
		{
			get
			{
				return radius;
			}
			set
			{
				radius = value;
			}
		}

		public float MaxForce
		{
			get
			{
				return maxForce;
			}
			set
			{
				maxForce = value;
			}
		}

		public float MaxSpeed
		{
			get
			{
				return maxSpeed;
			}
			set
			{
				maxSpeed = value;
			}
		}

		public Vector3 Position { get; protected set; }

		public float Speed
		{
			get
			{
				return speed;
			}
			protected set
			{
				speed = value;
				Velocity = Forward * speed;
			}
		}

		public Vector3 Velocity { get; private set; }

		public Vector3 Acceleration { get; protected set; }

		public Transform Transform { get; private set; }

		public Vector3 Forward
		{
			get
			{
				return forward;
			}
			protected set
			{
				forward = value;
				right = Vector3.Cross(Vector3.up, forward);
				Velocity = forward * speed;
			}
		}

		public Vector3 Right
		{
			get
			{
				return right;
			}
			protected set
			{
				right = value;
				forward = Vector3.Cross(right, Vector3.up);
				Velocity = forward * speed;
			}
		}

		[Inject]
		public Kampai.Game.Environment environment { get; set; }

		public int UpdateInterval
		{
			get
			{
				return updateInterval;
			}
			set
			{
				if (value > 0)
				{
					updateInterval = value;
				}
			}
		}

		private void OnEnable()
		{
			agentList.Add(this);
			if (Transform == null)
			{
				Transform = GetComponent<Transform>();
			}
			Position = Transform.position;
			Forward = VectorUtils.ZeroY(Transform.forward);
		}

		private void OnDisable()
		{
			agentList.Remove(this);
		}

		protected virtual void Start()
		{
			Transform = GetComponent<Transform>();
			Transform.parent = null;
			UpdateSteeringBehaviours();
			Speed = MaxSpeed;
			InstanceID = base.gameObject.GetInstanceID();
		}

		public void UpdateSteeringBehaviours()
		{
			steeringBehaviours = GetComponents<SteeringBehaviour>();
			Array.Sort(steeringBehaviours, (SteeringBehaviour a, SteeringBehaviour b) => a.Priority.CompareTo(b.Priority));
		}

		protected virtual void Update()
		{
			if (lastUpdateFrame != Time.frameCount)
			{
				if (Agents == null)
				{
					Agents = new KDTree<Agent>(agentList);
				}
				else
				{
					Agents.Rebuild(agentList);
				}
				lastUpdateFrame = Time.frameCount;
			}
			Position = Transform.position;
			Forward = VectorUtils.ZeroY(Transform.forward);
			if (MaxSpeed > 0f)
			{
				Point p = Point.FromVector3(Position);
				if (environment.IsOccupied(p.x, p.y) && !environment.IsWalkable(p.x, p.y))
				{
					MoveToNavigable(p);
				}
				else
				{
					UpdateNormally(p);
				}
			}
			else
			{
				Acceleration = Vector3.zero;
				Speed = 0f;
			}
		}

		protected virtual Vector3 CalculateForces(float deltaTime)
		{
			Vector3 vector = Vector3.zero;
			if (PrevForce == Vector3.zero || (Time.frameCount + InstanceID) % UpdateInterval == 0)
			{
				for (int i = 0; i < steeringBehaviours.Length; i++)
				{
					SteeringBehaviour steeringBehaviour = steeringBehaviours[i];
					if (steeringBehaviour.enabled && 0.1f < UnityEngine.Random.value)
					{
						Vector3 force = steeringBehaviour.Force;
						if (force != Vector3.zero)
						{
							vector += force;
							PrevForce = vector;
							break;
						}
					}
				}
			}
			else
			{
				vector = PrevForce;
			}
			vector.y = 0f;
			return vector;
		}

		protected virtual void ApplyForce(Vector3 force, float deltaTime)
		{
			Vector3 v = AdjustRawForce(force);
			Vector3 vector = v.Truncate(MaxForce);
			Vector3 b = vector / Mass;
			Vector3 velocity = Velocity;
			if (deltaTime > 0f)
			{
				float value = Mathf.Clamp(9f * deltaTime, 0.15f, 0.4f);
				Acceleration = Vector3.Lerp(Acceleration, b, Mathf.Clamp01(value));
			}
			velocity += Acceleration * deltaTime;
			velocity = velocity.Truncate(MaxSpeed);
			speed = velocity.magnitude;
			Position += velocity * deltaTime;
			if (Speed > 0f)
			{
				Forward = velocity.normalized;
			}
		}

		protected virtual Vector3 AdjustRawForce(Vector3 force)
		{
			if (MaxSpeed <= 0.0001f)
			{
				return Vector3.zero;
			}
			float num = 0.2f * MaxSpeed;
			if (Speed > num || force == Vector3.zero)
			{
				return force;
			}
			float f = Speed / num;
			float cosMaxAngle = Mathf.Lerp(1f, -1f, Mathf.Pow(f, 20f));
			return LimitVectorDeviation(force, cosMaxAngle, Forward, Right, true);
		}

		private void MoveToNavigable(Point p)
		{
			points.Clear();
			environment.GetClosestWalkableGridSquares(p.x, p.y, 1, points);
			speed = maxSpeed;
			Forward = (points.Dequeue().XZProjection - Position).normalized;
			Position += Velocity * Time.deltaTime;
			UpdateTransform();
		}

		private void UpdateNormally(Point p)
		{
			Vector3 force = CalculateForces(Time.deltaTime);
			ApplyForce(force, Time.deltaTime);
			p = Point.FromVector3(Position);
			if (environment.IsWalkable(p.x, p.y) && !environment.Definition.IsWater(p.x, p.y))
			{
				UpdateTransform();
				return;
			}
			if (Time.time - stuckTime > 2f)
			{
				MoveToNavigable(p);
				return;
			}
			Position = Transform.position;
			Speed = 0f;
			Acceleration = Vector3.zero;
			Forward = -Forward;
			UpdateTransform();
		}

		private Vector3 LimitVectorDeviation(Vector3 source, float cosMaxAngle, Vector3 basis, Vector3 orthoBasis, bool interiorConstraint)
		{
			float magnitude = source.magnitude;
			if (magnitude < 1E-05f)
			{
				return source;
			}
			Vector3 lhs = source / magnitude;
			float num = Vector3.Dot(lhs, basis);
			if (num == -1f)
			{
				return source + orthoBasis * 0.1f;
			}
			if (interiorConstraint)
			{
				if (num >= cosMaxAngle)
				{
					return source;
				}
			}
			else if (num <= cosMaxAngle)
			{
				return source;
			}
			float num2 = Vector3.Dot(source, basis);
			Vector3 vector = basis * num2;
			Vector3 vector2 = source - vector;
			vector2.Normalize();
			float num3 = Mathf.Sqrt(1f - cosMaxAngle * cosMaxAngle);
			Vector3 vector3 = basis * cosMaxAngle;
			Vector3 vector4 = vector2 * num3;
			return (vector3 + vector4) * magnitude;
		}

		private void UpdateTransform()
		{
			Transform.position = Position;
			Transform.forward = Forward;
		}

		private void DebugDraw()
		{
			DebugUtil.DrawXZCircle(Position, Radius);
			Debug.DrawLine(Position, Position + Velocity * (3f / MaxSpeed), new Color(0.4f, 0.4f, 1f, 1f));
			Debug.DrawLine(Position, Position + Acceleration * (3f / MaxForce), new Color(1f, 0.4f, 1f, 1f));
			if (Time.frameCount % 20 == 0)
			{
				Debug.DrawLine(Position, Position - Velocity * 0.1f, Color.gray, 2f);
			}
		}
	}
}
