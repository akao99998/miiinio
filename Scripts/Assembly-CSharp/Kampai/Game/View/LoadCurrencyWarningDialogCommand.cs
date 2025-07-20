using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class LoadCurrencyWarningDialogCommand : Command
	{
		[Inject]
		public CurrencyWarningModel model { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand;
			if (model.Type == StoreItemType.PremiumCurrency)
			{
				if (!model.GrindFromPremium)
				{
					showMTXStoreSignal.Dispatch(new Tuple<int, int>(800002, model.Cost));
					return;
				}
				iGUICommand = guiService.BuildCommand(GUIOperation.Load, "PremiumCurrencyWarning");
			}
			else
			{
				iGUICommand = guiService.BuildCommand(GUIOperation.Load, "GrindCurrencyWarning");
			}
			iGUICommand.skrimScreen = "CurrencySkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.singleSkrimClose = true;
			iGUICommand.Args.Add(model);
			guiService.Execute(iGUICommand);
		}
	}
}
