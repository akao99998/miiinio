using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class UnlinkAccountCommand : Command
	{
		private const string ACCOUNT_UNLINK_ENDPOINT = "rest/user/{0}/identity/unlink";

		public IKampaiLogger logger = LogManager.GetClassLogger("UnlinkAccountCommand") as IKampaiLogger;

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService FacebookService { get; set; }

		[Inject(SocialServices.GAMECENTER)]
		public ISocialService GameCenterService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService GooglePlayService { get; set; }

		[Inject]
		public IdentityType UnlinkType { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			UserSession userSession = userSessionService.UserSession;
			string userID = userSession.UserID;
			UnlinkAccountRequest unlinkAccountRequest = new UnlinkAccountRequest();
			unlinkAccountRequest.identityType = UnlinkType.ToString().ToLower();
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(OnUnlinkAccountResponse);
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("rest/user/{0}/identity/unlink", userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal)
				.WithEntity(unlinkAccountRequest));
		}

		private void OnUnlinkAccountResponse(IResponse httpResponse)
		{
			string text = UnlinkType.ToString();
			string empty = string.Empty;
			switch (httpResponse.Code)
			{
			case 200:
				logger.Debug(string.Format("Successfully unlinked user from identities with type {0}", text));
				LogOutSocialService(UnlinkType);
				RemoveSocialIdentities(UnlinkType);
				break;
			case 400:
				empty = "Invalid identity type";
				logger.Error(string.Format("{0}.  Failed to unlink user from identities with type {1}", empty, text));
				break;
			default:
				empty = "Unknown error";
				logger.Error(string.Format("{0}.  Failed to unlink user from identities with type {1}", empty, text));
				break;
			}
		}

		private void LogOutSocialService(IdentityType type)
		{
			Dictionary<IdentityType, ISocialService> dictionary = new Dictionary<IdentityType, ISocialService>();
			dictionary.Add(IdentityType.facebook, FacebookService);
			dictionary.Add(IdentityType.gamecenter, GameCenterService);
			dictionary.Add(IdentityType.googleplay, GooglePlayService);
			Dictionary<IdentityType, ISocialService> dictionary2 = dictionary;
			ISocialService value;
			if (dictionary2.TryGetValue(type, out value) && value.isLoggedIn)
			{
				value.Logout();
			}
		}

		private void RemoveSocialIdentities(IdentityType type)
		{
			IList<UserIdentity> socialIdentities = userSessionService.UserSession.SocialIdentities;
			for (int num = socialIdentities.Count - 1; num >= 0; num--)
			{
				if (socialIdentities[num].Type == type)
				{
					socialIdentities.RemoveAt(num);
				}
			}
		}
	}
}
