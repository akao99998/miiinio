using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class LoadPlayerCommand : Command
	{
		public const string PLAYER_DATA_ENDPOINT = "/rest/gamestate/{0}";

		public IKampaiLogger logger = LogManager.GetClassLogger("LoadPlayerCommand") as IKampaiLogger;

		private bool retried;

		[Inject]
		public IResourceService resourceService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public LoadedPlayerDataSignal loadedPlayerDataSignal { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject]
		public ITimedSocialEventService socialEventService { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public SplashProgressUpdateSignal splashProgressUpdateSignal { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			logger.EventStart("LoadPlayerCommand.Execute");
			TimeProfiler.StartSection("load player");
			string text = localPersistService.GetData("LoadMode");
			string text2 = string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				switch (text)
				{
				case "default":
					break;
				case "local":
				{
					string text3 = localPersistService.GetData("LocalID");
					text2 = localPersistService.GetData("Player_" + text3);
					goto IL_00fe;
				}
				case "file":
				{
					string path = localPersistService.GetData("LocalFileName");
					text2 = resourceService.LoadText(path);
					goto IL_00fe;
				}
				case "remote":
					RemoteLoadPlayerData();
					goto IL_00fe;
				case "externalLogin":
					RemoteLoadPlayerData();
					goto IL_00fe;
				default:
					goto IL_00fe;
				}
			}
			text2 = defService.GetInitialPlayer();
			goto IL_00fe;
			IL_00fe:
			if (!string.IsNullOrEmpty(text2))
			{
				loadedPlayerDataSignal.Dispatch(text2, new PlayerMetaData());
				splashProgressUpdateSignal.Dispatch(35, 10f);
			}
			logger.EventStop("LoadPlayerCommand.Execute");
		}

		private void RemoteLoadPlayerData()
		{
			if (!localPersistService.HasKeyPlayer("COPPA_Age_Year") && !localPersistService.HasKey("RelinkingAccount"))
			{
				logger.Debug("New User");
				string initialPlayer = defService.GetInitialPlayer();
				loadedPlayerDataSignal.Dispatch(initialPlayer, new PlayerMetaData());
				return;
			}
			UserSession userSession = userSessionService.UserSession;
			logger.Debug("Existing User");
			if (userSession != null)
			{
				string userID = userSession.UserID;
				LoadCurrentSocialTeam();
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(OnPlayerLoaded);
				downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/gamestate/{0}", userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
					.WithResponseSignal(signal));
				logger.Debug("LoadPlayerCommand: Requesting player data with user id {0}", userSession.UserID);
			}
			else
			{
				logger.Fatal(FatalCode.CMD_LOAD_PLAYER, "No user session");
			}
		}

		private void LoadCurrentSocialTeam()
		{
			TimedSocialEventDefinition currentSocialEvent = socialEventService.GetCurrentSocialEvent();
			if (currentSocialEvent != null)
			{
				Signal<SocialTeamResponse, ErrorResponse> resultSignal = new Signal<SocialTeamResponse, ErrorResponse>();
				socialEventService.GetSocialEventState(currentSocialEvent.ID, resultSignal);
			}
		}

		private void OnPlayerLoaded(IResponse response)
		{
			logger.Debug("LoadPlayerCommand: Received player data with response code {0}", response.Code);
			TimeProfiler.EndSection("load player");
			string empty = string.Empty;
			PlayerMetaData playerMetaData = new PlayerMetaData();
			if (!response.Success)
			{
				int code = response.Code;
				if (code != 404)
				{
					if (!retried)
					{
						retried = true;
						logger.Error("OnPlayerLoaded failed with response code {0}", code);
						networkConnectionLostSignal.Dispatch();
					}
					else
					{
						logger.Fatal(FatalCode.GS_ERROR_LOAD_PLAYER, "Response code {0}", code);
					}
					return;
				}
				empty = defService.GetInitialPlayer();
			}
			else
			{
				IDictionary<string, string> headers = response.Headers;
				if (headers.ContainsKey("X-Kampai-Cumulative"))
				{
					float result = 0f;
					if (float.TryParse(headers["X-Kampai-Cumulative"], out result))
					{
						playerMetaData.USD = (int)result;
					}
				}
				if (headers.ContainsKey("X-Kampai-Segments"))
				{
					playerMetaData.Segments = headers["X-Kampai-Segments"];
				}
				if (headers.ContainsKey("X-Kampai-Churn"))
				{
					playerMetaData.Churn = headers["X-Kampai-Churn"];
					logger.Info("churn={0}", playerMetaData.Churn);
				}
				empty = response.Body;
				if (string.IsNullOrEmpty(empty))
				{
					logger.Fatal(FatalCode.PS_EMPTY_SERVER_JSON);
					return;
				}
			}
			loadedPlayerDataSignal.Dispatch(empty, playerMetaData);
		}
	}
}
