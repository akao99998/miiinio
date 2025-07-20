using System.Collections.Generic;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StopProductionBuffCommand : Command
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
			timeEventService.StopBuff(TimeEventType.ProductionBuff, tuple.Item2);
			AdjustMinionTaskingTimes(item, tuple.Item2, tuple.Item3);
			AdjustCraftingBuildingTimes(item, tuple.Item2, tuple.Item3);
		}

		private void AdjustMinionTaskingTimes(int buffStartTime, int buffDuration, float multipler)
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				item.IsInMinionParty = false;
				if (item.State != MinionState.Tasking)
				{
					continue;
				}
				if (buffStartTime < item.UTCTaskStartTime)
				{
					int num = buffDuration + buffStartTime - item.UTCTaskStartTime;
					if (num < 0)
					{
						num = 0;
					}
					item.PartyTimeReduction += (int)((float)num * multipler);
				}
				else
				{
					item.PartyTimeReduction += (int)((float)buffDuration * multipler);
				}
			}
		}

		private void AdjustCraftingBuildingTimes(int buffStartTime, int buffDuration, float multipler)
		{
			List<CraftingBuilding> instancesByType = playerService.GetInstancesByType<CraftingBuilding>();
			foreach (CraftingBuilding item in instancesByType)
			{
				if (item.State != BuildingState.Working && item.State != BuildingState.HarvestableAndWorking)
				{
					continue;
				}
				if (buffStartTime < item.CraftingStartTime)
				{
					int num = buffDuration + buffStartTime - item.CraftingStartTime;
					if (num < 0)
					{
						num = 0;
					}
					item.PartyTimeReduction += (int)((float)num * multipler);
				}
				else
				{
					item.PartyTimeReduction += (int)((float)buffDuration * multipler);
				}
			}
		}
	}
}
