using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ShowDialogCommand : Command
	{
		[Inject]
		public string key { get; set; }

		[Inject]
		public QuestDialogSetting settings { get; set; }

		[Inject]
		public Tuple<int, int> questIds { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_Dialog");
			GUIArguments args = iGUICommand.Args;
			args.Add(key);
			args.Add(settings);
			args.Add(questIds);
			guiService.Execute(iGUICommand);
		}
	}
}
