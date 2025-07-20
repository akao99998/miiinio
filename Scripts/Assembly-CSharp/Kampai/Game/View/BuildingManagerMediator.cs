using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using Kampai.Util.Audio;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	internal sealed class BuildingManagerMediator : EventMediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("BuildingManagerMediator") as IKampaiLogger;

		private DummyBuildingObject currentDummyBuilding;

		private Signal<int> triggerGagAnimation = new Signal<int>();

		private Signal<int> minionTaskingAnimationDone = new Signal<int>();

		private bool allowStorable = true;

		private SetBuildingRushedSignal setBuildingRushedSignal;

		private RushRevealBuildingSignal rushRevealBuildingSignal;

		private int buildingID = -1;

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public BuildingManagerView view { get; set; }

		[Inject]
		public SelectBuildingSignal selectBuildingSignal { get; set; }

		[Inject]
		public DeselectBuildingSignal deselectBuildingSignal { get; set; }

		[Inject]
		public MoveBuildingSignal moveBuildingSignal { get; set; }

		[Inject]
		public MoveScaffoldingSignal moveScaffoldingSignal { get; set; }

		[Inject]
		public StartTaskSignal startTaskSignal { get; set; }

		[Inject]
		public SignalActionSignal stopTaskSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal playLocalAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopLocalAudioSignal { get; set; }

		[Inject]
		public RevealBuildingSignal revealBuildingSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public CreateDummyBuildingSignal createDummyBuildingSignal { get; set; }

		[Inject]
		public ShowBuildingFootprintSignal showBuildingFootprintSignal { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public BuildingHarvestSignal buildingHarvestSignal { get; set; }

		[Inject]
		public UpdateTaskedMinionSignal updateTaskedMinionSignal { get; set; }

		[Inject]
		public StartConstructionSignal startConstructionSignal { get; set; }

		[Inject]
		public RestoreBuildingSignal restoreBuildingSignal { get; set; }

		[Inject]
		public RestoreScaffoldingViewSignal restoreScaffoldingViewSignal { get; set; }

		[Inject]
		public RestoreRibbonViewSignal restoreRibbonViewSignal { get; set; }

		[Inject]
		public RestorePlatformViewSignal restorePlatformViewSignal { get; set; }

		[Inject]
		public RestoreBuildingViewSignal restoreBuildingViewSignal { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public UITryHarvestSignal tryHarvestSignal { get; set; }

		[Inject]
		public TryHarvestBuildingSignal harvestBuildingSignal { get; set; }

		[Inject]
		public RepairBuildingSignal repairBuildingSignal { get; set; }

		[Inject]
		public RecreateBuildingSignal recreateBuildingSignal { get; set; }

		[Inject]
		public BuildingReactionSignal buildingReactionSignal { get; set; }

		[Inject]
		public MinionTaskCompleteSignal minionTaskCompleteSignal { get; set; }

		[Inject]
		public EjectMinionFromBuildingSignal ejectMinionFromBuildingSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal minionStateChange { get; set; }

		[Inject]
		public EjectAllMinionsFromBuildingSignal ejectAllMinionsFromBuildingSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PurchaseNewBuildingSignal purchaseNewBuildingSignal { get; set; }

		[Inject]
		public SendBuildingToInventorySignal sendBuildingToInventorySignal { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventoryBuildingSignal { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		[Inject]
		public SetBuildingPositionSignal setBuildingPositionSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public AddFootprintSignal addFootprintSignal { get; set; }

		[Inject]
		public SetupBrokenBridgesSignal setupBrokenBridgesSignal { get; set; }

		[Inject]
		public BurnLandExpansionSignal burnLandExpansionSignal { get; set; }

		[Inject]
		public SetupLandExpansionsSignal setupLandExpansionsSignal { get; set; }

		[Inject]
		public SetupDebrisSignal setupDebrisSignal { get; set; }

		[Inject]
		public SetupAspirationalBuildingsSignal setupAspirationalBuildingsSignal { get; set; }

		[Inject]
		public DisplayBuildingSignal displayBuildingSignal { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public BRBCelebrationAnimationSignal brbExitAnimimationSignal { get; set; }

		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		[Inject]
		public InitBuildingObjectSignal initBuildingObjectSignal { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public CleanupDebrisSignal cleanupDebrisSignal { get; set; }

		[Inject]
		public ToggleMinionRendererSignal toggleMinionSignal { get; set; }

		[Inject]
		public RelocateTaskedMinionsSignal relocateMinionsSignal { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreSignal { get; set; }

		[Inject]
		public HighlightBuildingSignal highlightBuildingSignal { get; set; }

		[Inject]
		public SetMinionPartyBuildingStateSignal setMinionPartyBuildingStateSignal { get; set; }

		[Inject]
		public PreLoadPartyAssetsSignal preLoadPartyAssetsSignal { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal playerTrainingSignal { get; set; }

		[Inject]
		public PrepareTaskingMinionsForPartySignal prepareTaskingMinionsForPartySignal { get; set; }

		[Inject]
		public RestoreTaskingMinionsFromPartySignal restoreTaskingMinionFromPartySignal { get; set; }

		[Inject]
		public EnableVillainIslandCollidersSignal enableVillainIslandCollidersSignal { get; set; }

		[Inject]
		public SetStorageMenuEnabledSignal setStorageMenuEnabledSignal { get; set; }

		[Inject]
		public PhilSignFixedSignal philSignFixedSignal { get; set; }

		[Inject]
		public StopGagAnimationSignal stopGagAnimationSignal { get; set; }

		[Inject]
		public ShowHarvestReadySignal showHarvestReadySignal { get; set; }

		[Inject]
		public ISpecialEventService specialEventService { get; set; }

		[Inject]
		public MasterPlanCompleteSignal masterPlanCompleteSignal { get; set; }

		[Inject]
		public RevealMasterPlanComponentSignal revealMasterPlanComponentSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideWayfinder { get; set; }

		[Inject]
		public SetMasterPlanWayfinderIconToCompleteSignal setCompleteWayfinderSignal { get; set; }

		[Inject]
		public DisplayVolcanoLairVillainWayfinderSignal displayVolcanoWayfinderSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public RelocateCharacterSignal relocateCharacterSignal { get; set; }

		[Inject]
		public DecoGridModel decoGridModel { get; set; }

		[Inject]
		public UpdateConnectablesSignal updateConnectablesSignal { get; set; }

		[Inject]
		public Scaffolding currentScaffolding { get; set; }

		public float HarvestTimer { get; set; }

		public int LastHarvestedBuildingID { get; set; }

		public override void OnRegister()
		{
			view.Init(logger, definitionService, masterPlanService, specialEventService.IsSpecialEventActive());
			view.updateMinionSignal.AddListener(UpdateTaskedMinions);
			view.addFootprintSignal.AddListener(AddFootprint);
			ManageRegisterStackSize();
			restoreBuildingViewSignal.AddListener(RestoreBuildingView);
			buildingChangeStateSignal.AddListener(UpdateViewFromBuildingState);
			triggerGagAnimation.AddListener(TriggerGagAnimation);
			repairBuildingSignal.AddListener(RepairBuilding);
			recreateBuildingSignal.AddListener(RecreateBuilding);
			displayBuildingSignal.AddListener(DisplayBuilding);
			tryHarvestSignal.AddListener(TryHarvest);
			deselectBuildingSignal.AddListener(DeselectBuilding);
			purchaseNewBuildingSignal.AddListener(PurchaseNewBuilding);
			sendBuildingToInventorySignal.AddListener(SendToInventory);
			createInventoryBuildingSignal.AddListener(CreateInventoryBuilding);
			cancelPurchaseSignal.AddListener(CancelPurchaseStart);
			setBuildingPositionSignal.AddListener(SetBuildingPosition);
			ejectAllMinionsFromBuildingSignal.AddListener(EjectAllMinionsFromBuilding);
			minionTaskingAnimationDone.AddListener(OnMinionTaskingAnimationDone);
			burnLandExpansionSignal.AddListener(OnBurnedLandExpansion);
			setMinionPartyBuildingStateSignal.AddListener(SetMinionPartyBuildingState);
			preLoadPartyAssetsSignal.AddListener(PreLoadMinionPartyBuildings);
			ejectMinionFromBuildingSignal.AddListener(EjectMinionFromBuilding);
			stopGagAnimationSignal.AddListener(StopGagAnimation);
			showHarvestReadySignal.AddListener(ShowHarvestReady);
			coroutineProgressMonitor.StartTask(Init(), "init buildings");
		}

		private void ManageRegisterStackSize()
		{
			view.updateResourceBuildingSignal.AddListener(VerifyResourceBuildingSlots);
			view.setBuildingNumberSignal.AddListener(SetBuildingNumber);
			setBuildMenuEnabledSignal.AddListener(AllowStorable);
			view.initBuildingObject.AddListener(InitBuildingObject);
			selectBuildingSignal.AddListener(SelectBuilding);
			moveBuildingSignal.AddListener(MoveBuilding);
			moveScaffoldingSignal.AddListener(MoveDummyBuildingObject);
			startTaskSignal.AddListener(StartMinionTask);
			revealBuildingSignal.AddListener(OnRevealBuilding);
			createDummyBuildingSignal.AddListener(CreateDummyBuilding);
			buildingHarvestSignal.AddListener(HarvestComplete);
			restoreScaffoldingViewSignal.AddListener(RestoreScaffoldingView);
			restoreRibbonViewSignal.AddListener(RestoreRibbonView);
			restorePlatformViewSignal.AddListener(RestorePlatformView);
			highlightBuildingSignal.AddListener(HighlightBuilding);
			prepareTaskingMinionsForPartySignal.AddListener(PrepareTaskingMinionForMinionParty);
			restoreTaskingMinionFromPartySignal.AddListener(RestoreTaskingMinionFromParty);
			setBuildingRushedSignal = uiContext.injectionBinder.GetInstance<SetBuildingRushedSignal>();
			rushRevealBuildingSignal = uiContext.injectionBinder.GetInstance<RushRevealBuildingSignal>();
			setBuildingRushedSignal.AddListener(SetBuildingRushed);
			rushRevealBuildingSignal.AddListener(RushRevealBuilding);
		}

		public override void OnRemove()
		{
			view.addFootprintSignal.RemoveListener(AddFootprint);
			ManageRemoveStackSize();
			buildingHarvestSignal.RemoveListener(HarvestComplete);
			restoreScaffoldingViewSignal.RemoveListener(RestoreScaffoldingView);
			restorePlatformViewSignal.RemoveListener(RestorePlatformView);
			restoreRibbonViewSignal.RemoveListener(RestoreRibbonView);
			restoreBuildingViewSignal.RemoveListener(RestoreBuildingView);
			buildingChangeStateSignal.RemoveListener(UpdateViewFromBuildingState);
			triggerGagAnimation.RemoveListener(TriggerGagAnimation);
			repairBuildingSignal.RemoveListener(RepairBuilding);
			recreateBuildingSignal.RemoveListener(RecreateBuilding);
			displayBuildingSignal.RemoveListener(DisplayBuilding);
			setBuildMenuEnabledSignal.RemoveListener(AllowStorable);
			tryHarvestSignal.RemoveListener(TryHarvest);
			deselectBuildingSignal.RemoveListener(DeselectBuilding);
			purchaseNewBuildingSignal.RemoveListener(PurchaseNewBuilding);
			sendBuildingToInventorySignal.RemoveListener(SendToInventory);
			createInventoryBuildingSignal.RemoveListener(CreateInventoryBuilding);
			cancelPurchaseSignal.RemoveListener(CancelPurchaseStart);
			setBuildingPositionSignal.RemoveListener(SetBuildingPosition);
			ejectAllMinionsFromBuildingSignal.RemoveListener(EjectAllMinionsFromBuilding);
			minionTaskingAnimationDone.RemoveListener(OnMinionTaskingAnimationDone);
			burnLandExpansionSignal.RemoveListener(OnBurnedLandExpansion);
			ejectMinionFromBuildingSignal.RemoveListener(EjectMinionFromBuilding);
			stopGagAnimationSignal.RemoveListener(StopGagAnimation);
			showHarvestReadySignal.RemoveListener(ShowHarvestReady);
			view.initBuildingObject.RemoveListener(InitBuildingObject);
		}

		private void ManageRemoveStackSize()
		{
			view.updateResourceBuildingSignal.RemoveListener(VerifyResourceBuildingSlots);
			view.setBuildingNumberSignal.RemoveListener(SetBuildingNumber);
			selectBuildingSignal.RemoveListener(SelectBuilding);
			moveBuildingSignal.RemoveListener(MoveBuilding);
			moveScaffoldingSignal.RemoveListener(MoveDummyBuildingObject);
			startTaskSignal.RemoveListener(StartMinionTask);
			revealBuildingSignal.RemoveListener(OnRevealBuilding);
			createDummyBuildingSignal.RemoveListener(CreateDummyBuilding);
			highlightBuildingSignal.RemoveListener(HighlightBuilding);
			prepareTaskingMinionsForPartySignal.RemoveListener(PrepareTaskingMinionForMinionParty);
			restoreTaskingMinionFromPartySignal.RemoveListener(RestoreTaskingMinionFromParty);
			setBuildingRushedSignal.RemoveListener(SetBuildingRushed);
			rushRevealBuildingSignal.RemoveListener(RushRevealBuilding);
			setMinionPartyBuildingStateSignal.RemoveListener(SetMinionPartyBuildingState);
			preLoadPartyAssetsSignal.RemoveListener(PreLoadMinionPartyBuildings);
		}

		private IEnumerator Init()
		{
			yield return null;
			TimeProfiler.StartSection("buildings");
			ICollection<Building> buildingList = playerService.GetInstancesByType<Building>();
			TimeProfiler.StartSection("restoring");
			Stopwatch sw = Stopwatch.StartNew();
			foreach (Building building in buildingList)
			{
				restoreBuildingSignal.Dispatch(building);
				if (sw.ElapsedMilliseconds > 1500)
				{
					sw.Reset();
					sw.Start();
					yield return null;
				}
			}
			sw.Stop();
			TimeProfiler.EndSection("restoring");
			yield return coroutineProgressMonitor.waitForPreviousTaskToComplete;
			TimeProfiler.StartSection("expansions");
			setupBrokenBridgesSignal.Dispatch();
			setupLandExpansionsSignal.Dispatch();
			setupDebrisSignal.Dispatch();
			TimeProfiler.EndSection("expansions");
			setupAspirationalBuildingsSignal.Dispatch();
			TimeProfiler.EndSection("buildings");
		}

		private void AllowStorable(bool storable)
		{
			allowStorable = storable;
		}

		private void InitBuildingObject(BuildingObject buildingObject, Dictionary<string, RuntimeAnimatorController> controllers, Building building)
		{
			initBuildingObjectSignal.Dispatch(buildingObject, controllers, building);
		}

		public void AddFootprint(Building building, Location location)
		{
			addFootprintSignal.Dispatch(building, location);
		}

		public void Update()
		{
			if (HarvestTimer > 0f)
			{
				HarvestTimer -= Time.deltaTime;
			}
			if (model.ForceDisabled)
			{
				DestroyDummyBuilding();
			}
		}

		private void InjectBuildingObject(GameObject go, int id)
		{
			switch (id)
			{
			case 3041:
				SetBuildingBinding(go, StaticItem.TIKI_BAR_BUILDING_ID_DEF);
				break;
			case 3070:
				SetBuildingBinding(go, StaticItem.WELCOME_BOOTH_BUILDING_ID_DEF);
				break;
			}
		}

		private void SetBuildingBinding(GameObject go, object name)
		{
			IInjectionBinding binding = injectionBinder.GetBinding<GameObject>(name);
			if (binding == null)
			{
				injectionBinder.Bind<GameObject>().ToValue(go).ToName(name);
			}
			else
			{
				binding.SetValue(go);
			}
		}

		private void RepairBuilding(Building building)
		{
			int iD = building.ID;
			BuildingState state = building.State;
			if (state != BuildingState.Broken && state != BuildingState.MissingTikiSign)
			{
				return;
			}
			HandleBuildingState(iD, state);
			RecreateBuilding(building);
			BuildingObject buildingObject = view.GetBuildingObject(iD);
			buildingObject.SetVFXState("RepairBuilding");
			StageBuilding stageBuilding = building as StageBuilding;
			WelcomeHutBuilding welcomeHutBuilding = building as WelcomeHutBuilding;
			CabanaBuilding cabanaBuilding = building as CabanaBuilding;
			FountainBuilding fountainBuilding = building as FountainBuilding;
			StorageBuilding storageBuilding = building as StorageBuilding;
			TikiBarBuilding tikiBarBuilding = building as TikiBarBuilding;
			VillainLairEntranceBuilding villainLairEntranceBuilding = building as VillainLairEntranceBuilding;
			MinionUpgradeBuilding minionUpgradeBuilding = building as MinionUpgradeBuilding;
			if (stageBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.StageRepair, QuestTaskTransition.Complete);
			}
			else if (welcomeHutBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.WelcomeHutRepair, QuestTaskTransition.Complete);
			}
			else if (villainLairEntranceBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.LairPortalRepair, QuestTaskTransition.Complete);
			}
			else if (fountainBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.FountainRepair, QuestTaskTransition.Complete);
			}
			else if (minionUpgradeBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.MinionUpgradeBuildingRepair, QuestTaskTransition.Complete);
			}
			else if (storageBuilding != null)
			{
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.StorageRepair, QuestTaskTransition.Complete);
				updateSoldItemsSignal.Dispatch(false);
				setStorageMenuEnabledSignal.Dispatch(true);
			}
			else if (cabanaBuilding != null)
			{
				Dictionary<int, IQuestController> questMap = questService.GetQuestMap();
				foreach (IQuestController value in questMap.Values)
				{
					if (value.State == QuestState.RunningTasks && value.IsTrackingThisBuilding(iD, QuestStepType.CabanaRepair))
					{
						value.UpdateTask(QuestStepType.CabanaRepair, QuestTaskTransition.Complete, building);
					}
				}
			}
			else if (tikiBarBuilding != null && state == BuildingState.MissingTikiSign)
			{
				StartCoroutine(WaitAFrame(delegate
				{
					philSignFixedSignal.Dispatch();
				}));
			}
			CheckForMinionParty(building);
		}

		private void HandleBuildingState(int buildingId, BuildingState buildingState)
		{
			globalAudioSignal.Dispatch("Play_building_repair_01");
			removeWayFinderSignal.Dispatch(buildingId);
			if (buildingState == BuildingState.MissingTikiSign)
			{
				buildingChangeStateSignal.Dispatch(buildingId, BuildingState.Idle);
			}
			else if (buildingId == 313)
			{
				buildingChangeStateSignal.Dispatch(buildingId, BuildingState.MissingTikiSign);
			}
			else
			{
				buildingChangeStateSignal.Dispatch(buildingId, BuildingState.Idle);
			}
		}

		private void CheckForMinionParty(Building building = null)
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance != null && minionPartyInstance.IsPartyHappening)
			{
				if (building != null && building.Definition is IMinionPartyBuilding)
				{
					view.StartBuildingMinionParty(building.ID, (IMinionPartyBuilding)building, building.Location, minionPartyInstance.PartyType);
				}
				else
				{
					SetMinionPartyBuildingState(true);
				}
			}
		}

		private void HighlightBuilding(int buildingId, bool highlight)
		{
			view.HighlightBuilding(buildingId, highlight);
		}

		private void RecreateBuilding(Building building)
		{
			int iD = building.ID;
			view.RemoveBuilding(iD);
			InjectBuildingObject(view.CreateBuilding(building), building.Definition.ID);
		}

		private void RestoreBuildingView(Building building)
		{
			logger.Debug("Restoring building with id: {0}, type: {1}, state: {2}", building.ID, building.GetType(), building.State);
			if (building is ConnectableBuilding)
			{
				RestoreConnectableDeco(building);
			}
			else
			{
				InjectBuildingObject(view.CreateBuilding(building), building.Definition.ID);
			}
			BuildingState state = building.State;
			if ((state == BuildingState.Construction || state == BuildingState.Complete || state == BuildingState.Inactive) && (building.Definition.ConstructionTime > 0 || building is MasterPlanComponentBuilding))
			{
				DisplayBuilding(false, building.ID);
			}
		}

		private void RestoreConnectableDeco(Building building)
		{
			ConnectableBuilding connectableBuilding = building as ConnectableBuilding;
			Location location = building.Location;
			ConnectableBuildingDefinition connectableBuildingDefinition = building.Definition as ConnectableBuildingDefinition;
			decoGridModel.AddDeco(location.x, location.y, connectableBuildingDefinition.connectableType);
			GameObject gameObject = view.CreateBuilding(building, (int)connectableBuilding.pieceType);
			BuildingObject component = gameObject.GetComponent<BuildingObject>();
			component.transform.localEulerAngles = new Vector3(0f, connectableBuilding.rotation, 0f);
		}

		private void PrepareTaskingMinionForMinionParty()
		{
			MoveMinionInTaskingBuildingForParty(true);
		}

		private void RestoreTaskingMinionFromParty()
		{
			MoveMinionInTaskingBuildingForParty(false);
		}

		private void MoveMinionInTaskingBuildingForParty(bool moveOut)
		{
			List<TaskableBuilding> instancesByType = playerService.GetInstancesByType<TaskableBuilding>();
			foreach (TaskableBuilding item in instancesByType)
			{
				BuildingState state = item.State;
				IList<int> minionList = item.MinionList;
				if ((state != BuildingState.Working && state != BuildingState.HarvestableAndWorking && state != BuildingState.Harvestable) || minionList.Count <= 0)
				{
					continue;
				}
				if (moveOut)
				{
					view.PrepareTaskingMinionForMinionParty(item);
					if (item is DebrisBuilding)
					{
						cleanupDebrisSignal.Dispatch(item.ID, false);
						timeEventService.RemoveEvent(item.ID);
					}
					foreach (int item2 in minionList)
					{
						minionStateChange.Dispatch(item2, MinionState.Idle);
					}
					continue;
				}
				buildingChangeStateSignal.Dispatch(buildingID, BuildingState.Working);
				foreach (int item3 in minionList)
				{
					Minion byInstanceId = playerService.GetByInstanceId<Minion>(item3);
					byInstanceId.IsInMinionParty = false;
					GameObject gameObject = minionManager.GetComponent<MinionManagerView>().GetGameObject(item3);
					view.StartMinionTask(item, gameObject.GetComponent<MinionObject>(), false);
					minionStateChange.Dispatch(item3, MinionState.Tasking);
				}
			}
		}

		private void PurchaseNewBuilding(Building building)
		{
			DestroyDummyBuilding();
			BuildingDefinition definition = building.Definition;
			Location location = building.Location;
			bool flag = building is MasterPlanComponentBuilding;
			ConnectableBuilding connectableBuilding = building as ConnectableBuilding;
			int prefabIndex = 0;
			int outRotation = 0;
			if (connectableBuilding != null)
			{
				prefabIndex = InitializeConnectableBuilding(connectableBuilding, out outRotation);
			}
			Vector3 position = new Vector3(location.x, 0f, location.y);
			int iD = building.ID;
			GameObject gameObject = view.CreateBuilding(building, prefabIndex);
			BuildingObject component = gameObject.GetComponent<BuildingObject>();
			if (connectableBuilding != null)
			{
				component.transform.localEulerAngles = new Vector3(0f, outRotation, 0f);
			}
			component.ID = iD;
			InjectBuildingObject(gameObject, definition.ID);
			if (definition.ConstructionTime > 0 || flag)
			{
				DisplayBuilding(false, iD);
				if (!flag)
				{
					view.CreatePlatformBuildingObject(building, position);
				}
				ScaffoldingBuildingObject scaffoldingBuildingObject = view.CreateScaffoldingBuildingObject(building, position);
				if (masterPlanService.CurrentMasterPlan != null && definition.ID == masterPlanService.CurrentMasterPlan.Definition.BuildingDefID)
				{
					playLocalAudioSignal.Dispatch(GetAudioEmitter.Get(scaffoldingBuildingObject.gameObject, "partile"), "Play_crate_sparkle_01", null);
				}
				GrowScaffolding(component, true);
				if (definition.Type != BuildingType.BuildingTypeIdentifier.MASTER_COMPONENT)
				{
					TriggerVFX(position, "FX_Drop_Prefab", definition);
				}
			}
			else
			{
				GrowScaffolding(component, false);
			}
		}

		private int InitializeConnectableBuilding(ConnectableBuilding building, out int outRotation)
		{
			Location location = building.Location;
			ConnectableBuildingDefinition definition = building.Definition;
			ConnectableBuildingPieceType connectablePieceType = decoGridModel.GetConnectablePieceType(location.x, location.y, definition.connectableType, out outRotation);
			building.pieceType = connectablePieceType;
			building.rotation = outRotation;
			decoGridModel.AddDeco(location.x, location.y, definition.connectableType);
			updateConnectablesSignal.Dispatch(location, definition.connectableType);
			return (int)connectablePieceType;
		}

		private void GrowScaffolding(BuildingObject building, bool isScaffoldingPrefab)
		{
			if (building != null)
			{
				if (isScaffoldingPrefab)
				{
					PlaySFX(building.ID, "Play_scaffold_construction_01", true);
				}
				else
				{
					PlaySFX(building.ID, "Play_prop_land_01", true);
				}
				if (!(building is ConnectableBuildingObject))
				{
					Vector3 center = building.Center;
					Boxed<Vector3> type = new Boxed<Vector3>(new Vector3(center.x, 0f, center.z));
					buildingReactionSignal.Dispatch(type, true);
				}
				startConstructionSignal.Dispatch(building.ID, false);
			}
		}

		private void SetBuildingRushed(int buildingId)
		{
			view.SetBuildingRushed(buildingId);
		}

		private void RushRevealBuilding(int buildingId)
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingId);
			if (byInstanceId != null)
			{
				RevealBuilding(byInstanceId);
			}
		}

		private void OnRevealBuilding(Building building)
		{
			RevealBuilding(building);
		}

		private void PlaySFX(int buildingId, string sfxName, bool enable)
		{
			BuildingObject buildingObject = view.GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				if (enable)
				{
					stopLocalAudioSignal.Dispatch(buildingObject.localAudioEmitter);
					playLocalAudioSignal.Dispatch(buildingObject.localAudioEmitter, sfxName, null);
				}
				else
				{
					stopLocalAudioSignal.Dispatch(buildingObject.localAudioEmitter);
				}
			}
		}

		private void RestoreScaffoldingView(Building building, bool restoreTimer)
		{
			if (building.Definition.ConstructionTime <= 0 && !(building is MasterPlanComponentBuilding))
			{
				return;
			}
			logger.Debug("Restoring scaffolding for building id: {0} type: {1}", building.ID, building.GetType());
			view.CreateScaffoldingBuildingObject(building, new Vector3(building.Location.x, 0f, building.Location.y));
			if (restoreTimer)
			{
				StartCoroutine(WaitAFrame(delegate
				{
					startConstructionSignal.Dispatch(building.ID, true);
				}));
			}
		}

		private void RestorePlatformView(Building building)
		{
			if (building.Definition.ConstructionTime > 0)
			{
				logger.Debug("Restoring platform for building id: {0} type: {1}", building.ID, building.GetType());
				view.CreatePlatformBuildingObject(building, new Vector3(building.Location.x, 0f, building.Location.y));
			}
		}

		private void RestoreRibbonView(Building building)
		{
			if (building.Definition.ConstructionTime > 0)
			{
				logger.Debug("Restoring ribbon for building id: {0} type: {1}", building.ID, building.GetType());
				view.CreateRibbonBuildingObject(building, new Vector3(building.Location.x, 0f, building.Location.y));
				PlaceWayFinderOnBuilding(building);
			}
		}

		private void CreateInventoryBuilding(Building building, Location location)
		{
			DestroyDummyBuilding();
			GameObject gameObject = view.CreateBuilding(building);
			InjectBuildingObject(gameObject, building.Definition.ID);
			gameObject.transform.position = new Vector3(location.x, 0f, location.y);
			BuildingObject component = gameObject.GetComponent<BuildingObject>();
			if (component != null && !(component is ConnectableBuildingObject))
			{
				Vector3 center = component.Center;
				Boxed<Vector3> type = new Boxed<Vector3>(new Vector3(center.x, 0f, center.z));
				buildingReactionSignal.Dispatch(type, true);
			}
			if (model.CurrentMode != PickControllerModel.Mode.DragAndDrop)
			{
				model.SelectedBuilding = null;
			}
		}

		private void RevealBuilding(Building building)
		{
			BuildingState state = building.State;
			int iD = building.ID;
			if (state != BuildingState.Complete)
			{
				logger.Info("Can't reveal building id:{0} when construction is not complete!", iD);
				return;
			}
			CheckForMasterPlan(building);
			Location location = building.Location;
			buildingChangeStateSignal.Dispatch(iD, BuildingState.Idle);
			RemoveWayFinderFromBuilding(building);
			view.RemoveAllScaffoldingParts(iD);
			BuildingObject buildingObject = view.GetBuildingObject(iD);
			DisplayBuilding(true, buildingObject.ID);
			Vector3 center = buildingObject.Center;
			AnimateRevealBuilding(buildingObject);
			BuildingDefinition definition = building.Definition;
			string prefabName = ((!string.IsNullOrEmpty(definition.RevealVFX)) ? definition.RevealVFX : "FX_Reveal_Prefab");
			TriggerVFX(new Vector3(center.x, 0f, center.z), prefabName, definition);
			if (definition.RewardTransactionId != 0)
			{
				TransactionArg transactionArg = new TransactionArg();
				transactionArg.Source = definition.TaxonomyType;
				playerService.RunEntireTransaction(definition.RewardTransactionId, TransactionTarget.REWARD_BUILDING, null, transactionArg);
				uiContext.injectionBinder.GetInstance<SpawnDooberSignal>().Dispatch(center, DestinationType.XP, -1, true);
			}
			Boxed<Vector3> type = new Boxed<Vector3>(new Vector3(center.x, 0f, center.z));
			buildingReactionSignal.Dispatch(type, true);
			globalAudioSignal.Dispatch("Play_building_repair_01");
			if (!(building is MasterPlanComponentBuilding))
			{
				addFootprintSignal.Dispatch(building, location);
			}
			else
			{
				DispatchComponentSignals(definition);
			}
			ShowRevealVFX(definition, iD);
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Construction);
			if (definition.PlayerTrainingDefinitionID > 0)
			{
				playerTrainingSignal.Dispatch(definition.PlayerTrainingDefinitionID, false, new Signal<bool>());
			}
		}

		private void DispatchComponentSignals(BuildingDefinition def)
		{
			revealMasterPlanComponentSignal.Dispatch(def.ID);
			displayVolcanoWayfinderSignal.Dispatch();
			hideWayfinder.Dispatch(false);
			setCompleteWayfinderSignal.Dispatch();
		}

		private void ShowRevealVFX(BuildingDefinition def, int buildingId)
		{
			if (masterPlanService.CurrentMasterPlan != null && def.ID == masterPlanService.CurrentMasterPlan.Definition.BuildingDefID)
			{
				PlaySFX(buildingId, "Play_villainLair_scaffoldReveal_01", true);
			}
			else
			{
				PlaySFX(buildingId, "Play_scaffold_disappear_01", true);
			}
		}

		private void CheckForMasterPlan(Building building)
		{
			List<MasterPlanDefinition> all = definitionService.GetAll<MasterPlanDefinition>();
			for (int i = 0; i < all.Count; i++)
			{
				if (all[i].BuildingDefID == building.Definition.ID)
				{
					masterPlanCompleteSignal.Dispatch(all[i]);
					break;
				}
			}
		}

		private void AnimateRevealBuilding(BuildingObject buildingObject)
		{
			buildingObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			Go.to(buildingObject.transform, 0.5f, new GoTweenConfig().scale(new Vector3(1f, 1f, 1f)).setEaseType(GoEaseType.BackOut).onComplete(delegate(AbstractGoTween tween)
			{
				tween.destroy();
			})).play();
		}

		private void RemoveWayFinderFromBuilding(Building building)
		{
			removeWayFinderSignal.Dispatch(building.ID);
		}

		private void SelectBuilding(int buildingId, string footprint)
		{
			view.SelectBuilding(buildingId);
			ToggleTaskableMinions(buildingId, false);
			globalAudioSignal.Dispatch("Play_building select_01");
			disableCameraSignal.Dispatch(1);
			enableCameraSignal.Dispatch(8);
			BuildingObject buildingObject = view.GetBuildingObject(buildingId);
			if (!(buildingObject == null))
			{
				showBuildingFootprintSignal.Dispatch(buildingObject, buildingObject.transform, Tuple.Create(buildingObject.Width, buildingObject.Depth), true);
				Building byInstanceId = playerService.GetByInstanceId<Building>(buildingId);
				GetUISignal<ShowActivitySpinnerSignal>().Dispatch(false, Vector3.zero);
				model.activitySpinnerExists = false;
				if (!byInstanceId.Definition.Storable || !allowStorable)
				{
					GetUISignal<ShowMoveBuildingMenuSignal>().Dispatch(true, new MoveBuildingSetting(byInstanceId.ID, 1, false, false));
				}
				else
				{
					GetUISignal<ShowMoveBuildingMenuSignal>().Dispatch(true, new MoveBuildingSetting(byInstanceId.ID, 16, false, false));
				}
				GetUISignal<ShowHUDSignal>().Dispatch(false);
				GetUISignal<ShowWorldCanvasSignal>().Dispatch(false);
				GetUISignal<HideAllResourceIconsSignal>().Dispatch();
				GetUISignal<ToggleAllFloatingTextSignal>().Dispatch(false);
				enableVillainIslandCollidersSignal.Dispatch(false);
			}
		}

		private void DeselectBuilding(int buildingId)
		{
			view.DeselectBuilding(buildingId);
			ToggleTaskableMinions(buildingId, true);
			disableCameraSignal.Dispatch(8);
			enableCameraSignal.Dispatch(1);
			HideFootprint();
			BuildingDefinition definition = currentScaffolding.Definition;
			if (buildingId == -1 && definition != null)
			{
				DecorationBuildingDefinition decorationBuildingDefinition = definition as DecorationBuildingDefinition;
				bool flag = playerService.CheckIfBuildingIsCapped(definition.ID);
				bool flag2 = decorationBuildingDefinition != null && decorationBuildingDefinition.AutoPlace && !flag;
				if (definition.Type != BuildingType.BuildingTypeIdentifier.CONNECTABLE && !flag2)
				{
					HideBuildingPlacementMenu(buildingId);
				}
			}
			else
			{
				HideBuildingPlacementMenu(buildingId);
			}
			enableVillainIslandCollidersSignal.Dispatch(true);
		}

		private void ToggleTaskableMinions(int buildingID, bool enableRenderers)
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			TaskableBuilding taskableBuilding = byInstanceId as TaskableBuilding;
			LeisureBuilding leisureBuilding = byInstanceId as LeisureBuilding;
			IList<int> list3;
			if (taskableBuilding == null)
			{
				IList<int> list2;
				IList<int> list;
				if (leisureBuilding == null)
				{
					list = null;
					list2 = list;
				}
				else
				{
					list2 = leisureBuilding.MinionList;
				}
				list = list2;
				list3 = list;
			}
			else
			{
				list3 = taskableBuilding.MinionList;
			}
			IList<int> list4 = list3;
			if (list4 == null)
			{
				return;
			}
			foreach (int item in list4)
			{
				toggleMinionSignal.Dispatch(item, enableRenderers);
			}
			if (enableRenderers)
			{
				relocateMinionsSignal.Dispatch(byInstanceId);
			}
		}

		private void DisplayBuilding(bool isVisible, int buildingId)
		{
			BuildingObject buildingObject = view.GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				buildingObject.gameObject.SetActive(isVisible);
			}
		}

		private void MoveBuilding(int buildingId, Vector3 position, bool isValidPosition)
		{
			BuildingObject buildingObject = view.GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				PlayAudioOnMove(buildingObject.transform.position, position, isValidPosition);
			}
			view.MoveBuilding(buildingId, position, isValidPosition);
		}

		private void MoveDummyBuildingObject(Vector3 position, bool isValidPosition)
		{
			if (currentDummyBuilding != null)
			{
				position = new Vector3(Mathf.Round(position.x), currentDummyBuilding.transform.position.y, Mathf.Round(position.z));
				PlayAudioOnMove(currentDummyBuilding.transform.position, position, isValidPosition);
				currentDummyBuilding.transform.position = position;
				currentDummyBuilding.SetBlendedColor((!isValidPosition) ? GameConstants.Building.INVALID_PLACEMENT_COLOR : GameConstants.Building.VALID_PLACEMENT_COLOR);
			}
		}

		private void PlayAudioOnMove(Vector3 oldPosition, Vector3 newPosition, bool isValidPosition)
		{
			Vector3 vector = new Vector3(Mathf.Round(oldPosition.x), oldPosition.y, Mathf.Round(oldPosition.z));
			Vector3 vector2 = new Vector3(Mathf.Round(newPosition.x), newPosition.y, Mathf.Round(newPosition.z));
			if (vector2 != vector)
			{
				if (isValidPosition)
				{
					globalAudioSignal.Dispatch("Play_click_snap_01");
				}
				else
				{
					globalAudioSignal.Dispatch("Play_error_button_01");
				}
			}
		}

		private void StartMinionTask(MinionObject minion, Building building)
		{
			int iD = building.ID;
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			if (taskableBuilding != null)
			{
				bool alreadyRushed = minion.GetMinion().AlreadyRushed;
				buildingChangeStateSignal.Dispatch(iD, BuildingState.Working);
				view.StartMinionTask(taskableBuilding, minion, alreadyRushed);
				if (taskableBuilding is TikiBarBuilding)
				{
					Prestige prestigeFromMinionInstance = characterService.GetPrestigeFromMinionInstance(minion.GetMinion());
					characterService.ChangeToPrestigeState(prestigeFromMinionInstance, PrestigeState.Questing);
					return;
				}
				MignetteBuilding mignetteBuilding = taskableBuilding as MignetteBuilding;
				if (mignetteBuilding == null)
				{
					PlaceMinionInBuilding(taskableBuilding, minion.GetMinion());
				}
				if (alreadyRushed)
				{
					return;
				}
				if (mignetteBuilding != null)
				{
					if (!mignetteBuilding.AreAllMinionSlotsFilled())
					{
						return;
					}
					BuildingObject buildingObject = view.GetBuildingObject(iD);
					if (buildingObject != null)
					{
						MignetteBuildingObject mignetteBuildingObject = buildingObject as MignetteBuildingObject;
						if (mignetteBuildingObject != null && mignetteBuildingObject.GetMignetteMinionCount() == mignetteBuilding.GetMinionSlotsOwned())
						{
							MoveCameraAndFocusOnBuilding(mignetteBuilding, true);
						}
					}
				}
				else if (taskableBuilding is DebrisBuilding)
				{
					cleanupDebrisSignal.Dispatch(iD, true);
					view.AppendMinionTaskAnimationCompleteCallback(minion, minionTaskingAnimationDone);
				}
				else
				{
					if (!taskableBuilding.IsEligibleForGag())
					{
						return;
					}
					int num = iD;
					int nextGagPlayTime = taskableBuilding.GetNextGagPlayTime(timeService.CurrentTime());
					if (timeEventService.GetEventDuration(num) == 0)
					{
						if (nextGagPlayTime > 0)
						{
							timeEventService.AddEvent(num, timeService.CurrentTime(), nextGagPlayTime, triggerGagAnimation);
						}
						else
						{
							TriggerGagAnimation(num);
						}
					}
				}
			}
			else
			{
				logger.Fatal(FatalCode.BV_NOT_A_TASKING_BUILDING);
			}
		}

		private void PlaceMinionInBuilding(TaskableBuilding taskableBuilding, Minion minion)
		{
			IList<int> minionList = taskableBuilding.MinionList;
			if (minionList.Count <= 1)
			{
				return;
			}
			minionList.Remove(minion.ID);
			bool flag = false;
			for (int i = 0; i < minionList.Count; i++)
			{
				int id = minionList[i];
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(id);
				if (byInstanceId.UTCTaskStartTime > minion.UTCTaskStartTime)
				{
					minionList.Insert(i, minion.ID);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				minionList.Add(minion.ID);
			}
		}

		private void TriggerGagAnimation(int buildingId)
		{
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(buildingId);
			int utcTime = timeService.CurrentTime();
			if (byInstanceId.IsEligibleForGag() && byInstanceId.GetNextGagPlayTime(utcTime) == 0 && view.TriggerGagAnimation(buildingId))
			{
				byInstanceId.GagPlayed(utcTime);
				if (byInstanceId.IsEligibleForGag())
				{
					timeEventService.AddEvent(buildingId, timeService.CurrentTime(), byInstanceId.GetNextGagPlayTime(utcTime), triggerGagAnimation);
				}
			}
		}

		private void MoveCameraAndFocusOnBuilding(TaskableBuilding building, bool showModalOnArrive = false)
		{
			showHiddenBuildingsSignal.Dispatch();
			BuildingObject buildingObject = view.GetBuildingObject(building.ID);
			Vector3 center = buildingObject.Center;
			MignetteBuildingObject mignetteBuildingObject = buildingObject as MignetteBuildingObject;
			AlligatorSkiingBuildingViewObject alligatorSkiingBuildingViewObject = null;
			if (mignetteBuildingObject != null)
			{
				alligatorSkiingBuildingViewObject = mignetteBuildingObject.GetComponent<AlligatorSkiingBuildingViewObject>();
			}
			ScreenPosition screenPosition = new ScreenPosition();
			screenPosition = screenPosition.Clone(building.Definition.ScreenPosition);
			if (alligatorSkiingBuildingViewObject != null)
			{
				screenPosition.x = 0.5f;
				screenPosition.z = 0.5f;
				screenPosition.zoom = 0.484f;
			}
			if (screenPosition == null || !(center != Vector3.zero))
			{
				return;
			}
			int num = building.Definition.GagID;
			if (num == 0)
			{
				num = -1;
			}
			if (num != -1)
			{
				if (playerService.GetByDefinitionId<Item>(num).Count > 0)
				{
					return;
				}
				Item i = new Item(definitionService.Get<ItemDefinition>(num));
				playerService.Add(i);
			}
			CameraMovementSettings.Settings settings = (showModalOnArrive ? CameraMovementSettings.Settings.ShowMenu : CameraMovementSettings.Settings.None);
			autoMoveSignal.Dispatch(center, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(settings, building, null), false);
		}

		private void StopGagAnimation(int buildingId)
		{
			if (view.IsGagAnimationPlaying(buildingId))
			{
				view.StopGagAnimation(buildingId);
			}
		}

		private void ShowHarvestReady(Tuple<int, int> ids)
		{
			view.HarvestReady(ids.Item1, ids.Item2);
		}

		private void EjectAllMinionsFromBuilding(int buildingID)
		{
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(buildingID);
			if (byInstanceId != null)
			{
				int minionsInBuilding = byInstanceId.GetMinionsInBuilding();
				for (int i = 0; i < minionsInBuilding; i++)
				{
					int minionByIndex = byInstanceId.GetMinionByIndex(0);
					EjectMinionFromBuilding(byInstanceId, minionByIndex);
				}
			}
		}

		private void HarvestComplete(int buildingID)
		{
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(buildingID);
			if (byInstanceId != null)
			{
				ResourceBuilding resourceBuilding = byInstanceId as ResourceBuilding;
				if (resourceBuilding != null)
				{
					resourceBuilding.CompleteHarvest();
					return;
				}
				int minionID = byInstanceId.HarvestFromCompleteMinions();
				EjectMinionFromBuilding(byInstanceId, minionID);
			}
		}

		private void EjectMinionFromBuilding(TaskableBuilding taskableBuilding, int minionID)
		{
			int iD = taskableBuilding.ID;
			view.UntrackMinion(iD, minionID, taskableBuilding);
			playerService.StopTask(minionID);
			stopTaskSignal.Dispatch(minionID);
			if (taskableBuilding is ResourceBuilding)
			{
				brbExitAnimimationSignal.Dispatch(minionID);
			}
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			MinionObject minionObject = component.Get(minionID);
			minionObject.gameObject.SetLayerRecursively(8);
			if (minionObject.currentAction is ConstantSpeedPathAction || minionObject.currentAction is RotateAction || minionObject.currentAction is SetAnimatorAction)
			{
				minionObject.ClearActionQueue();
				minionStateChange.Dispatch(minionID, MinionState.Idle);
			}
			else
			{
				relocateCharacterSignal.Dispatch(minionObject);
			}
			if (taskableBuilding.GetNumCompleteMinions() != 0 || taskableBuilding.Definition.Type == BuildingType.BuildingTypeIdentifier.RESOURCE)
			{
				return;
			}
			if (taskableBuilding.GetMinionsInBuilding() == 0)
			{
				if (taskableBuilding.Definition is MignetteBuildingDefinition)
				{
					buildingChangeStateSignal.Dispatch(iD, BuildingState.Cooldown);
				}
				else
				{
					buildingChangeStateSignal.Dispatch(iD, BuildingState.Idle);
				}
			}
			else
			{
				buildingChangeStateSignal.Dispatch(iD, BuildingState.Working);
			}
		}

		private void UpdateViewFromBuildingState(int buildingId, BuildingState buildingState)
		{
			view.UpdateBuildingState(buildingId, buildingState);
			if (buildingState != BuildingState.Complete)
			{
				return;
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingId);
			if (byInstanceId.Definition.ConstructionTime > 0)
			{
				Location location = byInstanceId.Location;
				Vector3 position = new Vector3(location.x, 0f, location.y);
				if (!view.IsBuildingRushed(buildingId))
				{
					PlaySFX(buildingId, null, false);
					view.CreateRibbonBuildingObject(byInstanceId, position);
					TriggerVFX(position, "FX_Bow_Prefab", byInstanceId.Definition);
					PlaceWayFinderOnBuilding(byInstanceId);
				}
			}
		}

		private void AdjustVFXPosition(BuildingDefinition buildingDef, Transform transform)
		{
			if (view.Is8x8Building(buildingDef))
			{
				transform.localPosition += new Vector3(1f, 0f, -1f);
			}
		}

		private void TriggerVFX(Vector3 position, string prefabName, BuildingDefinition buildingDef)
		{
			GameObject gameObject = KampaiResources.Load<GameObject>(prefabName);
			if (!(gameObject != null))
			{
				return;
			}
			GameObject vfxInstance = UnityEngine.Object.Instantiate(gameObject);
			Transform transform = vfxInstance.transform;
			transform.position = position;
			AdjustVFXPosition(buildingDef, transform);
			float num = 0f;
			foreach (Transform item in transform)
			{
				ParticleSystem component = item.GetComponent<ParticleSystem>();
				if (component != null && component.duration > num)
				{
					num = component.duration;
				}
				component.Play();
			}
			StartCoroutine(WaitSomeTime(num, delegate
			{
				UnityEngine.Object.Destroy(vfxInstance);
			}));
		}

		private void PlaceWayFinderOnBuilding(Building building)
		{
			WayFinderSettings type = new WayFinderSettings(building.ID);
			createWayFinderSignal.Dispatch(type);
		}

		private void UpdateTaskedMinions(int minionID, MinionTaskInfo taskInfo)
		{
			updateTaskedMinionSignal.Dispatch(minionID, taskInfo);
		}

		private void CreateDummyBuilding(BuildingDefinition buildingDefinition, Vector3 position, bool isInInventory)
		{
			buildingID = buildingDefinition.ID;
			currentDummyBuilding = view.CreateDummyBuilding(buildingDefinition, position);
			showBuildingFootprintSignal.Dispatch(currentDummyBuilding, currentDummyBuilding.transform, Tuple.Create(currentDummyBuilding.Width, currentDummyBuilding.Depth), true);
			showHUDSignal.Dispatch(false);
			hideAllWayFindersSignal.Dispatch();
		}

		public DummyBuildingObject GetCurrentDummyBuilding()
		{
			return currentDummyBuilding;
		}

		private void SetBuildingPosition(int buildingId, Vector3 position)
		{
			view.SetBuildingPosition(buildingId, position);
		}

		private void CancelPurchaseStart(bool invalidLocation)
		{
			if (currentDummyBuilding != null)
			{
				HideFootprint();
				openStoreSignal.Dispatch(buildingID, true);
				Vector3 destination = BuildingUtil.UIToWorldCoords(Camera.main, model.LastBuildingStorePosition);
				view.TweenBuildingToMenu(currentDummyBuilding.gameObject, destination, CancelPurchaseEnd);
				if (invalidLocation)
				{
					string @string = localService.GetString("InvalidLocation");
					popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
				}
			}
			GetUISignal<ShowMoveBuildingMenuSignal>().Dispatch(false, new MoveBuildingSetting(-1, 0, false, false));
			showHUDSignal.Dispatch(true);
			showAllWayFindersSignal.Dispatch();
		}

		private void CancelPurchaseEnd(GameObject lastScaffolding)
		{
			UnityEngine.Object.Destroy(lastScaffolding);
			lastScaffolding = null;
		}

		private void SendToInventory(int buildingId)
		{
			HideFootprint();
			ConnectableBuilding byInstanceId = playerService.GetByInstanceId<ConnectableBuilding>(buildingId);
			if (byInstanceId != null)
			{
				ConnectableBuildingDefinition definition = byInstanceId.Definition;
				Location location = byInstanceId.Location;
				decoGridModel.RemoveDeco(location.x, location.y);
				updateConnectablesSignal.Dispatch(location, definition.connectableType);
			}
			view.ToInventory(buildingId);
			HideBuildingPlacementMenu(buildingId);
		}

		private void HideBuildingPlacementMenu(int buildingId)
		{
			GetUISignal<ShowMoveBuildingMenuSignal>().Dispatch(false, new MoveBuildingSetting(buildingId, 0, false, false));
			GetUISignal<ShowHUDSignal>().Dispatch(true);
			GetUISignal<ShowStoreSignal>().Dispatch(true);
			GetUISignal<ShowWorldCanvasSignal>().Dispatch(true);
			GetUISignal<ShowAllResourceIconsSignal>().Dispatch();
			GetUISignal<ToggleAllFloatingTextSignal>().Dispatch(true);
		}

		private void HideFootprint()
		{
			showBuildingFootprintSignal.Dispatch(null, null, Tuple.Create(1, 1), false);
		}

		private T GetUISignal<T>()
		{
			return uiContext.injectionBinder.GetInstance<T>();
		}

		private IEnumerator WaitAFrame(Action a)
		{
			yield return null;
			a();
		}

		private IEnumerator WaitSomeTime(float delayTime, Action a)
		{
			yield return new WaitForSeconds(delayTime);
			a();
		}

		private void OnMinionTaskingAnimationDone(int minionId)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionId);
			if (byInstanceId != null)
			{
				int num = byInstanceId.BuildingID;
				TaskableBuilding byInstanceId2 = playerService.GetByInstanceId<TaskableBuilding>(num);
				if (byInstanceId2 is DebrisBuilding)
				{
					minionTaskCompleteSignal.Dispatch(minionId);
					cleanupDebrisSignal.Dispatch(num, false);
				}
			}
		}

		private void OnBurnedLandExpansion(int buildingId)
		{
			LandExpansionBuilding buildingByInstanceID = landExpansionService.GetBuildingByInstanceID(buildingId);
			view.RemoveBuilding(buildingByInstanceID.ID);
		}

		private void VerifyResourceBuildingSlots(Building building)
		{
			ResourceBuilding resourceBuilding = building as ResourceBuilding;
			int num = 0;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			SetBuildingNumber(building);
			int num2 = resourceBuilding.BuildingNumber - 1;
			int count = resourceBuilding.Definition.SlotUnlocks.Count;
			num2 = ((num2 <= count - 1) ? num2 : (count - 1));
			foreach (int slotUnlockLevel in resourceBuilding.Definition.SlotUnlocks[num2].SlotUnlockLevels)
			{
				if (slotUnlockLevel <= quantity)
				{
					num++;
				}
			}
			if (num > resourceBuilding.MinionSlotsOwned)
			{
				resourceBuilding.MinionSlotsOwned = num;
			}
		}

		private void SetBuildingNumber(Building building)
		{
			if (building.BuildingNumber == 0)
			{
				building.BuildingNumber = playerService.GetInstancesByDefinitionID(building.Definition.ID).Count;
			}
		}

		private void TryHarvest(int buildingID, Action callback, bool fromUI)
		{
			harvestBuildingSignal.Dispatch(view.GetBuildingObject(buildingID), callback, fromUI);
		}

		private void PreLoadMinionPartyBuildings()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			ICollection<MinionPartyBuilding> instancesByType = playerService.GetInstancesByType<MinionPartyBuilding>();
			foreach (MinionPartyBuilding item in instancesByType)
			{
				if (item.IsBuildingRepaired())
				{
					view.PreloadBuildingMinionParty(item.ID, item, item.Location, minionPartyInstance.PartyType);
				}
			}
		}

		private void SetMinionPartyBuildingState(bool enabled)
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			ICollection<IMinionPartyBuilding> instancesByType = playerService.GetInstancesByType<IMinionPartyBuilding>();
			if (enabled)
			{
				foreach (IMinionPartyBuilding item in instancesByType)
				{
					RepairableBuilding repairableBuilding = (RepairableBuilding)item;
					if (repairableBuilding.IsBuildingRepaired())
					{
						view.StartBuildingMinionParty(repairableBuilding.ID, item, repairableBuilding.Location, minionPartyInstance.PartyType);
					}
				}
				return;
			}
			foreach (IMinionPartyBuilding item2 in instancesByType)
			{
				view.EndBuildingMinionParty(item2.ID);
			}
		}

		private void DestroyDummyBuilding()
		{
			if (currentDummyBuilding != null)
			{
				UnityEngine.Object.Destroy(currentDummyBuilding.gameObject);
			}
		}
	}
}
