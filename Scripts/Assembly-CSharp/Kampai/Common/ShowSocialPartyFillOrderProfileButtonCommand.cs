using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyFillOrderProfileButtonCommand : Command
	{
		[Inject]
		public SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData mediatorData { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = null;
			iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_TeamPlayer");
			iGUICommand.Args.Add(typeof(SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData), mediatorData);
			guiService.Execute(iGUICommand);
		}
	}
}
