using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayMasterPlanCooldownRewardDialogCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public ShowDialogSignal showDialogSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			QuestDialogSetting questDialogSetting = new QuestDialogSetting();
			questDialogSetting.type = QuestDialogType.NORMAL;
			QuestDialogSetting questDialogSetting2 = questDialogSetting;
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			questDialogSetting2.additionalStringParameter = currentMasterPlan.Definition.LocalizedKey;
			showDialogSignal.Dispatch(currentMasterPlan.Definition.CooldownRewardDialogKey, questDialogSetting2, new Tuple<int, int>(0, 0));
			promptReceivedSignal.AddOnce(DialogComplete);
		}

		private void DialogComplete(int questID, int stepID)
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			TransactionDefinition value = (masterPlanService.HasReceivedInitialRewardFromCurrentPlan() ? definitionService.Get<TransactionDefinition>(currentMasterPlan.Definition.SubsequentCooldownRewardTransactionID) : definitionService.Get<TransactionDefinition>(currentMasterPlan.Definition.CooldownRewardTransactionID));
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MasterPlanCooldownReward");
			iGUICommand.skrimScreen = "MasterPlan";
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(value);
			iGUICommand.Args.Add(currentMasterPlan.ID);
			guiService.Execute(iGUICommand);
		}
	}
}
