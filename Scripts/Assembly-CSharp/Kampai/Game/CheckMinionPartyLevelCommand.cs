using Kampai.Game.Transaction;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class CheckMinionPartyLevelCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public bool IsPartyStart { get; set; }

		public override void Execute()
		{
			if (!playerService.IsMinionPartyUnlocked())
			{
				return;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_PARTY_INDEX_ID);
			int newIndex = quantity;
			int quantity2 = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			int newLevel = quantity2;
			int quantity3 = (int)playerService.GetQuantity(StaticItem.XP_ID);
			int newPoints = quantity3;
			if (IsPartyStart)
			{
				partyService.GetNewLevelIndexAndPointsAfterParty(quantity2, quantity, quantity3, out newLevel, out newIndex, out newPoints);
				int amount = (quantity - newIndex) * -1;
				playerService.AlterQuantity(StaticItem.LEVEL_PARTY_INDEX_ID, amount);
				int amount2 = (quantity3 - newPoints) * -1;
				playerService.AlterQuantity(StaticItem.XP_ID, amount2);
				if (quantity2 != newLevel)
				{
					gameContext.injectionBinder.GetInstance<LevelUpSignal>().Dispatch();
					return;
				}
				TransactionDefinition partyTransaction = RewardUtil.GetPartyTransaction(definitionService, playerService);
				awardLevelSignal.Dispatch(partyTransaction);
				base.injectionBinder.GetInstance<DisplayLevelUpRewardSignal>().Dispatch(false);
			}
		}
	}
}
