using System;
using System.Collections;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.impl;

namespace Kampai.Main
{
	public class MainCompleteCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("MainCompleteCommand") as IKampaiLogger;

		private AsyncOperation async;

		[Inject]
		public AutoSavePlayerStateSignal autoSavePlayerSignal { get; set; }

		[Inject]
		public ReloadConfigurationsPeriodicSignal reloadConfigs { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public LoadDevicePrefsSignal loadDevicePrefsSignal { get; set; }

		[Inject]
		public LoadSharedBundlesSignal bundleSignal { get; set; }

		[Inject]
		public IClientHealthService clientHealthService { get; set; }

		[Inject]
		public LogTapEventMetricsSignal logTapEventMetricsSignal { get; set; }

		[Inject]
		public LoadLocalizationServiceSignal localServiceSignal { get; set; }

		[Inject]
		public SetupPushNotificationsSignal setupPushNotificationsSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IAssetBundlesService assetBundlesService { get; set; }

		[Inject]
		public ITimedSocialEventService socialEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public DLCModel dlcModel { get; set; }

		[Inject]
		public NimbleOTSignal nimbleOTSignal { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public LoadAudioSignal loadAudioSignal { get; set; }

		[Inject]
		public LaunchDownloadSignal launchDownloadSignal { get; set; }

		[Inject]
		public SplashProgressUpdateSignal splashProgressDoneSignal { get; set; }

		[Inject]
		public ILoadInService loadInService { get; set; }

		[Inject]
		public FastCommandPool fastCommandPool { get; set; }

		[Inject]
		public CheckDLCTierSignal checkDLCTier { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public LoadVillainLairAssetsSignal loadVillainLairAssetsSignal { get; set; }

		[Inject]
		public IAssetsPreloadService assetsPreloadService { get; set; }

		[Inject]
		public IHindsightService hindsightService { get; set; }

		public override void Execute()
		{
			logger.EventStart("MainCompleteCommand.Execute");
			checkDLCTier.Dispatch();
			int quantity = (int)playerService.GetQuantity(StaticItem.TIER_ID);
			int quantity2 = (int)playerService.GetQuantity(StaticItem.TIER_GATE_ID);
			dlcService.SetPlayerDLCTier(quantity);
			if (dlcModel.HighestTierDownloaded < quantity2)
			{
				logger.Debug("DLC Highest Tier Downloaded is less than the tier Gate, launching DLC Download");
				launchDownloadSignal.Dispatch(false);
			}
			else
			{
				TimeProfiler.StartSection("loading scenes");
				loadDevicePrefsSignal.Dispatch();
				loadAudioSignal.Dispatch();
				routineRunner.StartCoroutine(PostExternalScenes());
			}
			hindsightService.Initialize();
			logger.EventStop("MainCompleteCommand.Execute");
		}

		private IEnumerator PostExternalScenes()
		{
			yield return null;
			while (coroutineProgressMonitor.HasRunningTasks())
			{
				yield return null;
			}
			logger.Debug("Starting Load Post External Scene");
			bundleSignal.Dispatch();
			assetsPreloadService.StopAssetsPreload();
			localServiceSignal.Dispatch();
			DeviceCapabilities.Initialize();
			TimeProfiler.StartSection("loading game scene");
			logger.EventStart("MainCompleteCommand.LoadGame");
			SceneManager.LoadScene("Game", LoadSceneMode.Additive);
			SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);
			splashProgressDoneSignal.Dispatch(100, 3f);
			routineRunner.StartCoroutine(LevelLoadComplete());
		}

		private IEnumerator LevelLoadComplete()
		{
			yield return null;
			while (coroutineProgressMonitor.HasRunningTasks())
			{
				yield return null;
			}
			logger.EventStop("MainCompleteCommand.LoadGame");
			TimeProfiler.EndSection("loading game scene");
			logger.EventStart("MainCompleteCommand.LoadUI");
			autoSavePlayerSignal.Dispatch();
			reloadConfigs.Dispatch();
			nimbleOTSignal.Dispatch();
			clientHealthService.MarkMeterEvent("AppFlow.AppStart");
			telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("100 - Load Complete", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
			loadDevicePrefsSignal.Dispatch();
			logTapEventMetricsSignal.Dispatch();
			setupPushNotificationsSignal.Dispatch();
			socialEventService.GetPastEventsWithUnclaimedReward();
			loadInService.SaveTipsForNextLaunch((int)playerService.GetQuantity(StaticItem.LEVEL_ID));
			while (coroutineProgressMonitor.HasRunningTasks())
			{
				yield return null;
			}
			logger.EventStop("MainCompleteCommand.LoadUI");
			TimeProfiler.EndSection("loading scenes");
			TimeProfiler.StartSection("cleanup");
			logger.EventStart("MainCompleteCommand.CleanUp");
			assetBundlesService.UnloadDLCBundles();
			async = Resources.UnloadUnusedAssets();
			routineRunner.StartCoroutine(CleanupComplete());
		}

		private IEnumerator CleanupComplete()
		{
			while (!async.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			fastCommandPool.Warmup();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			logger.EventStop("MainCompleteCommand.CleanUp");
			TimeProfiler.EndSection("cleanup");
			ICrossContextCapable splashContext = null;
			try
			{
				splashContext = base.injectionBinder.GetInstance<ICrossContextCapable>(SplashElement.CONTEXT);
			}
			catch (InjectionException ex)
			{
				InjectionException e = ex;
				logger.Warning(e.ToString());
			}
			Shader.WarmupAllShaders();
			if (splashContext != null)
			{
				splashContext.injectionBinder.GetInstance<HideSplashSignal>().Dispatch();
				yield return null;
				ResumeCurrencyService();
			}
			VillainLairEntranceBuilding portal = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			if (portal != null && portal.IsUnlocked)
			{
				loadVillainLairAssetsSignal.Dispatch(portal.VillainLairInstanceID);
			}
			if (localPersistService.HasKey("RelinkingAccount"))
			{
				localPersistService.DeleteKey("RelinkingAccount");
			}
		}

		private void ResumeCurrencyService()
		{
			ICurrencyService instance = base.injectionBinder.GetInstance<ICurrencyService>();
			instance.ResumeTransactionsHandling();
		}
	}
}
