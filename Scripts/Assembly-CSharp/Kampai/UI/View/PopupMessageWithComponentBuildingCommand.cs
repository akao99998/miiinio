using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class PopupMessageWithComponentBuildingCommand : Command
	{
		private float fadeTime = 0.5f;

		private float openDuration = 2f;

		[Inject]
		public string localizedText { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public bool autoCloseOverride { get; set; }

		[Inject]
		public int componentBuildingDefinitionID { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		public override void Execute()
		{
			closeAllDialogsSignal.Dispatch();
			if (ghostService.DisplayAutoCloseGhostComponent(componentBuildingDefinitionID, fadeTime, openDuration))
			{
				IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.LoadStatic, "popup_LairMessageBox");
				GUIArguments args = iGUICommand.Args;
				args.Add(localizedText);
				args.Add(MessagePopUpAnchor.TOP_CENTER);
				args.Add(autoCloseOverride);
				args.Add(new Tuple<float, float>(fadeTime, openDuration));
				guiService.Execute(iGUICommand);
			}
		}
	}
}
