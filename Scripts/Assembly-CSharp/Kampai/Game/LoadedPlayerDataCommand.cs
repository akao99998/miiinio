using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadedPlayerDataCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadedPlayerDataCommand") as IKampaiLogger;

		[Inject]
		public string playerJSON { get; set; }

		[Inject]
		public PlayerMetaData meta { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public MainCompleteSignal completeSignal { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ISwrveService swrveService { get; set; }

		public override void Execute()
		{
			string text = localPersistService.GetData("LoadMode");
			if (text == "remote")
			{
				if (string.IsNullOrEmpty(playerJSON))
				{
					logger.Fatal(FatalCode.GS_ERROR_FETCHING_PLAYER_DATA);
				}
			}
			else if (text == "externalLogin")
			{
				if (!string.IsNullOrEmpty(playerJSON))
				{
					reloadGameSignal.Dispatch();
					return;
				}
				logger.Fatal(FatalCode.GS_ERROR_FETCHING_PLAYER_DATA);
			}
			this.telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("90 - Loaded Player Data", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
			if (playerJSON.Length != 0)
			{
				if (DeserializePlayerData(playerJSON))
				{
					TelemetryService telemetryService = this.telemetryService as TelemetryService;
					if (telemetryService != null)
					{
						telemetryService.SetPlayerServiceReference(playerService);
						telemetryService.SetPlayerDurationServiceReference(playerDurationService);
					}
					SwrveService swrveService = this.swrveService as SwrveService;
					if (swrveService != null)
					{
						swrveService.SetPlayerServiceReference(playerService);
					}
					else
					{
						logger.Log(KampaiLogLevel.Error, "SwrveService was not setup properly");
					}
					this.swrveService.SendUserStatsUpdate();
					completeSignal.Dispatch();
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "DeserializingPlayerData returned false");
				}
			}
			else
			{
				logger.FatalNoThrow(FatalCode.CMD_LOAD_PLAYER);
			}
		}

		private bool DeserializePlayerData(string json)
		{
			try
			{
				TimeProfiler.StartSection("read player");
				playerService.Deserialize(json);
				if (meta != null)
				{
					playerService.IngestPlayerMeta(meta);
				}
				TimeProfiler.EndSection("read player");
				return true;
			}
			catch (FatalException ex)
			{
				logger.Error("LoadedPlayerDataCommand().DeserializePlayerData: FatalException. Json: {0}", json);
				logger.FatalNoThrow(ex.FatalCode, ex.ReferencedId, "Message: {0}, Reason: {1}", ex.Message, (ex.InnerException == null) ? ex.ToString() : ex.InnerException.ToString());
			}
			catch (Exception ex2)
			{
				logger.Error("Unexpected player deser-n error. Json: {0}", json);
				logger.FatalNoThrow(FatalCode.PS_JSON_PARSE_ERR, 6, "Reason: {0}", ex2);
			}
			return false;
		}
	}
}
