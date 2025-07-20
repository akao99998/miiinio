using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SocialPartyInviteAlertMediator : UIStackMediator<SocialPartyInviteAlertView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyInviteAlertMediator") as IKampaiLogger;

		private int pageNumber;

		private IList<SocialEventInvitation> friendInvites;

		public Signal<SocialTeamResponse, ErrorResponse> acceptSignal = new Signal<SocialTeamResponse, ErrorResponse>();

		public Signal<SocialTeamResponse, ErrorResponse> declineSignal = new Signal<SocialTeamResponse, ErrorResponse>();

		private SocialTeam cachedTeam;

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public ShowSocialPartyFillOrderSignal showPartyFillOrderSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ShowSocialPartyStartSignal showSocialPartyStartSignal { get; set; }

		[Inject]
		public ShowSocialPartyRewardSignal socialPartyRewardSignal { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			acceptSignal.AddListener(AcceptResponse);
			declineSignal.AddListener(DeclineResponse);
			base.view.acceptButton.ClickedSignal.AddListener(AcceptButton);
			base.view.declineButton.ClickedSignal.AddListener(DeclineButton);
			base.view.nextButton.ClickedSignal.AddListener(NextButton);
			base.view.previousButton.ClickedSignal.AddListener(PreviousButton);
			base.view.closeButton.ClickedSignal.AddListener(CloseButton);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
			base.view.acceptButtonText.text = localService.GetString("AcceptButtonText");
			base.view.declineButtonText.text = localService.GetString("DeclineButtonText");
			base.view.title.text = localService.GetString("socialpartyinvitealerttitle");
		}

		public override void OnRemove()
		{
			base.OnRemove();
			friendInvites = null;
			acceptSignal.RemoveListener(AcceptResponse);
			declineSignal.RemoveListener(DeclineResponse);
			base.view.acceptButton.ClickedSignal.RemoveListener(AcceptButton);
			base.view.declineButton.ClickedSignal.RemoveListener(DeclineButton);
			base.view.nextButton.ClickedSignal.RemoveListener(NextButton);
			base.view.previousButton.ClickedSignal.RemoveListener(PreviousButton);
			base.view.closeButton.ClickedSignal.RemoveListener(CloseButton);
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
		}

		public override void Initialize(GUIArguments args)
		{
			pageNumber = 1;
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID);
			cachedTeam = socialEventStateCached.Team;
			friendInvites = socialEventStateCached.UserEvent.Invitations;
			base.view.previousButton.gameObject.SetActive(false);
			base.view.nextButton.gameObject.SetActive((friendInvites.Count > 1) ? true : false);
			UpdateScreenValues();
		}

		private bool IsPlayerAlreadyOnTeam(int num)
		{
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID);
			return socialEventStateCached.Team != null && socialEventStateCached.Team.Members != null && socialEventStateCached.Team.Members.Count > num;
		}

		private void EnableButtons(bool isEnabled)
		{
			Button component = base.view.acceptButton.GetComponent<Button>();
			component.interactable = isEnabled;
			component = base.view.declineButton.GetComponent<Button>();
			component.interactable = isEnabled;
			component = base.view.nextButton.GetComponent<Button>();
			component.interactable = isEnabled;
			component = base.view.previousButton.GetComponent<Button>();
			component.interactable = isEnabled;
			component = base.view.closeButton.GetComponent<Button>();
			component.interactable = isEnabled;
		}

		public void AcceptButton()
		{
			EnableButtons(false);
			if (IsPlayerAlreadyOnTeam(1))
			{
				Signal<bool> signal = new Signal<bool>();
				signal.AddOnce(ConfirmationResponse);
				PopupConfirmationSetting type = new PopupConfirmationSetting("socialpartyjointeamconfirmationtitle", "socialpartyjointeamconfirmationdescription", false, "img_char_Min_FeedbackChecklist01", signal, string.Empty, string.Empty);
				gameContext.injectionBinder.GetInstance<DisplayConfirmationSignal>().Dispatch(type);
			}
			else
			{
				AcceptTeam();
			}
		}

		public void AcceptResponse(SocialTeamResponse response, ErrorResponse error)
		{
			EnableButtons(true);
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			if (cachedTeam != null && cachedTeam.Members != null && cachedTeam.Members.Count == 1)
			{
				SocialTeam team = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID).Team;
				foreach (SocialOrderProgress item in cachedTeam.OrderProgress)
				{
					bool flag = false;
					foreach (SocialOrderProgress item2 in team.OrderProgress)
					{
						if (item.OrderId == item2.OrderId)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
						signal.AddListener(FillOrderComplete);
						timedSocialEventService.FillOrder(team.SocialEventId, team.ID, item.OrderId, signal);
					}
				}
			}
			CloseMenu();
		}

		public void FillOrderComplete(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
			}
			else if (!response.UserEvent.RewardClaimed && response.Team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
			{
				socialPartyRewardSignal.Dispatch(timedSocialEventService.GetCurrentSocialEvent().ID);
			}
		}

		public void DeclineButton()
		{
			EnableButtons(false);
			SocialEventInvitation currentFriendInvite = GetCurrentFriendInvite();
			timedSocialEventService.RejectInvitation(currentFriendInvite.EventID, currentFriendInvite.Team.TeamID, declineSignal);
		}

		public void DeclineResponse(SocialTeamResponse response, ErrorResponse error)
		{
			EnableButtons(true);
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			if (response != null && response.UserEvent != null)
			{
				friendInvites = response.UserEvent.Invitations;
			}
			else
			{
				friendInvites = null;
			}
			if (friendInvites == null || friendInvites.Count == 0)
			{
				CloseMenu();
				return;
			}
			pageNumber--;
			NextButton();
		}

		public void ConfirmationResponse(bool bAccept)
		{
			if (bAccept)
			{
				AcceptTeam();
			}
			else
			{
				EnableButtons(true);
			}
		}

		private void AcceptTeam()
		{
			SocialEventInvitation currentFriendInvite = GetCurrentFriendInvite();
			timedSocialEventService.JoinSocialTeam(currentFriendInvite.EventID, currentFriendInvite.Team.TeamID, acceptSignal);
		}

		public void CloseButton()
		{
			EnableButtons(true);
			CloseMenu();
		}

		public void NextButton()
		{
			pageNumber++;
			base.view.previousButton.gameObject.SetActive(true);
			if (pageNumber >= friendInvites.Count)
			{
				pageNumber = friendInvites.Count;
				base.view.nextButton.gameObject.SetActive(false);
			}
			UpdateScreenValues();
		}

		public void PreviousButton()
		{
			pageNumber--;
			base.view.nextButton.gameObject.SetActive(true);
			if (pageNumber <= 1)
			{
				pageNumber = 1;
				base.view.previousButton.gameObject.SetActive(false);
			}
			UpdateScreenValues();
		}

		private void UpdateScreenValues()
		{
			SocialEventInvitation currentFriendInvite = GetCurrentFriendInvite();
			FacebookService facebookService = this.facebookService as FacebookService;
			UserIdentity inviter = currentFriendInvite.inviter;
			base.view.playerImage.sprite = null;
			if (inviter != null)
			{
				Signal<string> signal = new Signal<string>();
				signal.AddListener(OnFacebookPictureComplete);
				StartCoroutine(facebookService.DownloadUserPicture(inviter.ExternalID, signal));
			}
			else
			{
				base.view.playerImage.sprite = UIUtils.LoadSpriteFromPath("icn_questGiver_minPhil_fill");
			}
			string empty = string.Empty;
			if (facebookService.friends != null)
			{
				FBUser friend = facebookService.GetFriend(inviter.ExternalID);
				if (friend != null)
				{
					empty = friend.name;
				}
				else
				{
					logger.Error("fbService.friends[invitation.inviter.ExternalID] is null");
				}
			}
			base.view.playerName.text = empty;
			base.view.socialPartyDescription.text = empty + localService.GetString("socialpartyinvitealertdescription");
			base.view.recipesFilledDescription.text = currentFriendInvite.Team.CompletedOrdersCount + "/" + timedSocialEventService.GetCurrentSocialEvent().Orders.Count + localService.GetString("socialpartyinviterecipesfilled");
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		private void CloseMenu()
		{
			base.view.Close();
		}

		public void CloseAnimationComplete()
		{
			hideSignal.Dispatch("Social Invite");
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_InviteAlert");
			if (IsPlayerAlreadyOnTeam(0))
			{
				showPartyFillOrderSignal.Dispatch(0);
			}
			else
			{
				showSocialPartyStartSignal.Dispatch();
			}
		}

		protected override void Close()
		{
			CloseButton();
		}

		private SocialEventInvitation GetCurrentFriendInvite()
		{
			return friendInvites[pageNumber - 1];
		}

		private void OnFacebookPictureComplete(string id)
		{
			SocialEventInvitation currentFriendInvite = GetCurrentFriendInvite();
			FacebookService facebookService = this.facebookService as FacebookService;
			Texture userPicture = facebookService.GetUserPicture(id);
			if (userPicture != null && currentFriendInvite.inviter.ExternalID == id)
			{
				Sprite sprite = Sprite.Create(userPicture as Texture2D, new Rect(0f, 0f, userPicture.width, userPicture.height), new Vector2(0f, 0f));
				base.view.playerImage.sprite = sprite;
			}
		}
	}
}
