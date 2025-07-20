using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelPremiumPurchaseCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CancelPremiumPurchaseCommand") as IKampaiLogger;

		[Inject]
		public string ExternalIdentifier { get; set; }

		[Inject]
		public uint errorCode { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ShowBillingNotAvailablePanelSignal showBillingNotAvailablePanelSignal { get; set; }

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		public override void Execute()
		{
			logger.Debug("[NCS] CancelPremiumPurchaseCommand.Execute: ExternalIdentifier = {0}, cancel Kampai pending tr-n", ExternalIdentifier);
			playerService.CancelPendingTransaction(ExternalIdentifier);
			switch (errorCode)
			{
			case 20000u:
				logger.Debug("[NCS] CancelPremiumPurchaseCommand.Execute: show billing not available UI, error {0}", errorCode);
				showBillingNotAvailablePanelSignal.Dispatch();
				break;
			case 20006u:
			case 20019u:
			case 30001u:
				logger.Debug("[NCS] CancelPremiumPurchaseCommand.Execute: skip error UI dialog on error: {0}", errorCode);
				break;
			default:
			{
				logger.Debug("[NCS] CancelPremiumPurchaseCommand.Execute: show cancel tr-n UI, errorCode: {0}", errorCode);
				string @string = localService.GetString("CancelTransaction");
				popupMessageSignal.Dispatch(@string, PopupMessageType.QUEUED);
				break;
			}
			}
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, false));
		}
	}
}
