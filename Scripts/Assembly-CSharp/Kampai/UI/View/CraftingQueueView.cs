using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CraftingQueueView : KampaiView, IPointerExitHandler, IEventSystemHandler, IPointerEnterHandler
	{
		public RectTransform inProgressPanel;

		public RectTransform availablePanel;

		public RectTransform lockedPanel;

		public RectTransform clockPanel;

		public GameObject ClockIcon;

		public GameObject PartyIcon;

		public Transform ClockGroupForPulsing;

		public KampaiImage inProgressImage;

		public Text inProgressTime;

		public Text inProgressCost;

		public ScrollableButtonView inProgressRush;

		public ScrollableButtonView inProgressHarvest;

		public KampaiImage availableImage;

		public Text availableText;

		public Text lockedCost;

		public ScrollableButtonView lockedPurchase;

		internal bool isLocked;

		internal IngredientsItemDefinition itemDef;

		internal bool inProduction;

		internal bool harvestReady;

		private bool isPartying;

		internal bool isCorrectBuffType;

		private ITimeEventService timeEventService;

		private ILocalizationService localizationService;

		private IPlayerService playerService;

		private BuildingChangeStateSignal changeStateSignal;

		internal Signal<PointerEventData> onPointerEnterSignal = new Signal<PointerEventData>();

		internal Signal<PointerEventData> onPointerExitSignal = new Signal<PointerEventData>();

		public CraftingBuilding building { get; set; }

		public int index { get; set; }

		public int purchaseCost { get; set; }

		public int rushCost { get; set; }

		internal void Init(IDefinitionService definitionService, ITimeEventService timeEventService, ILocalizationService localService, IPlayerService playerService, BuildingChangeStateSignal changeStateSignal)
		{
			this.timeEventService = timeEventService;
			localizationService = localService;
			this.playerService = playerService;
			this.changeStateSignal = changeStateSignal;
			lockedPurchase.EnableDoubleConfirm();
			SetPartyState(false);
			if (index >= building.RecipeInQueue.Count)
			{
				return;
			}
			itemDef = definitionService.Get<IngredientsItemDefinition>(building.RecipeInQueue[index]);
			if (index == 0)
			{
				inProduction = true;
				inProgressPanel.gameObject.SetActive(true);
				inProgressRush.ResetTapState();
				inProgressRush.EnableDoubleConfirm();
				availablePanel.gameObject.SetActive(false);
				lockedPanel.gameObject.SetActive(false);
				inProgressImage.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
				inProgressImage.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
				Update();
			}
			else
			{
				inProgressPanel.gameObject.SetActive(false);
				availablePanel.gameObject.SetActive(true);
				lockedPanel.gameObject.SetActive(false);
				if (itemDef != null)
				{
					availableText.gameObject.SetActive(false);
					availableImage.gameObject.SetActive(true);
					availableImage.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
					availableImage.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
				}
			}
		}

		public void Update()
		{
			if (index == 0 && inProduction)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(building.ID);
				bool flag = playerService.GetMinionPartyInstance().IsBuffHappening && isCorrectBuffType;
				if (isPartying != flag)
				{
					SetPartyState(flag);
				}
				inProgressTime.text = UIUtils.FormatTime(timeRemaining, localizationService);
				rushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.CRAFTING);
				inProgressCost.text = rushCost.ToString();
				if (timeRemaining <= 0)
				{
					inProduction = false;
					harvestReady = true;
					SwapToHarvest();
				}
			}
		}

		private void SwapToHarvest()
		{
			changeStateSignal.Dispatch(building.ID, (building.RecipeInQueue.Count <= 1) ? BuildingState.Harvestable : BuildingState.HarvestableAndWorking);
			inProgressRush.gameObject.SetActive(false);
			clockPanel.gameObject.SetActive(false);
			inProgressHarvest.gameObject.SetActive(true);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			onPointerEnterSignal.Dispatch(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			onPointerExitSignal.Dispatch(eventData);
		}

		internal void SetPartyState(bool isPartying)
		{
			this.isPartying = isPartying;
			ClockIcon.SetActive(!isPartying);
			PartyIcon.SetActive(isPartying);
			if (isPartying)
			{
				Vector3 originalScale;
				TweenUtil.Throb(ClockGroupForPulsing, 1.1f, 0.2f, out originalScale);
				UIUtils.FlashingColor(inProgressTime, 0);
				return;
			}
			Go.killAllTweensWithTarget(ClockGroupForPulsing);
			Go.killAllTweensWithTarget(inProgressTime);
			inProgressTime.color = Color.white;
			ClockGroupForPulsing.localScale = Vector3.one;
		}
	}
}
