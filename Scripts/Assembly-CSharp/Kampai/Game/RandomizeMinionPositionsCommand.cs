using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RandomizeMinionPositionsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RandomizeMinionPositionsCommand") as IKampaiLogger;

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public IPlayerService player { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			Queue<int> minionListSortedByDistanceAndState = component.GetMinionListSortedByDistanceAndState(Vector3.zero, false);
			while (minionListSortedByDistanceAndState.Count > 0)
			{
				int num = minionListSortedByDistanceAndState.Dequeue();
				MinionObject minionObject = component.Get(num);
				Minion byInstanceId = player.GetByInstanceId<Minion>(num);
				minionObject.transform.position = pathFinder.RandomPosition(byInstanceId.Partying);
			}
			logger.Debug("RandomizeMinionPositionsCommand: Completed");
		}
	}
}
