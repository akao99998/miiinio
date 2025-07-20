using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyFillOrderButtonCommand : Command
	{
		[Inject]
		public SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData mediatorData { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public int row { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = null;
			iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_SocialFillOrder");
			iGUICommand.Args.Add(typeof(SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData), mediatorData);
			iGUICommand.Args.Add(typeof(int), row);
			guiService.Execute(iGUICommand);
		}
	}
}
