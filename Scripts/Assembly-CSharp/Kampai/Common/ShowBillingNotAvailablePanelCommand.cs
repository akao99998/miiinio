using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowBillingNotAvailablePanelCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "BillingNotAvailablePanel");
			iGUICommand.skrimScreen = "BillingSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
