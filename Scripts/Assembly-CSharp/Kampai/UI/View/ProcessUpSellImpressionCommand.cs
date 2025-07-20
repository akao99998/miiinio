using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ProcessUpSellImpressionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ProcessUpSellImpressionCommand") as IKampaiLogger;

		[Inject]
		public int salePackInstanceID { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public StartUpSellImpressionSignal startUpSellImpressionSignal { get; set; }

		public override void Execute()
		{
			if (salePackInstanceID == 0)
			{
				logger.Error("sale instance id is 0 for this impression, returning.");
				return;
			}
			Sale byInstanceId = playerService.GetByInstanceId<Sale>(salePackInstanceID);
			if (byInstanceId == null)
			{
				return;
			}
			SalePackDefinition definition = byInstanceId.Definition;
			if (definition.Impressions != 0)
			{
				byInstanceId.Impressions++;
				if (byInstanceId.Impressions < byInstanceId.Definition.Impressions)
				{
					byInstanceId.UTCLastImpressionTime = timeService.CurrentTime();
					timeEventService.AddEvent(definition.ID, byInstanceId.UTCLastImpressionTime, definition.ImpressionInterval, startUpSellImpressionSignal);
				}
			}
		}
	}
}
