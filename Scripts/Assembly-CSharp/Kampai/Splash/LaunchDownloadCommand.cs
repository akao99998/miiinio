using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Splash
{
	public class LaunchDownloadCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LaunchDownloadCommand") as IKampaiLogger;

		[Inject]
		public bool shouldLoadAudio { get; set; }

		[Inject]
		public ReconcileDLCSignal reconcileSignal { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IConfigurationsService configurationService { get; set; }

		[Inject]
		public DownloadResponseSignal downloadResponseSignal { get; set; }

		[Inject]
		public DownloadInitializeSignal initSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public DownloadProgressSignal progressSignal { get; set; }

		[Inject]
		public DownloadDLCPartSignal downloadDLCPartSignal { get; set; }

		[Inject]
		public DLCDownloadFinishedSignal downloadFinishedSignal { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public ShowNoWiFiPanelSignal showNoWiFiPanelSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		[Inject]
		public IBackgroundDownloadDlcService backgroundDownloadDlcService { get; set; }

		[Inject]
		public CheckAvailableStorageSignal checkAvailableStorageSignal { get; set; }

		public override void Execute()
		{
			if ((dlcModel.PendingRequests != null && dlcModel.PendingRequests.Count > 0) || (dlcModel.RunningRequests != null && dlcModel.RunningRequests.Count > 0))
			{
				dlcModel.ShouldLaunchDownloadAgain = true;
				dlcModel.NextDownloadShouldLoadAudio = shouldLoadAudio;
				return;
			}
			dlcModel.ShouldLoadAudio = shouldLoadAudio;
			reconcileSignal.Dispatch(false);
			dlcModel.PendingRequests = CreateNetworkRequests();
			dlcModel.RunningRequests = new List<IRequest>();
			dlcModel.LastNetworkFailureTime = -1f;
			checkAvailableStorageSignal.Dispatch(GameConstants.PERSISTENT_DATA_PATH, dlcModel.TotalSize, StartDownloadProxy);
		}

		private void StartDownloadProxy()
		{
			routineRunner.StartCoroutine(StartDownload());
		}

		private IEnumerator StartDownload()
		{
			yield return new WaitForEndOfFrame();
			initSignal.Dispatch(dlcModel.TotalSize);
			if (dlcModel.NeededBundles != null && dlcModel.NeededBundles.Count == 0)
			{
				downloadFinishedSignal.Dispatch();
				yield break;
			}
			if (networkModel.reachability == NetworkReachability.ReachableViaCarrierDataNetwork && !dlcModel.AllowDownloadOnMobileNetwork)
			{
				resumeNetworkOperationSignal.AddOnce(StartDownloadProxy);
				showNoWiFiPanelSignal.Dispatch(true);
				yield break;
			}
			dlcModel.DownloadStartTime = DateTime.Now;
			dlcModel.DownloadedTotalSize = 0L;
			if (LoadState.Get() == LoadStateType.STARTED)
			{
				backgroundDownloadDlcService.Start();
				yield break;
			}
			string dlcPath = GameConstants.DLC_PATH;
			if (!Directory.Exists(dlcPath))
			{
				Directory.CreateDirectory(dlcPath);
			}
			downloadDLCPartSignal.Dispatch();
		}

		private Queue<IRequest> CreateNetworkRequests()
		{
			string userId = UnityEngine.Random.Range(0, 100).ToString();
			bool udpEnabled = FeatureAccessUtil.isAccessible(AccessControlledFeature.AKAMAI_UDP, configurationService.GetConfigurations(), userId, logger);
			dlcModel.UdpEnabled = udpEnabled;
			Queue<IRequest> queue = new Queue<IRequest>(dlcModel.NeededBundles.Count);
			foreach (BundleInfo neededBundle in dlcModel.NeededBundles)
			{
				IRequest item = CreateRequest(neededBundle, udpEnabled);
				queue.Enqueue(item);
			}
			return queue;
		}

		private IRequest CreateRequest(BundleInfo bundleInfo, bool udpEnabled)
		{
			string name = bundleInfo.name;
			string uri = DownloadUtil.CreateBundleURL(manifestService.GetDLCURL(), name);
			string filePath = DownloadUtil.CreateBundlePath(GameConstants.DLC_PATH, name);
			return requestFactory.Resource(uri).WithOutputFile(filePath).WithMd5(bundleInfo.sum)
				.WithGZip(true)
				.WithUdp(udpEnabled)
				.WithRetry(true, 1)
				.WithResume(true)
				.WithResponseSignal(downloadResponseSignal)
				.WithProgressSignal(progressSignal);
		}
	}
}
