using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class CurrencyButtonMediator : Mediator
	{
		private bool purchaseOnDisable;

		[Inject]
		public CurrencyButtonView view { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public CloseHUDSignal closeSignal { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		[Inject]
		public CurrencyButtonClickSignal clickedSignal { get; set; }

		public override void OnRegister()
		{
			view.InfoClickedSignal.AddListener(OnInfoClicked);
			view.PurchaseClickedSignal.AddListener(OnPurchaseClicked);
			purchaseOnDisable = false;
		}

		public override void OnRemove()
		{
			view.InfoClickedSignal.RemoveListener(OnInfoClicked);
			view.PurchaseClickedSignal.RemoveListener(OnPurchaseClicked);
		}

		private void OnInfoClicked()
		{
			if (view.PlaySoundOnClick)
			{
				playSFXSignal.Dispatch("Play_button_click_01");
			}
			openUpSellModalSignal.Dispatch(definitionService.Get<PackDefinition>(view.Definition.ReferencedDefID), "MTXStore", false);
			cancelPurchaseSignal.Dispatch(false);
		}

		private void OnPurchaseClicked()
		{
			if (view.PlaySoundOnClick)
			{
				playSFXSignal.Dispatch("Play_button_click_01");
			}
			purchaseOnDisable = true;
			closeSignal.Dispatch(false);
		}

		private void OnDisable()
		{
			if (purchaseOnDisable)
			{
				purchaseOnDisable = false;
				bool type = false;
				PackDefinition definition;
				if (definitionService.TryGet<PackDefinition>(view.Definition.ReferencedDefID, out definition))
				{
					type = definition.DisableDynamicUnlock;
				}
				clickedSignal.Dispatch(view.Definition, type);
			}
		}
	}
}
