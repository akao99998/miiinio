using Kampai.Main;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowProceduralQuestPanelCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public int questInstanceId { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		public override void Execute()
		{
			sfxSignal.Dispatch("Play_menu_popUp_01");
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_TSM_SellItems");
			iGUICommand.skrimScreen = "ProceduralTaskSkrim";
			iGUICommand.darkSkrim = false;
			iGUICommand.Args.Add(questInstanceId);
			guiService.Execute(iGUICommand);
		}
	}
}
