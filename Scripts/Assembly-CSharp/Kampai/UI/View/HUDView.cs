using System;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class HUDView : KampaiView
	{
		public ParticleSystem PremiumStarVFX;

		public ParticleSystem PremiumImageVFX;

		public ParticleSystem GrindStarVFX;

		public ParticleSystem GrindImageVFX;

		public ParticleSystem StorageStarVFX;

		public ParticleSystem StorageImageVFX;

		public ButtonView PremiumMenuButton;

		public ButtonView PremiumIconButton;

		public ButtonView PremiumTextButton;

		public ButtonView GrindMenuButton;

		public ButtonView GrindIconButton;

		public ButtonView GrindTextButton;

		public ButtonView StorageButton;

		public ButtonView StorageExpandButton;

		public GameObject SettingsButton;

		public ButtonView PetsButton;

		public ButtonView BackgroundButton;

		public ButtonView StoreMenuButton;

		public ButtonView ExitLairButton;

		public RectTransform CurrencyStore;

		public RectTransform StorageFillBar;

		public RectTransform PointsPanel;

		public RectTransform PartyMeterPanel;

		public RectTransform ExitLairPanel;

		public WayFinderPanelView WayFinder;

		public BuildMenuView BuildMenu;

		public GameObject StoreMenu;

		public SalePackHUDPanelView salePackPanel;

		public Text GrindCurrency;

		public Text PremiumCurrency;

		public GameObject SaleBadge;

		public Text SaleCount;

		public Text StorageAmount;

		public Animator StorageAmountAnim;

		public GameObject DarkSkrim;

		public Signal<bool> MenuMoved = new Signal<bool>();

		private Animator animator;

		private int darkSkrimCount;

		private bool lastPopupState;

		private int popupsOpened;

		private bool playStorageVFX;

		private HUDChangedSiblingIndexSignal hudChangedSiblingIndexSignal;

		internal GoTween storageTextTween;

		internal GoTween storageFillTween;

		internal bool isInForeground;

		private bool shown;

		public int expTweenCount { get; set; }

		public int storageTweenCount { get; set; }

		public int premiumTweenCount { get; set; }

		public int grindTweenCount { get; set; }

		public void Init(HUDChangedSiblingIndexSignal hudChangedSiblingIndexSignal)
		{
			this.hudChangedSiblingIndexSignal = hudChangedSiblingIndexSignal;
			(base.transform as RectTransform).offsetMin = Vector2.zero;
			(base.transform as RectTransform).offsetMax = Vector2.zero;
			animator = GetComponent<Animator>();
			DisableSoundForMenuButtons();
			BuildMenu.transform.SetAsLastSibling();
		}

		private void DisableSoundForMenuButtons()
		{
			PremiumMenuButton.PlaySoundOnClick = (PremiumIconButton.PlaySoundOnClick = (PremiumTextButton.PlaySoundOnClick = false));
			GrindMenuButton.PlaySoundOnClick = (GrindIconButton.PlaySoundOnClick = (GrindTextButton.PlaySoundOnClick = false));
			StorageButton.PlaySoundOnClick = false;
			BackgroundButton.PlaySoundOnClick = false;
			StorageExpandButton.PlaySoundOnClick = false;
			PetsButton.PlaySoundOnClick = false;
			ButtonView component = SettingsButton.GetComponent<ButtonView>();
			if (component != null)
			{
				component.PlaySoundOnClick = false;
			}
		}

		internal void SetStorage(uint current, uint max)
		{
			if (storageTextTween != null)
			{
				storageTextTween.destroy();
				storageTextTween = null;
			}
			storageTextTween = Go.to(this, 1f, new GoTweenConfig().intProp("storageTweenCount", (int)current).onUpdate(delegate
			{
				SetStorageText((uint)storageTweenCount, max);
			}).onComplete(delegate
			{
				storageTextTween.destroy();
				storageTextTween = null;
			}));
			if (StorageFillBar != null && max != 0)
			{
				if (storageFillTween != null)
				{
					storageFillTween.destroy();
					storageFillTween = null;
				}
				if (current > max)
				{
					current = max;
				}
				storageFillTween = Go.to(StorageFillBar, 1f, new GoTweenConfig().vector2Prop("anchorMax", new Vector2((float)current / (float)max, 1f)).onComplete(delegate
				{
					storageFillTween.destroy();
					storageFillTween = null;
				}));
			}
		}

		internal void SetStorageText(uint current, uint max)
		{
			StorageAmount.text = string.Format("{0}/{1}", current, max);
			int num = Math.Abs((int)(max - current));
			bool flag = num < 10;
			bool flag2 = current >= max;
			if (flag || flag2)
			{
				EnableOutline(true, StorageAmount, Color.white);
				StorageAmountAnim.Play("AlmostFull");
			}
			else
			{
				EnableOutline(false, StorageAmount, Color.white);
				StorageAmountAnim.Play("Init");
			}
		}

		private static void EnableOutline(bool enable, Text text, Color outlineColor)
		{
			Outline outline = text.GetComponent<Outline>();
			if (enable)
			{
				if (outline == null)
				{
					outline = text.gameObject.AddComponent<Outline>();
				}
				outline.effectColor = outlineColor;
			}
			else if (outline != null)
			{
				UnityEngine.Object.Destroy(outline);
				outline = null;
			}
		}

		public void SetGrindCurrency(uint amount)
		{
			Go.to(this, 1f, new GoTweenConfig().intProp("grindTweenCount", (int)amount).onUpdate(delegate
			{
				GrindCurrency.text = UIUtils.FormatLargeNumber(grindTweenCount);
			}));
		}

		public void SetPremiumCurrency(uint amount)
		{
			if (amount < premiumTweenCount)
			{
				PremiumCurrency.text = UIUtils.FormatLargeNumber((int)amount);
				return;
			}
			Go.to(this, 1f, new GoTweenConfig().intProp("premiumTweenCount", (int)amount).onUpdate(delegate
			{
				PremiumCurrency.text = UIUtils.FormatLargeNumber(premiumTweenCount);
			}));
		}

		public void ActivateBackgroundButton()
		{
			if (darkSkrimCount == 0)
			{
				BackgroundButton.gameObject.SetActive(true);
				ToggleDarkSkrim(true);
			}
		}

		public void MoveMenu(bool show)
		{
			if (show != shown)
			{
				shown = show;
				if (show)
				{
					BringToForeground();
					MenuMoved.Dispatch(true);
					animator.SetBool("OnHide", false);
					animator.SetBool("OnPopup", false);
				}
				else
				{
					BackgroundButton.gameObject.SetActive(false);
					BringToBackground();
					MenuMoved.Dispatch(false);
					animator.SetBool("OnPopup", lastPopupState);
					ToggleDarkSkrim(false);
				}
			}
		}

		internal void EnableVillainHud(bool isEnabled)
		{
			ExitLairPanel.gameObject.SetActive(isEnabled);
			animator.SetBool("OnVillainLair", isEnabled);
		}

		internal void ToggleDarkSkrim(bool show)
		{
			if (show)
			{
				darkSkrimCount++;
			}
			else
			{
				darkSkrimCount--;
				darkSkrimCount = Mathf.Max(0, darkSkrimCount);
			}
			DarkSkrim.SetActive((darkSkrimCount > 0) ? true : false);
		}

		internal void TogglePopup(bool show)
		{
			if (show)
			{
				popupsOpened++;
				ToggleDarkSkrim(true);
				if (popupsOpened == 1)
				{
					BringToForeground();
				}
			}
			else
			{
				popupsOpened--;
				popupsOpened = Mathf.Max(0, popupsOpened);
				ToggleDarkSkrim(false);
				if (popupsOpened == 0)
				{
					BringToBackground();
				}
			}
			lastPopupState = popupsOpened > 0;
			animator.SetBool("OnPopup", lastPopupState);
		}

		internal void Toggle(bool show)
		{
			animator.SetBool("OnHide", !show);
		}

		internal void ToggleSettings(bool show)
		{
			SettingsButton.SetActive(show);
		}

		internal void TogglePetsButton(bool visible)
		{
			PetsButton.gameObject.SetActive(visible);
		}

		internal bool IsHiding()
		{
			return animator.GetBool("OnHide");
		}

		public void SetStorageButtonVisible(bool visible)
		{
			StorageButton.gameObject.SetActive(visible);
		}

		public void SetButtonsVisible(bool visible)
		{
			SettingsButton.SetActive(visible);
		}

		internal void PlayPremiumVFX()
		{
			PremiumStarVFX.Play();
			PremiumImageVFX.Play();
		}

		internal void PlayGrindVFX()
		{
			GrindStarVFX.Play();
			GrindImageVFX.Play();
		}

		internal void PlayStorageVFX()
		{
			if (playStorageVFX)
			{
				StorageStarVFX.Play();
				StorageImageVFX.Play();
			}
			playStorageVFX = true;
		}

		private void BringToForeground()
		{
			isInForeground = true;
			base.transform.SetAsLastSibling();
			hudChangedSiblingIndexSignal.Dispatch(base.transform.GetSiblingIndex());
			salePackPanel.transform.SetSiblingIndex(WayFinder.transform.GetSiblingIndex() + 1);
			BuildMenu.transform.SetSiblingIndex(WayFinder.transform.GetSiblingIndex() + 1);
		}

		private void BringToBackground()
		{
			isInForeground = false;
			base.transform.SetAsFirstSibling();
			hudChangedSiblingIndexSignal.Dispatch(base.transform.GetSiblingIndex());
			salePackPanel.transform.SetAsLastSibling();
			BuildMenu.transform.SetAsLastSibling();
		}

		internal void EnableStoreMenuButton(bool enable)
		{
			Button component = StoreMenuButton.gameObject.GetComponent<Button>();
			if (component != null)
			{
				component.interactable = enable;
			}
		}
	}
}
