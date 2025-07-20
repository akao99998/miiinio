using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util.AI
{
	public class SteerCharacterToSeek : SteerToSeek
	{
		public float Threshold;

		private CharacterObject obj;

		[Inject]
		public CharacterArrivedAtDestinationSignal arrivedSignal { get; set; }

		public override Vector3 Force
		{
			get
			{
				if (agent == null)
				{
					agent = GetComponent<Agent>();
				}
				if (agent == null)
				{
					return Vector3.zero;
				}
				Vector3 vector = Target - agent.Position;
				float magnitude = vector.magnitude;
				if (magnitude > Threshold)
				{
					return vector / magnitude * agent.MaxForce;
				}
				if (arrivedSignal != null && obj != null)
				{
					arrivedSignal.Dispatch(obj.ID);
				}
				base.enabled = false;
				return Vector3.zero;
			}
		}

		protected override void Start()
		{
			base.Start();
			obj = GetComponentInParent<CharacterObject>();
		}
	}
}
