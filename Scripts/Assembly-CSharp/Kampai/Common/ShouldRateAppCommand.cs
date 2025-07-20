using Elevation.Logging;
using Kampai.Game;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShouldRateAppCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShouldRateAppCommand") as IKampaiLogger;

		[Inject]
		public ConfigurationDefinition.RateAppAfterEvent from { get; set; }

		[Inject]
		public ILocalPersistanceService persistService { get; set; }

		[Inject]
		public ShowRateAppPanelSignal showRateAppPanelSignal { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			if (!NetworkUtil.IsConnected())
			{
				return;
			}
			string dataPlayer = persistService.GetDataPlayer("RateApp");
			if (dataPlayer == "Disabled")
			{
				return;
			}
			if (configurationsService.GetConfigurations().rateAppAfter == null)
			{
				logger.Log(KampaiLogLevel.Error, "No rate app configurations.");
				return;
			}
			bool value = false;
			if (!configurationsService.GetConfigurations().rateAppAfter.TryGetValue(from, out value))
			{
				logger.Log(KampaiLogLevel.Error, "No configuration value for key: {0}", from);
			}
			else
			{
				if (!value)
				{
					return;
				}
				uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) >= 7)
				{
					quantity -= 7;
					if (quantity % 2 == 0)
					{
						showRateAppPanelSignal.Dispatch();
					}
				}
			}
		}
	}
}
