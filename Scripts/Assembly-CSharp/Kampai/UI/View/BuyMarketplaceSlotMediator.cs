using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class BuyMarketplaceSlotMediator : Mediator
	{
		private bool isCOPPAGated;

		[Inject]
		public BuyMarketplaceSlotView view { get; set; }

		[Inject]
		public UpdateBuySlotSignal updateBuySlot { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public ShowSocialPartyFBConnectSignal showFacebookPopupSignal { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public HaltSlotMachine haltSlotMachine { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public IMarketplaceService marketPlaceService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.BuyButtonView.ClickedSignal.AddListener(OnBuyButtonClick);
			view.FacebookButtonView.ClickedSignal.AddListener(OnBuyButtonClick);
			isCOPPAGated = coppaService.Restricted();
			view.Init();
			view.SetupBuyItem(localService, definitionService, facebookService, isCOPPAGated, view.BuyItem, soundFXSignal, marketPlaceService);
			updateBuySlot.AddListener(UpdateSlot);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.BuyButtonView.ClickedSignal.RemoveListener(OnBuyButtonClick);
			view.FacebookButtonView.ClickedSignal.RemoveListener(OnBuyButtonClick);
			updateBuySlot.RemoveListener(UpdateSlot);
			view.ClearOldTweens();
		}

		private void UpdateSlot(int slotIndex, bool success)
		{
			view.BuyButtonView.ResetAnim();
			if (slotIndex == view.slotIndex)
			{
				if (success)
				{
					view.SetIsSold(true);
					tweenSignal.Dispatch(uiCamera.WorldToScreenPoint(view.ItemImage.transform.position), DestinationType.STORAGE, view.BuyItem.Definition.ItemID, false);
				}
				else
				{
					view.ShakeIcon();
					soundFXSignal.Dispatch("Play_error_button_01");
				}
			}
		}

		private void OnBuyButtonClick()
		{
			if (view.isSlotAnimationPlaying)
			{
				haltSlotMachine.Dispatch();
			}
			else if (view.CurrentState == BuyMarketplaceSlotView.State.Facebook)
			{
				facebookService.LoginSource = "Marketplace";
				showFacebookPopupSignal.Dispatch(delegate
				{
				});
			}
			else if (view.BuyButtonView.isDoubleConfirmed())
			{
				soundFXSignal.Dispatch("Play_marketplace_purchased_01");
				gameContext.injectionBinder.GetInstance<BuyMarketplaceItemSignal>().Dispatch(view.BuyItem, view.slotIndex);
			}
		}
	}
}
