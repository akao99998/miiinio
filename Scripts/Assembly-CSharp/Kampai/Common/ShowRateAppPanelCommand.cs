using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowRateAppPanelCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "RateAppPanel");
			iGUICommand.skrimScreen = "RateAppSkrim";
			iGUICommand.darkSkrim = false;
			guiService.Execute(iGUICommand);
		}
	}
}
