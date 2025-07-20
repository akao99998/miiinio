using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MinionSliderMediator : Mediator
	{
		private IdleMinionSignal idleMinionSignal;

		[Inject]
		public MinionSliderView view { get; set; }

		[Inject]
		public UpdateSliderSignal updateSliderSignal { get; set; }

		[Inject]
		public UpdateVillainLairMenuViewSignal updateVillainLairMenuViewSignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public CallMinionSignal callMinionSignal { get; set; }

		[Inject]
		public SendMinionToLairResourcePlotSignal callMinionToResourcePlotSignal { get; set; }

		[Inject]
		public FinishCallMinionSignal finishCallMinionSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public UITryHarvestSignal tryHarvestSignal { get; set; }

		[Inject]
		public AwardLairBonusDropsThenSetHarvestReadySignal awardDropsThenHarvestReadySiganl { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public UpdateIdleMinionCountSignal updateMinionCountSignal { get; set; }

		[Inject]
		public UpdateSlotPurchaseButtonSignal updatePurchaseSlotSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(localService, definitionService);
			view.rushButton.ClickedSignal.AddListener(ProcessClick);
			view.callButton.ClickedSignal.AddListener(ProcessClick);
			view.harvestButton.ClickedSignal.AddListener(ProcessClick);
			view.lockedButton.ClickedSignal.AddListener(ProcessClick);
			finishCallMinionSignal.AddListener(FinishCallMinion);
			updateMinionCountSignal.AddListener(UpdateMinionCount);
			updatePurchaseSlotSignal.AddListener(UpdatePurchaseSlot);
			idleMinionSignal = gameContext.injectionBinder.GetInstance<IdleMinionSignal>();
			idleMinionSignal.AddListener(UpdateMinionCount);
			updateVillainLairMenuViewSignal.AddListener(UpdateParentPanel);
		}

		public override void OnRemove()
		{
			view.rushButton.ClickedSignal.RemoveListener(ProcessClick);
			view.callButton.ClickedSignal.RemoveListener(ProcessClick);
			view.harvestButton.ClickedSignal.RemoveListener(ProcessClick);
			view.lockedButton.ClickedSignal.RemoveListener(ProcessClick);
			finishCallMinionSignal.RemoveListener(FinishCallMinion);
			updateMinionCountSignal.RemoveListener(UpdateMinionCount);
			updatePurchaseSlotSignal.RemoveListener(UpdatePurchaseSlot);
			idleMinionSignal.RemoveListener(UpdateMinionCount);
			updateVillainLairMenuViewSignal.RemoveListener(UpdateParentPanel);
		}

		private void ProcessClick()
		{
			resetDoubleTapSignal.Dispatch(view.identifier);
			if (view.rushButton.GetComponent<Button>().interactable || view.callButton.GetComponent<Button>().interactable)
			{
				switch (view.state)
				{
				case MinionSliderState.Working:
					RushMinion();
					break;
				case MinionSliderState.Available:
					CallMinion();
					break;
				case MinionSliderState.Locked:
					PurchaseSlot();
					break;
				case MinionSliderState.Harvestable:
					Harvest();
					break;
				case MinionSliderState.Rushable:
					RushMinion();
					break;
				}
			}
		}

		private void RushMinion()
		{
			if (view.rushButton.isDoubleConfirmed())
			{
				if (view.isResourcePlotSlider)
				{
					playerService.ProcessRush((int)view.GetRushCost(), true, RushTransactionCallback, view.resourcePlot.parentLair.Definition.ResourceItemID);
				}
				else
				{
					playerService.ProcessRush((int)view.GetRushCost(), true, RushTransactionCallback, view.building.Definition.ItemId);
				}
			}
			else if (view.isLockedHighlighted)
			{
				view.SetRushHighlight(false);
				view.rushButton.ShowConfirmMessage();
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				return;
			}
			globalSFXSignal.Dispatch("Play_button_premium_01");
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(view.minionID);
			if (view.isResourcePlotSlider)
			{
				if (timeEventService.HasEventID(view.resourcePlot.ID))
				{
					timeEventService.RushEvent(view.resourcePlot.ID);
				}
				else
				{
					awardDropsThenHarvestReadySiganl.Dispatch(view.resourcePlot.ID);
				}
			}
			else
			{
				timeEventService.RushEvent(view.minionID);
				TaskableBuilding byInstanceId2 = playerService.GetByInstanceId<TaskableBuilding>(byInstanceId.BuildingID);
				bool alreadyRushed = byInstanceId2 is ResourceBuilding;
				playerService.GetByInstanceId<Minion>(view.minionID).AlreadyRushed = alreadyRushed;
			}
			view.ClearSlot();
			updateMinionCountSignal.Dispatch();
		}

		private void FinishCallMinion(Tuple<int, int, GameObject> tuple)
		{
			if (view.gameObject == tuple.Item3)
			{
				view.minionID = tuple.Item1;
				view.startTime = timeService.CurrentTime();
				view.CallMinion();
			}
			view.ChangeMinionCount(false);
		}

		private void CallMinion()
		{
			if (view.isResourcePlotSlider)
			{
				callMinionToResourcePlotSignal.Dispatch(view.resourcePlot.ID);
				view.minionID = view.resourcePlot.MinionIDInBuilding;
				view.startTime = timeService.CurrentTime();
				view.UpdateHarvestTime();
				view.CallMinion();
				view.ChangeMinionCount(false);
				UpdateParentPanel();
			}
			else
			{
				callMinionSignal.Dispatch(view.building, view.gameObject);
			}
			globalSFXSignal.Dispatch("Play_whistle_call_01");
			updateMinionCountSignal.Dispatch();
		}

		private void PurchaseSlot()
		{
			if (view.lockedButton.isDoubleConfirmed())
			{
				if (!view.isResourcePlotSlider)
				{
					playerService.ProcessSlotPurchase(view.GetPurchaseCost(), true, view.identifier + 1, PurchaseSlotTransactionCallback, view.building.ID);
				}
			}
			else if (view.isLockedHighlighted)
			{
				view.SetLockedHighlight(false);
				view.lockedButton.ShowConfirmMessage();
			}
		}

		private void Harvest()
		{
			int type = ((!view.isResourcePlotSlider) ? view.building.ID : view.resourcePlot.ID);
			tryHarvestSignal.Dispatch(type, delegate
			{
				view.PurchaseSlot();
				UpdateParentPanel();
			}, true);
		}

		private void UpdateParentPanel()
		{
			updateSliderSignal.Dispatch();
		}

		private void UpdateMinionCount()
		{
			view.SetIdleMinionCount();
			view.SetMinionLevel();
		}

		private void UpdatePurchaseSlot()
		{
			view.UpdateLockedButton();
		}

		private void PurchaseSlotTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				int slotUnlockLevelByIndex = view.building.GetSlotUnlockLevelByIndex(view.identifier);
				playerService.PurchaseSlotForBuilding(view.building.ID, slotUnlockLevelByIndex);
				globalSFXSignal.Dispatch("Play_button_premium_01");
				view.PurchaseSlot();
				setPremiumCurrencySignal.Dispatch();
				updatePurchaseSlotSignal.Dispatch();
			}
		}
	}
}
