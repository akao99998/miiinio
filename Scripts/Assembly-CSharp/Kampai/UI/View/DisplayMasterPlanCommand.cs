using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayMasterPlanCommand : Command
	{
		[Inject]
		public int componentIDSelectedFromPlatform { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllMessageDialogs { get; set; }

		public override void Execute()
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			MasterPlanDefinition definition = currentMasterPlan.Definition;
			int iD = definition.ID;
			bool flag = false;
			int definitionId = definition.ComponentDefinitionIDs[0];
			if (playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(definitionId) == null)
			{
				masterPlanService.CreateMasterPlanComponents(currentMasterPlan);
				flag = true;
			}
			closeAllMessageDialogs.Dispatch();
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MasterPlanComponentSelection");
			iGUICommand.skrimScreen = "MasterPlan";
			iGUICommand.darkSkrim = false;
			GUIArguments args = iGUICommand.Args;
			args.Add(new Tuple<int, int>(iD, componentIDSelectedFromPlatform));
			args.Add(flag);
			guiService.Execute(iGUICommand);
		}
	}
}
