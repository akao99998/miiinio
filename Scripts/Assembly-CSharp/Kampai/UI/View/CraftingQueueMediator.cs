using System;
using System.Collections;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class CraftingQueueMediator : Mediator
	{
		private bool isMidRecipeDrag;

		private GoTween activeScaleTween;

		[Inject]
		public CraftingQueueView view { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public UpdateQueueIcon updateQueueSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public RemoveCraftingQueueSignal removeCraftingQueueSignal { get; set; }

		[Inject]
		public RefreshQueueSlotSignal purchaseSignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public CraftingRecipeDragStartSignal recipeDragStartSignal { get; set; }

		[Inject]
		public CraftingRecipeDragStopSignal recipeDragStopSignal { get; set; }

		[Inject]
		public CraftingRecipeUpdateSignal recipeUpdateSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public CraftingCompleteSignal craftingComplete { get; set; }

		[Inject]
		public OpenStorageBuildingSignal openStorageBuildingSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeStateSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(definitionService, timeEventService, localizationService, playerService, changeStateSignal);
			view.inProgressRush.ClickedSignal.AddListener(RushButton);
			view.inProgressHarvest.ClickedSignal.AddListener(HarvestCraftable);
			view.lockedPurchase.ClickedSignal.AddListener(UnlockButton);
			updateQueueSignal.AddListener(UpdateView);
			recipeDragStartSignal.AddListener(OnRecipeDragStart);
			recipeDragStopSignal.AddListener(OnRecipeDragStop);
			view.onPointerEnterSignal.AddListener(OnPointerEnter);
			view.onPointerExitSignal.AddListener(OnPointerExit);
		}

		public override void OnRemove()
		{
			HandleHarvestables();
			view.inProgressRush.ClickedSignal.RemoveListener(RushButton);
			view.inProgressHarvest.ClickedSignal.RemoveListener(HarvestCraftable);
			view.lockedPurchase.ClickedSignal.RemoveListener(UnlockButton);
			updateQueueSignal.RemoveListener(UpdateView);
			recipeDragStartSignal.RemoveListener(OnRecipeDragStart);
			recipeDragStopSignal.RemoveListener(OnRecipeDragStop);
			view.onPointerEnterSignal.RemoveListener(OnPointerEnter);
			view.onPointerExitSignal.RemoveListener(OnPointerExit);
		}

		private void UpdateView()
		{
			view.Init(definitionService, timeEventService, localizationService, playerService, changeStateSignal);
		}

		private void HandleHarvestables()
		{
			if (view.harvestReady)
			{
				CraftingBuilding building = view.building;
				craftingComplete.Dispatch(building.ID);
			}
		}

		private void RushButton()
		{
			if (Input.touchCount <= 1)
			{
				resetDoubleTapSignal.Dispatch(view.index);
				if (view.isLocked)
				{
					playerService.ProcessRush(view.purchaseCost, true, PurchaseTransactionCallback);
				}
				else
				{
					Rush(view.rushCost);
				}
			}
		}

		public void Rush(int rushCost, bool checkStorage = true, bool checkDoubleConfirmation = true)
		{
			if (checkStorage && playerService.isStorageFull())
			{
				ShowStore();
			}
			else if (!checkDoubleConfirmation || view.inProgressRush.isDoubleConfirmed())
			{
				playerService.ProcessRush(rushCost, true, RushTransactionCallback, view.itemDef.ID);
			}
		}

		private void HarvestCraftable()
		{
			if (playerService.isStorageFull())
			{
				ShowStore();
				return;
			}
			CraftingBuilding building = view.building;
			HandleTween(uiCamera.WorldToScreenPoint(view.inProgressImage.transform.position));
			removeCraftingQueueSignal.Dispatch(new Tuple<int, int>(building.ID, 0));
			view.harvestReady = false;
			StartNextCraft(building);
		}

		private void ShowStore()
		{
			if (playerService.HasStorageBuilding())
			{
				StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
				openStorageBuildingSignal.Dispatch(byInstanceId, false);
			}
		}

		private void UnlockButton()
		{
			if (view.lockedPurchase.isDoubleConfirmed() && view.isLocked)
			{
				playerService.ProcessSlotPurchase(view.purchaseCost, true, view.index + 1, PurchaseTransactionCallback, view.building.ID);
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				CraftingBuilding building = view.building;
				globalSFXSignal.Dispatch("Play_button_premium_01");
				if (view.inProduction)
				{
					HandleTween(uiCamera.WorldToScreenPoint(view.inProgressImage.transform.position));
					timeEventService.RushEvent(view.building.ID);
					removeCraftingQueueSignal.Dispatch(new Tuple<int, int>(building.ID, 0));
				}
				else
				{
					HandleTween(uiCamera.WorldToScreenPoint(view.availableImage.transform.position));
					removeCraftingQueueSignal.Dispatch(new Tuple<int, int>(building.ID, view.index));
				}
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Harvest, QuestTaskTransition.Complete);
				setPremiumCurrencySignal.Dispatch();
				StartNextCraft(building);
			}
		}

		private void StartNextCraft(CraftingBuilding building)
		{
			if (building.RecipeInQueue.Count > 0)
			{
				building.CraftingStartTime = timeService.CurrentTime();
				IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(building.RecipeInQueue[0]);
				timeEventService.AddEvent(building.ID, Convert.ToInt32(timeService.CurrentTime()), (int)ingredientsItemDefinition.TimeToHarvest, craftingComplete, TimeEventType.ProductionBuff);
			}
		}

		private void HandleTween(Vector3 origin)
		{
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(view.itemDef.TransactionId);
			foreach (QuantityItem output in transactionDefinition.Outputs)
			{
				tweenSignal.Dispatch(origin, DooberUtil.GetDestinationType(output.ID, definitionService), output.ID, false);
			}
			StartCoroutine(WaitForDooberTween());
		}

		private IEnumerator WaitForDooberTween()
		{
			yield return new WaitForSeconds(2.5f);
			recipeUpdateSignal.Dispatch(view.itemDef.ID);
		}

		private void PurchaseTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				globalSFXSignal.Dispatch("Play_button_premium_01");
				purchaseSignal.Dispatch(true);
				setPremiumCurrencySignal.Dispatch();
			}
			else if (pct.ParentSuccess)
			{
				RushButton();
			}
		}

		private void OnRecipeDragStart(int recipeDefId)
		{
			isMidRecipeDrag = true;
		}

		private void OnRecipeDragStop(int recipeDefId)
		{
			isMidRecipeDrag = false;
			TweenScale(false);
		}

		private void OnPointerEnter(PointerEventData eventData)
		{
			if (isMidRecipeDrag && view.index >= view.building.RecipeInQueue.Count && !view.isLocked)
			{
				TweenScale(true);
			}
		}

		private void OnPointerExit(PointerEventData eventData)
		{
			TweenScale(false);
		}

		private void TweenScale(bool isFocused)
		{
			if (activeScaleTween != null)
			{
				activeScaleTween.destroy();
			}
			Vector3 one = Vector3.one;
			if (view.isLocked || view.index > 0)
			{
				one *= 0.8f;
			}
			if (isMidRecipeDrag && isFocused)
			{
				one *= 1.15f;
			}
			if (one != view.transform.localScale)
			{
				activeScaleTween = Go.to(view.transform, 0.2f, new GoTweenConfig().scale(one).onComplete(delegate(AbstractGoTween tween)
				{
					tween.destroy();
					activeScaleTween = null;
				}));
			}
		}
	}
}
