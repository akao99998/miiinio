using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyFillOrderCommand : Command
	{
		[Inject]
		public int forceFillOrderId { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenuSignal { get; set; }

		public override void Execute()
		{
			globalSFX.Dispatch("Play_menu_popUp_01");
			closeAllOtherMenuSignal.Dispatch(null);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "SocialPartyFillOrderScreen");
			iGUICommand.skrimScreen = "SocialSkrim";
			iGUICommand.darkSkrim = true;
			if (forceFillOrderId > 0)
			{
				GUIAutoAction<int> gUIAutoAction = new GUIAutoAction<int>();
				gUIAutoAction.value = forceFillOrderId;
				iGUICommand.Args.Add(gUIAutoAction);
			}
			guiService.Execute(iGUICommand);
		}
	}
}
