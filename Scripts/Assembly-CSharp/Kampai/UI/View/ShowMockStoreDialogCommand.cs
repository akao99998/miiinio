using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	internal sealed class ShowMockStoreDialogCommand : Command
	{
		[Inject]
		public KampaiPendingTransaction product { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "PurchaseAuthorizationWarning");
			Tuple<string, string> value = new Tuple<string, string>(product.ExternalIdentifier, currencyService.GetPriceWithCurrencyAndFormat(product.ExternalIdentifier));
			iGUICommand.Args.Add(value);
			guiService.Execute(iGUICommand);
			playSFXSignal.Dispatch("Play_not_enough_items_01");
		}
	}
}
