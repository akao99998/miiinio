using System.Collections;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class MinionSliderView : KampaiView
	{
		public GameObject ClockPanel;

		public GameObject AvailableMinionPanel;

		public GameObject lockedPanel;

		public GameObject HarvestPanel;

		public GameObject PartyIcon;

		public GameObject ClockIcon;

		public Transform ClockGroupForPulsing;

		public Text durationText;

		public Text costText;

		public Text minionCount;

		public Text lockedText;

		public Text lockedCostText;

		public Text availableText;

		public Text rushText;

		public Text confirmText;

		public Text minionLevel;

		public KampaiImage buttonImage;

		public KampaiImage buttonFillImage;

		public KampaiImage costImage;

		public KampaiImage rushCostImage;

		public KampaiImage rushFillImage;

		public KampaiImage levelArrow;

		public ScrollableButtonView rushButton;

		public ScrollableButtonView callButton;

		public UnlockableScrollableButtonView lockedButton;

		public ScrollableButtonView harvestButton;

		public ResourceBuilding building;

		public VillainLairResourcePlot resourcePlot;

		public bool isResourcePlotSlider;

		public float PaddingInPixels;

		internal Signal completeSignal = new Signal();

		internal IPlayerService playerService;

		internal int identifier;

		internal int minionID;

		internal bool isLockedHighlighted;

		internal MinionSliderState state;

		public double startTime;

		private uint rushCost;

		private int harvestTime;

		private int count;

		public Color lockedTextColor;

		public Color lockedOverlayColorWithAlpha;

		private bool completed;

		private bool isPartying;

		internal bool isCorrectBuffType;

		private ILocalizationService localService;

		private IDefinitionService definitionService;

		private Vector3 originalScale = new Vector3(1f, 1f, 1f);

		private ModalSettings modalSettings;

		private static RuntimeAnimatorController harvestController;

		private static RuntimeAnimatorController purchaseController;

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		internal void Init(ILocalizationService localService, IDefinitionService definitionService)
		{
			this.localService = localService;
			this.definitionService = definitionService;
			SetPartyState(false);
			UpdateHarvestTime();
			setMinionText();
			lockedButton.EnableDoubleConfirm();
			if (harvestController == null)
			{
				harvestController = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Harvest");
			}
			if (purchaseController == null)
			{
				purchaseController = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Purchase");
			}
		}

		internal void UpdateHarvestTime()
		{
			if (definitionService != null)
			{
				if (isResourcePlotSlider)
				{
					harvestTime = resourcePlot.parentLair.Definition.SecondsToHarvest;
				}
				else
				{
					harvestTime = BuildingUtil.GetHarvestTimeForTaskableBuilding(building, definitionService);
				}
			}
		}

		internal void UpdateLockedButton()
		{
			if ((!isResourcePlotSlider) ? ((identifier == 2 && building.MinionSlotsOwned < 2) || !modalSettings.enableLockedButtons) : (resourcePlot.State == BuildingState.Inaccessible))
			{
				lockedButton.gameObject.GetComponent<Button>().interactable = false;
				lockedText.color = lockedTextColor;
				lockedCostText.color = lockedTextColor;
				buttonFillImage.Overlay = lockedOverlayColorWithAlpha;
				costImage.Overlay = lockedOverlayColorWithAlpha;
			}
			else
			{
				lockedText.color = Color.white;
				lockedCostText.color = Color.white;
				buttonFillImage.Overlay = Color.clear;
				costImage.Overlay = Color.clear;
				lockedButton.gameObject.GetComponent<Button>().interactable = true;
			}
		}

		internal void SetMinionLevel()
		{
			int highestUntaskedMinionLevel = playerService.GetHighestUntaskedMinionLevel();
			levelArrow.gameObject.SetActive(highestUntaskedMinionLevel != 0);
			minionLevel.text = (highestUntaskedMinionLevel + 1).ToString();
		}

		internal void SetIdleMinionCount()
		{
			count = playerService.GetIdleMinions().Count;
			minionCount.text = count.ToString();
			if (state == MinionSliderState.Harvestable)
			{
				harvestButton.GetComponent<Button>().interactable = modalSettings.enableHarvestButtons;
			}
			else
			{
				harvestButton.GetComponent<Button>().interactable = false;
			}
			if (state == MinionSliderState.Available)
			{
				SetCallButtonState();
			}
		}

		private void SetCallButtonState()
		{
			if (count == 0)
			{
				SetCallHighlight(false);
				callButton.Disable();
				callButton.GetComponent<Button>().interactable = false;
			}
			else
			{
				callButton.ResetAnim();
				callButton.GetComponent<Button>().interactable = modalSettings.enableCallButtons;
			}
		}

		public void Update()
		{
			if (minionID == -1)
			{
				return;
			}
			int num = 0;
			num = ((!isResourcePlotSlider) ? timeEventService.GetTimeRemaining(minionID) : timeEventService.GetTimeRemaining(resourcePlot.ID));
			bool flag = playerService.GetMinionPartyInstance().IsBuffHappening && isCorrectBuffType;
			if (isPartying != flag)
			{
				SetPartyState(flag);
			}
			if (num > harvestTime)
			{
				durationText.text = UIUtils.FormatTime(harvestTime, localService);
				rushCost = (uint)timeEventService.CalculateRushCostForTimer(harvestTime, RushActionType.HARVESTING);
				costText.text = string.Format("{0}", rushCost);
				return;
			}
			if (!isResourcePlotSlider)
			{
				if (num == -1 && building.State != BuildingState.Working)
				{
					completed = true;
				}
			}
			else if (num <= 0 && resourcePlot.State == BuildingState.Working)
			{
				completed = true;
			}
			string text = UIUtils.FormatTime(num, localService);
			if (num > -1 && !completed)
			{
				durationText.text = text;
				rushCost = (uint)timeEventService.CalculateRushCostForTimer(num, RushActionType.HARVESTING);
				if (rushCost == 0 && state != MinionSliderState.Rushable)
				{
					SetMinionSliderState(MinionSliderState.Rushable);
				}
				else if (rushCost != 0)
				{
					costText.text = string.Format("{0}", rushCost);
				}
				if (num <= 0)
				{
					completed = true;
				}
			}
			if (completed)
			{
				ClearSlot();
				completeSignal.Dispatch();
				completed = false;
			}
		}

		internal void SetPartyState(bool isPartying)
		{
			this.isPartying = isPartying;
			ClockIcon.SetActive(!isPartying);
			PartyIcon.SetActive(isPartying);
			if (isPartying)
			{
				Vector3 vector;
				TweenUtil.Throb(ClockGroupForPulsing, 1.1f, 0.2f, out vector);
				UIUtils.FlashingColor(durationText, 0);
				return;
			}
			Go.killAllTweensWithTarget(ClockGroupForPulsing);
			Go.killAllTweensWithTarget(durationText);
			durationText.color = Color.white;
			ClockGroupForPulsing.localScale = Vector3.one;
		}

		internal void ClearSlot()
		{
			costText.text = string.Empty;
			minionID = -1;
			SetMinionSliderState(MinionSliderState.Harvestable);
		}

		internal void CallMinion()
		{
			if (isResourcePlotSlider)
			{
				harvestTime = resourcePlot.parentLair.Definition.SecondsToHarvest;
			}
			else
			{
				harvestTime = BuildingUtil.GetHarvestTimeForTaskableBuilding(building, definitionService);
			}
			durationText.text = UIUtils.FormatTime(harvestTime, localService);
			rushCost = (uint)timeEventService.CalculateRushCostForTimer(harvestTime, RushActionType.HARVESTING);
			if (rushCost == 0)
			{
				SetMinionSliderState(MinionSliderState.Rushable);
				return;
			}
			costText.text = string.Format("{0}", rushCost);
			SetMinionSliderState(MinionSliderState.Working);
		}

		internal void ChangeMinionCount(bool increase)
		{
			if (increase)
			{
				count++;
			}
			else
			{
				count--;
			}
			minionCount.text = count.ToString();
			SetCallButtonState();
		}

		internal void setMinionText()
		{
			availableText.text = localService.GetString("ResourceAvailable");
		}

		internal void PurchaseSlot()
		{
			SetMinionSliderState(MinionSliderState.Available);
			minionID = -1;
		}

		internal void SetMinionSliderState(MinionSliderState i_state)
		{
			state = i_state;
			switch (state)
			{
			case MinionSliderState.Working:
				SetSliderWorking();
				break;
			case MinionSliderState.Available:
				SetMinionLevel();
				SetSliderAvailable();
				break;
			case MinionSliderState.Locked:
				SetSliderLocked();
				break;
			case MinionSliderState.Harvestable:
				SetSliderHarvestable();
				break;
			case MinionSliderState.Rushable:
				SetSliderRushable();
				break;
			}
			SetIdleMinionCount();
		}

		private void SetSliderAvailable()
		{
			callButton.gameObject.SetActive(true);
			harvestButton.gameObject.SetActive(false);
			ResetAndHideRushButton();
			ClockPanel.SetActive(false);
			AvailableMinionPanel.SetActive(true);
			ResetAndHideLockedButton();
			HarvestPanel.SetActive(false);
			if (modalSettings.enableCallThrob)
			{
				SetCallHighlight(true);
			}
		}

		private void SetSliderLocked()
		{
			callButton.gameObject.SetActive(false);
			ResetAndHideRushButton();
			harvestButton.gameObject.SetActive(false);
			ClockPanel.SetActive(false);
			AvailableMinionPanel.SetActive(false);
			lockedPanel.SetActive(true);
			HarvestPanel.SetActive(false);
			if (modalSettings.enableLockedThrob && lockedButton.GetComponent<Button>().interactable)
			{
				SetLockedHighlight(true);
			}
		}

		private void SetSliderHarvestable()
		{
			harvestButton.gameObject.SetActive(true);
			callButton.gameObject.SetActive(false);
			ResetAndHideRushButton();
			ClockPanel.SetActive(false);
			AvailableMinionPanel.SetActive(false);
			ResetAndHideLockedButton();
			HarvestPanel.SetActive(true);
			harvestButton.GetComponent<Button>().interactable = modalSettings.enableHarvestButtons;
		}

		private void SetSliderWorking()
		{
			harvestButton.gameObject.SetActive(false);
			callButton.gameObject.SetActive(false);
			rushButton.gameObject.SetActive(true);
			rushButton.EnableDoubleConfirm();
			ClockPanel.SetActive(true);
			AvailableMinionPanel.SetActive(false);
			ResetAndHideLockedButton();
			HarvestPanel.SetActive(false);
			rushButton.GetComponent<Button>().interactable = modalSettings.enableRushButtons;
			if (modalSettings.enableRushThrob)
			{
				SetRushHighlight(true);
			}
			rushFillImage.color = GameConstants.UI.UI_PURCHASE_BUTTON_COLOR;
			rushButton.EnableDoubleConfirm();
			Animator component = rushButton.gameObject.GetComponent<Animator>();
			component.runtimeAnimatorController = purchaseController;
			component.Play("Normal");
			rushCostImage.gameObject.SetActive(true);
			rushText.gameObject.SetActive(false);
			costText.gameObject.SetActive(true);
		}

		private void SetSliderRushable()
		{
			harvestButton.gameObject.SetActive(false);
			callButton.gameObject.SetActive(false);
			rushButton.gameObject.SetActive(true);
			rushButton.EnableDoubleConfirm();
			ClockPanel.SetActive(true);
			AvailableMinionPanel.SetActive(false);
			ResetAndHideLockedButton();
			HarvestPanel.SetActive(false);
			rushButton.GetComponent<Button>().interactable = modalSettings.enableRushButtons;
			rushFillImage.color = GameConstants.UI.UI_ACTION_BUTTON_COLOR;
			rushButton.DisableDoubleConfirm();
			Animator component = rushButton.gameObject.GetComponent<Animator>();
			component.runtimeAnimatorController = harvestController;
			component.Play("Normal");
			rushCostImage.gameObject.SetActive(false);
			rushText.gameObject.SetActive(true);
			costText.gameObject.SetActive(false);
			confirmText.gameObject.SetActive(false);
		}

		private void ResetAndHideRushButton()
		{
			rushButton.ResetAnim();
			StartCoroutine(WaitAFrame(rushButton.gameObject));
		}

		private void ResetAndHideLockedButton()
		{
			lockedButton.ResetAnim();
			StartCoroutine(WaitAFrame(lockedPanel));
		}

		private IEnumerator WaitAFrame(GameObject go)
		{
			yield return new WaitForEndOfFrame();
			if (go != null)
			{
				go.SetActive(false);
			}
		}

		internal void SetRushHighlight(bool isHighlighted)
		{
			if (!rushButton.enabled)
			{
				return;
			}
			isLockedHighlighted = true;
			Animator[] componentsInChildren = rushButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(rushButton.transform, 0.85f, 0.5f, out originalScale);
				return;
			}
			isLockedHighlighted = false;
			Go.killAllTweensWithTarget(rushButton.transform);
			rushButton.transform.localScale = originalScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}

		internal void SetCallHighlight(bool isHighlighted)
		{
			if (!callButton.enabled)
			{
				return;
			}
			isLockedHighlighted = true;
			Animator[] componentsInChildren = callButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(callButton.transform, 0.85f, 0.5f, out originalScale);
				return;
			}
			isLockedHighlighted = false;
			Go.killAllTweensWithTarget(callButton.transform);
			callButton.transform.localScale = originalScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}

		internal void SetLockedHighlight(bool isHighlighted)
		{
			if (!lockedButton.enabled)
			{
				return;
			}
			isLockedHighlighted = true;
			Animator[] componentsInChildren = lockedButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				TweenUtil.Throb(lockedButton.transform, 0.85f, 0.5f, out originalScale);
				return;
			}
			isLockedHighlighted = false;
			Go.killAllTweensWithTarget(lockedButton.transform);
			lockedButton.transform.localScale = originalScale;
			Animator[] array2 = componentsInChildren;
			foreach (Animator animator2 in array2)
			{
				animator2.enabled = true;
			}
		}

		internal uint GetRushCost()
		{
			return rushCost;
		}

		internal void SetRushCost()
		{
			if (!isResourcePlotSlider)
			{
				rushCost = (uint)building.Definition.RushCost;
			}
		}

		internal int GetPurchaseCost()
		{
			if (isResourcePlotSlider)
			{
				return -1;
			}
			return building.GetSlotCostByIndex(identifier);
		}

		internal void SetModalSettings(ModalSettings modalSettings)
		{
			this.modalSettings = modalSettings;
		}
	}
}
