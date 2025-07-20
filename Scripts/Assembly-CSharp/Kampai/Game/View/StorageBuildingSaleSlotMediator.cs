using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class StorageBuildingSaleSlotMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StorageBuildingSaleSlotMediator") as IKampaiLogger;

		[Inject]
		public StorageBuildingSaleSlotView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public ShowSocialPartyFBConnectSignal showFacebookPopupSignal { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public OpenCreateNewSalePanelSignal createNewSalePanelSignal { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		[Inject]
		public UpdateSaleSlotsStateSignal updateSaleSlotState { get; set; }

		[Inject]
		public CollectMarketplaceSaleSignal collectSaleSignal { get; set; }

		[Inject]
		public PurchaseMarketplaceSlotSignal purchaseSlotSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public RefreshSlotsSignal refreshSlotsSignal { get; set; }

		[Inject]
		public UpdateMarketplaceSlotStateSignal updateSlotStateSignal { get; set; }

		[Inject]
		public CloseCreateNewSalePanelSignal closeCreateNewSaleSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(localizationService, definitionService.Get<MarketplaceDefinition>().SaleCancellationCost);
			updateSaleSlotState.AddListener(CheckIfNoValidItems);
			view.CheckIfValidItemsSignal.AddListener(CheckIfNoValidItems);
			view.CreateButtonView.ClickedSignal.AddListener(CreateNewSell);
			view.CollectButtonView.ClickedSignal.AddListener(CollectSale);
			view.FacebookButtonView.ClickedSignal.AddListener(FacebookButton);
			view.PremiumButtonView.ClickedSignal.AddListener(PurchaseSlot);
			view.PendingPanel.ClickedSignal.AddListener(FlipButton);
			view.CancelPendingButtonView.ClickedSignal.AddListener(CancelSaleButton);
			loginSuccess.AddListener(OnLoginSuccess);
			updateSaleSlot.AddListener(UpdateView);
			closeCreateNewSaleSignal.AddListener(CloseCreateNewSalePanel);
			updateSlotStateSignal.Dispatch();
			UpdateView(view.slotId);
		}

		public override void OnRemove()
		{
			updateSaleSlotState.RemoveListener(CheckIfNoValidItems);
			view.CheckIfValidItemsSignal.RemoveListener(CheckIfNoValidItems);
			view.CreateButtonView.ClickedSignal.RemoveListener(CreateNewSell);
			view.CollectButtonView.ClickedSignal.RemoveListener(CollectSale);
			view.FacebookButtonView.ClickedSignal.RemoveListener(FacebookButton);
			view.PremiumButtonView.ClickedSignal.RemoveListener(PurchaseSlot);
			view.PendingPanel.ClickedSignal.RemoveListener(FlipButton);
			view.CancelPendingButtonView.ClickedSignal.RemoveListener(CancelSaleButton);
			loginSuccess.RemoveListener(OnLoginSuccess);
			updateSaleSlot.RemoveListener(UpdateView);
			closeCreateNewSaleSignal.RemoveListener(CloseCreateNewSalePanel);
		}

		private void UpdateView(int slotId)
		{
			if (slotId != view.slotId)
			{
				return;
			}
			MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(view.slotId);
			if (byInstanceId == null)
			{
				return;
			}
			MarketplaceSaleItem byInstanceId2 = playerService.GetByInstanceId<MarketplaceSaleItem>(byInstanceId.itemId);
			view.UpdateSlot(byInstanceId);
			if (byInstanceId2 != null)
			{
				QuantityItem quantityItem = MarketplaceUtil.GetQuantityItem(definitionService, byInstanceId2.Definition);
				int rewardValue = MarketplaceUtil.GetRewardValue(quantityItem, byInstanceId2);
				ItemDefinition quantityItemDef = definitionService.Get<ItemDefinition>(quantityItem.ID);
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(byInstanceId2.Definition.ItemID);
				if (byInstanceId.Definition.type != MarketplaceSaleSlotDefinition.SlotType.FACEBOOK_UNLOCKABLE || facebookService.isLoggedIn || marketplaceService.DebugFacebook)
				{
					view.UpdateItem(byInstanceId2, itemDefinition, quantityItemDef, rewardValue);
				}
				view.EnableDebugTimer(marketplaceService.IsDebugMode, byInstanceId2.LengthOfSale + byInstanceId2.SaleStartTime - timeService.CurrentTime());
			}
		}

		private void CollectSale()
		{
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.fromGlass = true;
			transactionArg.StartPosition = uiCamera.WorldToScreenPoint(view.CollectButtonView.gameObject.transform.position);
			transactionArg.Source = "Marketplace";
			collectSaleSignal.Dispatch(view.slotId, transactionArg);
		}

		private void FlipButton()
		{
			view.Flip(soundFXSignal);
		}

		private void CancelSaleButton()
		{
			MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(view.slotId);
			if (byInstanceId == null)
			{
				return;
			}
			MarketplaceSaleItem byInstanceId2 = playerService.GetByInstanceId<MarketplaceSaleItem>(byInstanceId.itemId);
			if (byInstanceId2 != null)
			{
				if (view.CancelPendingButtonView.isDoubleConfirmed())
				{
					soundFXSignal.Dispatch("Play_delete_ticket_01");
					gameContext.injectionBinder.GetInstance<CancelMarketPlaceSaleSignal>().Dispatch(byInstanceId2.ID);
				}
			}
			else
			{
				logger.Error("Failed to find the pending item");
			}
		}

		private void FacebookButton()
		{
			SocialButton();
		}

		private void PurchaseSlot()
		{
			if (view.PremiumButtonView.isDoubleConfirmed())
			{
				purchaseSlotSignal.Dispatch(view.slotId);
			}
		}

		private void SocialButton()
		{
			facebookService.LoginSource = "Marketplace";
			showFacebookPopupSignal.Dispatch(delegate
			{
			});
		}

		private void OnLoginSuccess(ISocialService socialService)
		{
			MarketplaceSaleSlot byInstanceId = playerService.GetByInstanceId<MarketplaceSaleSlot>(view.slotId);
			if (socialService.type == SocialServices.FACEBOOK && byInstanceId.Definition.type == MarketplaceSaleSlotDefinition.SlotType.FACEBOOK_UNLOCKABLE)
			{
				UpdateView(byInstanceId.ID);
				refreshSlotsSignal.Dispatch(false);
			}
		}

		private void CreateNewSell()
		{
			createNewSalePanelSignal.Dispatch(view.slotId);
		}

		private void CheckIfNoValidItems()
		{
			Button component = view.CreateButtonView.GetComponent<Button>();
			if (component != null)
			{
				component.interactable = marketplaceService.AreThereValidItemsInStorage();
			}
		}

		private void CloseCreateNewSalePanel()
		{
			view.PremiumButtonView.ResetTapState();
		}
	}
}
