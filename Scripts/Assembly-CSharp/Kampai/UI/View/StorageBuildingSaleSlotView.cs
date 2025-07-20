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
	public class StorageBuildingSaleSlotView : KampaiView
	{
		private enum SaleSlotState
		{
			CreateNew = 0,
			Pending = 1,
			Sold = 2,
			Facebook = 3,
			Premium = 4
		}

		public KampaiImage IconImage;

		public Animator FlipAnimator;

		public Text QuantityText;

		public Text TitleText;

		public Text ConfirmText;

		public Color TicketColor;

		public ButtonView CreateButtonView;

		public ButtonView FacebookButtonView;

		public ScrollableButtonView PendingPanel;

		public GameObject PendingAmountPanel;

		public Text PendingAmountText;

		public Text DeleteAmountText;

		public ScrollableButtonView CancelPendingButtonView;

		public KampaiImage TrashIcon;

		public GameObject SoldPanel;

		public ButtonView CollectButtonView;

		public KampaiImage SoldCurrencyImage;

		public Text SoldCurrencyText;

		public ScrollableButtonView PremiumButtonView;

		public Text PremiumCostText;

		public float FadeInScaleTime = 0.5f;

		public float FadeOutScaleTime = 0.1666666f;

		public Vector2 FadeOutSize = new Vector2(0.75f, 0.75f);

		internal Signal CheckIfValidItemsSignal = new Signal();

		private IEnumerator waitFrame;

		private bool showTimer;

		private float remainingTime;

		private ItemDefinition itemDefinition;

		private MarketplaceSaleItem item;

		private int cancelSalePrice;

		private ILocalizationService localizationService;

		public int slotId { get; set; }

		internal void Init(ILocalizationService localizationService, int cancelSalePrice)
		{
			this.localizationService = localizationService;
			this.cancelSalePrice = cancelSalePrice;
			PremiumButtonView.EnableDoubleConfirm();
			ConfirmText.text = localizationService.GetString("confirmButtonText");
		}

		internal void UpdateSlot(MarketplaceSaleSlot slot)
		{
			switch (slot.state)
			{
			case MarketplaceSaleSlot.State.LOCKED:
			{
				bool flag = slot.Definition.type == MarketplaceSaleSlotDefinition.SlotType.FACEBOOK_UNLOCKABLE;
				bool flag2 = slot.Definition.type == MarketplaceSaleSlotDefinition.SlotType.PREMIUM_UNLOCKABLE;
				if (flag)
				{
					SetSlotState(SaleSlotState.Facebook);
				}
				else if (flag2)
				{
					PremiumButtonView.ResetTapState();
					SetSlotState(SaleSlotState.Premium, slot.premiumCost);
				}
				break;
			}
			case MarketplaceSaleSlot.State.UNLOCKED:
				SetSlotState(SaleSlotState.CreateNew);
				CheckIfValidItemsSignal.Dispatch();
				break;
			}
		}

		internal void UpdateItem(MarketplaceSaleItem item, ItemDefinition itemDefinition, ItemDefinition quantityItemDef, int rewardValue)
		{
			if (item != null && itemDefinition != null && quantityItemDef != null)
			{
				this.item = item;
				this.itemDefinition = itemDefinition;
				switch (item.state)
				{
				case MarketplaceSaleItem.State.PENDING:
					SetSlotState(SaleSlotState.Pending, rewardValue);
					break;
				case MarketplaceSaleItem.State.SOLD:
					SetSlotState(SaleSlotState.Sold, rewardValue);
					break;
				}
			}
		}

		internal void Flip(PlayGlobalSoundFXSignal soundFxSignal)
		{
			soundFxSignal.Dispatch("Play_marketplace_sellCardFlip_01");
			FlipAnimator.Play("Flip");
		}

		internal void EnableDebugTimer(bool isEnabled, int timeRemaining)
		{
			showTimer = isEnabled;
			remainingTime = timeRemaining;
		}

		private void Update()
		{
			if (showTimer)
			{
				remainingTime -= Time.deltaTime;
				TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
				TitleText.text = UIUtils.FormatTime(timeSpan.TotalSeconds, localizationService);
			}
		}

		internal void FadeOut()
		{
			Go.killAllTweensWithTarget(base.transform);
			GoTween tween = new GoTween(base.transform, FadeInScaleTime, new GoTweenConfig().scale(FadeOutSize));
			Go.addTween(tween);
		}

		internal void FadeIn()
		{
			Go.killAllTweensWithTarget(base.transform);
			base.transform.localScale = new Vector3(0.75f, 0.75f);
			GoTween tween = new GoTween(base.transform, FadeOutScaleTime, new GoTweenConfig().scale(Vector3.one));
			Go.addTween(tween);
		}

		public void FlipSwitchToDeleteState()
		{
			string @string = localizationService.GetString("SellSaleDeleteSale");
			if (TitleText.text.Equals(@string))
			{
				SetItemTitleText();
				TrashIcon.gameObject.SetActive(true);
				PendingAmountPanel.gameObject.SetActive(true);
				CancelPendingButtonView.gameObject.SetActive(false);
			}
			else
			{
				SetTitleText(@string);
				TrashIcon.gameObject.SetActive(false);
				PendingAmountPanel.gameObject.SetActive(false);
				CancelPendingButtonView.gameObject.SetActive(true);
			}
		}

		private void SetSlotState(SaleSlotState state, int costValue = 0)
		{
			CreateButtonView.gameObject.SetActive(false);
			FacebookButtonView.gameObject.SetActive(false);
			PendingPanel.gameObject.SetActive(false);
			SoldPanel.gameObject.SetActive(false);
			if (state != SaleSlotState.Premium && waitFrame == null)
			{
				waitFrame = WaitAFrame();
				StartCoroutine(waitFrame);
			}
			switch (state)
			{
			case SaleSlotState.CreateNew:
				SetupCreateState();
				CreateButtonView.gameObject.SetActive(true);
				break;
			case SaleSlotState.Facebook:
			case SaleSlotState.Premium:
				SetupCreateState();
				if (state == SaleSlotState.Facebook)
				{
					FacebookButtonView.gameObject.SetActive(true);
					break;
				}
				if (waitFrame != null)
				{
					StopCoroutine(waitFrame);
					waitFrame = null;
				}
				PremiumButtonView.gameObject.SetActive(true);
				PremiumCostText.text = costValue.ToString();
				break;
			case SaleSlotState.Pending:
				SetupPendingState(costValue);
				PendingPanel.gameObject.SetActive(true);
				break;
			case SaleSlotState.Sold:
				SetupSoldState(costValue);
				SoldPanel.gameObject.SetActive(true);
				break;
			}
		}

		private void SetupCreateState()
		{
			SetTitleCreateNewSale();
			SetIconToTag();
			SetQuantityTextEnabled(false);
		}

		private void SetupPendingState(int rewardValue)
		{
			QuantityText.gameObject.SetActive(true);
			PendingPanel.gameObject.SetActive(true);
			PendingAmountPanel.gameObject.SetActive(true);
			CancelPendingButtonView.gameObject.SetActive(false);
			TrashIcon.gameObject.SetActive(true);
			DeleteAmountText.text = cancelSalePrice.ToString();
			PendingAmountText.text = rewardValue.ToString();
			QuantityText.text = string.Format("x{0}", item.QuantitySold);
			IconImage.color = Color.white;
			SetImage(IconImage, itemDefinition.Image, itemDefinition.Mask);
			SetItemTitleText();
		}

		private void SetupSoldState(int rewardValue)
		{
			SoldCurrencyText.text = rewardValue.ToString();
			IconImage.gameObject.SetActive(false);
			SetItemTitleText();
			IconImage.color = Color.white;
		}

		internal IEnumerator WaitAFrame()
		{
			yield return new WaitForEndOfFrame();
			if (!(this == null) && !(PremiumButtonView == null))
			{
				PremiumButtonView.gameObject.SetActive(false);
				waitFrame = null;
			}
		}

		private void SetIconToTag()
		{
			SetImage(IconImage, "btn_Main01_overlay_fill", "icn_nav_salesMinion_mask");
			IconImage.color = TicketColor;
		}

		private void SetQuantityTextEnabled(bool isEnabled)
		{
			QuantityText.gameObject.SetActive(isEnabled);
		}

		private void SetItemTitleText()
		{
			SetTitleText(LocalizeTitle(itemDefinition));
		}

		private void SetTitleText(string title)
		{
			TitleText.text = title;
		}

		private void SetTitleCreateNewSale()
		{
			SetTitleText(localizationService.GetString("SellPanelCreateNewSale"));
		}

		private string LocalizeTitle(ItemDefinition itemDefinition)
		{
			if (item != null)
			{
				return localizationService.GetString(itemDefinition.LocalizedKey);
			}
			if (item == null || item.Definition == null)
			{
				return "ITEM";
			}
			return localizationService.GetString(item.Definition.LocalizedKey);
		}

		private void SetImage(KampaiImage image, string iconPath, string maskPath)
		{
			image.gameObject.SetActive(true);
			if (string.IsNullOrEmpty(iconPath))
			{
				iconPath = "btn_Main01_fill";
			}
			if (string.IsNullOrEmpty(maskPath))
			{
				maskPath = "btn_Main01_mask";
			}
			image.sprite = UIUtils.LoadSpriteFromPath(iconPath);
			image.maskSprite = UIUtils.LoadSpriteFromPath(maskPath);
		}
	}
}
