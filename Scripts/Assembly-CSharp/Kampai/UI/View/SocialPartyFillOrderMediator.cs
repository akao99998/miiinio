using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SocialPartyFillOrderMediator : UIStackMediator<SocialPartyFillOrderView>
	{
		private const int MAX_FRIENDS = 4;

		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyFillOrderMediator") as IKampaiLogger;

		private int autoFillOrder;

		private IList<FBUser> friends;

		private SocialTeam team;

		private DateTime finishTime;

		private bool bShowFinishMenu;

		private int count;

		private int picturesComplete;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ShowSocialPartyFillOrderButtonSignal showPartyFillOrderButton { get; set; }

		[Inject]
		public ShowSocialPartyFillOrderProfileButtonSignal showPartyFillOrderProfile { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ShowSocialPartyStartSignal showSocialPartyStartSignal { get; set; }

		[Inject]
		public ShowSocialPartyInviteAlertSignal showSocialPartyInviteAlertSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ShowSocialPartyEventEndSignal showSocialPartyEventEndSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public SocialPartyFillOrderSetupUISignal socialPartyFillOrderSetupUISignal { get; set; }

		[Inject]
		public SocialPartyFillOrderButtonMediatorUpdateSignal socialPartyFillOrderButtonMediatorUpdateSignal { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public HideHUDAndIconsSignal hideHUDSignal { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDevicePrefsService notifPrefs { get; set; }

		public override void OnRemove()
		{
			base.OnRemove();
			FacebookService facebookService = this.facebookService as FacebookService;
			if (facebookService.userPictures != null)
			{
				facebookService.userPictures.Clear();
			}
			base.view.leaveTeamButton.ClickedSignal.RemoveListener(LeaveTeamButton);
			base.view.messageAlertButton.ClickedSignal.RemoveListener(MessageAlertButton);
			base.view.teamPanelButton.ClickedSignal.RemoveListener(OpenTeam);
			base.view.closeTeamButton.ClickedSignal.RemoveListener(CloseTeam);
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
			socialPartyFillOrderSetupUISignal.RemoveListener(RefreshUI);
			loginSuccess.RemoveListener(OnLoginSuccess);
			hideHUDSignal.Dispatch(true);
		}

		public override void Initialize(GUIArguments args)
		{
			GUIAutoAction<int> gUIAutoAction = args.Get<GUIAutoAction<int>>();
			if (gUIAutoAction != null)
			{
				autoFillOrder = gUIAutoAction.value;
			}
			bShowFinishMenu = false;
			count = 0;
			base.view.Init();
			base.view.leaveTeamButton.ClickedSignal.AddListener(LeaveTeamButton);
			base.view.messageAlertButton.ClickedSignal.AddListener(MessageAlertButton);
			base.view.teamPanelButton.ClickedSignal.AddListener(OpenTeam);
			base.view.closeTeamButton.ClickedSignal.AddListener(CloseTeam);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
			socialPartyFillOrderSetupUISignal.AddListener(RefreshUI);
			loginSuccess.AddListener(OnLoginSuccess);
			base.view.messageAlertButton.gameObject.SetActive(false);
			LoadTeam();
			hideHUDSignal.Dispatch(false);
		}

		private void OnLoginSuccess(ISocialService socialService)
		{
			if (socialService.type == SocialServices.FACEBOOK)
			{
				LoadTeam();
			}
		}

		private void LoadTeam()
		{
			TimedSocialEventDefinition currentSocialEvent = timedSocialEventService.GetCurrentSocialEvent();
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(currentSocialEvent.ID);
			if (socialEventStateCached != null && socialEventStateCached.Team != null)
			{
				OnGetTeamSuccess(socialEventStateCached, null);
				return;
			}
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(OnGetTeamSuccess);
			timedSocialEventService.GetSocialEventState(timedSocialEventService.GetCurrentSocialEvent().ID, signal);
		}

		private void OpenTeam()
		{
			base.view.OpenTeam();
		}

		private void CloseTeam()
		{
			base.view.CloseTeam();
		}

		public void OnGetTeamSuccess(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
			}
			else if (response != null && response.Team != null)
			{
				team = response.Team;
				if (response.Team != null && response.Team.Members != null && response.Team.Members.Count > 1)
				{
					base.view.leaveTeamButton.gameObject.SetActive(true);
				}
				else
				{
					base.view.leaveTeamButton.gameObject.SetActive(false);
				}
				if (response.UserEvent.Invitations != null && response.UserEvent.Invitations.Count > 0)
				{
					handleInviteButton(response.UserEvent.Invitations);
				}
				else
				{
					base.view.messageAlertButton.gameObject.SetActive(false);
				}
				GetFBFriendsInTeam();
				long num = timedSocialEventService.GetCurrentSocialEvent().FinishTime;
				finishTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				finishTime = finishTime.AddSeconds(num).ToLocalTime();
			}
		}

		private void handleInviteButton(IList<SocialEventInvitation> invitations)
		{
			bool flag = false;
			foreach (SocialEventInvitation invitation in invitations)
			{
				if (!playerService.SeenSocialInvitation(invitation.Team.TeamID))
				{
					flag = true;
					playerService.AddSocialInvitationSeen(invitation.Team.TeamID);
				}
			}
			base.view.messageAlertButton.gameObject.SetActive(true);
			Animator component = base.view.messageAlertButton.gameObject.GetComponent<Animator>();
			component.SetBool("Normal", true);
			if (flag)
			{
				if (component != null)
				{
					component.SetBool("Pulse", true);
				}
			}
			else if (component != null)
			{
				component.SetBool("Pulse", false);
			}
		}

		public void GetFBFriendsInTeam()
		{
			FacebookService facebookService = this.facebookService as FacebookService;
			if (!facebookService.isLoggedIn)
			{
				SetupUI();
				return;
			}
			friends = new List<FBUser>();
			picturesComplete = 0;
			object obj;
			if (team != null)
			{
				IList<UserIdentity> members = team.Members;
				obj = members;
			}
			else
			{
				obj = null;
			}
			IList<UserIdentity> list = (IList<UserIdentity>)obj;
			if (list == null)
			{
				return;
			}
			int num = Mathf.Min(list.Count, 4);
			foreach (UserIdentity item in list)
			{
				if (item != null && item.ExternalID != null && item.Type == IdentityType.facebook)
				{
					friends.Add(facebookService.GetFriend(item.ExternalID) ?? new FBUser(string.Empty, item.ExternalID));
					Signal<string> signal = new Signal<string>();
					signal.AddListener(OnFacebookPictureComplete);
					StartCoroutine(facebookService.DownloadUserPicture(item.ExternalID, signal));
					if (friends.Count == num)
					{
						break;
					}
				}
			}
		}

		private void OnFacebookPictureComplete(string id)
		{
			FacebookService facebookService = this.facebookService as FacebookService;
			if (facebookService.GetUserPicture(id) == null)
			{
				logger.Warning("OnFacebookPictureComplete texture is null for Facebook ID: {0}", id);
			}
			picturesComplete++;
			if (picturesComplete == friends.Count)
			{
				SetupUI();
			}
		}

		private SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData GetData(SocialEventOrderDefinition orderDefinition, int index)
		{
			SocialOrderProgress progress = (team.OrderProgress as List<SocialOrderProgress>).Find((SocialOrderProgress p) => p.OrderId == orderDefinition.OrderID);
			SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData socialPartyFillOrderButtonMediatorData = new SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData();
			socialPartyFillOrderButtonMediatorData.team = team;
			socialPartyFillOrderButtonMediatorData.progress = progress;
			socialPartyFillOrderButtonMediatorData.orderDefintion = orderDefinition;
			socialPartyFillOrderButtonMediatorData.parent = base.view.SocialFillOrderButtonContainer;
			socialPartyFillOrderButtonMediatorData.index = index;
			socialPartyFillOrderButtonMediatorData.fillOrderSignal = base.view.FillOrderSignal;
			socialPartyFillOrderButtonMediatorData.autoFill = orderDefinition.OrderID == autoFillOrder;
			return socialPartyFillOrderButtonMediatorData;
		}

		public void RefreshUI(SocialTeam socialTeam)
		{
			int num = 0;
			if (socialTeam != null)
			{
				team = socialTeam;
			}
			if (team == null)
			{
				return;
			}
			foreach (SocialEventOrderDefinition order in team.Definition.Orders)
			{
				SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData data = GetData(order, num);
				socialPartyFillOrderButtonMediatorUpdateSignal.Dispatch(data);
				num++;
			}
			UpdateProgress();
		}

		public void SetupUI()
		{
			if (team != null)
			{
				int num = 0;
				foreach (SocialEventOrderDefinition order in team.Definition.Orders)
				{
					SocialPartyFillOrderButtonMediator.SocialPartyFillOrderButtonMediatorData data = GetData(order, num);
					showPartyFillOrderButton.Dispatch(data, num);
					num++;
				}
				if (!coppaService.Restricted())
				{
					int num2 = team.Members.Count;
					int num3 = 0;
					foreach (UserIdentity member in team.Members)
					{
						SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData socialPartyFillOrderProfileButtonMediatorData = new SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData();
						socialPartyFillOrderProfileButtonMediatorData.identity = member;
						socialPartyFillOrderProfileButtonMediatorData.parent = base.view.SocialFillTeamPanel;
						socialPartyFillOrderProfileButtonMediatorData.index = num3;
						showPartyFillOrderProfile.Dispatch(socialPartyFillOrderProfileButtonMediatorData);
						num3++;
					}
					for (int i = num2; i < 4; i++)
					{
						SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData socialPartyFillOrderProfileButtonMediatorData2 = new SocialPartyFillOrderProfileButtonMediator.SocialPartyFillOrderProfileButtonMediatorData();
						socialPartyFillOrderProfileButtonMediatorData2.identity = null;
						socialPartyFillOrderProfileButtonMediatorData2.parent = base.view.SocialFillTeamPanel;
						socialPartyFillOrderProfileButtonMediatorData2.index = num3;
						showPartyFillOrderProfile.Dispatch(socialPartyFillOrderProfileButtonMediatorData2);
						num3++;
					}
				}
				UpdateTime();
				UpdateProgress();
			}
			setupText();
		}

		private void UpdateProgress()
		{
			int num = team.Definition.Orders.Count;
			int num2 = team.OrderProgress.Count;
			base.view.progressBar.transform.localScale = new Vector3((float)num2 / (float)num, 1f, 1f);
			if (num == num2)
			{
				base.view.ordersRemaining.text = localService.GetString("socialpartyfillordercompleted");
				gameContext.injectionBinder.GetInstance<CancelNotificationSignal>().Dispatch(NotificationType.SocialEventComplete.ToString());
				return;
			}
			base.view.ordersRemaining.text = string.Format("{0} / {1}", num2, num);
			if (notifPrefs.GetDevicePrefs().SocialEventNotif && num2 > 0)
			{
				gameContext.injectionBinder.GetInstance<SocialEventNotificationsSignal>().Dispatch();
			}
		}

		private void setupText()
		{
			base.view.leaveTeamButtonText.text = localService.GetString("socialpartyfillorderleavebutton");
			base.view.teamTitle.text = localService.GetString("socialpartyfillorderteamtitle");
			base.view.teamOrderBoardText.text = localService.GetString("socialpartyfillorderboard");
			base.view.descriptionText.text = localService.GetString("socialpartyfillorderdescription");
			base.view.questTitle.text = localService.GetString("Rewards");
			base.view.teamButtonText.text = localService.GetString("socialpartyfillorderteamtitle");
			TimedSocialEventDefinition currentSocialEvent = timedSocialEventService.GetCurrentSocialEvent();
			TransactionDefinition reward = currentSocialEvent.GetReward(definitionService);
			base.view.grindRewardText.text = "0";
			base.view.premiumRewardText.text = "0";
			foreach (QuantityItem output in reward.Outputs)
			{
				if (output.ID == 0)
				{
					base.view.grindRewardText.text = UIUtils.FormatLargeNumber((int)output.Quantity);
				}
				else if (output.ID == 1)
				{
					base.view.premiumRewardText.text = UIUtils.FormatLargeNumber((int)output.Quantity);
				}
			}
		}

		public void UpdateTime()
		{
			count++;
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeService.CurrentTime()).ToLocalTime();
			TimeSpan timeSpan = finishTime.Subtract(dateTime);
			if (dateTime < finishTime)
			{
				base.view.timeRemaining.text = UIUtils.FormatTime(timeSpan.TotalSeconds, localService);
				return;
			}
			if (base.view.timeRemaining != null)
			{
				base.view.timeRemaining.text = localService.GetString("socialpartyfillordereventfinished");
			}
			if (!bShowFinishMenu && count > 10)
			{
				bShowFinishMenu = true;
				UnloadFillOrderScreen();
				showSocialPartyEventEndSignal.Dispatch();
			}
		}

		private void UnloadFillOrderScreen()
		{
			hideSignal.Dispatch("SocialSkrim");
			guiService.Execute(GUIOperation.Unload, "SocialPartyFillOrderScreen");
			displaySignal.Dispatch(19000014, false, new Signal<bool>());
		}

		protected override void Update()
		{
			UpdateTime();
		}

		public void LeaveTeamResponse(bool bLeave)
		{
			if (bLeave)
			{
				Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
				signal.AddListener(LeaveSocialTeamServerResponse);
				timedSocialEventService.LeaveSocialTeam(team.SocialEventId, team.ID, signal);
			}
		}

		public void LeaveSocialTeamServerResponse(SocialTeamResponse newTeam, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			UnloadFillOrderScreen();
			if (newTeam.Team != null)
			{
				team = newTeam.Team;
				SetupUI();
				return;
			}
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID);
			if (socialEventStateCached.UserEvent != null && socialEventStateCached.UserEvent.Invitations != null && socialEventStateCached.UserEvent.Invitations.Count > 0 && facebookService.isLoggedIn)
			{
				showSocialPartyInviteAlertSignal.Dispatch();
			}
			else
			{
				showSocialPartyStartSignal.Dispatch();
			}
		}

		public void MessageAlertButton()
		{
			globalSFX.Dispatch("Play_button_click_01");
			UnloadFillOrderScreen();
			showSocialPartyInviteAlertSignal.Dispatch();
		}

		public void LeaveTeamButton()
		{
			Signal<bool> signal = new Signal<bool>();
			signal.AddListener(LeaveTeamResponse);
			PopupConfirmationSetting type = new PopupConfirmationSetting("socialpartyleaveteamconfirmationtitle", "socialpartyleaveteamconfirmationdescription", false, "img_char_Min_FeedbackChecklist01", signal, string.Empty, string.Empty);
			gameContext.injectionBinder.GetInstance<DisplayConfirmationSignal>().Dispatch(type);
		}

		public void CloseAnimationComplete()
		{
			UnloadFillOrderScreen();
		}

		public void QuitButton()
		{
			globalSFX.Dispatch("Play_button_click_01");
			base.view.Close();
		}

		protected override void Close()
		{
			QuitButton();
		}
	}
}
