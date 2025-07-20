using Kampai.Common;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Splash
{
	public class LogoPanelMediator : Mediator
	{
		[Inject]
		public LogoPanelView view { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public NetworkTypeChangedSignal networkTypeChangedSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public ShowNoWiFiPanelSignal showNoWiFiPanelSignal { get; set; }

		public override void OnRegister()
		{
			view.SetupRefs();
			Screen.sleepTimeout = -1;
			networkTypeChangedSignal.AddListener(OnNetworkTypeChanged);
			resumeNetworkOperationSignal.AddListener(OnNetworkResume);
			showNoWiFiPanelSignal.AddListener(view.ShowNoWiFi);
		}

		public override void OnRemove()
		{
			networkTypeChangedSignal.RemoveListener(OnNetworkTypeChanged);
			resumeNetworkOperationSignal.RemoveListener(OnNetworkResume);
			showNoWiFiPanelSignal.RemoveListener(view.ShowNoWiFi);
			Screen.sleepTimeout = -2;
		}

		private void OnNetworkTypeChanged(NetworkReachability type)
		{
			switch (type)
			{
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				dlcModel.AllowDownloadOnMobileNetwork = false;
				break;
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				if (!networkModel.isConnectionLost && !dlcModel.AllowDownloadOnMobileNetwork)
				{
					PauseDownload();
				}
				break;
			}
		}

		private void OnNetworkResume()
		{
			if (networkModel.reachability == NetworkReachability.ReachableViaCarrierDataNetwork && !dlcModel.AllowDownloadOnMobileNetwork)
			{
				PauseDownload();
			}
		}

		private void PauseDownload()
		{
			downloadService.Restart();
			showNoWiFiPanelSignal.Dispatch(true);
		}
	}
}
