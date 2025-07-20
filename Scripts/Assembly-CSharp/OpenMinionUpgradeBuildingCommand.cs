using Kampai.UI.View;
using strange.extensions.command.impl;

public class OpenMinionUpgradeBuildingCommand : Command
{
	[Inject]
	public IGUIService guiService { get; set; }

	public override void Execute()
	{
		IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MinionUpgrade");
		iGUICommand.darkSkrim = false;
		guiService.Execute(iGUICommand);
	}
}
