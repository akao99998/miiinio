using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyEventCompletedCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		public override void Execute()
		{
			globalSFX.Dispatch("Play_menu_popUp_01");
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_SocialParty_EventCompleted");
			iGUICommand.skrimScreen = "SocialCompleteSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
