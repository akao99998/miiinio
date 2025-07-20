using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

public class UpdateUserCommand : Command
{
	public const string USER_UPDATE_ENDPOINT = "/rest/user/{0}";

	public IKampaiLogger logger = LogManager.GetClassLogger("UpdateUserCommand") as IKampaiLogger;

	[Inject]
	public string synergyID { get; set; }

	[Inject("game.server.host")]
	public string ServerUrl { get; set; }

	[Inject]
	public IDownloadService downloadService { get; set; }

	[Inject]
	public IUserSessionService userSessionService { get; set; }

	[Inject]
	public IRequestFactory requestFactory { get; set; }

	public override void Execute()
	{
		if (string.IsNullOrEmpty(synergyID))
		{
			logger.Error("Failed to update user with new synergyID. Provided synergyID is null or empty.");
			return;
		}
		UserSession userSession = userSessionService.UserSession;
		if (userSession != null)
		{
			string userID = userSession.UserID;
			UserUpdateRequest userUpdateRequest = new UserUpdateRequest();
			userUpdateRequest.SynergyID = synergyID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				userSessionService.UserUpdateRequestCallback(synergyID, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/user/{0}", userID)).WithHeaderParam("user_id", userID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(userUpdateRequest)
				.WithResponseSignal(signal));
		}
		else
		{
			logger.Error("Failed to update user with new synergyID {0}. No user session yet.", synergyID);
		}
	}
}
