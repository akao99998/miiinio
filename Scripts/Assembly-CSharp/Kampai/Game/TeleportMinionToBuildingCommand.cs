using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class TeleportMinionToBuildingCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			MinionObject minionObject = component.Get(minionID);
			if (minionObject != null && minionObject.currentAction != null && minionObject.currentAction is ConstantSpeedPathAction)
			{
				minionObject.currentAction.Abort();
				minionObject.StopLocalAudio();
			}
		}
	}
}
