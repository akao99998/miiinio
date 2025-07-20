using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class OrderBoardModalView : PopupMenuView
	{
		private const int TICKET_NUMBER = 9;

		public ButtonView CloseButton;

		public FillOrderButtonView FillOrderButton;

		public ButtonView DeleteButton;

		public ButtonView AdVideoButton;

		internal List<OrderBoardTicketView> TicketSlots = new List<OrderBoardTicketView>(9);

		internal ModalSettings modalSettings;

		private bool enabledDeleteButton = true;

		private bool isClosing;

		private float ticketRepopTime;

		private IEnumerator changeTicketCoRoutine;

		private bool adVideoButton;

		public void Init(OrderBoardBuildingTicketsView ticketsView, IPositionService positionService, IGUIService guiService, float ticketRepopTime, ILocalizationService localService, bool adVideoButton)
		{
			base.Init();
			this.ticketRepopTime = ticketRepopTime;
			for (int i = 0; i < 9; i++)
			{
				PositionData positionData = positionService.GetPositionData(ticketsView.GetTicketPosition(i));
				CreateTicket(i, modalSettings, positionData.WorldPositionInUI, guiService, localService);
			}
			foreach (OrderBoardTicketView ticketSlot in TicketSlots)
			{
				ticketSlot.gameObject.SetActive(false);
			}
			this.adVideoButton = adVideoButton;
			FillOrderButton.Init();
			base.Open();
		}

		public void EnableRewardedAdRushButton(bool enable)
		{
			adVideoButton = enable;
			if (FillOrderButton.previousState == OrderBoardButtonState.Rush)
			{
				AdVideoButton.gameObject.SetActive(enable);
			}
		}

		public void DestoryTickets(bool destory = true)
		{
			foreach (OrderBoardTicketView ticketSlot in TicketSlots)
			{
				if (destory)
				{
					ticketSlot.OnRemove();
					Object.Destroy(ticketSlot.gameObject);
				}
				else
				{
					ticketSlot.gameObject.SetActive(false);
				}
			}
			if (destory)
			{
				if (changeTicketCoRoutine != null)
				{
					StopCoroutine(changeTicketCoRoutine);
				}
				TicketSlots.Clear();
			}
		}

		internal void SetFillOrderButtonState(OrderBoardButtonState state, int rushCost = -1)
		{
			FillOrderButton.SetFillOrderButtonState(state, rushCost);
			if (adVideoButton)
			{
				bool active = false;
				if (state == OrderBoardButtonState.Rush)
				{
					active = true;
				}
				AdVideoButton.gameObject.SetActive(active);
			}
		}

		public new void Close(bool IsInstant = false)
		{
			isClosing = true;
			DestoryTickets(false);
			base.Close(IsInstant);
		}

		private void CreateTicket(int index, ModalSettings modalSettings, Vector3 position, IGUIService guiService, ILocalizationService localService)
		{
			GameObject gameObject = guiService.Execute(GUIOperation.LoadUntrackedInstance, "cmp_TicketPrefab");
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.position = position;
			OrderBoardTicketView component = gameObject.GetComponent<OrderBoardTicketView>();
			component.Index = index;
			component.Init(localService);
			if (modalSettings.enableTicketThrob)
			{
				component.HighlightTicket(true);
			}
			TicketSlots.Add(component);
			rectTransform.parent = base.transform;
		}

		internal void SetupDeleteOrderButton(bool active)
		{
			Button component = DeleteButton.GetComponent<Button>();
			component.interactable = enabledDeleteButton && active;
		}

		internal void AddTicket(OrderBoardTicket ticket, bool isInProgress, int duration, string locText, IPrestigeService prestigeService, bool isInit, GetBuffStateSignal getBuffStateSignal, OrderBoardTicketClickedSignal ticketClicked, IPlayerService playerService)
		{
			if (!isClosing)
			{
				OrderBoardTicketView orderBoardTicketView = TicketSlots[ticket.BoardIndex];
				orderBoardTicketView.gameObject.SetActive(true);
				if (isInit)
				{
					SetTicketInfo(orderBoardTicketView, ticket, isInProgress, duration, locText, prestigeService, getBuffStateSignal, null, playerService);
					return;
				}
				changeTicketCoRoutine = ChangeTicket(orderBoardTicketView, ticket, isInProgress, duration, locText, prestigeService, getBuffStateSignal, ticketClicked, playerService);
				StartCoroutine(changeTicketCoRoutine);
			}
		}

		private IEnumerator ChangeTicket(OrderBoardTicketView view, OrderBoardTicket ticket, bool isInProgress, int duration, string locText, IPrestigeService prestigeService, GetBuffStateSignal getBuffStateSignal, OrderBoardTicketClickedSignal ticketClicked, IPlayerService playerService)
		{
			view.SetRootAnimation(false);
			yield return new WaitForSeconds(ticketRepopTime);
			SetTicketInfo(view, ticket, isInProgress, duration, locText, prestigeService, getBuffStateSignal, ticketClicked, playerService);
			view.SetRootAnimation(true);
			changeTicketCoRoutine = null;
		}

		private void SetTicketInfo(OrderBoardTicketView view, OrderBoardTicket ticket, bool isInProgress, int duration, string locText, IPrestigeService prestigeService, GetBuffStateSignal getBuffStateSignal, OrderBoardTicketClickedSignal ticketClicked, IPlayerService playerService)
		{
			view.Title = locText;
			view.getBuffStateSignal = getBuffStateSignal;
			view.SetTicketInstance(ticket);
			view.NormalPanel.SetActive(ticket.CharacterDefinitionId == 0);
			view.PrestigePanel.SetActive(ticket.CharacterDefinitionId != 0);
			if (playerService.IsMinionPartyUnlocked())
			{
				view.FunPointIcon.SetActive(true);
				view.XpIcon.SetActive(false);
			}
			else
			{
				view.FunPointIcon.SetActive(false);
				view.XpIcon.SetActive(true);
			}
			if (isInProgress)
			{
				view.StartTimer(ticket.BoardIndex, duration);
			}
			else
			{
				view.SetTicketState(true);
				if (ticket.CharacterDefinitionId != 0)
				{
					int characterDefinitionId = ticket.CharacterDefinitionId;
					Sprite characterImage;
					Sprite characterMask;
					prestigeService.GetCharacterImageBasedOnMood(characterDefinitionId, CharacterImageType.SmallAvatarIcon, out characterImage, out characterMask);
					Prestige prestige = prestigeService.GetPrestige(characterDefinitionId);
					view.SetCharacterImage(characterImage, characterMask, prestige.CurrentPrestigeLevel == 0);
				}
			}
			if (ticketClicked != null)
			{
				ticketClicked.Dispatch(ticket, locText, false);
			}
		}

		internal void SetTicketClicks(bool enabled)
		{
			foreach (OrderBoardTicketView ticketSlot in TicketSlots)
			{
				ticketSlot.SetTicketClick(enabled);
			}
		}

		internal OrderBoardTicketView GetFirstClickableTicketIndex()
		{
			for (int i = 0; i < TicketSlots.Count; i++)
			{
				if (TicketSlots[i].gameObject.activeSelf && !TicketSlots[i].IsCounting())
				{
					return TicketSlots[i];
				}
			}
			return TicketSlots[0];
		}

		internal void SetDeleteButtonEnabled(bool isEnabled)
		{
			enabledDeleteButton = isEnabled;
			DeleteButton.GetComponent<Button>().interactable = isEnabled;
		}

		internal void ResetDoubleTap(int viewId)
		{
			foreach (OrderBoardTicketView ticketSlot in TicketSlots)
			{
				if (ticketSlot.Index != viewId)
				{
					ticketSlot.TicketMeter.RushButton.ResetTapState();
					ticketSlot.TicketMeter.RushButton.ResetAnim();
				}
			}
		}
	}
}
