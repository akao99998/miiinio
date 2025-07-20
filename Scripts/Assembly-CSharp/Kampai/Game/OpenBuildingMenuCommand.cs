using System.Collections;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Mignette;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class OpenBuildingMenuCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("OpenBuildingMenuCommand") as IKampaiLogger;

		[Inject]
		public BuildingObject BuildingObject { get; set; }

		[Inject]
		public Building Building { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject BuildingManager { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public CameraAutoMoveSignal AutoMoveSignal { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public ShowBuildingDetailMenuSignal ShowBuildingDetailmenuSignal { get; set; }

		[Inject]
		public OpenStageBuildingSignal openStageBuildingSignal { get; set; }

		[Inject]
		public ShowBridgeUISignal BridgeSignal { get; set; }

		[Inject]
		public ShowNeedXMinionsSignal ShowNeedXMinionsSignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal OpenStorageBuildingSignal { get; set; }

		[Inject]
		public OpenVillainLairPortalBuildingSignal openVillainLairPortalSignal { get; set; }

		[Inject]
		public OpenMinionUpgradeBuildingSignal openMinionUpgradeBuildingSignal { get; set; }

		[Inject]
		public BuildingZoomSignal BuildingZoomSignal { get; set; }

		[Inject]
		public MignetteGameModel MignetteGameModel { get; set; }

		[Inject]
		public SpawnDooberModel SpawnDooberModel { get; set; }

		[Inject]
		public MIBBuildingSelectedSignal MessageInABottleSelectedSignal { get; set; }

		[Inject]
		public RepairBuildingSignal RepairBuildingSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public OpenOrderBoardSignal openOrderBoardSignal { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public OrderBoardUpdateTicketOnBoardSignal updateTicketOnBoardSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFXSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public DCNShowFeaturedContentSignal dcnShowFeaturedContentSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal moveToBuildingSignal { get; set; }

		[Inject]
		public OpenVillainLairResourcePlotBuildingSignal openResourcePlotBuildingSignal { get; set; }

		[Inject]
		public ClickedVillainLairComponentBuildingSignal clickedVillainLairComponentBuildingSignal { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public TikiBarViewPickSignal tikiBarViewPickSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void Execute()
		{
			TryOpenMenu(BuildingObject, Building);
		}

		private void TryOpenMenu(BuildingObject buildingObject, Building building)
		{
			BuildingManagerMediator component = BuildingManager.GetComponent<BuildingManagerMediator>();
			if ((component.LastHarvestedBuildingID == building.ID && component.HarvestTimer > 0f) || pickControllerModel.IsInstanceIgnored(buildingObject.ID))
			{
				return;
			}
			if (building.State == BuildingState.Broken || building.State == BuildingState.MissingTikiSign)
			{
				RepairBuildingSignal.Dispatch(building);
				if (building is OrderBoard)
				{
					routineRunner.StartCoroutine(UpdateTickets());
				}
			}
			else
			{
				TryOpenBuildingMenu(buildingObject, building);
			}
		}

		private IEnumerator UpdateTickets()
		{
			yield return null;
			updateTicketOnBoardSignal.Dispatch();
		}

		private void TryOpenBuildingMenu(BuildingObject buildingObject, Building building)
		{
			BuildingState state = building.State;
			VillainLairEntranceBuilding villainLairEntranceBuilding = building as VillainLairEntranceBuilding;
			if (villainLairEntranceBuilding != null)
			{
				openVillainLairPortalSignal.Dispatch(villainLairEntranceBuilding, buildingObject as VillainLairEntranceBuildingObject);
			}
			else
			{
				if (((state != BuildingState.Idle && state != BuildingState.Working && state != BuildingState.Cooldown) || pickControllerModel.SelectedBuilding.HasValue || pickControllerModel.HeldTimer >= 0.75f) && !(building is CraftingBuilding))
				{
					return;
				}
				MIBBuilding mIBBuilding = building as MIBBuilding;
				if (mIBBuilding != null)
				{
					MessageInABottleSelectedSignal.Dispatch();
					return;
				}
				MignetteBuilding mignetteBuilding = building as MignetteBuilding;
				if (mignetteBuilding != null)
				{
					if (MignetteGameModel.IsMignetteActive || mignetteBuilding.AreAllMinionSlotsFilled() || SpawnDooberModel.DooberCounter > 0)
					{
						return;
					}
					MinionParty minionPartyInstance = PlayerService.GetMinionPartyInstance();
					if (minionPartyInstance != null && minionPartyInstance.IsPartyReady)
					{
						return;
					}
					int levelUnlocked = mignetteBuilding.Definition.LevelUnlocked;
					if (PlayerService.GetQuantity(StaticItem.LEVEL_ID) < levelUnlocked)
					{
						string aspirationalMessage = mignetteBuilding.Definition.AspirationalMessage;
						globalSFXSignal.Dispatch("Play_action_locked_01");
						popupMessageSignal.Dispatch(localService.GetString(aspirationalMessage, levelUnlocked), PopupMessageType.NORMAL);
						return;
					}
					if (!HasEnoughFreeMinionsToAssignToBuilding(PlayerService, mignetteBuilding))
					{
						ShowNeedXMinionsSignal.Dispatch(mignetteBuilding.GetMinionSlotsOwned());
						return;
					}
				}
				DebrisBuilding debrisBuilding = building as DebrisBuilding;
				if (debrisBuilding != null && debrisBuilding.PaidInputCostToClear)
				{
					logger.Warning("Already bought debris building: {0}", building.ID);
					return;
				}
				TikiBarBuilding tikiBarBuilding = building as TikiBarBuilding;
				if (tikiBarBuilding != null)
				{
					tikiBarViewPickSignal.Dispatch(pickControllerModel.EndHitObject);
					BuildingZoomSignal.Dispatch(new BuildingZoomSettings(ZoomType.IN, BuildingZoomType.TIKIBAR));
					return;
				}
				if (debrisBuilding == null && building.State != BuildingState.Cooldown)
				{
					TaskableBuilding taskableBuilding = building as TaskableBuilding;
					if (taskableBuilding != null && pickControllerModel.SelectedMinions.Count > 0 && taskableBuilding.GetMinionsInBuilding() < taskableBuilding.Definition.WorkStations)
					{
						return;
					}
				}
				StorageBuilding storageBuilding = building as StorageBuilding;
				if (storageBuilding != null && MignetteGameModel.IsMignetteActive)
				{
					return;
				}
				if (!(building is DecorationBuilding) && mignetteBuilding == null)
				{
					buildingObject.Bounce();
				}
				if (storageBuilding != null)
				{
					if (marketplaceService.AreThereSoldItems())
					{
						uiContext.injectionBinder.GetInstance<OpenSellBuildingModalSignal>().Dispatch();
					}
					else
					{
						OpenStorageBuildingSignal.Dispatch(storageBuilding, false);
					}
					return;
				}
				OrderBoard orderBoard = building as OrderBoard;
				if (orderBoard != null)
				{
					if (orderBoard.menuEnabled && !uiModel.LevelUpUIOpen)
					{
						openOrderBoardSignal.Dispatch(orderBoard);
					}
					return;
				}
				VillainLairResourcePlot villainLairResourcePlot = building as VillainLairResourcePlot;
				if (villainLairResourcePlot != null)
				{
					openResourcePlotBuildingSignal.Dispatch(villainLairResourcePlot);
					return;
				}
				MasterPlanComponentBuilding masterPlanComponentBuilding = building as MasterPlanComponentBuilding;
				if (masterPlanComponentBuilding != null)
				{
					MasterPlanDefinition definition = masterPlanService.CurrentMasterPlan.Definition;
					for (int i = 0; i < definition.CompBuildingDefinitionIDs.Count; i++)
					{
						if (masterPlanComponentBuilding.Definition.ID == definition.CompBuildingDefinitionIDs[i])
						{
							int id = definition.ComponentDefinitionIDs[i];
							MasterPlanComponentDefinition type = DefinitionService.Get<MasterPlanComponentDefinition>(id);
							clickedVillainLairComponentBuildingSignal.Dispatch(type);
							break;
						}
					}
				}
				BridgeBuilding bridgeBuilding = building as BridgeBuilding;
				if (bridgeBuilding != null)
				{
					moveToBuildingSignal.Dispatch(building, new PanInstructions(building));
					BridgeSignal.Dispatch(bridgeBuilding.ID);
					return;
				}
				StageBuilding stageBuilding = building as StageBuilding;
				if (stageBuilding != null)
				{
					openStageBuildingSignal.Dispatch(stageBuilding);
					return;
				}
				MinionUpgradeBuilding minionUpgradeBuilding = building as MinionUpgradeBuilding;
				if (minionUpgradeBuilding != null)
				{
					openMinionUpgradeBuildingSignal.Dispatch();
					return;
				}
				DCNBuilding dCNBuilding = building as DCNBuilding;
				if (dCNBuilding != null)
				{
					dcnShowFeaturedContentSignal.Dispatch();
				}
				ShowBuildingDetailMenu(buildingObject, building);
			}
		}

		private void ShowBuildingDetailMenu(BuildingObject buildingObject, Building building)
		{
			ShowBuildingDetailmenuSignal.Dispatch(building);
			if (AllowPan(building))
			{
				PanToBuilding(buildingObject, building);
			}
		}

		private void PanToBuilding(BuildingObject buildingObject, Building building)
		{
			Vector3 zoomCenter = buildingObject.ZoomCenter;
			CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, building, null);
			cameraMovementSettings.cameraSpeed = 0.4f;
			Boxed<ScreenPosition> type = new Boxed<ScreenPosition>(building.Definition.ScreenPosition);
			AutoMoveSignal.Dispatch(zoomCenter, type, cameraMovementSettings, false);
			showHiddenBuildingsSignal.Dispatch();
		}

		public bool AllowPan(Building building)
		{
			CompositeBuilding compositeBuilding = building as CompositeBuilding;
			if (compositeBuilding != null)
			{
				return false;
			}
			return true;
		}

		public static bool HasEnoughFreeMinionsToAssignToBuilding(IPlayerService playerService, TaskableBuilding building)
		{
			return playerService.HasPurchasedMinigamePack() || playerService.GetIdleMinions().Count >= building.GetMinionSlotsOwned() - building.GetMinionsInBuilding();
		}
	}
}
