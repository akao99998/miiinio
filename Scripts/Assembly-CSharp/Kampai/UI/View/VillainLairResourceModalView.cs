using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class VillainLairResourceModalView : PopupMenuView
	{
		[Header("Plot ID")]
		public Text title;

		public ButtonView prevButton;

		public ButtonView nextButton;

		[Header("Resource Info")]
		public KampaiImage resourceItem;

		public Text productionDescription;

		public Text resourceItemAmt;

		[Header("Call Minions")]
		public ScrollableButtonView callMinionButton;

		public GameObject availableMinionsPanel;

		public Text idleMinionCount;

		public KampaiImage levelArrow;

		public Text minionLevel;

		[Header("Rush and Timer")]
		public ScrollableButtonView rushButton;

		public Text rushCost;

		public Text clockTime;

		public Text rushText;

		public GameObject clockPanel;

		public GameObject clockIcon;

		public KampaiImage rushFillImage;

		public KampaiImage premiumIcon;

		[Header("Collect")]
		public ButtonView collectButton;

		public KampaiImage collectButtonImage;

		public GameObject collectPanel;

		[Header("PartyBuff")]
		public GameObject partyBuffPanel;

		public Text partyBuffAmt;

		public GameObject clockInBuffIcon;

		public RectTransform clockInBuffIconTransform;

		private int minionsNeeded = 1;

		internal int rushPrice;

		private RuntimeAnimatorController harvestController;

		private RuntimeAnimatorController purchaseController;

		internal void Setup()
		{
			if (harvestController == null)
			{
				harvestController = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Harvest");
			}
			if (purchaseController == null)
			{
				purchaseController = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Purchase");
			}
		}

		internal void SetResourcePlotTitle(string titleString)
		{
			title.text = titleString;
		}

		internal void SetResourceDescription(ItemDefinition itemDef, string desc)
		{
			productionDescription.text = desc;
			resourceItem.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
			resourceItem.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
			collectButtonImage.sprite = resourceItem.sprite;
			collectButtonImage.maskSprite = resourceItem.maskSprite;
		}

		internal void SetResourceItemAmount(int itemAmount)
		{
			resourceItemAmt.text = itemAmount.ToString();
		}

		internal void EnableArrows(bool enable)
		{
			prevButton.gameObject.SetActive(enable);
			nextButton.gameObject.SetActive(enable);
		}

		internal void SetClockTimeAndRushCost(string timeRemaining, string rushCostAmt)
		{
			clockTime.text = timeRemaining;
			rushCost.text = rushCostAmt;
		}

		internal void SetPartyInfo(float boost, string boostString, bool isOn = true)
		{
			if (partyBuffPanel != null)
			{
				partyBuffAmt.text = boostString;
				bool flag = isOn && (int)(boost * 100f) != 100;
				partyBuffPanel.SetActive(flag);
				clockIcon.SetActive(!flag);
				clockInBuffIcon.SetActive(flag);
				if (flag)
				{
					Vector3 originalScale;
					TweenUtil.Throb(clockInBuffIconTransform, 1.1f, 0.2f, out originalScale);
					UIUtils.FlashingColor(clockTime, 0);
					return;
				}
				Go.killAllTweensWithTarget(clockInBuffIconTransform);
				Go.killAllTweensWithTarget(clockTime);
				clockTime.color = Color.white;
				clockInBuffIconTransform.localScale = Vector3.one;
			}
		}

		internal void SetAvailableMinionInformation(int count)
		{
			idleMinionCount.text = count.ToString();
			if (count < minionsNeeded)
			{
				callMinionButton.Disable();
				callMinionButton.GetComponent<Button>().interactable = false;
				callMinionButton.enabled = false;
			}
			else
			{
				callMinionButton.ResetAnim();
				callMinionButton.GetComponent<Button>().interactable = true;
				callMinionButton.enabled = true;
			}
		}

		internal void SetMinionLevel(IPlayerService playerService)
		{
			int highestUntaskedMinionLevel = playerService.GetHighestUntaskedMinionLevel();
			levelArrow.gameObject.SetActive(highestUntaskedMinionLevel != 0);
			minionLevel.text = (highestUntaskedMinionLevel + 1).ToString();
		}

		internal void SetStateCallMinion()
		{
			EnableCallMinion(true);
			EnableRushAndClock(false);
			EnableCollect(false);
		}

		internal void SetStateRush()
		{
			EnableCallMinion(false);
			EnableRushAndClock(true);
			EnableCollect(false);
		}

		internal void SetStateFreeRush()
		{
			rushFillImage.color = GameConstants.UI.UI_ACTION_BUTTON_COLOR;
			rushButton.DisableDoubleConfirm();
			Animator component = rushButton.gameObject.GetComponent<Animator>();
			component.runtimeAnimatorController = harvestController;
			component.Play("Normal");
			rushCost.gameObject.SetActive(false);
			premiumIcon.gameObject.SetActive(false);
			rushText.gameObject.SetActive(true);
		}

		internal void SetStateCollect()
		{
			EnableCallMinion(false);
			EnableRushAndClock(false);
			EnableCollect(true);
		}

		private void EnableCallMinion(bool enable)
		{
			callMinionButton.gameObject.SetActive(enable);
			availableMinionsPanel.SetActive(enable);
		}

		private void EnableRushAndClock(bool enable)
		{
			if (enable)
			{
				rushFillImage.color = GameConstants.UI.UI_PURCHASE_BUTTON_COLOR;
				Animator component = rushButton.gameObject.GetComponent<Animator>();
				component.runtimeAnimatorController = purchaseController;
				component.Play("Normal");
				rushCost.gameObject.SetActive(true);
				premiumIcon.gameObject.SetActive(true);
				rushText.gameObject.SetActive(false);
				rushButton.EnableDoubleConfirm();
				rushButton.ResetAnim();
			}
			rushButton.gameObject.SetActive(enable);
			clockPanel.gameObject.SetActive(enable);
		}

		private void EnableCollect(bool enable)
		{
			collectButton.gameObject.SetActive(enable);
			collectPanel.gameObject.SetActive(enable);
		}
	}
}
