using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayMasterPlanCooldownAlertCommand : Command
	{
		[Inject]
		public MasterPlan plan { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			if (!timeEventService.HasEventID(plan.ID))
			{
				plan.displayCooldownAlert = false;
				return;
			}
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_MasterPlanCooldownAlert");
			iGUICommand.skrimScreen = "MasterPlanCooldownAlert";
			iGUICommand.disableSkrimButton = true;
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(plan);
			guiService.Execute(iGUICommand);
		}
	}
}
