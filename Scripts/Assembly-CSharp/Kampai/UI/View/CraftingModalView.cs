using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CraftingModalView : PopupMenuView
	{
		public Text title;

		public Text partyBoostText;

		public ButtonView backArrow;

		public ButtonView forwardArrow;

		public List<RectTransform> recipeLocations;

		public List<RectTransform> oneOffLocations;

		public RectTransform queueScrollView;

		public GameObject PartyPanel;

		public ButtonView freeRush;

		public HelpTipButtonView helpTipButtonView;

		public RectTransform helpPopoupParent;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private IQuestService questService;

		internal CraftingBuilding building;

		internal CraftableBuildingObject buildingObject;

		private List<GameObject> recipeIcons = new List<GameObject>();

		private List<CraftingQueueView> queueIcons = new List<CraftingQueueView>();

		private List<int> availableOneOffs = new List<int>();

		internal bool highlightItem;

		internal int higlightItemId;

		private GameObject dragTarget;

		private bool isProductionBuff;

		private float queueWidth;

		private float queuePadding;

		private CraftingRecipeView dragTutorialView;

		private int dragTutorialId;

		private GameObject crvCopy;

		private GoTween tutorialPosition;

		private GoTween tutorialFade;

		private GoTweenChain tutorialChain;

		internal void Init(IPlayerService playerService, IDefinitionService definitionService, IQuestService questService, CraftingBuilding building, CraftableBuildingObject buildingObject)
		{
			base.Init();
			this.playerService = playerService;
			this.definitionService = definitionService;
			this.questService = questService;
			this.building = building;
			this.buildingObject = buildingObject;
			helpTipButtonView.rectTransform = helpPopoupParent;
			StoreOneOffCraftables();
			PopulateRecipeIcons(HighlightType.DRAG);
			PopulateQueueIcons();
			base.Open();
		}

		internal void SetTitle(string localizedString)
		{
			title.text = localizedString;
		}

		internal void SetPartyInfo(float boost, string boostString, bool isOn = true)
		{
			partyBoostText.text = boostString;
			isProductionBuff = (int)(boost * 100f) != 100;
			PartyPanel.SetActive(isOn && isProductionBuff);
			SetChildrenAsPartying();
		}

		public void SetChildrenAsPartying()
		{
			foreach (CraftingQueueView queueIcon in queueIcons)
			{
				queueIcon.isCorrectBuffType = isProductionBuff;
			}
		}

		internal void RePopulateModal(CraftingBuilding building, CraftableBuildingObject buildingObject, HighlightType type)
		{
			this.building = building;
			if (this.buildingObject != null)
			{
				this.buildingObject.DisableHighLightBuilding();
			}
			this.buildingObject = buildingObject;
			CleanupRecipeIcons();
			StoreOneOffCraftables();
			PopulateRecipeIcons(type);
			RefreshQueue();
		}

		internal void RefreshQueue()
		{
			CleanupQueueIcons();
			PopulateQueueIcons();
		}

		private void StoreOneOffCraftables()
		{
			availableOneOffs.Clear();
			for (int i = 0; i < building.Definition.RecipeDefinitions.Count; i++)
			{
				int itemID = building.Definition.RecipeDefinitions[i].ItemID;
				DynamicIngredientsDefinition definition;
				if (definitionService.TryGet<DynamicIngredientsDefinition>(itemID, out definition) && IsDynamicRecipeAvailable(building, definition))
				{
					availableOneOffs.Add(itemID);
				}
			}
		}

		private Transform GetOneOffLocation(int count)
		{
			if (availableOneOffs.Count == 1)
			{
				return oneOffLocations[0];
			}
			if (availableOneOffs.Count == 2)
			{
				Vector2 anchoredPosition = oneOffLocations[count].anchoredPosition;
				oneOffLocations[count].anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y + 60f);
				return oneOffLocations[count];
			}
			return oneOffLocations[count - 1];
		}

		private void PopulateRecipeIcons(HighlightType type)
		{
			GameObject original = KampaiResources.Load<GameObject>("cmp_CraftingRecipe");
			int num = 0;
			int num2 = 0;
			List<RecipeDefinition> list = building.Definition.RecipeDefinitions as List<RecipeDefinition>;
			list.Sort(delegate(RecipeDefinition r1, RecipeDefinition r2)
			{
				DynamicIngredientsDefinition definition2;
				if (definitionService.TryGet<DynamicIngredientsDefinition>(r1.ItemID, out definition2))
				{
					return 1;
				}
				if (definitionService.TryGet<DynamicIngredientsDefinition>(r2.ItemID, out definition2))
				{
					return -1;
				}
				int levelItemUnlocksAt = definitionService.GetLevelItemUnlocksAt(r1.ItemID);
				int levelItemUnlocksAt2 = definitionService.GetLevelItemUnlocksAt(r2.ItemID);
				int num3 = levelItemUnlocksAt.CompareTo(levelItemUnlocksAt2);
				return (num3 != 0) ? num3 : r1.ItemID.CompareTo(r2.ItemID);
			});
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = false;
				int itemID = list[i].ItemID;
				DynamicIngredientsDefinition definition;
				GameObject gameObject;
				if (definitionService.TryGet<DynamicIngredientsDefinition>(itemID, out definition))
				{
					if (!availableOneOffs.Contains(itemID))
					{
						continue;
					}
					flag = true;
					num++;
					gameObject = Object.Instantiate(original);
					Transform oneOffLocation = GetOneOffLocation(num);
					gameObject.transform.SetParent(oneOffLocation, false);
				}
				else
				{
					gameObject = Object.Instantiate(original);
					gameObject.transform.SetParent(recipeLocations[num2], false);
					num2++;
				}
				CraftingRecipeView component = gameObject.GetComponent<CraftingRecipeView>();
				component.IsValidDragAreaSignal.AddListener(HighlightBuildingObject);
				if (flag)
				{
					component.groupQuantity.gameObject.SetActive(false);
				}
				if (highlightItem && higlightItemId == itemID)
				{
					switch (type)
					{
					case HighlightType.THROB:
						component.isHighlighted = true;
						break;
					case HighlightType.DRAG:
						dragTutorialView = component;
						dragTutorialId = itemID;
						break;
					default:
						logger.Error("Unknown highlight type {0}", type);
						break;
					}
				}
				component.recipeID = itemID;
				component.instanceID = building.ID;
				recipeIcons.Add(gameObject);
			}
		}

		public override void FinishedOpening()
		{
			ShowDragTutorial();
		}

		public void ShowDragTutorial()
		{
			if (dragTutorialView != null)
			{
				EnableDragTutorial(dragTutorialId, dragTutorialView);
				dragTutorialView = null;
			}
		}

		public void CleanupTweens()
		{
			TweenUtil.Cleanup(ref tutorialFade);
			TweenUtil.Cleanup(ref tutorialPosition);
			TweenUtil.Cleanup(ref tutorialChain);
			if (crvCopy != null)
			{
				Object.Destroy(crvCopy);
				crvCopy = null;
			}
		}

		private void EnableDragTutorial(int itemId, CraftingRecipeView crv)
		{
			CleanupTweens();
			crvCopy = CreateItemCopy(itemId, crv);
			tutorialPosition = Go.to(crvCopy.transform, 2f, new GoTweenConfig().position(DragTarget().position).setIterations(-1, GoLoopType.RestartFromBeginning));
			tutorialFade = new GoTween(crvCopy.GetComponent<KampaiImage>(), 1f, new GoTweenConfig().colorProp("color", new Color(1f, 1f, 1f, 0f)));
			tutorialChain = new GoTweenChain(new GoTweenCollectionConfig().setIterations(-1, GoLoopType.RestartFromBeginning));
			tutorialChain.append(tutorialFade).appendDelay(1f);
			tutorialChain.autoRemoveOnComplete = true;
			tutorialChain.play();
		}

		private GameObject CreateItemCopy(int itemId, CraftingRecipeView crv)
		{
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(itemId);
			GameObject gameObject = new GameObject("hint");
			gameObject.transform.parent = crv.unlockedImage.transform.parent;
			gameObject.layer = 5;
			KampaiImage kampaiImage = gameObject.AddComponent<KampaiImage>();
			kampaiImage.sprite = UIUtils.LoadSpriteFromPath(ingredientsItemDefinition.Image);
			kampaiImage.maskSprite = UIUtils.LoadSpriteFromPath(ingredientsItemDefinition.Mask);
			kampaiImage.material = crv.unlockedImage.material;
			kampaiImage.preserveAspect = true;
			RectTransform component = gameObject.GetComponent<RectTransform>();
			RectTransform component2 = crv.unlockedImage.gameObject.GetComponent<RectTransform>();
			RectUtil.Copy(component2, component);
			return gameObject;
		}

		private void HighlightBuildingObject(bool isValidArea, bool canCraftRecipe)
		{
			if (isValidArea)
			{
				buildingObject.EnableHighLightBuilding(canCraftRecipe);
			}
			else
			{
				buildingObject.DisableHighLightBuilding();
			}
		}

		private void CleanupRecipeIcons()
		{
			CleanupTweens();
			foreach (GameObject recipeIcon in recipeIcons)
			{
				Object.Destroy(recipeIcon);
			}
		}

		private void PopulateQueueIcons()
		{
			if (building == null)
			{
				return;
			}
			int num = building.Slots + 1;
			if (num > building.Definition.MaxQueueSlots)
			{
				num = building.Definition.MaxQueueSlots;
			}
			GameObject gameObject = KampaiResources.Load("cmp_CraftQueue") as GameObject;
			queueWidth = (gameObject.transform as RectTransform).sizeDelta.x;
			queuePadding = queueWidth / 5f;
			for (int i = 0; i < num; i++)
			{
				GameObject gameObject2 = SetupQueueView(i, gameObject);
				RectTransform rectTransform = gameObject2.transform as RectTransform;
				if (i == 0)
				{
					rectTransform.offsetMin = new Vector2(queueWidth * (float)i, 0f);
					rectTransform.offsetMax = new Vector2(queueWidth * (float)(i + 1), 0f);
				}
				else
				{
					rectTransform.offsetMin = new Vector2(queueWidth * (float)i + queuePadding, 0f);
					rectTransform.offsetMax = new Vector2(queueWidth * (float)(i + 1) + queuePadding, 0f);
				}
			}
			int num2 = 3 * (int)queueWidth;
			int num3 = num * (int)queueWidth + (int)queuePadding;
			int num4 = 0;
			if (num3 > num2)
			{
				num4 = (num3 - num2) / 2;
			}
			queueScrollView.sizeDelta = new Vector2(num3, 0f);
			queueScrollView.localPosition = new Vector2(num4, queueScrollView.localPosition.y);
		}

		private GameObject SetupQueueView(int index, GameObject prefab)
		{
			GameObject gameObject = Object.Instantiate(prefab);
			gameObject.transform.SetParent(queueScrollView);
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			rectTransform.localScale = Vector3.one;
			CraftingQueueView component = gameObject.GetComponent<CraftingQueueView>();
			component.index = index;
			component.building = building;
			if (index < building.Slots)
			{
				if (index > 0)
				{
					rectTransform.localScale *= 0.8f;
				}
				else
				{
					rectTransform.pivot = Vector2.one / 2f;
				}
				component.inProgressPanel.gameObject.SetActive(false);
				component.availablePanel.gameObject.SetActive(true);
				component.lockedPanel.gameObject.SetActive(false);
			}
			else
			{
				rectTransform.localScale *= 0.8f;
				component.isLocked = true;
				component.availablePanel.gameObject.SetActive(false);
				component.inProgressPanel.gameObject.SetActive(false);
				component.lockedPanel.gameObject.SetActive(true);
				component.purchaseCost = building.getNextIncrementalCost();
				component.lockedCost.text = component.purchaseCost.ToString();
			}
			queueIcons.Add(component);
			return gameObject;
		}

		internal CraftingQueueView GetFirstQueueItem()
		{
			return (queueIcons.Count <= 0) ? null : queueIcons[0];
		}

		private void CleanupQueueIcons()
		{
			foreach (CraftingQueueView queueIcon in queueIcons)
			{
				if (queueIcon != null && queueIcon.gameObject != null)
				{
					Object.Destroy(queueIcon.gameObject);
				}
			}
			queueIcons.Clear();
		}

		internal void UpdateQueuePosition()
		{
		}

		internal void SetArrowButtonState(bool enable)
		{
			backArrow.GetComponent<Button>().interactable = enable;
			forwardArrow.GetComponent<Button>().interactable = enable;
		}

		private bool IsDynamicRecipeAvailable(CraftingBuilding fromBuilding, DynamicIngredientsDefinition definition)
		{
			int iD = definition.ID;
			int num = 0;
			num = questService.IsOneOffCraftableDisplayable(definition.QuestDefinitionUnlockId, iD);
			if (num == 0)
			{
				return false;
			}
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(iD);
			if (quantityByDefinitionId >= num)
			{
				return false;
			}
			int num2 = 0;
			ICollection<CraftingBuilding> byDefinitionId = playerService.GetByDefinitionId<CraftingBuilding>(fromBuilding.Definition.ID);
			foreach (CraftingBuilding item in byDefinitionId)
			{
				foreach (int item2 in item.RecipeInQueue)
				{
					if (item2 == iD)
					{
						num2++;
					}
				}
				foreach (int completedCraft in item.CompletedCrafts)
				{
					if (completedCraft == iD)
					{
						num2++;
					}
				}
			}
			if (quantityByDefinitionId + num2 >= num)
			{
				return false;
			}
			return true;
		}

		internal void ResetDoubleTap(int viewId)
		{
			foreach (CraftingQueueView queueIcon in queueIcons)
			{
				if (queueIcon.index != viewId)
				{
					if (queueIcon.inProduction && queueIcon.inProgressRush != null)
					{
						queueIcon.inProgressRush.ResetTapState();
						queueIcon.inProgressRush.ResetAnim();
					}
					else if (queueIcon.isLocked && queueIcon.lockedPurchase != null)
					{
						queueIcon.lockedPurchase.ResetTapState();
						queueIcon.lockedPurchase.ResetAnim();
					}
				}
			}
		}

		private Transform DragTarget()
		{
			if (dragTarget == null)
			{
				dragTarget = base.gameObject.FindChild("drag_hint");
				if (dragTarget == null)
				{
					logger.Error("No drag target found");
				}
			}
			return dragTarget.transform;
		}

		public override void Close(bool instant = false)
		{
			base.Close(instant);
			CleanupTweens();
		}

		public void EnableRewardedAdRushButton(bool enable)
		{
			freeRush.gameObject.SetActive(enable);
		}
	}
}
