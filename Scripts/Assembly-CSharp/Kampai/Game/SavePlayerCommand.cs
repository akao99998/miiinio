using System;
using System.Collections;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	internal sealed class SavePlayerCommand : Command
	{
		public const string PLAYER_DATA_ENDPOINT = "/rest/gamestate/{0}";

		public IKampaiLogger logger = LogManager.GetClassLogger("SavePlayerCommand") as IKampaiLogger;

		private bool retried;

		private SaveLocation saveLocation;

		private string saveID;

		private bool saveImmediately;

		private DateTime saveTimestampUTC;

		[Inject]
		public Tuple<SaveLocation, string, bool> tuple { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public ILocalPersistanceService persistService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public PlayerSavedSignal playerSavedSignal { get; set; }

		public override void Execute()
		{
			saveLocation = tuple.Item1;
			saveID = tuple.Item2;
			saveImmediately = tuple.Item3;
			saveTimestampUTC = DateTime.UtcNow;
			if (PlayerPrefs.HasKey("Debug.StopSaving"))
			{
				OnSavingResult(false);
				return;
			}
			if (LoadState.Get() != LoadStateType.STARTED)
			{
				OnSavingResult(false);
				return;
			}
			if (persistService.HasKey("AutoSaveLock"))
			{
				if (!saveImmediately)
				{
					OnSavingResult(false);
					return;
				}
				persistService.DeleteKey("AutoSaveLock");
			}
			else if (!saveImmediately)
			{
				persistService.PutDataInt("AutoSaveLock", 1);
			}
			playerDurationService.UpdateGameplayDuration();
			byte[] array = playerService.Serialize();
			if (array == null || array.Length == 0)
			{
				OnSavingResult(false);
			}
			else if (saveImmediately)
			{
				ImmediateSanityCheck(array);
			}
			else
			{
				routineRunner.StartCoroutine(SanityCheck(array));
			}
		}

		private void OnSavingResult(bool success)
		{
			playerSavedSignal.Dispatch(saveLocation, saveID, saveTimestampUTC, success);
		}

		private Player.SanityCheckFailureReason DeepScan(Player prevSave)
		{
			return playerService.DeepScan(prevSave);
		}

		private IEnumerator SanityCheck(byte[] playerData)
		{
			yield return null;
			Player previousSave = playerService.LastSave;
			Player currentSave = playerService.LoadPlayerData(Encoding.UTF8.GetString(playerData));
			yield return null;
			Player.SanityCheckFailureReason sanityCheck = SanityCheck(previousSave, currentSave);
			yield return null;
			if (sanityCheck == Player.SanityCheckFailureReason.Passed)
			{
				PassedSanityCheck(playerData, currentSave);
				if (persistService.HasKey("AutoSaveLock"))
				{
					persistService.DeleteKey("AutoSaveLock");
				}
			}
			else
			{
				if (persistService.HasKey("AutoSaveLock"))
				{
					persistService.DeleteKey("AutoSaveLock");
				}
				FailedSanityCheck(previousSave, currentSave, sanityCheck);
			}
		}

		private void ImmediateSanityCheck(byte[] playerData)
		{
			Player lastSave = playerService.LastSave;
			Player currentSave = playerService.LoadPlayerData(Encoding.UTF8.GetString(playerData));
			Player.SanityCheckFailureReason sanityCheckFailureReason = SanityCheck(lastSave, currentSave);
			if (sanityCheckFailureReason == Player.SanityCheckFailureReason.Passed)
			{
				PassedSanityCheck(playerData, currentSave);
			}
			else
			{
				FailedSanityCheck(lastSave, currentSave, sanityCheckFailureReason);
			}
		}

		private Player.SanityCheckFailureReason SanityCheck(Player previousSave, Player currentSave)
		{
			Player.SanityCheckFailureReason result = Player.SanityCheckFailureReason.Passed;
			if (saveLocation != SaveLocation.REMOTE_NOSANITY)
			{
				result = currentSave.ValidateSaveData(previousSave);
			}
			return result;
		}

		private void PassedSanityCheck(byte[] playerData, Player currentSave)
		{
			playerService.LastSave = currentSave;
			if (saveLocation == SaveLocation.REMOTE || saveLocation == SaveLocation.REMOTE_NOSANITY)
			{
				if (saveImmediately || persistService.HasKey("AutoSaveLock"))
				{
					RemoteSavePlayerData(playerData, saveImmediately);
				}
				else
				{
					OnSavingResult(false);
				}
			}
			else
			{
				persistService.PutData("Player_" + saveID, Encoding.UTF8.GetString(playerData));
				OnSavingResult(true);
			}
		}

		private void FailedSanityCheck(Player previousSave, Player currentSave, Player.SanityCheckFailureReason sanityCheck)
		{
			OnSavingResult(false);
			Player.SanityCheckFailureReason sanityCheckFailureReason = DeepScan(previousSave);
			if (sanityCheckFailureReason == Player.SanityCheckFailureReason.Passed)
			{
				string @string = Encoding.UTF8.GetString(playerService.SavePlayerData(previousSave));
				string string2 = Encoding.UTF8.GetString(playerService.SavePlayerData(currentSave));
				logger.Fatal(FatalCode.PS_FAILED_SANITY_CHECK, (int)sanityCheck, "Failed sanity check on save because {0} \n{1}\n--------\n{2}", sanityCheck.ToString(), @string, string2);
			}
			else
			{
				logger.Fatal(FatalCode.PS_FAILED_DEEP_SCAN, (int)sanityCheckFailureReason, "Failed deep scan because {0}\n", sanityCheckFailureReason.ToString());
			}
		}

		private void RemoteSavePlayerData(byte[] playerData, bool gameIsGoingToSleep)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession != null)
			{
				string userID = userSession.UserID;
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(OnPlayerSaved);
				IRequest request = requestFactory.Resource(ServerUrl + string.Format("/rest/gamestate/{0}", userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
					.WithContentType("text/plain")
					.WithMethod("POST")
					.WithRequestCount(playerService.getAndIncrementRequestCounter())
					.WithBody(playerData);
				IDownloadService obj = downloadService;
				IRequest request3;
				if (gameIsGoingToSleep)
				{
					IRequest request2 = request;
					request3 = request2;
				}
				else
				{
					request3 = request.WithResponseSignal(signal);
				}
				obj.Perform(request3, gameIsGoingToSleep);
			}
			else
			{
				OnSavingResult(false);
				logger.Fatal(FatalCode.CMD_SAVE_PLAYER, "No user session");
			}
		}

		private void OnPlayerSaved(IResponse response)
		{
			if (!response.Success)
			{
				if (response.Code == 409)
				{
					OnSavingResult(false);
					ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response.Body);
					logger.Fatal(FatalCode.GS_ERROR_CORRUPT_SAVE_DETECTED, errorResponse.Error.Message);
					return;
				}
				if (!retried)
				{
					retried = true;
					networkConnectionLostSignal.Dispatch();
				}
				logger.Error("Unable to save player to server: {0}", response.Code);
				OnSavingResult(false);
			}
			else
			{
				OnSavingResult(true);
				logger.Log(KampaiLogLevel.Debug, "User data saved to the server");
			}
		}
	}
}
