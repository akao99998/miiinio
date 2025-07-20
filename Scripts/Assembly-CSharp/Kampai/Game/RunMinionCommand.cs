using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RunMinionCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public Vector3 goalPos { get; set; }

		[Inject]
		public float speed { get; set; }

		[Inject]
		public bool muteStatus { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public MinionWalkPathSignal walkPathSignal { get; set; }

		[Inject]
		public MinionAppearSignal appearSignal { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			if (!component.HasObject(minionID))
			{
				return;
			}
			Vector3 objectPosition = component.GetObjectPosition(minionID);
			IList<Vector3> list = pathFinder.FindPath(objectPosition, goalPos, 4);
			if (list != null)
			{
				walkPathSignal.Dispatch(minionID, list, speed, muteStatus);
				return;
			}
			Point point = new Point(Mathf.RoundToInt(goalPos.x), Mathf.RoundToInt(goalPos.z));
			if (environment.IsWalkable(point.x, point.y))
			{
				appearSignal.Dispatch(minionID, goalPos);
			}
		}
	}
}
