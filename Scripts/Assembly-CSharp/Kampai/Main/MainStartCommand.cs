using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Main
{
	public class MainStartCommand : Command
	{
		private const string MEDIA_RECEIVER_CLASS_NAME = "com.ea.gp.minions.app.MediaReceiver";

		private IKampaiLogger logger;

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject]
		public InitLocalizationServiceSignal initLocalizationServiceSignal { get; set; }

		[Inject]
		public CheckAvailableStorageSignal checkAvailableStorageSignal { get; set; }

		[Inject]
		public IInvokerService invokerService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public SetupHockeyAppSignal setupHockeyAppSignal { get; set; }

		[Inject]
		public SetupEventSystemSignal loadEventSystemSignal { get; set; }

		[Inject]
		public LoadConfigurationSignal loadConfigurationSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public NimbleTelemetrySender nimbleTelemetryService { get; set; }

		[Inject]
		public SetupNativeAlertManagerSignal setupNativeAlertManagerSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public SetupLoggingTargetsSignal setupLoggingTargetsSignal { get; set; }

		[Inject]
		public MediaStateChangedSignal mediaStateChangedSignal { get; set; }

		[Inject]
		public ReloadGameSignal reloadGameSignal { get; set; }

		public override void Execute()
		{
			setupLoggingTargetsSignal.Dispatch();
			logger = LogManager.GetClassLogger("MainStartCommand") as IKampaiLogger;
			logger.EventStart("MainStartCommand.Execute");
			KampaiResources.SetLogger();
			initLocalizationServiceSignal.Dispatch();
			checkAvailableStorageSignal.Dispatch(string.Empty, 2097152uL, ContinueExecution);
			logger.EventStop("MainStartCommand.Execute");
		}

		private void ContinueExecution()
		{
			telemetryService.AddTelemetrySender(nimbleTelemetryService);
			telemetryService.SharingUsageCompliance();
			telemetryService.COPPACompliance();
			string sWRVEGroup = playerService.SWRVEGroup;
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("10 - Load Start", sWRVEGroup, dlcService.GetDownloadQualityLevel());
			MediaClient.Start();
			ScreenUtils.ToggleAutoRotation(true);
			SetupBindings();
			RegisterMediaReceiver();
			reloadGameSignal.AddOnce(UnregisterMediaReceiver);
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("13 - Akamai Media Client Start", sWRVEGroup, dlcService.GetDownloadQualityLevel());
			setupHockeyAppSignal.Dispatch();
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("16 - HockeyApp And Loggy Start", sWRVEGroup, dlcService.GetDownloadQualityLevel());
			loadEventSystemSignal.Dispatch();
			loadConfigurationSignal.Dispatch(true);
			setupNativeAlertManagerSignal.Dispatch();
		}

		private void SetupBindings()
		{
			managers.transform.SetParent(contextView.transform, false);
			GameObject gameObject = new GameObject("Invoker");
			gameObject.transform.parent = managers.transform;
			Invoker invoker = gameObject.AddComponent<Invoker>();
			InvokerService invokerService = this.invokerService as InvokerService;
			if (invokerService != null)
			{
				invoker.Initialize(invokerService);
			}
			else
			{
				logger.Error("Unexpected binding to IInvokerService. InvokerService is expected.");
			}
			GameObject gameObject2 = new GameObject("NetworkMonitor");
			gameObject2.transform.SetParent(managers.transform, false);
			gameObject2.AddComponent<NetworkMonitorView>();
			mediaStateChangedSignal.AddOnce(OnMediaStateChanged);
		}

		private void OnMediaStateChanged(Tuple<string, string> state)
		{
			string item = state.Item1;
			string item2 = state.Item2;
			logger.Warning("Media state changed to {0} for {1}", item.Substring(item.LastIndexOf('.') + 1), item2);
			reloadGameSignal.Dispatch();
		}

		private void RegisterMediaReceiver()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.app.MediaReceiver"))
			{
				androidJavaClass.CallStatic("setOnReceiveBroadcastListener", new Native.OnReceiveBroadcastListener(delegate(AndroidJavaObject context, AndroidJavaObject intent)
				{
					invokerService.Add(delegate
					{
						try
						{
							using (AndroidJavaObject androidJavaObject = intent.Call<AndroidJavaObject>("getData", new object[0]))
							{
								mediaStateChangedSignal.Dispatch(Tuple.Create(intent.Call<string>("getAction", new object[0]), androidJavaObject.Call<string>("getPath", new object[0])));
							}
						}
						finally
						{
							context.Dispose();
							intent.Dispose();
						}
					});
				}));
			}
		}

		private void UnregisterMediaReceiver()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.app.MediaReceiver"))
			{
				androidJavaClass.CallStatic("setOnReceiveBroadcastListener", null);
			}
		}
	}
}
