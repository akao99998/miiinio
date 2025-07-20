using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class CraftingRecipeMediator : Mediator
	{
		private GoTween tween;

		private GameObject dragIcon;

		private GameObject dragGlow;

		private GameObject dragPrefab;

		private CraftingBuilding craftingBuilding;

		private IngredientsItemDefinition currentItemDef;

		private RectTransform dragTransform;

		private Vector2 initialIconPosition;

		private bool midDrag;

		private IEnumerator PointerDownWait;

		[Inject]
		public CraftingRecipeView view { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeStateSignal { get; set; }

		[Inject]
		public CraftingCompleteSignal craftingComplete { get; set; }

		[Inject]
		public UpdateQueueIcon updateQueueSignal { get; set; }

		[Inject]
		public CraftingQueuePositionUpdateSignal queuePositionSignal { get; set; }

		[Inject]
		public CraftingModalClosedSignal closedSignal { get; set; }

		[Inject]
		public CraftingRecipeUpdateSignal updateSignal { get; set; }

		[Inject]
		public CraftingUpdateReagentsSignal craftingUpdateReagentsSignal { get; set; }

		[Inject]
		public DisplayItemPopupSignal displayItemPopupSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public RushDialogConfirmationSignal rushedSignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public CraftingRecipeDragStartSignal dragStartSignal { get; set; }

		[Inject]
		public CraftingRecipeDragStopSignal dragStopSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		public override void OnRegister()
		{
			pauseSignal.AddListener(OnPause);
			closedSignal.AddListener(HandleClose);
			updateSignal.AddListener(OnUpdate);
			craftingUpdateReagentsSignal.AddListener(UpdateReagents);
			rushedSignal.AddListener(ItemRushed);
			hideItemPopupSignal.AddListener(RemovePopupDelay);
			view.pointerDownSignal.AddListener(PointerDown);
			view.pointerDragSignal.AddListener(PointerDrag);
			view.pointerUpSignal.AddListener(PointerUp);
			Init();
		}

		public override void OnRemove()
		{
			pauseSignal.RemoveListener(OnPause);
			closedSignal.RemoveListener(HandleClose);
			updateSignal.RemoveListener(OnUpdate);
			craftingUpdateReagentsSignal.RemoveListener(UpdateReagents);
			rushedSignal.RemoveListener(ItemRushed);
			hideItemPopupSignal.RemoveListener(RemovePopupDelay);
			view.pointerDownSignal.RemoveListener(PointerDown);
			view.pointerDragSignal.RemoveListener(PointerDrag);
			view.pointerUpSignal.RemoveListener(PointerUp);
		}

		private void Init()
		{
			view.Init(definitionService, playerService);
			craftingBuilding = playerService.GetByInstanceId<CraftingBuilding>(view.instanceID);
			dragPrefab = KampaiResources.Load<GameObject>("cmp_DragIcon");
		}

		private void OnPause()
		{
			if (midDrag && dragIcon != null)
			{
				HandleClose();
			}
			hideItemPopupSignal.Dispatch();
		}

		private void PointerDown(PointerEventData eventData, IngredientsItemDefinition iid)
		{
			resetDoubleTapSignal.Dispatch(-1);
			hideItemPopupSignal.Dispatch();
			displayItemPopupSignal.Dispatch(iid.ID, view.GetComponent<RectTransform>(), UIPopupType.CRAFTING);
			if (IsPointerDownValid())
			{
				GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
				if (!(gameObject == null))
				{
					currentItemDef = null;
					playSFXSignal.Dispatch("Play_pick_item_01");
					dragIcon = UnityEngine.Object.Instantiate(dragPrefab);
					dragIcon.transform.SetParent(glassCanvas.transform, false);
					KampaiIngoreRaycastImage component = dragIcon.transform.Find("img_RecipeItem").gameObject.GetComponent<KampaiIngoreRaycastImage>();
					component.sprite = UIUtils.LoadSpriteFromPath(iid.Image);
					component.maskSprite = UIUtils.LoadSpriteFromPath(iid.Mask);
					dragGlow = dragIcon.transform.Find("backing_glow").gameObject;
					SetSize();
					dragTransform.anchoredPosition = new Vector2((eventData.position / UIUtils.GetHeightScale()).x, (eventData.position / UIUtils.GetHeightScale()).y);
					initialIconPosition = dragTransform.anchoredPosition;
					midDrag = true;
					view.SetHighlight(false);
					dragStartSignal.Dispatch(iid.ID);
				}
			}
		}

		private void SetSize()
		{
			dragTransform = dragIcon.GetComponent<RectTransform>();
			dragTransform.localPosition = Vector3.zero;
			dragTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
			dragTransform.anchorMin = Vector2.zero;
			dragTransform.anchorMax = Vector2.zero;
			dragTransform.pivot = new Vector2(0.5f, 0.5f);
			dragTransform.sizeDelta = new Vector2(100f, 100f);
		}

		private bool IsPointerDownValid()
		{
			if (Input.touchCount > 1)
			{
				return false;
			}
			if (midDrag)
			{
				return false;
			}
			if (!view.isUnlocked)
			{
				playSFXSignal.Dispatch("Play_action_locked_01");
				return false;
			}
			return true;
		}

		private void PointerDrag(PointerEventData eventData)
		{
			if (!midDrag || !(dragIcon != null))
			{
				return;
			}
			dragTransform.anchoredPosition = new Vector2((eventData.position / UIUtils.GetHeightScale()).x, (eventData.position / UIUtils.GetHeightScale()).y);
			GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
			if (gameObject != null && gameObject.name.Equals("DragArea"))
			{
				if (craftingBuilding.RecipeInQueue.Count < craftingBuilding.Slots)
				{
					view.IsValidDragAreaSignal.Dispatch(true, view.greenCircle.gameObject.activeSelf);
				}
				dragGlow.SetActive(true);
			}
			else
			{
				view.IsValidDragAreaSignal.Dispatch(false, false);
				dragGlow.SetActive(false);
			}
		}

		private void PointerUp(PointerEventData eventData, IngredientsItemDefinition itemDef)
		{
			if (PointerDownWait != null)
			{
				return;
			}
			PointerDownWait = PopupDelay();
			StartCoroutine(PointerDownWait);
			view.IsValidDragAreaSignal.Dispatch(false, false);
			if (!midDrag)
			{
				return;
			}
			GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
			if (gameObject != null)
			{
				bool flag = gameObject.name == "DragArea";
				CraftingQueueView craftingQueueView = (flag ? null : gameObject.GetComponentInParent<CraftingQueueView>());
				if (flag || craftingQueueView != null)
				{
					if (craftingBuilding.RecipeInQueue.Count < craftingBuilding.Slots)
					{
						if (flag || (!craftingQueueView.isLocked && craftingQueueView.index >= craftingBuilding.RecipeInQueue.Count))
						{
							playSFXSignal.Dispatch("Play_place_item_01");
							currentItemDef = itemDef;
							RunTransaction();
							return;
						}
					}
					else
					{
						popupMessageSignal.Dispatch(localService.GetString("CraftQueueFull"), PopupMessageType.NORMAL);
					}
				}
			}
			TweenBackToOrigin();
		}

		private void TweenBackToOrigin()
		{
			tween = Go.to(dragTransform, 0.25f, new GoTweenConfig().setEaseType(GoEaseType.Linear).vector2Prop("anchoredPosition", initialIconPosition).onComplete(delegate
			{
				HandleClose();
			}));
		}

		private IEnumerator PopupDelay()
		{
			yield return new WaitForSeconds(0.5f);
			if (PointerDownWait != null)
			{
				hideItemPopupSignal.Dispatch();
			}
		}

		private void RemovePopupDelay()
		{
			if (PointerDownWait != null)
			{
				StopCoroutine(PointerDownWait);
				PointerDownWait = null;
			}
		}

		private void ItemRushed()
		{
			if (currentItemDef != null && craftingBuilding != null)
			{
				RunTransaction();
			}
		}

		private void RunTransaction()
		{
			playerService.StartTransaction(currentItemDef.TransactionId, TransactionTarget.INGREDIENT, TransactionCallback, new TransactionArg(craftingBuilding.ID));
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				setStorageSignal.Dispatch();
				int iD = craftingBuilding.ID;
				if (craftingBuilding.RecipeInQueue.Count == 0)
				{
					timeEventService.AddEvent(iD, Convert.ToInt32(timeService.CurrentTime()), (int)currentItemDef.TimeToHarvest, craftingComplete, TimeEventType.ProductionBuff);
					craftingBuilding.CraftingStartTime = timeService.CurrentTime();
					changeStateSignal.Dispatch(iD, BuildingState.Working);
				}
				playerService.UpdateCraftingQueue(iD, currentItemDef.ID);
				DynamicIngredientsDefinition definition;
				if (definitionService.TryGet<DynamicIngredientsDefinition>(currentItemDef.ID, out definition))
				{
					int iD2 = definition.ID;
					int num = questService.IsOneOffCraftableDisplayable(definition.QuestDefinitionUnlockId, iD2);
					int num2 = SumDynamicCount(iD2);
					if (num2 >= num)
					{
						view.gameObject.SetActive(false);
					}
				}
				updateQueueSignal.Dispatch();
				queuePositionSignal.Dispatch();
				craftingUpdateReagentsSignal.Dispatch();
				HandleClose();
			}
			else if (pct.ParentSuccess)
			{
				RunTransaction();
			}
			else
			{
				HandleClose();
			}
			currentItemDef = null;
		}

		private int SumDynamicCount(int defID)
		{
			int num = 0;
			num += (int)playerService.GetQuantityByDefinitionId(defID);
			ICollection<CraftingBuilding> byDefinitionId = playerService.GetByDefinitionId<CraftingBuilding>(craftingBuilding.Definition.ID);
			foreach (CraftingBuilding item in byDefinitionId)
			{
				foreach (int item2 in item.RecipeInQueue)
				{
					if (item2 == defID)
					{
						num++;
					}
				}
				foreach (int completedCraft in item.CompletedCrafts)
				{
					if (completedCraft == defID)
					{
						num++;
					}
				}
			}
			return num;
		}

		private void OnUpdate(int recipeDefId)
		{
			if (recipeDefId == view.recipeID && view.isUnlocked)
			{
				view.SetQuantity();
			}
		}

		private void UpdateReagents()
		{
			view.SetImageBorder();
			view.SetQuantity();
		}

		private void HandleClose()
		{
			if (midDrag)
			{
				midDrag = false;
				UnityEngine.Object.Destroy(dragIcon);
				if (tween != null)
				{
					tween.destroy();
				}
				dragStopSignal.Dispatch(view.recipeID);
			}
		}
	}
}
