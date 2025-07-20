using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayConfirmationCommand : Command
	{
		[Inject]
		public PopupConfirmationSetting confirmationSetting { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_Confirmation");
			iGUICommand.Args.Add(confirmationSetting);
			iGUICommand.skrimScreen = "ConfirmationSkrim";
			iGUICommand.disableSkrimButton = true;
			guiService.Execute(iGUICommand);
		}
	}
}
