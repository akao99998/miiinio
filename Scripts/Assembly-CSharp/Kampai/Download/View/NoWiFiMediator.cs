using Kampai.Common;
using Kampai.Splash;
using Kampai.Splash.View;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Download.View
{
	public class NoWiFiMediator : Mediator
	{
		private bool canShowSettings;

		[Inject]
		public NoWiFiView view { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public ShowNoWiFiPanelSignal showNoWiFiPanelSignal { get; set; }

		[Inject]
		public NetworkTypeChangedSignal networkTypeChangedSignal { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ShowOfflinePopupSignal showOfflinePopupSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		public override void OnRegister()
		{
			canShowSettings = Native.CanShowNetworkSettings();
			view.Init(!canShowSettings);
			if (canShowSettings)
			{
				view.continueButton2.ClickedSignal.AddListener(ContinueButton);
				view.exitButton2.ClickedSignal.AddListener(ExitButton);
				view.settingsButton.ClickedSignal.AddListener(SettingsButton);
			}
			else
			{
				view.continueButton1.ClickedSignal.AddListener(ContinueButton);
				view.exitButton1.ClickedSignal.AddListener(ExitButton);
			}
		}

		private void OnEnable()
		{
			if (view != null)
			{
				Start();
			}
		}

		private void Start()
		{
			networkTypeChangedSignal.AddListener(OnNetworkTypeChanged);
			showOfflinePopupSignal.AddListener(OnShowOfflinePopup);
		}

		private void OnDisable()
		{
			networkTypeChangedSignal.RemoveListener(OnNetworkTypeChanged);
			showOfflinePopupSignal.RemoveListener(OnShowOfflinePopup);
		}

		public override void OnRemove()
		{
			if (canShowSettings)
			{
				view.continueButton2.ClickedSignal.RemoveListener(ContinueButton);
				view.exitButton2.ClickedSignal.RemoveListener(ExitButton);
				view.settingsButton.ClickedSignal.RemoveListener(SettingsButton);
			}
			else
			{
				view.continueButton1.ClickedSignal.RemoveListener(ContinueButton);
				view.exitButton1.ClickedSignal.RemoveListener(ExitButton);
			}
		}

		private void OnNetworkTypeChanged(NetworkReachability type)
		{
			switch (type)
			{
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				Close();
				break;
			case NetworkReachability.NotReachable:
				if (!networkModel.isConnectionLost)
				{
					networkConnectionLostSignal.Dispatch();
				}
				break;
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				break;
			}
		}

		private void OnShowOfflinePopup(bool isShown)
		{
			if (isShown)
			{
				Close();
			}
		}

		private void ContinueButton()
		{
			dlcModel.AllowDownloadOnMobileNetwork = true;
			Close();
		}

		private void SettingsButton()
		{
			Native.OpenNetworkSettings();
		}

		private void ExitButton()
		{
			Application.Quit();
		}

		private void Close()
		{
			showNoWiFiPanelSignal.Dispatch(false);
			if (!networkModel.isConnectionLost && (networkModel.reachability == NetworkReachability.ReachableViaLocalAreaNetwork || dlcModel.AllowDownloadOnMobileNetwork))
			{
				resumeNetworkOperationSignal.Dispatch();
			}
		}
	}
}
