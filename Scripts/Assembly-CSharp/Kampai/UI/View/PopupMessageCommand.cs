using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class PopupMessageCommand : Command
	{
		[Inject]
		public string localizedText { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PopupMessageType type { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand((type == PopupMessageType.QUEUED) ? GUIOperation.Queue : GUIOperation.LoadStatic, "popup_MessageBox");
			iGUICommand.Args.Add(localizedText);
			iGUICommand.Args.Add(type == PopupMessageType.AUTO_CLOSE_OVERRIDE);
			guiService.Execute(iGUICommand);
		}
	}
}
