using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MoveMinionCommand : Command
	{
		[Inject]
		public int minionID { get; set; }

		[Inject]
		public Vector3 goalPos { get; set; }

		[Inject]
		public bool muteMinion { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public MinionWalkPathSignal walkPathSignal { get; set; }

		[Inject]
		public MinionRunPathSignal runPathSignal { get; set; }

		[Inject]
		public MinionAppearSignal appearSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

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
				float num = Random.Range(-0.5f, 0.5f);
				float num2 = Random.Range(-0.5f, 0.5f);
				float num3 = 0.5f;
				for (int i = 1; i < list.Count - 1; i++)
				{
					list[i] = new Vector3(list[i].x + num, list[i].y, list[i].z + num2);
					num = Mathf.Clamp(num + Random.Range(0f - num3, num3), -0.5f, 0.5f);
					num2 = Mathf.Clamp(num2 + Random.Range(0f - num3, num3), -0.5f, 0.5f);
				}
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
				if (byInstanceId.State == MinionState.Selected)
				{
					runPathSignal.Dispatch(minionID, list, 3.7f, muteMinion);
					return;
				}
				float type = ((byInstanceId.State == MinionState.Idle) ? 2f : 4.5f);
				walkPathSignal.Dispatch(minionID, list, type, muteMinion);
			}
			else
			{
				Point point = new Point(Mathf.RoundToInt(goalPos.x), Mathf.RoundToInt(goalPos.z));
				if (environment.IsWalkable(point.x, point.y))
				{
					appearSignal.Dispatch(minionID, goalPos);
				}
			}
		}
	}
}
