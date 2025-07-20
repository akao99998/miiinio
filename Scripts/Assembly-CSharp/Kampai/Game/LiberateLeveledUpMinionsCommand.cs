using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LiberateLeveledUpMinionsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LiberateLeveledUpMinionsCommand") as IKampaiLogger;

		[Inject]
		public int minLevel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RushTaskSignal rushTaskSignal { get; set; }

		public override void Execute()
		{
			Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(3002);
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				if (item.BuildingID != firstInstanceByDefinitionId.ID && item.Level >= minLevel && item.State == MinionState.Tasking)
				{
					logger.Info("Liberating {0}", item.ID);
					rushTaskSignal.Dispatch(item.ID);
				}
			}
		}
	}
}
