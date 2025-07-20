using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionStateChangeCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public MinionState state { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public IPlayerService player { get; set; }

		[Inject]
		public PartySignal partySignal { get; set; }

		[Inject]
		public IdleMinionSignal idleSignal { get; set; }

		public override void Execute()
		{
			Minion byInstanceId = player.GetByInstanceId<Minion>(minionID);
			if (byInstanceId == null)
			{
				return;
			}
			byInstanceId.State = state;
			byInstanceId.Partying = false;
			byInstanceId.IsInIncidental = false;
			if (state == MinionState.Idle)
			{
				idleSignal.Dispatch();
			}
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			if (component == null)
			{
				return;
			}
			GameObject gameObject = component.GetGameObject(minionID);
			if (gameObject == null)
			{
				return;
			}
			Collider component2 = gameObject.GetComponent<Collider>();
			if (component2 != null)
			{
				if (state == MinionState.Tasking || state == MinionState.PlayingMignette)
				{
					component2.enabled = false;
				}
				else
				{
					component2.enabled = true;
				}
			}
			partySignal.Dispatch();
		}
	}
}
