using System.Collections.Generic;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StopPartyPointBuffCommand : Command
	{
		[Inject]
		public Tuple<int, int, float> tuple { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			int item = tuple.Item1;
			timeEventService.StopBuff(TimeEventType.LeisureBuff, tuple.Item2);
			AdjustLeisureBuildingTimes(item, tuple.Item2, tuple.Item3);
		}

		private void AdjustLeisureBuildingTimes(int buffStartTime, int buffDuration, float multipler)
		{
			List<LeisureBuilding> instancesByType = playerService.GetInstancesByType<LeisureBuilding>();
			foreach (LeisureBuilding item in instancesByType)
			{
				if (item.State == BuildingState.Working)
				{
					if (buffStartTime < item.UTCLastTaskingTimeStarted)
					{
						int num = buffDuration + buffStartTime - item.UTCLastTaskingTimeStarted;
						item.UTCLastTaskingTimeStarted -= (int)((float)num * multipler);
					}
					else
					{
						item.UTCLastTaskingTimeStarted -= (int)((float)buffDuration * multipler);
					}
				}
			}
		}
	}
}
