using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class QueueConfirmationCommand : Command
	{
		[Inject]
		public PopupConfirmationSetting confirmationSetting { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_Confirmation");
			iGUICommand.Args.Add(confirmationSetting);
			iGUICommand.skrimScreen = "ConfirmationSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
