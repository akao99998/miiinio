using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EndSaleCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("EndSaleCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public UpdateSaleBadgeSignal updateSaleBadgeSignal { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public int instanceId { get; set; }

		public override void Execute()
		{
			logger.Debug("Sale Ended: {0}", instanceId);
			Sale byInstanceId = playerService.GetByInstanceId<Sale>(instanceId);
			if (byInstanceId != null)
			{
				timeEventService.RemoveEvent(instanceId);
				playerService.AddUpsellToPurchased(byInstanceId.Definition.ID);
				byInstanceId.Finished = true;
				updateSaleBadgeSignal.Dispatch();
				reconcileSalesSignal.Dispatch(0);
			}
		}
	}
}
