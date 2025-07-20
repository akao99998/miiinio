using System;
using System.Collections;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class BuyMarketplacePanelView : PopupMenuView
	{
		internal enum RefreshButtonState
		{
			None = 0,
			RefreshReady = 1,
			RefreshPending = 2,
			StopSpinning = 3
		}

		private IEnumerator premiumButtonDisableCoroutine;

		private IEnumerator adPanelDisableCoroutine;

		public ButtonView ArrowButtonView;

		public DoubleConfirmButtonView RefreshPremiumButtonView;

		public DoubleConfirmButtonView RefreshPremiumButtonViewAdPanel;

		public ButtonView RefreshButtonView;

		public ButtonView StopButtonView;

		public ButtonView AdVideoButtonView;

		public GameObject PanelRefreshPendingWithAd;

		public Text RefreshCostText;

		public Text RefreshCostTextAdPanel;

		public Text RefreshTitleText;

		public Text RefreshTitleTextAdPanel;

		public KampaiScrollView ScrollView;

		private ILocalizationService localService;

		internal bool AdButtonEnabled;

		internal Signal<bool> OnOpenSignal = new Signal<bool>();

		internal Signal OnCloseSignal = new Signal();

		internal RefreshButtonState refreshButtonState;

		public bool IsOpen { get; set; }

		protected override void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext, true);
		}

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
			refreshButtonState = RefreshButtonState.None;
			localService = localizationService;
			StopButtonView.PlaySoundOnClick = false;
			RefreshPremiumButtonView.PlaySoundOnClick = false;
			RefreshPremiumButtonViewAdPanel.PlaySoundOnClick = false;
			RefreshPremiumButtonView.EnableDoubleConfirm();
			RefreshPremiumButtonViewAdPanel.EnableDoubleConfirm();
			SetRefreshCost(0);
		}

		public bool HasSlot(MarketplaceBuyItem slot)
		{
			foreach (MonoBehaviour item in ScrollView)
			{
				BuyMarketplaceSlotView buyMarketplaceSlotView = item as BuyMarketplaceSlotView;
				if (buyMarketplaceSlotView == null || slot.ID != buyMarketplaceSlotView.slotId)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public void EnableRewardedAdRushButton(bool enable)
		{
			if (AdButtonEnabled != enable)
			{
				AdButtonEnabled = enable;
				if (refreshButtonState == RefreshButtonState.RefreshPending)
				{
					EnableRefreshPendingWithAdPanel(enable);
				}
			}
		}

		private void EnableRefreshPendingWithAdPanel(bool enable)
		{
			if (enable)
			{
				PanelRefreshPendingWithAd.SetActive(enable);
				if (adPanelDisableCoroutine != null)
				{
					StopCoroutine(adPanelDisableCoroutine);
					adPanelDisableCoroutine = null;
				}
			}
			else if (adPanelDisableCoroutine == null)
			{
				adPanelDisableCoroutine = DisableAdPanelOnEndOfFrame();
				StartCoroutine(adPanelDisableCoroutine);
			}
		}

		internal void SetupRefreshButtonState(RefreshButtonState state, int timeRemaining = 0)
		{
			if (state != refreshButtonState && state != RefreshButtonState.RefreshPending)
			{
				EnableRefreshPendingWithAdPanel(false);
			}
			switch (state)
			{
			case RefreshButtonState.RefreshReady:
				SetRefreshTitleText(localService.GetString("BuyPanelRefreshUserPrompt"));
				if (state != refreshButtonState)
				{
					RefreshButtonView.gameObject.SetActive(true);
					if (premiumButtonDisableCoroutine == null)
					{
						premiumButtonDisableCoroutine = DisablePremiumButtonOnEndOfFrame();
						StartCoroutine(premiumButtonDisableCoroutine);
					}
					StopButtonView.gameObject.SetActive(false);
				}
				break;
			case RefreshButtonState.RefreshPending:
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(timeRemaining);
				SetRefreshTitleText(string.Format("{0} {1}", localService.GetString("BuyPanelRefreshTitle"), UIUtils.FormatTime(timeSpan.TotalSeconds, localService)));
				if (state != refreshButtonState)
				{
					if (premiumButtonDisableCoroutine != null)
					{
						StopCoroutine(premiumButtonDisableCoroutine);
						premiumButtonDisableCoroutine = null;
					}
					SetActivePremium(true);
					RefreshButtonView.gameObject.SetActive(false);
					StopButtonView.gameObject.SetActive(false);
					EnableRefreshPendingWithAdPanel(AdButtonEnabled);
				}
				break;
			}
			case RefreshButtonState.StopSpinning:
				SetRefreshTitleText(localService.GetString("BuyPanelStopSpinningUserPrompt"));
				if (state != refreshButtonState)
				{
					RefreshPremiumButtonView.ResetAnim();
					RefreshPremiumButtonViewAdPanel.ResetAnim();
					StopButtonView.gameObject.SetActive(true);
					RefreshButtonView.gameObject.SetActive(false);
					if (premiumButtonDisableCoroutine == null)
					{
						premiumButtonDisableCoroutine = DisablePremiumButtonOnEndOfFrame();
						StartCoroutine(premiumButtonDisableCoroutine);
					}
				}
				break;
			}
			refreshButtonState = state;
		}

		internal void SetActivePremium(bool active)
		{
			RefreshPremiumButtonView.gameObject.SetActive(active);
			RefreshPremiumButtonViewAdPanel.gameObject.SetActive(active);
		}

		internal void SetRefreshTitleText(string text)
		{
			if (RefreshTitleText != null)
			{
				RefreshTitleText.text = text;
				RefreshTitleTextAdPanel.text = text;
			}
		}

		internal void SetRefreshCost(int cost)
		{
			string text = cost.ToString();
			RefreshCostText.text = text;
			RefreshCostTextAdPanel.text = text;
		}

		internal IEnumerator DisablePremiumButtonOnEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			SetActivePremium(false);
			premiumButtonDisableCoroutine = null;
		}

		internal IEnumerator DisableAdPanelOnEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			PanelRefreshPendingWithAd.SetActive(false);
			adPanelDisableCoroutine = null;
		}

		internal void SetOpen(bool show, bool dispatchSignals = true, bool isInstant = false)
		{
			if (show)
			{
				if (isInstant)
				{
					float lastFrame = 1f;
					int defaultLayer = -1;
					OpenInstantly(defaultLayer, lastFrame);
				}
				else
				{
					Open();
				}
				if (dispatchSignals)
				{
					OnOpenSignal.Dispatch(isInstant);
				}
			}
			else
			{
				Close();
				if (dispatchSignals)
				{
					OnCloseSignal.Dispatch();
				}
			}
			IsOpen = show;
		}
	}
}
