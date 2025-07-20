using System;
using System.Collections;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class LinkAccountCommand : Command
	{
		public const string ACCOUNT_LINK_ENDPOINT = "/rest/v2/user/{0}/identity";

		public IKampaiLogger logger = LogManager.GetClassLogger("LinkAccountCommand") as IKampaiLogger;

		[Inject]
		public bool restartOnSuccess { get; set; }

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public SocialLogoutSignal socialLogout { get; set; }

		[Inject]
		public ReLinkAccountSignal relinkSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public GooglePlayServerAuthCodeReceivedSignal googlePlayServerAuthCodeReceivedSignal { get; set; }

		public override void Execute()
		{
			GooglePlayService googlePlayService = socialService as GooglePlayService;
			if (googlePlayService != null && googlePlayService.isLoggedIn && googlePlayService.ServerAuthCode == null)
			{
				googlePlayServerAuthCodeReceivedSignal.AddOnce(OnGooglePlayServerAuthCodeReceived);
				googlePlayService.RequestServerAuthCode();
			}
			else
			{
				LinkAccount();
			}
		}

		private void OnGooglePlayServerAuthCodeReceived(bool success, ISocialService socialService)
		{
			LinkAccount();
		}

		private void LinkAccount()
		{
			UserSession userSession = userSessionService.UserSession;
			string userID = userSession.UserID;
			AccountLinkRequest accountLinkRequest = new AccountLinkRequest();
			if (socialService.isLoggedIn)
			{
				accountLinkRequest.credentials = socialService.accessToken;
				accountLinkRequest.externalId = socialService.userID;
				accountLinkRequest.identityType = socialService.type.ToString().ToLower();
			}
			UserIdentity userIdentity = new UserIdentity();
			userIdentity.ExternalID = socialService.userID;
			userIdentity.Type = (IdentityType)(int)Enum.Parse(typeof(IdentityType), socialService.type.ToString().ToLower());
			userSession.SocialIdentities.Add(userIdentity);
			logger.Debug("attempting to link type {0} for ID {1} ", accountLinkRequest.identityType, accountLinkRequest.externalId);
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(OnAccountLinkResponse);
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/v2/user/{0}/identity", userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(accountLinkRequest)
				.WithResponseSignal(signal));
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}

		private void OnAccountLinkResponse(IResponse response)
		{
			string body = response.Body;
			AccountLinkErrorResponse error = null;
			try
			{
				error = JsonConvert.DeserializeObject<AccountLinkErrorResponse>(body);
			}
			catch (Exception e)
			{
				HandleJsonException(e);
			}
			GooglePlayService googlePlayService = socialService as GooglePlayService;
			if (googlePlayService != null)
			{
				googlePlayService.ResetServerAuthCode();
			}
			if (response.Success)
			{
				UserIdentity item = JsonConvert.DeserializeObject<UserIdentity>(body);
				userSessionService.UserSession.SocialIdentities.Add(item);
				if (restartOnSuccess)
				{
					reloadGameSignal.Dispatch();
				}
			}
			else if (error != null && error.error.responseCode == 409)
			{
				logger.Error("Social Account is already linked to an account");
				RemoveSocialIdentity();
				routineRunner.StartCoroutine(WaitAFrame(delegate
				{
					Signal<bool> signal = new Signal<bool>();
					signal.AddListener(delegate(bool result)
					{
						PopUpCallback(result, error.error.details.conflictUserId);
					});
					string @string = localService.GetString("AccountConflictBody", localService.GetString(socialService.locKey));
					PopupConfirmationSetting type = new PopupConfirmationSetting("AccountConflictTitle", @string, true, "img_char_Min_FeedbackChecklist01", signal, "AccountConflictKeep", "AccountConflictRestore");
					gameContext.injectionBinder.GetInstance<DisplayConfirmationSignal>().Dispatch(type);
				}));
				logger.Debug(body ?? "json is null");
			}
			else
			{
				RemoveSocialIdentity();
				socialLogout.Dispatch(socialService);
				logger.Error("Error Linking Social Account");
				logger.Debug(body ?? "json is null");
			}
		}

		private void HandleJsonException(Exception e)
		{
			logger.Info("OnAccountLinkResponse exception: {0}", e.Message);
		}

		private void PopUpCallback(bool result, string conflictId)
		{
			if (result)
			{
				relinkSignal.Dispatch(socialService, conflictId, false);
			}
			else
			{
				relinkSignal.Dispatch(socialService, conflictId, true);
			}
		}

		private void RemoveSocialIdentity()
		{
			IList<UserIdentity> socialIdentities = userSessionService.UserSession.SocialIdentities;
			for (int i = 0; i < socialIdentities.Count; i++)
			{
				if (socialIdentities[i].ExternalID == socialService.userID)
				{
					socialIdentities.RemoveAt(i);
					break;
				}
			}
		}
	}
}
