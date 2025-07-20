using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadMinionDataCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadMinionDataCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService player { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public AllMinionLoadedSignal allMinionLoadedSignal { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public CreateMinionSignal createMinionSignal { get; set; }

		public override void Execute()
		{
			logger.EventStart("LoadMinionDataCommand.Execute");
			ICollection<Minion> instancesByType = player.GetInstancesByType<Minion>();
			List<Minion> minions = new List<Minion>(instancesByType);
			SortMinions(minions);
			characterService.Initialize();
			coroutineProgressMonitor.StartTask(LoadMinions(minions), "load minions");
		}

		public void SortMinions(List<Minion> minions)
		{
			minions.Sort((Minion x, Minion y) => x.UTCTaskStartTime.CompareTo(y.UTCTaskStartTime));
		}

		private IEnumerator LoadMinions(ICollection<Minion> minions)
		{
			yield return null;
			pathFinder.AllowWalkableUpdates();
			pathFinder.UpdateWalkableRegion();
			foreach (Minion i in minions)
			{
				createMinionSignal.Dispatch(i);
				yield return null;
			}
			yield return null;
			allMinionLoadedSignal.Dispatch();
			logger.EventStop("LoadMinionDataCommand.Execute");
		}
	}
}
