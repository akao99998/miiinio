using Kampai.Game;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayMasterPlanComponentBonusDialogCommand : Command
	{
		[Inject]
		public MasterPlanDefinition masterPlanDefinition { get; set; }

		[Inject]
		public int componentIndex { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			int definitionId = masterPlanDefinition.ComponentDefinitionIDs[componentIndex];
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(definitionId);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_MasterPlanComponentBonusReward");
			iGUICommand.skrimScreen = "MasterPlanComponentBonus";
			iGUICommand.darkSkrim = false;
			iGUICommand.Args.Add(firstInstanceByDefinitionId);
			guiService.Execute(iGUICommand);
		}
	}
}
