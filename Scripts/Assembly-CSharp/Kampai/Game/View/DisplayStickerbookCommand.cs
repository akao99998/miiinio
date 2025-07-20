using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class DisplayStickerbookCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_Stickerbook");
			iGUICommand.skrimScreen = "StickerBookSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
