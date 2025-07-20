using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class CurrencyWarningDialogMediator : UIStackMediator<CurrencyWarningDialogView>
	{
		private CurrencyWarningModel model;

		private bool purchaseInProgress;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.PurchaseButton.ClickedSignal.AddListener(PurchaseButtonClicked);
			base.view.CancelButton.ClickedSignal.AddListener(CancelButtonClicked);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.PurchaseButton.ClickedSignal.RemoveListener(PurchaseButtonClicked);
			base.view.CancelButton.ClickedSignal.RemoveListener(CancelButtonClicked);
		}

		protected override void Close()
		{
			Close();
		}

		public override void Initialize(GUIArguments args)
		{
			CurrencyWarningModel currencyQuantity = args.Get<CurrencyWarningModel>();
			SetCurrencyQuantity(currencyQuantity);
			purchaseInProgress = false;
			sfxSignal.Dispatch("Play_not_enough_items_01");
		}

		private void SetCurrencyQuantity(CurrencyWarningModel model)
		{
			this.model = model;
			base.view.SetCurrencyNeeded(model.Cost, model.Amount);
		}

		private bool CheckCurrencyType()
		{
			if (model.Type.Equals(StoreItemType.GrindCurrency) && playerService.CanAffordExchange(model.Amount))
			{
				playerService.ExchangePremiumForGrind(model.Amount, TransactionCallback);
				return true;
			}
			if (model.Type.Equals(StoreItemType.PremiumCurrency) && playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID) >= model.Amount)
			{
				Close(true);
				return true;
			}
			return false;
		}

		private void PurchaseButtonClicked()
		{
			if ((base.view.PurchaseButton.isDoubleConfirmed() || model.Type == StoreItemType.PremiumCurrency) && !CheckCurrencyType())
			{
				purchaseInProgress = true;
				currencyService.CurrencyDialogOpened(new PendingCurrencyTransaction(null, false, model.Amount, null, null, PremiumStoreClosedCallback));
				showMTXStoreSignal.Dispatch(new Tuple<int, int>(800002, model.Cost));
			}
		}

		private void PremiumStoreClosedCallback(PendingCurrencyTransaction pct)
		{
			CheckCurrencyType();
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				sfxSignal.Dispatch("Play_button_premium_01");
				Close(true);
			}
			else if (pct.ParentSuccess)
			{
				PurchaseButtonClicked();
			}
			else
			{
				Close();
			}
			setPremiumCurrencySignal.Dispatch();
			setGrindCurrencySignal.Dispatch();
		}

		private void CancelButtonClicked()
		{
			Close();
			createWayFinderSignal.Dispatch(new WayFinderSettings(309, false));
		}

		private void Close(bool success = false)
		{
			hideSkrim.Dispatch("CurrencySkrim");
			currencyService.CurrencyDialogClosed(success);
			SendRushTelemetry(model.PendingTransaction, model.PendingTransaction.GetPendingTransaction().Inputs, success | purchaseInProgress);
			if (model.Type.Equals(StoreItemType.GrindCurrency))
			{
				guiService.Execute(GUIOperation.Unload, "GrindCurrencyWarning");
			}
			else if (model.Type.Equals(StoreItemType.PremiumCurrency))
			{
				guiService.Execute(GUIOperation.Unload, "PremiumCurrencyWarning");
			}
		}

		private void SendRushTelemetry(PendingCurrencyTransaction pct, IList<QuantityItem> requiredItems, bool purchaseSuccess)
		{
			string sourceName = "unknown";
			if (pct != null && pct.GetTransactionArg() != null)
			{
				sourceName = pct.GetTransactionArg().Source;
			}
			telemetryService.Send_Telemetry_EVT_PINCH_PROMPT(sourceName, pct, requiredItems, purchaseSuccess.ToString());
		}
	}
}
