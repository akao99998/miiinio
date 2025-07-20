using System.Collections.Generic;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class SaleItemTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1182;
			}
		}

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.MarketplaceSaleItem;
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			if (base.transaction == null || context == null)
			{
				return;
			}
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			PlayerService playerService = injectionBinder.GetInstance<IPlayerService>() as PlayerService;
			if (playerService != null && playerService.timeService != null)
			{
				ICollection<MarketplaceSaleItem> byDefinitionId = playerService.GetByDefinitionId<MarketplaceSaleItem>(1000008094);
				MarketplaceSaleItem nextForSaleItem = null;
				SaleItemTriggerConditionDefinition.GetClosestSaleItem(playerService.timeService, byDefinitionId, int.MaxValue, ref nextForSaleItem);
				if (nextForSaleItem != null)
				{
					nextForSaleItem.state = MarketplaceSaleItem.State.SOLD;
					injectionBinder.GetInstance<MarketplaceUpdateSoldItemsSignal>().Dispatch(true);
				}
			}
		}
	}
}
