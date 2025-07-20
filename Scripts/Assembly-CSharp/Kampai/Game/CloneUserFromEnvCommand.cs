using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CloneUserFromEnvCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CloneUserFromEnvCommand") as IKampaiLogger;

		[Inject]
		public string FromEnvironment { get; set; }

		[Inject]
		public long UserId { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			logger.Log(KampaiLogLevel.Info, true, string.Format("Cloning {0} From {1}", UserId, FromEnvironment));
			FetchGamestate();
		}

		private void FetchGamestate()
		{
			UserSession gameStateRequestSession = GetGameStateRequestSession();
			string text = string.Format("https://kampai-{0}.appspot.com/rest/gamestate/{1}", FromEnvironment, UserId);
			logger.Log(KampaiLogLevel.Info, true, string.Format("Cloning: {0} using {1}:{2}", text, gameStateRequestSession.UserID, gameStateRequestSession.SessionID));
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				if (response.Success)
				{
					PostGamestate(response.Body);
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, true, string.Format("Server request failed: [{0}]{1}", response.Code, response.Body));
				}
			});
			downloadService.Perform(requestFactory.Resource(text).WithHeaderParam("user_id", gameStateRequestSession.UserID).WithHeaderParam("session_key", gameStateRequestSession.SessionID)
				.WithResponseSignal(signal));
		}

		private UserSession GetGameStateRequestSession()
		{
			UserSession userSession = new UserSession();
			if (FromEnvironment == "prod")
			{
				userSession.UserID = "4816488934408192";
				userSession.SessionID = "mSqL48RwjvpUoorT-T7Yb4D5IsA_YYZgaaquZY2qESQ";
			}
			else if (FromEnvironment == "stage")
			{
				userSession.UserID = "5165625081069568";
				userSession.SessionID = "WZdzsiUDlMN5gdWIt0ogSjenlImZNY-ZNXEqVKSSgBo";
			}
			else
			{
				UserSession userSession2 = userSessionService.UserSession;
				userSession.UserID = userSession2.UserID;
				userSession.SessionID = GameConstants.StaticConfig.SECRET_KEY;
			}
			return userSession;
		}

		private void PostGamestate(string gamestate)
		{
			UserSession userSession = userSessionService.UserSession;
			string text = string.Format("{0}/rest/gamestate/{1}?forceSave=true", GameConstants.StaticConfig.SERVER_URL, userSession.UserID);
			logger.Log(KampaiLogLevel.Info, true, string.Format("Cloning to: {0}", text));
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				if (response.Success)
				{
					reloadGameSignal.Dispatch();
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, true, string.Format("Server request failed: [{0}]{1}", response.Code, response.Body));
				}
			});
			downloadService.Perform(requestFactory.Resource(text).WithMethod("POST").WithBody(Encoding.UTF8.GetBytes(gamestate))
				.WithHeaderParam("user_id", userSession.UserID)
				.WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("text/plain")
				.WithResponseSignal(signal));
		}
	}
}
