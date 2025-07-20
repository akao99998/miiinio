using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RushOneMinionInBuildingCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RushOneMinionInBuildingCommand") as IKampaiLogger;

		[Inject]
		public int BuildingID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RushTaskSignal rushMinionSignal { get; set; }

		public override void Execute()
		{
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(BuildingID);
			if (byInstanceId == null)
			{
				logger.Error("Trying to rush minions in a non-taskable building or a non-building.");
				return;
			}
			int num = -1;
			float num2 = -1f;
			foreach (int minion in byInstanceId.MinionList)
			{
				Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(minion);
				if (byInstanceId2 != null && (num < 0 || (float)byInstanceId2.UTCTaskStartTime < num2))
				{
					num = minion;
					num2 = byInstanceId2.UTCTaskStartTime;
				}
			}
			if (num >= 0)
			{
				rushMinionSignal.Dispatch(num);
			}
		}
	}
}
