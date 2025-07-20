using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BuildingReactionCommand : Command
	{
		[Inject]
		public Boxed<Vector3> buildingPos { get; set; }

		[Inject]
		public bool celebrateWithPhil { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MinionReactSignal reactSignal { get; set; }

		[Inject]
		public PhilCelebrateSignal philSignal { get; set; }

		[Inject]
		public GameLoadedModel gameLoadedModel { get; set; }

		public override void Execute()
		{
			if (!gameLoadedModel.gameLoaded)
			{
				return;
			}
			ICollection<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			List<int> list = new List<int>();
			foreach (Minion item in instancesByType)
			{
				if (item.State == MinionState.Idle)
				{
					list.Add(item.ID);
				}
			}
			reactSignal.Dispatch(list, buildingPos);
			if (celebrateWithPhil)
			{
				philSignal.Dispatch();
			}
		}
	}
}
