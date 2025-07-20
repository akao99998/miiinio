using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MinionReactInRadiusCommand : Command
	{
		[Inject]
		public float radius { get; set; }

		[Inject]
		public Vector3 reactionPosition { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionReactSignal reactSignal { get; set; }

		public override void Execute()
		{
			ICollection<Agent> collection = Agent.Agents.WithinRange(reactionPosition, radius);
			List<int> list = new List<int>();
			foreach (Agent item in collection)
			{
				MinionObject component = item.GetComponent<MinionObject>();
				if (component != null)
				{
					Minion byInstanceId = playerService.GetByInstanceId<Minion>(component.ID);
					if (byInstanceId.State == MinionState.Idle)
					{
						list.Add(byInstanceId.ID);
					}
				}
			}
			Boxed<Vector3> type = new Boxed<Vector3>(new Vector3(reactionPosition.x, 0f, reactionPosition.z));
			reactSignal.Dispatch(list, type);
		}
	}
}
