using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class SaleSlotTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1183;
			}
		}

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.MarketplaceSaleSlot;
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			if (base.transaction != null && context != null)
			{
				ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
				injectionBinder.GetInstance<IPlayerService>().RunEntireTransaction(base.transaction.ToDefinition(), TransactionTarget.NO_VISUAL, null);
				injectionBinder.GetInstance<InitializeMarketplaceSlotsSignal>().Dispatch();
			}
		}
	}
}
