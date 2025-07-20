using Kampai.Game;
using Kampai.Main;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class OpenRewardedAdWatchModalCommand : Command
	{
		[Inject]
		public AdPlacementInstance adPlacementInstance { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenuSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		public override void Execute()
		{
			closeAllOtherMenuSignal.Dispatch(null);
			LoadRewardedModalUI();
			sfxSignal.Dispatch("Play_menu_popUp_01");
		}

		public void LoadRewardedModalUI()
		{
			string text = "popup_WatchRewardedAd";
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, text);
			iGUICommand.skrimScreen = "RewardedAdWatch";
			iGUICommand.darkSkrim = true;
			GUIArguments args = iGUICommand.Args;
			args.Add(text);
			args.Add(typeof(AdPlacementInstance), adPlacementInstance);
			guiService.Execute(iGUICommand);
		}
	}
}
