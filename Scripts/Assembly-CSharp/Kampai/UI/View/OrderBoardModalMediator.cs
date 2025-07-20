using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class OrderBoardModalMediator : UIStackMediator<OrderBoardModalView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("OrderBoardModalMediator") as IKampaiLogger;

		private OrderBoard building;

		private int currentSelectedTickedIndex;

		private TransactionDefinition currentTransactionDef;

		private IList<QuantityItem> currentMissingItems;

		private int currentFulfilledTicket = -1;

		private Prestige currentSelectedPrestige;

		private bool waitingDoobersToClose;

		private bool prestigeFull;

		private bool beingPrestiged;

		private AwardLevelSignal awardLevelSignal;

		private CloseDownOrderBoardUISignal closeDownOrderBoardUISignal;

		private readonly AdPlacementName adPlacementName = AdPlacementName.ORDERBOARD;

		private AdPlacementInstance adPlacementInstance;

		private bool alreadyClosing;

		private ModalSettings modalSettings = new ModalSettings();

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public OrderBoardTicketClickedSignal ticketClicked { get; set; }

		[Inject]
		public OrderBoardTicketDeletedSignal ticketDeletedSignal { get; set; }

		[Inject]
		public OrderBoardPrestigeSlotFullSignal prestigeSlotFullSignal { get; set; }

		[Inject]
		public OrderBoardFillOrderSignal fillOrderSignal { get; set; }

		[Inject]
		public OrderBoardDeleteOrderSignal deleteOrderSignal { get; set; }

		[Inject]
		public OrderBoardRefillTicketSignal refillTicketSignal { get; set; }

		[Inject]
		public RushDialogConfirmationSignal dialogConfirmedSignal { get; set; }

		[Inject]
		public OrderBoardTransactionFailedSignal transactionFailedSignal { get; set; }

		[Inject]
		public ShowHUDReminderSignal showHUDReminderSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public OrderBoardStartFillingPrestigeBarSignal startFillingPrestigeBarSignal { get; set; }

		[Inject]
		public OrderBoardFillOrderCompleteSignal fillOrderCompleteSignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public SetFTUETextSignal setFTUETextSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public DoobersFlownSignal doobersFlownSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListener { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public GetBuffStateSignal getBuffStateSignal { get; set; }

		[Inject]
		public GoToResourceButtonClickedSignal gotoResourceBuildingSignal { get; set; }

		[Inject]
		public IGoToService goToService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.CloseButton.ClickedSignal.AddListener(Close);
			base.view.FillOrderButton.ClickedSignal.AddListener(FillOrder);
			base.view.DeleteButton.ClickedSignal.AddListener(DeleteTicket);
			base.view.AdVideoButton.ClickedSignal.AddListener(AdVideo);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			ticketClicked.AddListener(TicketClicked);
			dialogConfirmedSignal.AddListener(ConfirmClicked);
			refillTicketSignal.AddListener(RefillTicket);
			transactionFailedSignal.AddListener(TransactionFailed);
			fillOrderCompleteSignal.AddListener(FillOrderComplete);
			resetDoubleTapSignal.AddListener(ResetDoubleTap);
			setFTUETextSignal.AddListener(SetFTUEText);
			doobersFlownSignal.AddListener(DoobersFlown);
			awardLevelSignal = gameContext.injectionBinder.GetInstance<AwardLevelSignal>();
			awardLevelSignal.AddListener(AwardLevel);
			closeDownOrderBoardUISignal = gameContext.injectionBinder.GetInstance<CloseDownOrderBoardUISignal>();
			closeDownOrderBoardUISignal.AddListener(SpecialCloseDownForPrePartyPause);
			gotoResourceBuildingSignal.AddListener(GotoResourceBuilding);
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.uiRemovedSignal.Dispatch(base.view.gameObject);
			ticketClicked.RemoveListener(TicketClicked);
			refillTicketSignal.RemoveListener(RefillTicket);
			base.view.CloseButton.ClickedSignal.RemoveListener(Close);
			base.view.FillOrderButton.ClickedSignal.RemoveListener(FillOrder);
			base.view.DeleteButton.ClickedSignal.RemoveListener(DeleteTicket);
			base.view.AdVideoButton.ClickedSignal.RemoveListener(AdVideo);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			dialogConfirmedSignal.RemoveListener(ConfirmClicked);
			transactionFailedSignal.RemoveListener(TransactionFailed);
			fillOrderCompleteSignal.RemoveListener(FillOrderComplete);
			resetDoubleTapSignal.RemoveListener(ResetDoubleTap);
			setFTUETextSignal.RemoveListener(SetFTUEText);
			doobersFlownSignal.RemoveListener(DoobersFlown);
			awardLevelSignal.RemoveListener(AwardLevel);
			closeDownOrderBoardUISignal.RemoveListener(SpecialCloseDownForPrePartyPause);
			gotoResourceBuildingSignal.RemoveListener(GotoResourceBuilding);
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
		}

		public override void Initialize(GUIArguments args)
		{
			base.view.SetDeleteButtonEnabled(!args.Contains<DisableDeleteOrderButton>());
			OrderBoard orderBoard = args.Get<OrderBoard>();
			modalSettings.enableTicketThrob = args.Contains<ThrobTicketButton>();
			base.view.modalSettings = modalSettings;
			base.view.Init(args.Get<OrderBoardBuildingTicketsView>(), positionService, guiService, orderBoard.Definition.TicketRepopTime, localService, false);
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			gameContext.injectionBinder.GetInstance<OrderBoardClearTicketOnBoardSignal>().Dispatch();
			LoadTicketsFromBuilding(orderBoard);
			if (modalSettings.enableTicketThrob)
			{
				setFTUETextSignal.Dispatch("ftue_q6_order");
				HideButtons(true);
			}
			if (playerService.GetMinionPartyInstance().IsPartyReady)
			{
				Close();
			}
			else
			{
				UpdateAdButton();
			}
		}

		internal void AwardLevel(TransactionDefinition td)
		{
			Close();
		}

		internal void DoobersFlown()
		{
			if (waitingDoobersToClose)
			{
				Close();
			}
		}

		internal void FillOrderComplete(int ticketIndex)
		{
			if (currentSelectedPrestige != null && !prestigeFull)
			{
				displayPlayerTrainingSignal.Dispatch(currentSelectedPrestige.Definition.PlayerTrainingNonPrestigeDefinitionId, false, new Signal<bool>());
			}
			currentFulfilledTicket = -1;
			currentSelectedTickedIndex = ticketIndex;
			foreach (OrderBoardTicket ticket in building.tickets)
			{
				if (ticket.BoardIndex == currentSelectedTickedIndex)
				{
					AddTicket(ticket, false);
				}
				CheckSingleTicketRequirementMatchingState(ticket);
			}
			SetTicketClicks(true);
		}

		internal void ConfirmClicked()
		{
			FillOrder();
		}

		internal void TransactionFailed(TransactionDefinition td)
		{
			if (td == currentTransactionDef)
			{
				SetDeleteOrderButton(true);
				base.view.SetFillOrderButtonState(OrderBoardButtonState.Enable);
				currentFulfilledTicket = -1;
			}
		}

		internal void CheckTicketRequirementMatchingState()
		{
			foreach (OrderBoardTicket ticket in building.tickets)
			{
				CheckSingleTicketRequirementMatchingState(ticket);
			}
		}

		internal void CheckSingleTicketRequirementMatchingState(OrderBoardTicket ticket)
		{
			TransactionInstance transactionInst = ticket.TransactionInst;
			bool ticketCheckmark = true;
			int count = transactionInst.Inputs.Count;
			for (int i = 0; i < count; i++)
			{
				QuantityItem quantityItem = transactionInst.Inputs[i];
				uint quantity = quantityItem.Quantity;
				uint quantityByDefinitionId = playerService.GetQuantityByDefinitionId(quantityItem.ID);
				if (quantity > quantityByDefinitionId)
				{
					ticketCheckmark = false;
				}
			}
			base.view.TicketSlots[ticket.BoardIndex].SetTicketCheckmark(ticketCheckmark);
		}

		internal void DeleteTicket()
		{
			resetDoubleTapSignal.Dispatch(-1);
			SetDeleteOrderButton(false);
			base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
			soundFXSignal.Dispatch("Play_delete_ticket_01");
			deleteOrderSignal.Dispatch(currentSelectedTickedIndex, currentTransactionDef, building);
			ticketDeletedSignal.Dispatch();
		}

		internal void AdVideo()
		{
			if (adPlacementInstance != null)
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
			}
		}

		protected override void Close()
		{
			Close(ZoomOut, null);
		}

		private void ZoomOut(Action onComplete)
		{
			BuildingZoomSettings type = new BuildingZoomSettings(ZoomType.OUT, BuildingZoomType.ORDERBOARD, onComplete);
			gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(type);
		}

		private void Close(Action<Action> moveAwayAction, Action onComplete)
		{
			if (!alreadyClosing)
			{
				alreadyClosing = true;
				if (playerService.GetHighestFtueCompleted() == 999999)
				{
					removeWayFinderSignal.Dispatch(309);
				}
				if (building != null)
				{
					building.MenuOpened = false;
				}
				else
				{
					OnMenuClose();
				}
				moveAudioListener.Dispatch(true, null);
				soundFXSignal.Dispatch("Play_menu_disappear_01");
				base.view.Close();
				moveAwayAction(onComplete);
				gameContext.injectionBinder.GetInstance<OrderBoardUpdateTicketOnBoardSignal>().Dispatch();
			}
		}

		private void SpecialCloseDownForPrePartyPause()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			gameContext.injectionBinder.GetInstance<OrderBoardUpdateTicketOnBoardSignal>().Dispatch();
			if (building != null)
			{
				building.MenuOpened = false;
			}
			base.view.Close();
		}

		private void OnMenuClose()
		{
			showHUDReminderSignal.Dispatch(true);
			moveAudioListener.Dispatch(true, null);
			base.view.DestoryTickets();
			hideSkrim.Dispatch("OrderBoardSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_OrderBoard");
		}

		internal void FillOrder()
		{
			resetDoubleTapSignal.Dispatch(-1);
			if (base.view.FillOrderButton.isDoubleConfirmed())
			{
				if (currentTransactionDef != null)
				{
					currentFulfilledTicket = currentSelectedTickedIndex;
					SetDeleteOrderButton(false);
					if (base.view.FillOrderButton.GetLastFillOrderButtonState() == OrderBoardButtonState.Rush)
					{
						int lastRushCost = base.view.FillOrderButton.GetLastRushCost();
						base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
						playerService.ProcessOrderFill(lastRushCost, currentMissingItems, true, RushTransactionCallback);
					}
					else
					{
						base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
						CheckIfItIsPrestigeTicketBeforeFillingOrder();
					}
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "Trying to start an empty black market transaction");
				}
			}
			else
			{
				soundFXSignal.Dispatch("Play_button_click_01");
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				setPremiumCurrencySignal.Dispatch();
				CheckIfItIsPrestigeTicketBeforeFillingOrder();
			}
		}

		private void CheckIfItIsPrestigeTicketBeforeFillingOrder()
		{
			SetTicketClicks(false);
			if (currentSelectedPrestige != null)
			{
				int currentPrestigePoints = currentSelectedPrestige.CurrentPrestigePoints;
				int num = currentPrestigePoints + TransactionUtil.ExtractQuantityFromTransaction(currentTransactionDef, 2);
				int neededPrestigePoints = currentSelectedPrestige.NeededPrestigePoints;
				if (num >= neededPrestigePoints)
				{
					base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
					prestigeFull = true;
					num = neededPrestigePoints;
				}
				beingPrestiged = true;
				startFillingPrestigeBarSignal.Dispatch(num, FillOrderAfterBarIsFilled);
			}
			else
			{
				fillOrderSignal.Dispatch(currentSelectedTickedIndex, currentTransactionDef, building);
			}
		}

		private void SetTicketClicks(bool enabled)
		{
			base.view.SetTicketClicks(enabled);
		}

		private void FillOrderAfterBarIsFilled()
		{
			if (prestigeFull)
			{
				waitingDoobersToClose = true;
			}
			beingPrestiged = false;
			fillOrderSignal.Dispatch(currentSelectedTickedIndex, currentTransactionDef, building);
		}

		internal void GetToNextAvailableTicket(bool mute)
		{
			if (building.tickets.Count != 0 && !modalSettings.enableTicketThrob)
			{
				OrderBoardTicketView firstClickableTicketIndex = base.view.GetFirstClickableTicketIndex();
				if (firstClickableTicketIndex.IsCounting())
				{
					SetDeleteOrderButton(false);
					base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
					ticketDeletedSignal.Dispatch();
				}
				else
				{
					ticketClicked.Dispatch(firstClickableTicketIndex.ticketInstance, firstClickableTicketIndex.Title, mute);
				}
			}
		}

		internal void RefillTicket(int negativeIndex)
		{
			StartCoroutine(GetNewTicket(-negativeIndex));
		}

		private IEnumerator GetNewTicket(int index)
		{
			yield return null;
			if (!(base.view != null))
			{
				yield break;
			}
			foreach (OrderBoardTicket ticket in building.tickets)
			{
				if (ticket.BoardIndex == index)
				{
					AddTicket(ticket, false);
					CheckSingleTicketRequirementMatchingState(ticket);
				}
			}
		}

		internal void TicketClicked(OrderBoardTicket ticketInstance, string title, bool mute)
		{
			resetDoubleTapSignal.Dispatch(-1);
			TransactionInstance transactionInst = ticketInstance.TransactionInst;
			if (!mute)
			{
				soundFXSignal.Dispatch("Play_button_click_01");
			}
			base.view.TicketSlots[currentSelectedTickedIndex].SetTicketSelected(false);
			currentSelectedTickedIndex = ticketInstance.BoardIndex;
			base.view.TicketSlots[currentSelectedTickedIndex].SetTicketSelected(true);
			currentTransactionDef = transactionInst.ToDefinition();
			SetDeleteOrderButton(true);
			bool flag = false;
			int characterDefinitionId = ticketInstance.CharacterDefinitionId;
			if (characterDefinitionId != 0)
			{
				currentSelectedPrestige = characterService.GetPrestige(characterDefinitionId);
				if (currentSelectedPrestige == null)
				{
					logger.Error("You have a prestige ticket that doesn't have a prestige instance: {0}", characterDefinitionId);
					return;
				}
				PrestigeType type = currentSelectedPrestige.Definition.Type;
				if ((type == PrestigeType.Minion && characterService.IsTikiBarFull()) || (type == PrestigeType.Villain && characterService.GetEmptyCabana() == null))
				{
					flag = true;
					prestigeSlotFullSignal.Dispatch((type != 0) ? "VillainSlotFull" : "MinionSlotFull");
				}
			}
			else
			{
				currentSelectedPrestige = null;
			}
			if (currentFulfilledTicket == -1)
			{
				MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
				if (minionPartyInstance.IsPartyReady)
				{
					base.view.SetFillOrderButtonState(OrderBoardButtonState.Disable);
				}
				else
				{
					currentMissingItems = playerService.GetMissingItemListFromTransaction(currentTransactionDef);
					if (currentMissingItems.Count == 0)
					{
						base.view.SetFillOrderButtonState(waitingDoobersToClose ? OrderBoardButtonState.Disable : OrderBoardButtonState.MeetRequirement);
					}
					else
					{
						int rushCost = playerService.CalculateRushCost(currentMissingItems);
						base.view.SetFillOrderButtonState(waitingDoobersToClose ? OrderBoardButtonState.Disable : OrderBoardButtonState.Rush, rushCost);
					}
				}
			}
			if (!base.view.DeleteButton.gameObject.activeSelf)
			{
				base.view.DeleteButton.gameObject.SetActive(true);
			}
			if (flag)
			{
				base.view.SetFillOrderButtonState(OrderBoardButtonState.Hide);
			}
			UpdateAdButton();
		}

		internal void SetDeleteOrderButton(bool active)
		{
			base.view.SetupDeleteOrderButton(active);
		}

		internal void LoadTicketsFromBuilding(OrderBoard building)
		{
			this.building = building;
			foreach (OrderBoardTicket ticket in building.tickets)
			{
				AddTicket(ticket, ticket.StartTime >= 0, true);
			}
			CheckTicketRequirementMatchingState();
			GetToNextAvailableTicket(true);
		}

		internal void AddTicket(OrderBoardTicket ticket, bool isInProgress, bool isInit = false)
		{
			string empty = string.Empty;
			if (ticket.CharacterDefinitionId == 0)
			{
				empty = building.Definition.OrderNames[ticket.OrderNameTableIndex];
			}
			else
			{
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(ticket.CharacterDefinitionId);
				empty = prestigeDefinition.LocalizedKey;
			}
			string @string = localService.GetString(empty);
			int eventDuration = timeEventService.GetEventDuration(-ticket.BoardIndex);
			base.view.AddTicket(ticket, isInProgress, eventDuration, @string, characterService, isInit, getBuffStateSignal, (!beingPrestiged) ? ticketClicked : null, playerService);
		}

		private void HideButtons(bool hide)
		{
			if (hide)
			{
				base.view.FillOrderButton.gameObject.SetActive(false);
				base.view.DeleteButton.gameObject.SetActive(false);
			}
			else
			{
				base.view.FillOrderButton.gameObject.SetActive(true);
				base.view.DeleteButton.gameObject.SetActive(true);
			}
		}

		private void ResetDoubleTap(int id)
		{
			base.view.ResetDoubleTap(id);
		}

		private void SetFTUEText(string title)
		{
			base.view.CloseButton.gameObject.SetActive(false);
		}

		private void GotoResourceBuilding(int itemDefinitionId)
		{
			int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(itemDefinitionId);
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID);
			if (byDefinitionId.Count > 0)
			{
				Building suitableBuilding = GotoBuildingHelpers.GetSuitableBuilding(byDefinitionId);
				if (suitableBuilding.State != BuildingState.Inventory)
				{
					Close(MoveToBuildingAction(itemDefinitionId), null);
					return;
				}
			}
			goToService.GoToBuildingFromItem(itemDefinitionId);
		}

		private Action<Action> MoveToBuildingAction(int itemDefID)
		{
			return delegate
			{
				goToService.GoToBuildingFromItem(itemDefID);
			};
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			adPlacementInstance = null;
			playerService.ProcessOrderFill(0, currentMissingItems, true, RushTransactionCallback);
			rewardedAdService.RewardPlayer(null, placement);
			telemetryService.Send_Telemetry_EVT_AD_INTERACTION(placement.Definition.Name, currentMissingItems, placement.RewardPerPeriodCount);
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			UpdateAdButton();
		}

		private void UpdateAdButton()
		{
			bool flag = rewardedAdService.IsPlacementActive(adPlacementName);
			if (!flag)
			{
				logger.Debug("Ads: placement '{0}' for the order board is disabled.", adPlacementName);
			}
			AdPlacementInstance placementInstance = rewardedAdService.GetPlacementInstance(adPlacementName);
			bool enable = flag && placementInstance != null;
			base.view.EnableRewardedAdRushButton(enable);
			adPlacementInstance = placementInstance;
		}
	}
}
