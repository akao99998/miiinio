using UnityEngine;

namespace Kampai.Util.AI
{
	[RequireComponent(typeof(Agent))]
	public abstract class SteeringBehaviour : MonoBehaviour
	{
		protected Agent agent;

		public abstract int Priority { get; }

		public abstract Vector3 Force { get; }

		protected virtual void Start()
		{
			agent = GetComponent<Agent>();
		}
	}
}
