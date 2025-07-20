using Elevation.Logging;
using Kampai.Game.Mtx;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CollectRedemptionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CollectRedemptionCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		public override void Execute()
		{
			ReceiptValidationResult receiptValidationResult = playerService.popPendingRedemption();
			if (receiptValidationResult != null)
			{
				logger.Info("Redeeming pending Redemption, sku = " + receiptValidationResult.sku);
				currencyService.CollectRedemption(receiptValidationResult);
			}
			else
			{
				logger.Error("Attempting to Collect Redemption but there is not pending redemption");
			}
		}
	}
}
