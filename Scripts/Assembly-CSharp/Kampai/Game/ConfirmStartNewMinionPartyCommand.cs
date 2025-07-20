using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ConfirmStartNewMinionPartyCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public CancelBuildingMovementSignal cancelBuildingMovementSignal { get; set; }

		public override void Execute()
		{
			closeAllMenuSignal.Dispatch(null);
			cancelBuildingMovementSignal.Dispatch(false);
			IGUICommand iGUICommand = null;
			iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_StartPartyPopup");
			iGUICommand.skrimScreen = "StartPartySkirm";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			iGUICommand.alphaAmt = 0.5f;
			iGUICommand.skrimBehavior = SkrimBehavior.partyEffectsAndFade;
			guiService.Execute(iGUICommand);
		}
	}
}
