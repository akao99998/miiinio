using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class CloneUserCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CloneUserCommand") as IKampaiLogger;

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
			UserSession userSession = userSessionService.UserSession;
			string text = string.Format("{0}/rest/gamestate/{1}/{2}", GameConstants.StaticConfig.SERVER_URL, UserId, userSession.UserID);
			logger.Log(KampaiLogLevel.Info, true, string.Format("Cloning: {0}", text));
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
			downloadService.Perform(requestFactory.Resource(text).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithResponseSignal(signal));
		}
	}
}
