using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class StoreButtonView : DoubleConfirmButtonView, IDragHandler, IEventSystemHandler
	{
		public Text ItemName;

		public Text ItemDescription;

		public Text ItemPartyPoints;

		public Text Capacity;

		public Text Cost;

		public Image CostBacking;

		public StoreBadgeView ItemBadge;

		public KampaiImage ItemIcon;

		public string DragSpritePath;

		public string DragMaskPath;

		public string DragAnimationController;

		public KampaiImage MoneyIcon;

		public Text UnlockedAtLevel;

		public GameObject Locked;

		public GameObject Unlocked;

		public GameObject Highlighted;

		public float PaddingInPixels;

		public Transform BackingImageRect;

		public KampaiImage Arrow;

		public Color LockedTopBackingColor;

		public Color CapacityReachedBackingColor;

		public new Signal<Definition> ClickedSignal = new Signal<Definition>();

		public Signal BlockedSignal = new Signal();

		public Signal<PointerEventData, Definition, TransactionDefinition, bool> pointerDownSignal = new Signal<PointerEventData, Definition, TransactionDefinition, bool>();

		public Signal<PointerEventData, Definition, TransactionDefinition, int> pointerDragSignal = new Signal<PointerEventData, Definition, TransactionDefinition, int>();

		public Signal<PointerEventData, Definition, TransactionDefinition> pointerUpSignal = new Signal<PointerEventData, Definition, TransactionDefinition>();

		private IPlayerService playerService;

		private Vector3 originalScale;

		private Vector3 starOriginalScale;

		private int currentBadgeCount;

		private bool shouldBeRendered;

		private int currentBuildingCount;

		private Color defaultTopLeftBackingColor;

		private KampaiButton myButton;

		private bool isInCapacityReachedState;

		private bool isFunRewarding;

		private GameObject dragPromptItem;

		public Definition definition { get; set; }

		public TransactionDefinition transactionDef { get; set; }

		public StoreItemDefinition storeItemDefinition { get; set; }

		public int CurrentCapacity { get; private set; }

		public void init(IPlayerService plService)
		{
			playerService = plService;
			originalScale = Highlighted.transform.localScale;
			starOriginalScale = Vector3.one;
			myButton = GetComponent<KampaiButton>();
			Image component = BackingImageRect.GetChild(0).GetComponent<Image>();
			if ((bool)component)
			{
				defaultTopLeftBackingColor = component.color;
			}
			else
			{
				defaultTopLeftBackingColor = Color.gray;
			}
		}

		public void OnEnable()
		{
			ResetTapState();
		}

		public override void OnClickEvent()
		{
			ClickedSignal.Dispatch(definition);
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (!isDoubleConfirmed())
			{
				base.OnClickEvent();
			}
			bool flag = isDoubleConfirmed();
			pointerDownSignal.Dispatch(eventData, definition, transactionDef, flag && (currentBuildingCount < CurrentCapacity || CurrentCapacity < 0));
			if (flag)
			{
				ResetTapState();
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			pointerDragSignal.Dispatch(eventData, definition, transactionDef, currentBadgeCount);
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			pointerUpSignal.Dispatch(eventData, definition, transactionDef);
		}

		internal void SetNewUnlockState(bool isNewThingUnlocked)
		{
			if (isNewThingUnlocked)
			{
				ItemBadge.SetNewUnlockCounter(1, false);
				TweenUtil.Throb(ItemBadge.transform, 0.85f, 0.5f, out starOriginalScale);
			}
			else
			{
				ItemBadge.HideNew();
				Go.killAllTweensWithTarget(ItemBadge.transform);
				ItemBadge.transform.localScale = starOriginalScale;
			}
		}

		internal void UpdatePartyPointText(ILocalizationService localizationService)
		{
			DecorationBuildingDefinition decorationBuildingDefinition = definition as DecorationBuildingDefinition;
			LeisureBuildingDefintiion leisureBuildingDefintiion = definition as LeisureBuildingDefintiion;
			if (decorationBuildingDefinition != null)
			{
				ItemPartyPoints.text = localizationService.GetString("DecorationProduction*", decorationBuildingDefinition.XPReward);
			}
			else if (leisureBuildingDefintiion != null)
			{
				ItemPartyPoints.text = string.Format(localizationService.GetString("LeisureProductionBuildMenu*", leisureBuildingDefintiion.PartyPointsReward), UIUtils.FormatTime(leisureBuildingDefintiion.LeisureTimeDuration, localizationService));
			}
			if (decorationBuildingDefinition != null || leisureBuildingDefintiion != null)
			{
				RectTransform rectTransform = ItemDescription.gameObject.transform as RectTransform;
				rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 22.4f);
				ItemPartyPoints.gameObject.SetActive(ItemDescription.gameObject.activeSelf);
				isFunRewarding = true;
			}
			else
			{
				RectTransform rectTransform2 = ItemDescription.gameObject.transform as RectTransform;
				rectTransform2.offsetMin = new Vector2(rectTransform2.offsetMin.x, -5f);
				ItemPartyPoints.gameObject.SetActive(false);
			}
		}

		internal void ChangeBuildingCount(bool isAdding)
		{
			if (CurrentCapacity >= 0)
			{
				Capacity.text = string.Format("{0}/{1}", (!isAdding) ? (--currentBuildingCount) : (++currentBuildingCount), CurrentCapacity);
				UpdateIconColors();
			}
		}

		internal void SetBuildingCount(int buildingCount)
		{
			if (CurrentCapacity >= 0)
			{
				currentBuildingCount = buildingCount;
				Capacity.text = string.Format("{0}/{1}", currentBuildingCount, CurrentCapacity);
				UpdateIconColors();
			}
		}

		internal void AdjustIncrementalCost()
		{
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			if (buildingDefinition != null && buildingDefinition.IncrementalCost > 0)
			{
				ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(buildingDefinition.ID);
				StaticItem staticItem = (TransactionUtil.IsOnlyPremiumInputs(transactionDef) ? StaticItem.PREMIUM_CURRENCY_ID : StaticItem.GRIND_CURRENCY_ID);
				int number = TransactionUtil.SumOutputsForStaticItem(transactionDef, staticItem, true) + byDefinitionId.Count * buildingDefinition.IncrementalCost;
				Cost.text = UIUtils.FormatLargeNumber(number);
			}
		}

		internal void SetCapacity(int capacity)
		{
			CurrentCapacity = capacity;
			if (capacity < 0)
			{
				Capacity.gameObject.SetActive(false);
			}
			else
			{
				Capacity.text = string.Format("{0}/{1}", currentBuildingCount, CurrentCapacity);
			}
			UpdateIconColors();
		}

		private void UpdateIconColors()
		{
			if (currentBuildingCount >= CurrentCapacity && CurrentCapacity >= 0)
			{
				if (!isInCapacityReachedState)
				{
					ChangeStateToCapacityReached();
				}
				Capacity.color = GameConstants.UI.UI_BLACK;
				ItemIcon.Desaturate = 1f;
			}
			else
			{
				if (isInCapacityReachedState)
				{
					ChangeStateOutOfCapacityReached();
				}
				Capacity.color = GameConstants.UI.UI_TEXT_LIGHT_BLUE;
				ItemIcon.Desaturate = 0f;
			}
		}

		public void DisplayOrHideUnlockedCostIcons()
		{
			if (currentBuildingCount >= CurrentCapacity && CurrentCapacity >= 0)
			{
				DisplayCost(false);
				return;
			}
			int inventoryCountByDefinitionID = playerService.GetInventoryCountByDefinitionID(definition.ID);
			if (inventoryCountByDefinitionID > 0)
			{
				DisplayCost(false);
			}
			else
			{
				DisplayCost(true);
			}
		}

		internal void SetBadge(int badgeCount)
		{
			currentBadgeCount = badgeCount;
			ItemBadge.SetInventoryCount(badgeCount);
		}

		internal void SetHighlight(bool isHighlighted, HighlightType type = HighlightType.DRAG)
		{
			if (isHighlighted)
			{
				if (type == HighlightType.DRAG)
				{
					EnableDragTutorial(isHighlighted);
				}
				else
				{
					TweenUtil.Throb(Highlighted.transform, 0.85f, 0.5f, out originalScale);
				}
			}
			else
			{
				Go.killAllTweensWithTarget(Highlighted.transform);
				Highlighted.transform.localScale = originalScale;
				EnableDragTutorial(false);
			}
		}

		internal bool ChangeStateToUnlocked()
		{
			bool result = false;
			if (Locked.activeSelf)
			{
				SetNewUnlockState(true);
				Locked.SetActive(false);
				ItemIcon.Desaturate = 0f;
				result = true;
			}
			Unlocked.SetActive(true);
			if (!isInCapacityReachedState)
			{
				ChangeButtonBackingColor(defaultTopLeftBackingColor);
			}
			myButton.interactable = true;
			SetButtonTeased(false);
			return result;
		}

		internal bool IsUnlocked()
		{
			return Unlocked.activeSelf;
		}

		internal void ChangeStateToCapacityReached()
		{
			ChangeButtonBackingColor(CapacityReachedBackingColor);
			SetBuildingDraggable(false);
			DisplayCost(false);
			isInCapacityReachedState = true;
		}

		internal void ChangeStateOutOfCapacityReached()
		{
			ChangeButtonBackingColor(defaultTopLeftBackingColor);
			SetBuildingDraggable(true);
			isInCapacityReachedState = false;
		}

		internal void SetBuildingDraggable(bool set)
		{
			Arrow.enabled = set;
			myButton.interactable = set;
		}

		internal void DisplayCost(bool display)
		{
			Cost.enabled = display;
			CostBacking.enabled = display;
			MoneyIcon.enabled = display;
		}

		internal void ChangeStateToLocked()
		{
			Unlocked.SetActive(false);
			Locked.SetActive(true);
			ItemIcon.Desaturate = 1f;
			ChangeButtonBackingColor(LockedTopBackingColor);
			myButton.interactable = false;
			SetButtonTeased(false);
		}

		private void ChangeButtonBackingColor(Color topLeftBackingColor)
		{
			for (int i = 0; i < BackingImageRect.childCount; i++)
			{
				Transform child = BackingImageRect.GetChild(i);
				if (child != null)
				{
					child.GetComponent<Image>().color = topLeftBackingColor;
				}
			}
			BackingImageRect.GetComponent<Image>().color = topLeftBackingColor;
		}

		internal void SetButtonTeased(bool isTeased)
		{
			if (ItemIcon.gameObject.activeSelf != isTeased)
			{
				return;
			}
			ItemIcon.gameObject.SetActive(!isTeased);
			ItemName.gameObject.SetActive(!isTeased);
			ItemDescription.gameObject.SetActive(!isTeased);
			ItemPartyPoints.gameObject.SetActive(!isTeased && isFunRewarding);
			for (int i = 0; i < BackingImageRect.childCount; i++)
			{
				Transform child = BackingImageRect.GetChild(i);
				if (child != null)
				{
					child.gameObject.SetActive(!isTeased);
				}
			}
		}

		public void SetShouldBerendered(bool value)
		{
			shouldBeRendered = value;
		}

		public bool ShouldBeRendered()
		{
			if (!shouldBeRendered)
			{
				ItemIcon.gameObject.SetActive(false);
			}
			return shouldBeRendered;
		}

		private void EnableDragTutorial(bool enabled)
		{
			if (enabled)
			{
				if (dragPromptItem == null)
				{
					dragPromptItem = new GameObject("drag");
					dragPromptItem.transform.parent = base.gameObject.transform;
					dragPromptItem.layer = 5;
					KampaiImage kampaiImage = dragPromptItem.AddComponent<KampaiImage>();
					kampaiImage.sprite = UIUtils.LoadSpriteFromPath(DragSpritePath);
					kampaiImage.maskSprite = UIUtils.LoadSpriteFromPath(DragMaskPath);
					kampaiImage.material = KampaiResources.Load<Material>("CircleIconAlphaMaskMat");
					Animator animator = dragPromptItem.AddComponent<Animator>();
					animator.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(DragAnimationController);
					animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
					animator.applyRootMotion = false;
					RectTransform component = dragPromptItem.GetComponent<RectTransform>();
					RectTransform component2 = ItemIcon.gameObject.GetComponent<RectTransform>();
					RectUtil.Copy(component2, component);
				}
				dragPromptItem.SetActive(true);
			}
			else if (dragPromptItem != null)
			{
				dragPromptItem.SetActive(false);
				Object.Destroy(dragPromptItem);
				dragPromptItem = null;
			}
		}
	}
}
