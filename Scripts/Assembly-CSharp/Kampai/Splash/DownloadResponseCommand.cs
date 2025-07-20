using System;
using System.Collections;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Audio;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class DownloadResponseCommand : Command
	{
		private const float FAILURE_MORATORIUM_PERIOD_SEC = 5f;

		public IKampaiLogger logger = LogManager.GetClassLogger("DownloadResponseCommand") as IKampaiLogger;

		[Inject]
		public IResponse response { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public DownloadDLCPartSignal downloadDLCPartSignal { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public DLCDownloadFinishedSignal finishedSignal { get; set; }

		[Inject]
		public LaunchDownloadSignal launchDownloadSignal { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		public override void Execute()
		{
			logger.Debug("DLC: DownloadResponseCommand.Execute() dlcModel.NeededBundles.Count = {0}", dlcModel.NeededBundles.Count);
			IRequest request = response.Request;
			dlcModel.RunningRequests.Remove(request);
			if (request.IsAborted())
			{
				logger.Debug("DLC: DownloadResponseCommand aborted, url = {0}", request.Uri);
				return;
			}
			IList<BundleInfo> neededBundles = dlcModel.NeededBundles;
			if (response.Success)
			{
				string bundleNameFromUrl = DownloadUtil.GetBundleNameFromUrl(request.Uri);
				sendDownloadTelemetry(bundleNameFromUrl);
				int num = 0;
				foreach (BundleInfo item in neededBundles)
				{
					if (item.name == bundleNameFromUrl)
					{
						neededBundles.RemoveAt(num);
						if (item.audio)
						{
							dlcModel.DownloadedAudioBundles.Enqueue(bundleNameFromUrl);
						}
						break;
					}
					num++;
				}
				dlcModel.LastNetworkFailureTime = -1f;
				UpdateDownloadTotals(response);
			}
			else
			{
				HandleRequestFailure(request);
			}
			if (neededBundles.Count == 0)
			{
				if (dlcModel.ShouldLoadAudio)
				{
					routineRunner.StartCoroutine(LoadAudioBundles());
				}
				else
				{
					HandleDownloadFinished();
				}
			}
			else
			{
				PlayerPrefs.SetInt("HasUpToDateDlc", 0);
				downloadDLCPartSignal.Dispatch();
			}
		}

		private void sendDownloadTelemetry(string bundleName)
		{
			if (!configurationsService.isKillSwitchOn(KillSwitch.DLC_TELEMETRY))
			{
				telemetryService.Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL(bundleName, response.DownloadTime, response.ContentLength, NetworkUtil.IsNetworkWiFi());
			}
		}

		private void UpdateDownloadTotals(IResponse response)
		{
			if (response.ContentLength > 0)
			{
				dlcModel.DownloadedTotalSize += response.ContentLength;
			}
		}

		private void HandleDownloadFinished()
		{
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("60 - Downloaded DLC", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
			PlayerPrefs.SetInt("HasUpToDateDlc", 1);
			if (dlcModel.ShouldLaunchDownloadAgain)
			{
				dlcModel.ShouldLaunchDownloadAgain = false;
				launchDownloadSignal.Dispatch(dlcModel.NextDownloadShouldLoadAudio);
			}
			else
			{
				dlcModel.HighestTierDownloaded = dlcService.GetPlayerDLCTier();
				finishedSignal.Dispatch();
				reconcileSalesSignal.Dispatch(0);
			}
			long num = (long)(DateTime.Now - dlcModel.DownloadStartTime).TotalMilliseconds;
			logger.Info("DownloadResponse DLC Download Speed Stats : DownloadedTotalTime: {0} , DownloadedTotalSize : {1}  ", num, dlcModel.DownloadedTotalSize);
			if (num > 0)
			{
				string text = ((!dlcModel.UdpEnabled) ? "Download.Http" : "Download.Udp");
				float num2 = (float)dlcModel.DownloadedTotalSize / (float)num;
				clientHealthService.MarkTimerEvent(text, num2);
				logger.Info("DownloadResponse DLC Download Speed Stats : eventname: {0} , downloadSpeed : {1} ", text, num2);
				dlcModel.DownloadedTotalSize = 0L;
				dlcModel.DownloadStartTime = DateTime.MaxValue;
			}
		}

		private void HandleRequestFailure(IRequest request)
		{
			if (dlcModel.LastNetworkFailureTime < 0f)
			{
				dlcModel.LastNetworkFailureTime = Time.realtimeSinceStartup;
			}
			float num = Time.realtimeSinceStartup - dlcModel.LastNetworkFailureTime;
			if (num > 5f)
			{
				logger.Debug("DLC: DownloadResponseCommand.HandleRequestFailure(): network switch time is up");
				logger.FatalNoThrow(FatalCode.DLC_REQ_FAIL, "Unable to download DLC {0}", request.Uri);
			}
			else
			{
				logger.Debug("DLC: DownloadResponseCommand.HandleRequestFailure(): give time to switch networks(possible reason of network failure).");
				dlcModel.PendingRequests.Enqueue(request);
			}
		}

		private IEnumerator LoadAudioBundles()
		{
			Queue<string> downloadedAudioBundles = new Queue<string>(dlcModel.DownloadedAudioBundles);
			while (downloadedAudioBundles.Count > 0)
			{
				string bundleName = downloadedAudioBundles.Dequeue();
				fmodService.LoadFromAssetBundleAsync(bundleName);
			}
			fmodService.StartAsyncBankLoadingProcessing();
			while (fmodService.BanksLoadingInProgress())
			{
				yield return null;
			}
			HandleDownloadFinished();
		}
	}
}
