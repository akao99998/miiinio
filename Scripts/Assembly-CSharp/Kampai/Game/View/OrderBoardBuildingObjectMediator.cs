using System.Collections;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class OrderBoardBuildingObjectMediator : EventMediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("OrderBoardBuildingObjectMediator") as IKampaiLogger;

		[Inject]
		public OrderBoardBuildingObjectView view { get; set; }

		[Inject]
		public OrderBoardRefillTicketSignal refillTicketSignal { get; set; }

		[Inject]
		public OrderBoardStartRefillTicketSignal startRefillTicketSignal { get; set; }

		[Inject]
		public OrderBoardSetNewTicketSignal setNewTicketSignal { get; set; }

		[Inject]
		public PostTransactionSignal postTransactionSignal { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IOrderBoardService orderBoardService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public OrderBoardUpdateTicketOnBoardSignal updateTicketOnBoardSignal { get; set; }

		[Inject]
		public OrderBoardClearTicketOnBoardSignal clearTicketOnBoardSignal { get; set; }

		[Inject]
		public ToggleHitboxSignal toggleHitboxSignal { get; set; }

		public override void OnRegister()
		{
			startRefillTicketSignal.AddListener(StartRefillTicket);
			postTransactionSignal.AddListener(PostTransaction);
			awardLevelSignal.AddListener(AwardLevel);
			refillTicketSignal.AddListener(RefillTicket);
			updateTicketOnBoardSignal.AddListener(UpdateTicketState);
			clearTicketOnBoardSignal.AddListener(ClearTickets);
			toggleHitboxSignal.AddListener(ToggleHitbox);
			StartCoroutine(RefreshTickets(true));
		}

		public override void OnRemove()
		{
			refillTicketSignal.RemoveListener(RefillTicket);
			startRefillTicketSignal.RemoveListener(StartRefillTicket);
			awardLevelSignal.RemoveListener(AwardLevel);
			updateTicketOnBoardSignal.RemoveListener(UpdateTicketState);
			clearTicketOnBoardSignal.RemoveListener(ClearTickets);
			postTransactionSignal.RemoveListener(PostTransaction);
			toggleHitboxSignal.RemoveListener(ToggleHitbox);
		}

		private IEnumerator RefreshTickets(bool clearBoard = false)
		{
			yield return null;
			if (clearBoard)
			{
				view.ClearBoard();
			}
			if (!view.orderBoard.MenuOpened || clearBoard)
			{
				UpdateTicketState();
			}
		}

		private void AwardLevel(TransactionDefinition td)
		{
			logger.Debug("Award Level: {0}", td.ID);
			StartCoroutine(RefreshTickets());
		}

		private void ToggleHitbox(BuildingZoomType zoomBuildingType, bool enable)
		{
			if (zoomBuildingType == BuildingZoomType.ORDERBOARD)
			{
				view.ToggleHitbox(enable);
			}
		}

		private void PostTransaction(TransactionUpdateData update)
		{
			StartCoroutine(RefreshTickets());
		}

		private void UpdateTicketState()
		{
			orderBoardService.UpdateOrderNumber();
			foreach (OrderBoardTicket ticket in view.orderBoard.tickets)
			{
				if (ticket.StartTime != -1)
				{
					view.SetTicketState(ticket.BoardIndex, OrderBoardTicketState.TIMER);
					continue;
				}
				bool flag = false;
				TransactionInstance transactionInst = ticket.TransactionInst;
				int count = transactionInst.Inputs.Count;
				for (int i = 0; i < count; i++)
				{
					QuantityItem quantityItem = transactionInst.Inputs[i];
					uint quantity = quantityItem.Quantity;
					uint quantityByDefinitionId = playerService.GetQuantityByDefinitionId(quantityItem.ID);
					if (quantity > quantityByDefinitionId)
					{
						flag = true;
					}
				}
				OrderBoardTicketState orderBoardTicketState = OrderBoardTicketState.NOT_AVAILABLE;
				if (ticket.CharacterDefinitionId != 0)
				{
					Prestige prestige = prestigeService.GetPrestige(ticket.CharacterDefinitionId);
					orderBoardTicketState = ((prestige == null || prestige.Definition.Type != PrestigeType.Villain) ? ((!flag) ? OrderBoardTicketState.PRESTIGE_CHECKED : OrderBoardTicketState.PRESTIGE_UNCHECKED) : ((!flag) ? OrderBoardTicketState.VILLAIN_CHECKED : OrderBoardTicketState.VILLAIN_UNCHECKED));
				}
				else
				{
					orderBoardTicketState = (flag ? OrderBoardTicketState.UNCHECKED : OrderBoardTicketState.CHECKED);
				}
				view.SetTicketState(ticket.BoardIndex, orderBoardTicketState);
			}
		}

		private void ClearTickets()
		{
			view.ClearBoard();
			view.orderBoard.MenuOpened = true;
		}

		private void StartRefillTicket(Tuple<int, int, float> tuple)
		{
			StartCoroutine(RefreshTickets());
		}

		private void RefillTicket(int index)
		{
			setNewTicketSignal.Dispatch(index, false);
			StartCoroutine(RefreshTickets());
		}
	}
}
