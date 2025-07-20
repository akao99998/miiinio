using Kampai.Game;
using Kampai.Game.Transaction;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayMasterPlanBonusCommand : Command
	{
		[Inject]
		public int componentDefinitionId { get; set; }

		[Inject]
		public TransactionDefinition transaction { get; set; }

		[Inject]
		public MasterPlanDefinition planDefinition { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitionId);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MasterPlanBonus");
			iGUICommand.skrimScreen = "MasterPlanBonus";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			GUIArguments args = iGUICommand.Args;
			args.Add(typeof(MasterPlanComponent), firstInstanceByDefinitionId);
			args.Add(transaction);
			args.Add(false);
			args.Add(planDefinition);
			guiService.Execute(iGUICommand);
		}
	}
}
