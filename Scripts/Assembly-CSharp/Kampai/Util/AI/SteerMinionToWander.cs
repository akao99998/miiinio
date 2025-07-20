using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util.AI
{
	[RequireComponent(typeof(MinionObject))]
	public class SteerMinionToWander : SteerToWander
	{
		public const float minRestTime = 5f;

		public const float maxRestTime = 8f;

		private MinionObject obj;

		private float timer;

		[Inject]
		public IncidentalAnimationSignal animSignal { get; set; }

		public override Vector3 Force
		{
			get
			{
				timer -= Time.deltaTime;
				if (timer < 0f)
				{
					timer = Random.Range(5f, 8f);
					animSignal.Dispatch(obj.ID);
					return Vector3.zero;
				}
				return base.Force;
			}
		}

		protected override void Start()
		{
			base.Start();
			obj = GetComponent<MinionObject>();
			timer = Random.Range(5f, 8f);
		}
	}
}
