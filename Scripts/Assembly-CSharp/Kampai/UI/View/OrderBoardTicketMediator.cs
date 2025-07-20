using System.Collections;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class OrderBoardTicketMediator : Mediator
	{
		[Inject]
		public OrderBoardTicketView view { get; set; }

		[Inject]
		public OrderBoardTicketClickedSignal ticketClickedSignal { get; set; }

		[Inject]
		public OrderBoardStartRefillTicketSignal startRefillTicket { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public ResetDoubleTapSignal resetDoubleTapSignal { get; set; }

		[Inject]
		public StartCurrencyBuffSignal startCurrencyBuffSignal { get; set; }

		[Inject]
		public StopCurrencyBuffSignal stopCurrencyBuffSignal { get; set; }

		public override void OnRegister()
		{
			view.TicketButton.ClickedSignal.AddListener(OnTicketClicked);
			view.TicketMeter.RushButton.ClickedSignal.AddListener(OnTicketRushed);
			startRefillTicket.AddListener(StartTimer);
			startCurrencyBuffSignal.AddListener(view.UpdateReward);
			stopCurrencyBuffSignal.AddListener(view.UpdateReward);
		}

		public override void OnRemove()
		{
			view.OnRemove();
			view.TicketButton.ClickedSignal.RemoveListener(OnTicketClicked);
			view.TicketMeter.RushButton.ClickedSignal.RemoveListener(OnTicketRushed);
			startRefillTicket.RemoveListener(StartTimer);
			startCurrencyBuffSignal.RemoveListener(view.UpdateReward);
			stopCurrencyBuffSignal.RemoveListener(view.UpdateReward);
		}

		private void StartTimer(Tuple<int, int, float> tuple)
		{
			if (view.Index == tuple.Item1)
			{
				view.SetRootAnimation(false);
				StartCoroutine(ChangeToDeleteState(tuple.Item1, tuple.Item2, tuple.Item3));
			}
		}

		private IEnumerator ChangeToDeleteState(int index, int duration, float repopTime)
		{
			yield return new WaitForSeconds(repopTime);
			view.SetRootAnimation(true);
			view.StartTimer(index, duration);
		}

		private void OnTicketRushed()
		{
			resetDoubleTapSignal.Dispatch(view.Index);
			if (view.TicketMeter.RushButton.isDoubleConfirmed())
			{
				playerService.ProcessRush(view.TicketMeter.rushCost, true, RushTransactionCallback, 3022);
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				globalSFXSignal.Dispatch("Play_button_premium_01");
				timeEventService.RushEvent(-view.Index);
			}
		}

		private void OnTicketClicked()
		{
			if (!view.IsSelected)
			{
				view.HighlightTicket(false);
				ticketClickedSignal.Dispatch(view.ticketInstance, view.Title, false);
			}
		}
	}
}
