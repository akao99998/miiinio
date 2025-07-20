using System.Collections.Generic;
using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class OpenStageBuildingCommand : Command
	{
		[Inject]
		public StageBuilding building { get; set; }

		[Inject]
		public BuildingChangeStateSignal stateChangeSignal { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ShowSocialPartyRewardSignal showRewardSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ILocalizationService localeService { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void Execute()
		{
			stateChangeSignal.Dispatch(building.ID, BuildingState.Working);
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId != null)
			{
				Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(firstInstanceByDefinitionId);
				if ((prestigeFromMinionInstance.state == PrestigeState.Taskable && prestigeFromMinionInstance.CurrentPrestigeLevel == 1) || prestigeFromMinionInstance.CurrentPrestigeLevel > 1)
				{
					OpenGUI();
					return;
				}
			}
			string aspirationalMessage = building.Definition.AspirationalMessage;
			uiContext.injectionBinder.GetInstance<PopupMessageSignal>().Dispatch(localeService.GetString(aspirationalMessage), PopupMessageType.NORMAL);
		}

		private void OpenGUI()
		{
			if (timedSocialEventService.GetCurrentSocialEvent() != null)
			{
				Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
				signal.AddListener(OnGetSocialEventStateResponse);
				timedSocialEventService.GetSocialEventState(timedSocialEventService.GetCurrentSocialEvent().ID, signal);
				return;
			}
			IList<int> pastEventsWithUnclaimedReward = timedSocialEventService.GetPastEventsWithUnclaimedReward();
			if (pastEventsWithUnclaimedReward.Count > 0)
			{
				showRewardSignal.Dispatch(pastEventsWithUnclaimedReward[0]);
			}
			else
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyNoEventSignal>().Dispatch();
			}
		}

		public void OnGetSocialEventStateResponse(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			SocialTeamUserEvent userEvent = response.UserEvent;
			SocialTeam team = response.Team;
			IList<int> pastEventsWithUnclaimedReward = timedSocialEventService.GetPastEventsWithUnclaimedReward();
			if (pastEventsWithUnclaimedReward.Count > 0)
			{
				showRewardSignal.Dispatch(pastEventsWithUnclaimedReward[0]);
			}
			else if (userEvent != null && !userEvent.RewardClaimed && team != null && team.OrderProgress != null && team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
			{
				showRewardSignal.Dispatch(timedSocialEventService.GetCurrentSocialEvent().ID);
			}
			else if (team != null)
			{
				if (userEvent != null && userEvent.RewardClaimed && team.OrderProgress != null && team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
				{
					uiContext.injectionBinder.GetInstance<ShowSocialPartyEventCompletedSignal>().Dispatch();
				}
				else
				{
					uiContext.injectionBinder.GetInstance<ShowSocialPartyFillOrderSignal>().Dispatch(0);
				}
			}
			else
			{
				CheckForFriendRequests(userEvent);
			}
		}

		private void CheckForFriendRequests(SocialTeamUserEvent userEvent)
		{
			if (facebookService.isLoggedIn && userEvent != null && userEvent.Invitations != null && userEvent.Invitations.Count > 0)
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyInviteAlertSignal>().Dispatch();
			}
			else
			{
				SocialEventStart();
			}
		}

		private void SocialEventStart()
		{
			model.StageUIOpen = true;
			uiContext.injectionBinder.GetInstance<ShowSocialPartyStartSignal>().Dispatch();
		}
	}
}
