using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartPremiumPurchaseCommand : Command
	{
		[Inject]
		public KampaiPendingTransaction kampaiPendingTransaction { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		public override void Execute()
		{
			if (playerService.GetPendingTransactions().Count > 0)
			{
				string @string = localService.GetString("PendingTransaction");
				popupMessageSignal.Dispatch(@string, PopupMessageType.QUEUED);
			}
			else
			{
				playerService.QueuePendingTransaction(kampaiPendingTransaction);
				currencyService.RequestPurchase(kampaiPendingTransaction);
			}
		}
	}
}
