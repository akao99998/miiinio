using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class LoadRushDialogCommand : Command
	{
		[Inject]
		public PendingCurrencyTransaction pendingCurrencyTransaction { get; set; }

		[Inject]
		public RushDialogView.RushDialogType type { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void Execute()
		{
			playSFXSignal.Dispatch("Play_not_enough_items_01");
			IGUICommand iGUICommand;
			if (type == RushDialogView.RushDialogType.STORAGE_EXPAND)
			{
				iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_MissingResources", "popup_OutOfResourceForStorage");
				iGUICommand.skrimScreen = "RushStorageSkrim";
			}
			else
			{
				iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_MissingResources");
				iGUICommand.skrimScreen = "RushSkrim";
			}
			iGUICommand.darkSkrim = true;
			iGUICommand.singleSkrimClose = true;
			iGUICommand.Args.Add(pendingCurrencyTransaction);
			iGUICommand.Args.Add(type);
			guiService.Execute(iGUICommand);
		}
	}
}
