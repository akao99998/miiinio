using System;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class InsufficientInputsCommand : Command
	{
		[Inject]
		public PendingCurrencyTransaction model { get; set; }

		[Inject]
		public LoadRushDialogSignal loadRushDialogSignal { get; set; }

		[Inject]
		public bool GrindFromPremium { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public LoadCurrencyWarningSignal loadCurrencyWarningSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			bool flag = false;
			int rushCost = model.GetRushCost();
			if (model.IsRushing() && model.GetPendingTransaction() != null)
			{
				loadRushDialogSignal.Dispatch(model, RushDialogView.RushDialogType.DEFAULT);
				return;
			}
			TransactionDefinition pendingTransaction = model.GetPendingTransaction();
			if (pendingTransaction != null)
			{
				if (TransactionUtil.IsOnlyGrindInputs(pendingTransaction))
				{
					int num = TransactionUtil.SumInputsForStaticItem(pendingTransaction, StaticItem.GRIND_CURRENCY_ID);
					num -= (int)playerService.GetQuantity(StaticItem.GRIND_CURRENCY_ID);
					flag = true;
					loadCurrencyWarningSignal.Dispatch(new CurrencyWarningModel(num, playerService.PremiumCostForGrind(num), StoreItemType.GrindCurrency, false, model));
				}
				else if (TransactionUtil.IsOnlyPremiumInputs(pendingTransaction))
				{
					int num2 = TransactionUtil.SumInputsForStaticItem(pendingTransaction, StaticItem.PREMIUM_CURRENCY_ID);
					int quantity = (int)playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID);
					int cost = num2 - quantity;
					flag = true;
					loadCurrencyWarningSignal.Dispatch(new CurrencyWarningModel(num2, cost, StoreItemType.PremiumCurrency, GrindFromPremium, model));
				}
			}
			if (rushCost > 0)
			{
				int num3 = rushCost - (int)playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID);
				if (num3 > 0)
				{
					flag = true;
					loadCurrencyWarningSignal.Dispatch(new CurrencyWarningModel(rushCost, num3, StoreItemType.PremiumCurrency));
				}
			}
			if (flag)
			{
				currencyService.CurrencyDialogOpened(model);
				return;
			}
			Action<PendingCurrencyTransaction> callback = model.GetCallback();
			if (callback != null)
			{
				model.Success = false;
				callback(model);
			}
		}
	}
}
