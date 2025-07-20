using Kampai.Game.Trigger;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayTreasureTeaserCommand : Command
	{
		[Inject]
		public TriggerInstance instance { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CaptainWaveAndCallCallbackSignal waveAndCallbackSignal { get; set; }

		public override void Execute()
		{
			waveAndCallbackSignal.Dispatch(DisplayTreasureTeaseView, instance.Definition.TreasureIntro);
		}

		private void DisplayTreasureTeaseView()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_MysteryMinionTeaserSelectionModal");
			iGUICommand.skrimScreen = "TSMTeaseSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			iGUICommand.Args.Add(typeof(TriggerInstance), instance);
			guiService.Execute(iGUICommand);
		}
	}
}
