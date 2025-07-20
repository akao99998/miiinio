using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Audio;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class GameStartCommand : Command
	{
		private const int maxSocialInitTries = 3;

		public IKampaiLogger logger = LogManager.GetClassLogger("GameStartCommand") as IKampaiLogger;

		[Inject]
		public PopulateEnvironmentSignal environmentSignal { get; set; }

		[Inject]
		public InitializeSpecialEventSignal initializeSpecialEventSignal { get; set; }

		[Inject]
		public CancelAllNotificationSignal cancelAllNotificationSignal { get; set; }

		[Inject]
		public SetupObjectManagersSignal setupMinionsSignal { get; set; }

		[Inject]
		public SetupBuildingManagerSignal setupBuildingSignal { get; set; }

		[Inject]
		public UpdateVolumeSignal updateVolumeSignal { get; set; }

		[Inject]
		public MuteVolumeSignal muteVolumeSignal { get; set; }

		[Inject]
		public PlayGlobalMusicSignal musicSignal { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public SetupTimeEventServiceSignal timeEventServiceSignal { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public SocialInitAllServicesSignal socialInitSignal { get; set; }

		[Inject]
		public INotificationService notificationService { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public SetupAudioSignal setupAudioSignal { get; set; }

		[Inject]
		public LoadMinionDataSignal loadMinionDataSignal { get; set; }

		[Inject]
		public SetupNamedCharactersSignal setupNamedCharactersSignal { get; set; }

		[Inject]
		public VillainIslandMessageSignal villainIslandMessageSignal { get; set; }

		[Inject]
		public RandomizeMinionPositionsSignal randomizeMinionPositionsSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public ToggleStickerbookGlowSignal stickerbookGlow { get; set; }

		[Inject]
		public RandomFlyOverSignal randomFlyOverSignal { get; set; }

		[Inject]
		public InitializeMarketplaceSlotsSignal initializeSlotsSignal { get; set; }

		[Inject]
		public RestorePlayersSalesSignal restoreSalesSignal { get; set; }

		[Inject]
		public RestoreMinionPartySignal restoreMinionPartySignal { get; set; }

		[Inject]
		public StartMarketplaceOnboardingSignal startMarketplaceOnboardingSignal { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public LoadServerSalesSignal loadServerSalesSignal { get; set; }

		[Inject]
		public DisplayRedemptionConfirmationSignal displayRedemptionSignal { get; set; }

		[Inject]
		public CheckSystemNotificationSettingsSignal checkNotificationsSignal { get; set; }

		[Inject]
		public LoadEnvironmentSignal loadEnvironmentSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtilities { get; set; }

		[Inject]
		public SetupTSMCharacterSignal setupTSMCharacterSignal { get; set; }

		public override void Execute()
		{
			logger.EventStart("GameStartCommand.Execute");
			TimeProfiler.StartSection("game start");
			loadEnvironmentSignal.Dispatch();
			Camera main = Camera.main;
			GameObject gameObject = main.gameObject;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(MainElement.CAMERA)
				.CrossContext();
			main.cullingMask = -148513;
			CameraUtils o = gameObject.AddComponent<CameraUtils>();
			base.injectionBinder.Bind<CameraUtils>().ToValue(o).ToSingleton();
			SetupGameObjects();
			SetupCamera(gameObject);
			masterPlanService.Initialize();
			if (localPersistence.HasKey("AutoSaveLock"))
			{
				localPersistence.DeleteKey("AutoSaveLock");
			}
		}

		private void SetupGameObjects()
		{
			GameObject gameObject = new GameObject("Flowers");
			gameObject.transform.parent = contextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(GameElement.FLOWER_PARENT);
			GameObject gameObject2 = new GameObject("ForSaleSigns");
			gameObject2.transform.parent = contextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject2).ToName(GameElement.FOR_SALE_SIGN_PARENT);
			GameObject gameObject3 = new GameObject("LandExpansions");
			gameObject3.transform.parent = contextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject3).ToName(GameElement.LAND_EXPANSION_PARENT);
			GameObject gameObject4 = new GameObject("Special_Event");
			gameObject4.transform.parent = contextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject4).ToName(GameElement.SPECIAL_EVENT_PARENT);
			GameObject gameObject5 = new GameObject("VillainLair");
			gameObject5.transform.parent = contextView.transform;
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject5).ToName(GameElement.VILLAIN_LAIR_PARENT);
			coroutineProgressMonitor.StartTask(StartGame(), "start game");
		}

		private IEnumerator PostLoadRoutine()
		{
			while (coroutineProgressMonitor.HasRunningTasks())
			{
				yield return null;
			}
			InitSocialIfNeeded();
			UpdatePlayerGridSize();
			routineRunner.StartCoroutine(WaitAFrame());
		}

		private void InitMarketplaceSlotsIfNeeded()
		{
			if (coppaService.IsBirthdateKnown())
			{
				initializeSlotsSignal.Dispatch();
			}
		}

		private void InitSocialIfNeeded()
		{
			if (coppaService.IsBirthdateKnown())
			{
				RetrySocialInit(0);
			}
		}

		private void UpdatePlayerGridSize()
		{
			int num = buildingUtilities.AvailableLandSpaceCount();
			logger.Debug("Total Available Space: {0}", num);
			playerService.SetQuantity(StaticItem.TOTAL_AVAILABLE_LAND_SPACE, num);
		}

		private void RetrySocialInit(int tries)
		{
			routineRunner.StartCoroutine(WaitSomeTime(1f, delegate
			{
				if (userSessionService.UserSession != null && !string.IsNullOrEmpty(userSessionService.UserSession.SessionID))
				{
					socialInitSignal.Dispatch();
				}
				else if (tries < 3)
				{
					tries++;
					logger.Log(KampaiLogLevel.Info, "User Session was not available, will retry to initialize social networks in " + 1f + " second");
					RetrySocialInit(tries);
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "User Session was never initilized so social services will not be initialized");
				}
			}));
		}

		private void SetupCamera(GameObject camera)
		{
			camera.AddComponent<TouchPanView>();
			camera.AddComponent<TouchZoomView>();
			camera.AddComponent<TouchDragPanView>();
			routineRunner.StartCoroutine(CameraSignalDelay());
		}

		private IEnumerator CameraSignalDelay()
		{
			yield return new WaitForEndOfFrame();
			enableCameraSignal.Dispatch(7);
			disableCameraSignal.Dispatch(8);
		}

		private IEnumerator WaitSomeTime(float time, Action a)
		{
			yield return new WaitForSeconds(time);
			a();
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			int tokenCount = (int)playerService.GetQuantity(StaticItem.MINION_LEVEL_TOKEN);
			int minions = playerService.GetMinionCount();
			int seconds = playerService.LastGameStartUTC;
			playerDurationService.SetLastGameStartUTC();
			if (seconds != 0)
			{
				seconds = playerService.LastGameStartUTC - seconds;
			}
			int expansionCount = playerService.GetPurchasedExpansionCount();
			string expansions = string.Format(arg1: landExpansionService.GetLandExpansionCount() + expansionCount, format: "{0}/{1}", arg0: expansionCount);
			if (localPersistence.HasKeyPlayer("StickerbookGlow"))
			{
				stickerbookGlow.Dispatch(true);
			}
			if (localPersistence.HasKeyPlayer("MarketSurfacing"))
			{
				int buildingID = Convert.ToInt32(localPersistence.GetDataPlayer("MarketSurfacing"));
				startMarketplaceOnboardingSignal.Dispatch(buildingID);
			}
			string swrveGroup = playerService.SWRVEGroup;
			telemetryService.Send_Telemetry_EVT_USER_DATA_AT_APP_START(seconds, tokenCount, minions, (!string.IsNullOrEmpty(swrveGroup)) ? swrveGroup : "anyVariant", expansions);
			if (localPersistence.HasKeyPlayer("StickerbookGlow"))
			{
				stickerbookGlow.Dispatch(true);
			}
			if (playerService.GetHighestFtueCompleted() == 999999)
			{
				randomFlyOverSignal.Dispatch(-1);
			}
			displayRedemptionSignal.Dispatch();
			setupTSMCharacterSignal.Dispatch();
			LoadState.Set(LoadStateType.STARTED);
			TimeProfiler.EndSection("game start");
		}

		private IEnumerator StartGame()
		{
			playerService.ID = Convert.ToInt64(userSessionService.UserSession.UserID);
			GameObject footprint = UnityEngine.Object.Instantiate(KampaiResources.Load("Footprint")) as GameObject;
			footprint.transform.parent = contextView.transform;
			footprint.AddComponent<FootprintView>();
			notificationService.Initialize();
			logger.Debug("GameStartCommand: Notification Service initialized.");
			cancelAllNotificationSignal.Dispatch();
			checkNotificationsSignal.Dispatch();
			timeEventServiceSignal.Dispatch(contextView);
			setupAudioSignal.Dispatch();
			environmentSignal.Dispatch(false);
			initializeSpecialEventSignal.Dispatch();
			restoreMinionPartySignal.Dispatch();
			Dictionary<string, float> parameters = new Dictionary<string, float>();
			musicSignal.Dispatch("Play_backGroundMusic_01", parameters);
			setupBuildingSignal.Dispatch();
			updateVolumeSignal.Dispatch();
			muteVolumeSignal.Dispatch();
			setupMinionsSignal.Dispatch();
			yield return coroutineProgressMonitor.waitForPreviousTaskToComplete;
			loadMinionDataSignal.Dispatch();
			yield return null;
			while (fmodService.BanksLoadingInProgress())
			{
				yield return coroutineProgressMonitor.waitForNextFrame;
			}
			setupNamedCharactersSignal.Dispatch();
			playerDurationService.InitLevelUpUTC();
			yield return null;
			randomizeMinionPositionsSignal.Dispatch();
			villainIslandMessageSignal.Dispatch(false);
			InitMarketplaceSlotsIfNeeded();
			MarketplaceDefinition definition = definitionService.Get<MarketplaceDefinition>();
			if (definition != null)
			{
				if (playerService.GetQuantity(StaticItem.LEVEL_ID) >= definition.LevelGate)
				{
					restoreSalesSignal.Dispatch();
				}
				else
				{
					updateSoldItemsSignal.Dispatch(false);
				}
			}
			else
			{
				logger.Warning("Marketplace Definition is null.");
			}
			loadServerSalesSignal.Dispatch();
			routineRunner.StartCoroutine(PostLoadRoutine());
			logger.EventStop("GameStartCommand.Execute");
		}
	}
}
