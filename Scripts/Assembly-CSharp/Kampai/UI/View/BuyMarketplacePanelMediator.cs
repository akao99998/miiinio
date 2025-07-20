using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class BuyMarketplacePanelMediator : UIStackMediator<BuyMarketplacePanelView>
	{
		private int refreshTimeSeconds;

		private MarketplaceRefreshTimer refreshTimer;

		private readonly AdPlacementName adPlacementName = AdPlacementName.MARKETPLACE;

		private AdPlacementInstance adPlacementInstance;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public RefreshSaleItemsSuccessSignal refreshSuccessSignal { get; set; }

		[Inject]
		public RushRefreshTimerSuccessSignal rushSuccessSignal { get; set; }

		[Inject]
		public HaltSlotMachine haltSlotMachine { get; set; }

		[Inject]
		public MarketplaceOpenBuyPanelSignal openBuyPanel { get; set; }

		[Inject]
		public MarketplaceCloseBuyPanelSignal closeBuyPanel { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public RemoveStorageBuildingItemDescriptionSignal removeItemDescriptionSignal { get; set; }

		[Inject]
		public IMarketplaceService marketPlaceService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		public override void OnRegister()
		{
			base.view.Init(localService);
			MarketplaceRefreshTimerDefinition marketplaceRefreshTimerDefinition = definitionService.Get<MarketplaceRefreshTimerDefinition>(1000008093);
			refreshTimeSeconds = marketplaceRefreshTimerDefinition.RefreshTimeSeconds;
			openBuyPanel.AddListener(OpenPanel);
			base.view.OnOpenSignal.AddListener(OpenPanel);
			base.view.OnCloseSignal.AddListener(Close);
			base.view.ArrowButtonView.ClickedSignal.AddListener(Close);
			base.view.RefreshButtonView.ClickedSignal.AddListener(OnRefreshButtonClick);
			base.view.RefreshPremiumButtonView.ClickedSignal.AddListener(OnRefreshButtonClick);
			base.view.RefreshPremiumButtonViewAdPanel.ClickedSignal.AddListener(OnRefreshButtonClick);
			base.view.AdVideoButtonView.ClickedSignal.AddListener(OnAdButtonButtonClick);
			base.view.StopButtonView.ClickedSignal.AddListener(OnRefreshButtonClick);
			refreshSuccessSignal.AddListener(LoadScrollViewItems);
			refreshSuccessSignal.AddListener(UpdateRefreshTime);
			rushSuccessSignal.AddListener(UpdateRefreshTime);
			haltSlotMachine.AddListener(CompleteSlotMachine);
			loginSuccess.AddListener(OnLoginSuccess);
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
			base.view.SetRefreshCost(marketplaceRefreshTimerDefinition.RushCost);
			UpdateAdButton();
		}

		public override void OnRemove()
		{
			openBuyPanel.RemoveListener(OpenPanel);
			base.view.OnOpenSignal.RemoveListener(OpenPanel);
			base.view.OnCloseSignal.RemoveListener(Close);
			base.view.ArrowButtonView.ClickedSignal.RemoveListener(Close);
			base.view.RefreshButtonView.ClickedSignal.RemoveListener(OnRefreshButtonClick);
			base.view.RefreshPremiumButtonView.ClickedSignal.RemoveListener(OnRefreshButtonClick);
			base.view.RefreshPremiumButtonViewAdPanel.ClickedSignal.RemoveListener(OnRefreshButtonClick);
			base.view.AdVideoButtonView.ClickedSignal.RemoveListener(OnAdButtonButtonClick);
			base.view.StopButtonView.ClickedSignal.RemoveListener(OnRefreshButtonClick);
			refreshSuccessSignal.RemoveListener(LoadScrollViewItems);
			refreshSuccessSignal.RemoveListener(UpdateRefreshTime);
			rushSuccessSignal.RemoveListener(UpdateRefreshTime);
			haltSlotMachine.RemoveListener(CompleteSlotMachine);
			loginSuccess.RemoveListener(OnLoginSuccess);
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
			StopAllCoroutines();
		}

		private void OpenPanel(bool isInstant)
		{
			base.view.SetOpen(true, false, isInstant);
			gameContext.injectionBinder.GetInstance<StartMarketplaceRefreshTimerSignal>().Dispatch(false);
			UpdateRefreshTime();
			telemetryService.Send_Telemtry_EVT_MARKETPLACE_VIEWED("VIEW");
			InvokeRepeating("UpdateRefreshTime", 0.001f, 1f);
			LoadScrollViewItems();
			base.uiAddedSignal.Dispatch(base.view.gameObject, Close);
			removeItemDescriptionSignal.Dispatch();
			soundFXSignal.Dispatch("Play_shop_pane_in_01");
		}

		protected override void Close()
		{
			CancelInvoke("UpdateRefreshTime");
			if (base.view.refreshButtonState == BuyMarketplacePanelView.RefreshButtonState.StopSpinning)
			{
				StopSlotSpinning();
			}
			else
			{
				UpdateButtonSpinningState();
			}
			base.view.SetOpen(false, false);
			closeBuyPanel.Dispatch();
			base.uiRemovedSignal.Dispatch(GetViewGameObject());
			soundFXSignal.Dispatch("Play_shop_pane_out_01");
		}

		internal void CompleteSlotMachine()
		{
			StopAllCoroutines();
			StopSlotSpinning();
			soundFXSignal.Dispatch("Play_marketplace_slotEnd_01");
			base.view.RefreshPremiumButtonView.ResetAnim();
			base.view.RefreshPremiumButtonViewAdPanel.ResetAnim();
		}

		internal void StopSlotSpinning()
		{
			foreach (BuyMarketplaceSlotView itemView in base.view.ScrollView.ItemViewList)
			{
				itemView.StopSlotMachine();
			}
			UpdateButtonSpinningState();
		}

		internal void UpdateButtonSpinningState()
		{
			base.view.ScrollView.EnableScrolling(false, true);
			if (base.view.refreshButtonState != BuyMarketplacePanelView.RefreshButtonState.StopSpinning)
			{
				return;
			}
			base.view.SetupRefreshButtonState(BuyMarketplacePanelView.RefreshButtonState.RefreshPending);
			gameContext.injectionBinder.GetInstance<StartMarketplaceRefreshTimerSignal>().Dispatch(true);
			UpdateRefreshTime();
			foreach (BuyMarketplaceSlotView itemView in base.view.ScrollView.ItemViewList)
			{
				if (itemView.BuyButtonView.isActiveAndEnabled && itemView.BuyButtonView.animator != null)
				{
					itemView.BuyButtonView.animator.enabled = true;
				}
			}
		}

		private void OnAdButtonButtonClick()
		{
			if (adPlacementInstance != null)
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
			}
		}

		private void UpdateAdButton()
		{
			bool flag = rewardedAdService.IsPlacementActive(adPlacementName);
			AdPlacementInstance placementInstance = rewardedAdService.GetPlacementInstance(adPlacementName);
			bool enable = flag && placementInstance != null;
			base.view.EnableRewardedAdRushButton(enable);
			adPlacementInstance = placementInstance;
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			if (placement.Equals(adPlacementInstance))
			{
				if (base.view.refreshButtonState == BuyMarketplacePanelView.RefreshButtonState.RefreshPending)
				{
					RushForPremium(true);
					rewardedAdService.RewardPlayer(null, placement);
					telemetryService.Send_Telemetry_EVT_AD_INTERACTION(placement.Definition.Name.ToString(), "Rush", placement.RewardPerPeriodCount);
				}
				adPlacementInstance = null;
				UpdateAdButton();
			}
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			UpdateAdButton();
		}

		private void OnRefreshButtonClick()
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			switch (base.view.refreshButtonState)
			{
			case BuyMarketplacePanelView.RefreshButtonState.RefreshReady:
			{
				if (refreshTimer == null)
				{
					refreshTimer = playerService.GetFirstInstanceByDefinitionId<MarketplaceRefreshTimer>(1000008093);
				}
				if (refreshTimer != null)
				{
					refreshTimer.UTCStartTime = timeService.CurrentTime() + 3;
				}
				RefreshSaleItemsSignalArgs refreshSaleItemsSignalArgs = new RefreshSaleItemsSignalArgs();
				refreshSaleItemsSignalArgs.RefreshItems = true;
				refreshSaleItemsSignalArgs.StopSpinning = false;
				RefreshSaleItemsSignalArgs type = refreshSaleItemsSignalArgs;
				injectionBinder.GetInstance<RefreshSaleItemsSignal>().Dispatch(type);
				base.view.SetupRefreshButtonState(BuyMarketplacePanelView.RefreshButtonState.StopSpinning);
				break;
			}
			case BuyMarketplacePanelView.RefreshButtonState.RefreshPending:
				if (base.view.RefreshPremiumButtonView.isDoubleConfirmed() || base.view.RefreshPremiumButtonViewAdPanel.isDoubleConfirmed())
				{
					soundFXSignal.Dispatch("Play_button_premium_01");
					RushForPremium();
					base.view.RefreshPremiumButtonView.ResetTapState();
					base.view.RefreshPremiumButtonViewAdPanel.ResetTapState();
				}
				break;
			case BuyMarketplacePanelView.RefreshButtonState.StopSpinning:
			{
				CompleteSlotMachine();
				RefreshSaleItemsSignalArgs refreshSaleItemsSignalArgs = new RefreshSaleItemsSignalArgs();
				refreshSaleItemsSignalArgs.RefreshItems = false;
				refreshSaleItemsSignalArgs.StopSpinning = true;
				RefreshSaleItemsSignalArgs type = refreshSaleItemsSignalArgs;
				injectionBinder.GetInstance<RefreshSaleItemsSignal>().Dispatch(type);
				break;
			}
			}
		}

		private void RushForPremium(bool rewardedForAdImpression = false)
		{
			RefreshSaleItemsSignalArgs refreshSaleItemsSignalArgs = new RefreshSaleItemsSignalArgs();
			refreshSaleItemsSignalArgs.RefreshItems = false;
			refreshSaleItemsSignalArgs.StopSpinning = false;
			RefreshSaleItemsSignalArgs refreshSaleItemsSignalArgs2 = refreshSaleItemsSignalArgs;
			if (rewardedForAdImpression)
			{
				refreshSaleItemsSignalArgs2.RushCost = 0;
			}
			gameContext.injectionBinder.GetInstance<RefreshSaleItemsSignal>().Dispatch(refreshSaleItemsSignalArgs2);
		}

		private void UpdateRefreshTime()
		{
			if (base.view.refreshButtonState != BuyMarketplacePanelView.RefreshButtonState.StopSpinning)
			{
				int num = 0;
				if (refreshTimer == null)
				{
					refreshTimer = playerService.GetFirstInstanceByDefinitionId<MarketplaceRefreshTimer>(1000008093);
				}
				if (refreshTimer != null)
				{
					num = refreshTimer.UTCStartTime + refreshTimeSeconds - timeService.CurrentTime();
				}
				if (num <= 0)
				{
					num = 0;
					base.view.SetupRefreshButtonState(BuyMarketplacePanelView.RefreshButtonState.RefreshReady);
				}
				else
				{
					base.view.SetupRefreshButtonState(BuyMarketplacePanelView.RefreshButtonState.RefreshPending, num);
				}
			}
		}

		private void LoadScrollViewItems()
		{
			bool isCOPPAGated = coppaService.Restricted();
			List<MarketplaceBuyItem> instancesByType = playerService.GetInstancesByType<MarketplaceBuyItem>();
			if (base.view.ScrollView.ItemViewList.Count == instancesByType.Count)
			{
				bool flag = false;
				foreach (BuyMarketplaceSlotView itemView in base.view.ScrollView.ItemViewList)
				{
					if (itemView.SetupBuyItem(localService, definitionService, facebookService, isCOPPAGated, instancesByType[itemView.slotIndex], soundFXSignal, marketPlaceService))
					{
						flag = true;
					}
				}
				if (flag)
				{
					StopAllCoroutines();
					base.view.ScrollView.TweenToPosition(new Vector2(0f, 1f), 0f);
					base.view.ScrollView.EnableScrolling(false, false);
					StartCoroutine("EnableScrollView");
				}
			}
			else
			{
				base.view.ScrollView.ClearItems();
				if (instancesByType != null)
				{
					base.view.ScrollView.AddList(instancesByType, CreateMarketplaceItem);
				}
			}
		}

		internal IEnumerator EnableScrollView()
		{
			yield return new WaitForSeconds(3.7f);
			UpdateButtonSpinningState();
		}

		private BuyMarketplaceSlotView CreateMarketplaceItem(int index, MarketplaceBuyItem item)
		{
			GameObject original = KampaiResources.Load("cmp_StorageBuildingBuySlot") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			BuyMarketplaceSlotView component = gameObject.GetComponent<BuyMarketplaceSlotView>();
			component.slotIndex = index;
			component.slotId = item.ID;
			component.BuyItem = item;
			return component;
		}

		private void OnLoginSuccess(ISocialService socialService)
		{
			if (socialService.type == SocialServices.FACEBOOK)
			{
				LoadScrollViewItems();
			}
		}
	}
}
