using Kampai.Game;
using Kampai.Game.Transaction;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayMasterPlanBonusRewardCommand : Command
	{
		[Inject]
		public int transactionDefinitionID { get; set; }

		[Inject]
		public MasterPlanComponent component { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			TransactionDefinition value = definitionService.Get<TransactionDefinition>(transactionDefinitionID);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MasterPlanBonus");
			iGUICommand.skrimScreen = "MasterPlanBonus";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			GUIArguments args = iGUICommand.Args;
			args.Add(value);
			args.Add(component);
			args.Add(true);
			guiService.Execute(iGUICommand);
		}
	}
}
