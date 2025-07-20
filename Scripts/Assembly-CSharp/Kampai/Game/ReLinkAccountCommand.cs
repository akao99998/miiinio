using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class ReLinkAccountCommand : Command
	{
		public const string ACCOUNT_LINK_ENDPOINT = "/rest/v2/user/{0}/identity/{1}";

		public const string ACCOUNT_REVERSE_LINK_ENDPOINT = "/rest/v2/user/{0}/identity/{1}/reverseLink";

		public IKampaiLogger logger = LogManager.GetClassLogger("ReLinkAccountCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public string toUserId { get; set; }

		[Inject]
		public bool reverseLink { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ILocalPersistanceService LocalPersistService { get; set; }

		[Inject]
		public IEncryptionService encryptionService { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSiganl { get; set; }

		[Inject]
		public ITimedSocialEventService socialEventService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public SocialLogoutSignal socialLogout { get; set; }

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
				RelinkAccount();
			}
		}

		private void OnGooglePlayServerAuthCodeReceived(bool success, ISocialService socialService)
		{
			RelinkAccount();
		}

		private void RelinkAccount()
		{
			UserSession userSession = userSessionService.UserSession;
			string arg = WWW.EscapeURL(userSession.UserID);
			string plainText = LocalPersistService.GetData("AnonymousID");
			encryptionService.TryDecrypt(plainText, "Kampai!", out plainText);
			string arg2 = WWW.EscapeURL(plainText);
			AccountReLinkRequest accountReLinkRequest = new AccountReLinkRequest();
			if (socialService.isLoggedIn)
			{
				accountReLinkRequest.credentials = socialService.accessToken;
				accountReLinkRequest.externalId = socialService.userID;
				accountReLinkRequest.identityType = socialService.type.ToString().ToLower();
			}
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(OnAccountLinkResponse);
			accountReLinkRequest.toUserId = toUserId;
			string format = "/rest/v2/user/{0}/identity/{1}";
			if (reverseLink)
			{
				format = "/rest/v2/user/{0}/identity/{1}/reverseLink";
			}
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format(format, arg, arg2)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(accountReLinkRequest)
				.WithResponseSignal(signal));
		}

		private void OnAccountLinkResponse(IResponse response)
		{
			string body = response.Body;
			int code = response.Code;
			if (response.Success)
			{
				logger.Debug("Relink Success: {0}", body);
				UserIdentity userIdentity = JsonConvert.DeserializeObject<UserIdentity>(body);
				if (!reverseLink)
				{
					LocalPersistService.PutDataInt("RelinkingAccount", 1);
					socialEventService.ClearCache();
					LocalPersistService.PutData("UserID", userIdentity.UserID);
					LocalPersistService.PutData("LoadMode", "externalLogin");
					userSessionService.UserSession.UserID = userIdentity.UserID;
					userSessionService.UserSession.SocialIdentities.Add(userIdentity);
					reloadGameSiganl.Dispatch();
				}
			}
			else if (code == 409)
			{
				logger.Error("Social Account is already linked to an account");
				RemoveSocialIdentity();
				logger.Debug(body);
			}
			else
			{
				RemoveSocialIdentity();
				logger.Error("Error ReLinking Social Account");
				logger.Debug(body);
			}
			if (!response.Success)
			{
				socialLogout.Dispatch(socialService);
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
