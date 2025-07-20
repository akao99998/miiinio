using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class SellPanelMediator : UIStackMediator<SellPanelView>
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public OpenCreateNewSalePanelSignal openSalePanelSignal { get; set; }

		[Inject]
		public CloseCreateNewSalePanelSignal closeCreateNewSaleSignal { get; set; }

		[Inject]
		public RefreshSlotsSignal refreshSlotsSignal { get; set; }

		[Inject]
		public MarketplaceOpenSalePanelSignal openSalePanel { get; set; }

		[Inject]
		public MarketplaceCloseSalePanelSignal closeSalePanelSignal { get; set; }

		[Inject]
		public MarketplaceCloseAllSalePanels closeAllSalePanels { get; set; }

		[Inject]
		public UpdateMarketplaceSaleOrderSignal updateSaleOrderSignal { get; set; }

		[Inject]
		public EnableStorageBuildingItemDescriptionSignal enableItemDescriptionSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		public override void OnRegister()
		{
			base.view.Init();
			base.view.OnOpenPanelSignal.AddListener(OnOpen);
			loginSuccess.AddListener(RefreshSlotsAfterLogin);
			openSalePanel.AddListener(Open);
			closeAllSalePanels.AddListener(CloseAllViews);
			openSalePanelSignal.AddListener(OpenCreateNewSalePanel);
			closeCreateNewSaleSignal.AddListener(CloseCreateNewSalePanel);
			refreshSlotsSignal.AddListener(PurchaseSlot);
			base.view.ArrowButtonView.ClickedSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.view.OnOpenPanelSignal.RemoveListener(OnOpen);
			loginSuccess.RemoveListener(RefreshSlotsAfterLogin);
			openSalePanel.RemoveListener(Open);
			closeAllSalePanels.RemoveListener(CloseAllViews);
			openSalePanelSignal.RemoveListener(OpenCreateNewSalePanel);
			closeCreateNewSaleSignal.RemoveListener(CloseCreateNewSalePanel);
			refreshSlotsSignal.RemoveListener(PurchaseSlot);
			base.view.ArrowButtonView.ClickedSignal.RemoveListener(Close);
		}

		private void OnOpen()
		{
			base.uiAddedSignal.Dispatch(GetViewGameObject(), Close);
			enableItemDescriptionSignal.Dispatch(true);
		}

		private void Open(bool isInstant)
		{
			if (facebookService.isLoggedIn || marketplaceService.DebugFacebook)
			{
				updateSaleOrderSignal.Dispatch();
			}
			enableItemDescriptionSignal.Dispatch(true);
			RefreshSlots();
			base.view.ScrollView.SetupScrollView();
			base.view.SetOpen(true);
			soundFXSignal.Dispatch("Play_shop_pane_in_01");
		}

		private void RefreshSlots()
		{
			base.view.ScrollView.ClearItems();
			List<MarketplaceSaleSlot> instancesByType = playerService.GetInstancesByType<MarketplaceSaleSlot>();
			instancesByType.Sort();
			if (instancesByType != null)
			{
				base.view.ScrollView.AddList(instancesByType, CreateSlotView, (MarketplaceSaleSlot slot) => !base.view.HasSlot(slot) && marketplaceService.IsSlotVisible(slot), false);
			}
		}

		private IEnumerator UpdateScrollViewAfterLogin()
		{
			yield return null;
			base.view.ScrollView.SetupScrollView();
		}

		private void RefreshSlotsAfterLogin(ISocialService socialService)
		{
			StartCoroutine(UpdateScrollViewAfterLogin());
		}

		private void PurchaseSlot(bool scroll)
		{
			RefreshSlots();
			if (scroll)
			{
				base.view.ScrollView.SetupScrollView(-1f, KampaiScrollView.MoveDirection.End);
			}
		}

		private StorageBuildingSaleSlotView CreateSlotView(int index, MarketplaceSaleSlot slot)
		{
			GameObject original = KampaiResources.Load("cmp_StorageBuildingSaleSlot") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			StorageBuildingSaleSlotView component = gameObject.GetComponent<StorageBuildingSaleSlotView>();
			component.slotId = slot.ID;
			return component;
		}

		private void OpenCreateNewSalePanel(int slotIndex)
		{
			if (base.view.isOpen)
			{
				base.view.FadeOutItems();
				base.view.FadeAnimation(true);
			}
		}

		private void CloseCreateNewSalePanel()
		{
			if (base.view.isOpen)
			{
				base.view.FadeAnimation(false);
			}
			base.view.FadeInItems();
		}

		private void CloseAllViews()
		{
			base.view.SetOpen(false);
			base.view.FadeInItems();
			closeCreateNewSaleSignal.Dispatch();
			closeSalePanelSignal.Dispatch();
			enableItemDescriptionSignal.Dispatch(false);
			soundFXSignal.Dispatch("Play_shop_pane_out_01");
			base.uiRemovedSignal.Dispatch(base.view.gameObject);
		}

		protected override void Close()
		{
			if (base.view.isCreateNewSalePanelOpened)
			{
				soundFXSignal.Dispatch("Play_marketplace_backFromSale_01");
				base.view.FadeAnimation(false);
				closeCreateNewSaleSignal.Dispatch();
				base.uiAddedSignal.Dispatch(GetViewGameObject(), Close);
			}
			else
			{
				CloseAllViews();
			}
		}
	}
}
