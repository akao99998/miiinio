using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class CurrencyButtonClickCommand : Command
	{
		[Inject]
		public StoreItemDefinition definition { get; set; }

		[Inject]
		public bool disableDynamicUnlock { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public StartPremiumPurchaseSignal startPremiumPurchaseSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public InsufficientInputsSignal insufficientInputsSignal { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		private void ShowConnectionErrorPopup()
		{
			popupMessageSignal.Dispatch(localService.GetString("NoInternetConnection"), PopupMessageType.NORMAL);
		}

		private void StartPurchase(PremiumCurrencyItemDefinition premium)
		{
			KampaiPendingTransaction kampaiPendingTransaction = new KampaiPendingTransaction();
			kampaiPendingTransaction.ExternalIdentifier = premium.SKU;
			if (definition.TransactionID != 0)
			{
				kampaiPendingTransaction.StoreItemDefinitionId = definition.ID;
				kampaiPendingTransaction.TransactionInstance = definitionService.Get<TransactionDefinition>(definition.TransactionID).ToInstance();
			}
			else
			{
				CurrencyStorePackDefinition currencyStorePackDefinition = definitionService.Get<CurrencyStorePackDefinition>(definition.ReferencedDefID);
				kampaiPendingTransaction.TransactionInstance = currencyStorePackDefinition.TransactionDefinition;
				kampaiPendingTransaction.StoreItemDefinitionId = definition.ReferencedDefID;
			}
			if (!disableDynamicUnlock)
			{
				BuildingPacksHelper.UpdateTransactionUnlocksList(kampaiPendingTransaction.TransactionInstance, base.injectionBinder);
			}
			kampaiPendingTransaction.UTCTimeCreated = timeService.CurrentTime();
			startPremiumPurchaseSignal.Dispatch(kampaiPendingTransaction);
		}

		public override void Execute()
		{
			bool flag = NetworkUtil.IsConnected();
			CurrencyItemDefinition currencyItemDefinition = definitionService.Get<CurrencyItemDefinition>(definition.ReferencedDefID);
			PremiumCurrencyItemDefinition premiumCurrencyItemDefinition = currencyItemDefinition as PremiumCurrencyItemDefinition;
			if (premiumCurrencyItemDefinition != null)
			{
				if (flag)
				{
					StartPurchase(premiumCurrencyItemDefinition);
				}
				else
				{
					ShowConnectionErrorPopup();
				}
			}
			else if (playerService.VerifyTransaction(definition.TransactionID))
			{
				playerService.RunEntireTransaction(definition.TransactionID, TransactionTarget.CURRENCY, GrindCurrencyTransactionCallback);
			}
			else if (flag)
			{
				TransactionDefinition pendingTransaction = definitionService.Get<TransactionDefinition>(definition.TransactionID);
				PendingCurrencyTransaction pendingCurrencyTransaction = new PendingCurrencyTransaction(pendingTransaction, false, 0, null, null, GrindFromPremiumCallback);
				currencyService.CurrencyDialogOpened(pendingCurrencyTransaction);
				insufficientInputsSignal.Dispatch(pendingCurrencyTransaction, true);
			}
			else
			{
				ShowConnectionErrorPopup();
			}
		}

		private void GrindFromPremiumCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				showMTXStoreSignal.Dispatch(new Tuple<int, int>(800001, 0));
			}
		}

		private void GrindCurrencyTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				currencyService.CurrencyDialogClosed(true);
				setGrindCurrencySignal.Dispatch();
				setPremiumCurrencySignal.Dispatch();
			}
			else if (pct.ParentSuccess)
			{
				Execute();
			}
			else
			{
				currencyService.CurrencyDialogClosed(false);
			}
		}
	}
}
