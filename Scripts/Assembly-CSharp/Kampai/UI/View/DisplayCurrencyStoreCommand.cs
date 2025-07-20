using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayCurrencyStoreCommand : Command
	{
		[Inject]
		public Tuple<int, int> categorySettings { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_Store");
			GUIArguments args = iGUICommand.Args;
			args.Add(categorySettings);
			guiService.Execute(iGUICommand);
		}
	}
}
