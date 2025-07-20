using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowBuddyWelcomePanelUICommand : Command
	{
		[Inject]
		public Boxed<Prestige> prestige { get; set; }

		[Inject]
		public CharacterWelcomeState state { get; set; }

		[Inject]
		public int minionCount { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void Execute()
		{
			uiModel.WelcomeBuddyOpen = true;
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_CharacterState");
			GUIArguments args = iGUICommand.Args;
			if (prestige.Value != null)
			{
				args.Add(prestige.Value);
			}
			args.Add(state);
			args.Add(minionCount);
			guiService.Execute(iGUICommand);
		}
	}
}
