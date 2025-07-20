using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayLevelUpRewardCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UnlockCharacterModel unlockCharacterModel { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public bool isInspiratioReward { get; set; }

		public override void Execute()
		{
			uiModel.LevelUpUIOpen = true;
			TransactionDefinition transaction = ((!isInspiratioReward) ? RewardUtil.GetPartyTransaction(definitionService, playerService) : RewardUtil.GetRewardTransaction(definitionService, playerService));
			List<RewardQuantity> rewardQuantityFromTransaction = RewardUtil.GetRewardQuantityFromTransaction(transaction, definitionService, playerService);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "screen_PhilsInspiration");
			iGUICommand.Args.Add(rewardQuantityFromTransaction);
			iGUICommand.Args.Add(isInspiratioReward);
			iGUICommand.ShouldShowPredicate = () => unlockCharacterModel.characterUnlocks.Count == 0;
			guiService.Execute(iGUICommand);
		}
	}
}
