using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class GoToService : IGoToService
	{
		public const float PLACEMENT_ZOOM_LEVEL = 0.4f;

		public IKampaiLogger logger = LogManager.GetClassLogger("GoToService") as IKampaiLogger;

		[Inject]
		public CameraAutoZoomSignal autoZoomSignal { get; set; }

		[Inject]
		public IBuildMenuService buildMenuService { get; set; }

		[Inject]
		public ClearNewBuildTabCount clearNewBuildTabCount { get; set; }

		[Inject]
		public CloseQuestBookSignal closeSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeUISignal { get; set; }

		[Inject]
		public CraftingModalParams craftingModalParams { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ExitVillainLairSignal exitVillainLairSignal { get; set; }

		[Inject]
		public OpenVillainLairResourcePlotBuildingSignal openVillainLairResourcePlotBuildingSignal { get; set; }

		[Inject]
		public FTUEQuestGoToSignal ftueGoToSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public HighlightTabSignal highlightTabSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal moveToBuildingSignal { get; set; }

		[Inject]
		public CameraAutoMoveToMignetteSignal moveToMignetteSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera myCamera { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreSignal { get; set; }

		[Inject]
		public OpenSellBuildingModalSignal openSellBuildingModalSignal { get; set; }

		[Inject]
		public PanAndOpenModalSignal panAndOpenModalSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public EnterVillainLairSignal enterVillainLairSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public CameraAutoMoveToPositionSignal cameraAutoMoveToPositionSignal { get; set; }

		[Inject]
		public CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public void GoToClicked(QuestStep step, QuestStepDefinition stepDefinition, IQuestController questController, int stepNumber)
		{
			ftueGoToSignal.Dispatch();
			switch (stepDefinition.Type)
			{
			case QuestStepType.Construction:
			case QuestStepType.BridgeRepair:
			case QuestStepType.CabanaRepair:
			case QuestStepType.WelcomeHutRepair:
			case QuestStepType.FountainRepair:
			case QuestStepType.StorageRepair:
			case QuestStepType.MinionUpgradeBuildingRepair:
				HandleBridgeAndConstruction(step, stepDefinition, questController.GetStepController(stepNumber));
				break;
			case QuestStepType.Delivery:
			case QuestStepType.Harvest:
			case QuestStepType.Leisure:
			case QuestStepType.PlayAnyLeisure:
			case QuestStepType.HarvestAnyLeisure:
				HandleDeliveryAndMinionTask(step, stepDefinition, questController.GetStepController(stepNumber));
				break;
			case QuestStepType.OrderBoard:
				HandleOrderBoard();
				break;
			case QuestStepType.Mignette:
				HandleMignette(stepDefinition.ItemDefinitionID);
				break;
			case QuestStepType.StageRepair:
				HandleStage(step.TrackedID);
				break;
			case QuestStepType.LairPortalRepair:
				HandleUniqueBuilding(step.TrackedID);
				break;
			case QuestStepType.MinionUpgrade:
			case QuestStepType.HaveUpgradedMinions:
				HandleUniqueBuilding(step.TrackedID, true);
				break;
			case QuestStepType.ThrowParty:
				HandleThrowParty();
				break;
			case QuestStepType.MinionTask:
				HandleMinionTask(stepDefinition.ItemDefinitionID);
				break;
			case QuestStepType.MysteryBoxOnboarding:
				HandleMysteryBoxOnboarding();
				break;
			case QuestStepType.MasterPlanTask:
				GoToClicked(stepDefinition as MasterPlanQuestType.ComponentTaskDefinition);
				break;
			case QuestStepType.MasterPlanComponentBuild:
			case QuestStepType.MasterPlanBuild:
				HandleMasterPlanConstruction();
				break;
			default:
				logger.Error("Attempting to handle QuestStepDefinition.Type case {0}: invalid enum type. No action taken.", (int)stepDefinition.Type);
				break;
			}
			closeSignal.Dispatch();
		}

		public void GoToClicked(MasterPlanQuestType.ComponentTaskDefinition taskDefinition)
		{
			switch (taskDefinition.taskDefinition.Type)
			{
			case MasterPlanComponentTaskType.Deliver:
			case MasterPlanComponentTaskType.Collect:
				GoToBuildingFromItem(taskDefinition.ItemDefinitionID);
				break;
			case MasterPlanComponentTaskType.CompleteOrders:
				HandleOrderBoard();
				break;
			case MasterPlanComponentTaskType.PlayMiniGame:
			case MasterPlanComponentTaskType.MiniGameScore:
			case MasterPlanComponentTaskType.EarnMignettePartyPoints:
				HandleMignette(taskDefinition.ItemDefinitionID);
				break;
			case MasterPlanComponentTaskType.EarnPartyPoints:
				HandlePartyPoints(taskDefinition.ItemDefinitionID);
				break;
			case MasterPlanComponentTaskType.EarnLeisurePartyPoints:
				HandleDistractivity();
				break;
			case MasterPlanComponentTaskType.EarnSandDollars:
				HandleSandDollars(taskDefinition.ItemDefinitionID);
				break;
			default:
				logger.Error("Attempting to handle MasterPlanComponentTaskType case {0}: invalid enum type. No action taken.", (int)taskDefinition.taskDefinition.Type);
				break;
			}
		}

		public void GoToBuildingFromItem(int itemDefID)
		{
			int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(itemDefID);
			TransitionToBuildingUIorStore(buildingDefintionIDFromItemDefintionID, itemDefID);
		}

		private void HandleBridgeAndConstruction(QuestStep step, QuestStepDefinition stepDefinition, IQuestStepController questStepController)
		{
			if ((step.TrackedID == 0 || (questStepController.ProgressBarAmount != 0 && questStepController.ProgressBarAmount < questStepController.ProgressBarTotal)) && definitionService.Has<BuildingDefinition>(stepDefinition.ItemDefinitionID))
			{
				OpenStoreFromAnywhere(stepDefinition.ItemDefinitionID);
				return;
			}
			Building building = playerService.GetByInstanceId<Building>(step.TrackedID);
			if (building != null)
			{
				PanInstructions pi = new PanInstructions(building);
				pi.ZoomDistance = new Boxed<float>(0.5f);
				TransitionAway(building.Definition.ID, delegate
				{
					moveToBuildingSignal.Dispatch(building, pi);
				});
			}
		}

		private void HandleDeliveryAndMinionTask(QuestStep step, QuestStepDefinition stepDefinition, IQuestStepController questStepController)
		{
			List<Building> accessibleBuildingList = GetAccessibleBuildingList(step, questStepController);
			if (accessibleBuildingList.Count > 0)
			{
				Building building = accessibleBuildingList[randomService.NextInt(accessibleBuildingList.Count)];
				TransitionToBuildingLocation(building, GotoBuildingHelpers.BuildingMenuIsAccessible(building), stepDefinition.ItemDefinitionID);
			}
			else if (step.TrackedID <= 0)
			{
				switch (stepDefinition.Type)
				{
				case QuestStepType.Leisure:
				case QuestStepType.PlayAnyLeisure:
				case QuestStepType.HarvestAnyLeisure:
					TransitionAndHighlightStoreTab(StoreItemType.Leisure);
					break;
				case QuestStepType.ThrowParty:
				case QuestStepType.StorageRepair:
					break;
				}
			}
			else
			{
				TransitionToBuildingUIorStore(step.TrackedID);
			}
		}

		private void HandleOrderBoard()
		{
			TransitionToBuildingUIorStore(3022);
		}

		private void HandleMignette(int buildingDefinitionID)
		{
			if (buildingDefinitionID == 0)
			{
				buildingDefinitionID = GotoBuildingHelpers.GetSuitableMignette(playerService, definitionService);
			}
			if (playerService.GetFirstInstanceByDefinitionId<Building>(buildingDefinitionID) == null)
			{
				BuildingDefinition definition = null;
				if (!definitionService.TryGet<BuildingDefinition>(buildingDefinitionID, out definition))
				{
					logger.Fatal(FatalCode.MIGNETTE_BAD_BUILDING_DEFINITION, buildingDefinitionID);
				}
			}
			TransitionAway(buildingDefinitionID, delegate
			{
				moveToMignetteSignal.Dispatch(buildingDefinitionID, false, new PanInstructions(null));
			});
		}

		private void HandleStage(int buildingID)
		{
			Building building = playerService.GetByInstanceId<Building>(buildingID);
			TransitionAway(building.Definition.ID, delegate
			{
				moveToBuildingSignal.Dispatch(building, new PanInstructions(building));
			});
		}

		private void HandleUniqueBuilding(int buildingID, bool moveToBuildingFirst = false)
		{
			Building building = playerService.GetByInstanceId<Building>(buildingID);
			BuildingManagerView component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
			BuildingObject bo = component.GetBuildingObject(building.ID);
			if (bo != null)
			{
				TransitionAway(building.Definition.ID, delegate
				{
					if (moveToBuildingFirst)
					{
						moveToBuildingSignal.Dispatch(building, new PanInstructions(building));
					}
					openBuildingMenuSignal.Dispatch(bo, building);
				});
			}
			else
			{
				logger.Error("could not open lair portal or minion upgrade asset: building is null");
			}
		}

		private void HandleThrowParty()
		{
			TransitionAway();
		}

		private void HandleMinionTask(int buildingDefID)
		{
			if (buildingDefID == 0)
			{
				buildingDefID = 3002;
			}
			TransitionToBuildingUIorStore(buildingDefID);
		}

		private void HandleMysteryBoxOnboarding()
		{
			MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(StaticItem.MINION_BENEFITS_DEF_ID);
			Building firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Building>(minionBenefitLevelBandDefintion.FirstBuildingId);
			ResourceBuilding resourceBuilding = firstInstanceByDefinitionId as ResourceBuilding;
			if (resourceBuilding != null)
			{
				bool openUI = true;
				if (resourceBuilding.BonusMinionItems != null && resourceBuilding.BonusMinionItems.Count > 0)
				{
					openUI = false;
				}
				TransitionToBuildingLocation(firstInstanceByDefinitionId, openUI);
			}
		}

		private void HandleMasterPlanConstruction()
		{
			if (villainLairModel.currentActiveLair == null)
			{
				zoomCameraModel.ZoomedIn = false;
				VillainLair firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<VillainLair>(3137);
				enterVillainLairSignal.Dispatch(firstInstanceByDefinitionId.ID, false);
			}
		}

		private void HandlePartyPoints(int buildingDefinitionID)
		{
			if (buildingDefinitionID != 0)
			{
				BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(buildingDefinitionID);
				MignetteBuildingDefinition mignetteBuildingDefinition = buildingDefinition as MignetteBuildingDefinition;
				if (mignetteBuildingDefinition != null)
				{
					TransitionAway(buildingDefinitionID, delegate
					{
						moveToMignetteSignal.Dispatch(buildingDefinitionID, false, new PanInstructions(null));
					});
				}
				else
				{
					TransitionToBuildingUIorStore(buildingDefinitionID);
				}
			}
			else
			{
				GoToSuitableLeisureBuilding(true);
			}
		}

		private void HandleDistractivity()
		{
			GoToSuitableLeisureBuilding(false);
		}

		private void HandleSandDollars(int itemDefinitionID)
		{
			if (itemDefinitionID == 0)
			{
				Action onComplete = delegate
				{
					openSellBuildingModalSignal.Dispatch();
				};
				TransitionAway(0, onComplete);
			}
			else
			{
				TransitionToBuildingUIorStore(itemDefinitionID);
			}
		}

		public void OpenStoreFromAnywhere(int buildingDefinitionID)
		{
			if (villainLairModel.currentActiveLair != null)
			{
				exitVillainLairSignal.Dispatch(new Boxed<Action>(delegate
				{
					OpenStore(buildingDefinitionID);
				}));
			}
			else if (buildingDefinitionID == 3123 && questService.GetQuestMap().ContainsKey(101120) && questService.GetQuestMap()[101120].State == QuestState.RunningTasks && playerService.GetInstancesByDefinitionID(3123).Count == 0)
			{
				cameraAutoMoveToPositionSignal.Dispatch(new Vector3(120f, 23f, 167f), 0.4f, true);
				cameraAutoPanCompleteSignal.AddOnce(delegate
				{
					OpenStore(buildingDefinitionID);
				});
			}
			else
			{
				PanOutTikiBar(delegate
				{
					OpenStore(buildingDefinitionID);
				});
			}
		}

		private void OpenStore(int buildingDefinitionID)
		{
			closeUISignal.Dispatch(null);
			uiModel.GoToInEffect = true;
			openStoreSignal.Dispatch(buildingDefinitionID, true);
			ZoomOutToPlacementLevel();
			int storeItemDefinitionIDFromBuildingID = buildMenuService.GetStoreItemDefinitionIDFromBuildingID(buildingDefinitionID);
			StoreItemDefinition storeItemDefinition = definitionService.Get<StoreItemDefinition>(storeItemDefinitionIDFromBuildingID);
			clearNewBuildTabCount.Dispatch(storeItemDefinition.Type);
		}

		private void TransitionAndHighlightStoreTab(StoreItemType itemType)
		{
			Action onComplete = delegate
			{
				highlightTabSignal.Dispatch(itemType);
			};
			TransitionAway(0, onComplete);
			ZoomOutToPlacementLevel();
		}

		private void TransitionAway(int targetBuildingDefID = 0, Action onComplete = null, bool forceTargetInLair = false)
		{
			bool flag = villainLairModel.currentActiveLair != null;
			bool flag2 = forceTargetInLair || BuildingBelongsInLair(targetBuildingDefID);
			if (flag)
			{
				if (flag2)
				{
					RunOnComplete(onComplete);
					return;
				}
				exitVillainLairSignal.Dispatch(new Boxed<Action>(delegate
				{
					RunOnComplete(onComplete);
				}));
			}
			else
			{
				PanOutTikiBar(delegate
				{
					RunOnComplete(onComplete);
				});
			}
		}

		private void TransitionToBuildingLocation(Building building, bool openUI = false, int itemDefinitionID = 0)
		{
			if (building != null)
			{
				if (!openUI && building.State != BuildingState.Inventory)
				{
					TransitionAway(building.Definition.ID, delegate
					{
						moveToBuildingSignal.Dispatch(building, new PanInstructions(building));
					});
				}
				else
				{
					TransitionToBuildingInstanceUIorStore(building, itemDefinitionID);
				}
			}
			else
			{
				logger.Error("Building instance is null when trying to pan to location");
			}
		}

		private void TransitionToBuildingUIorStore(int buildingDefinitionID, int itemDefinitionID = 0)
		{
			if (buildingDefinitionID == 0)
			{
				return;
			}
			List<Building> list = new List<Building>();
			foreach (Building item in playerService.GetByDefinitionId<Building>(buildingDefinitionID))
			{
				if (item.State != BuildingState.Inventory && item.State != BuildingState.Inaccessible)
				{
					list.Add(item);
				}
			}
			Building building = null;
			if (list.Count > 0)
			{
				building = list[randomService.NextInt(list.Count)];
			}
			if (building != null)
			{
				TransitionToBuildingInstanceUIorStore(building, itemDefinitionID);
				return;
			}
			int num = UpdateDefinitionToUseForNullBuilding(buildingDefinitionID);
			if (buildingDefinitionID == num)
			{
				OpenStoreFromAnywhere(buildingDefinitionID);
				return;
			}
			building = playerService.GetFirstInstanceByDefinitionId<Building>(num);
			TransitionToBuildingInstanceUIorStore(building, itemDefinitionID);
		}

		private void TransitionToBuildingInstanceUIorStore(Building building, int itemDefinitionID = 0)
		{
			if (building != null)
			{
				int iD = building.Definition.ID;
				if (GotoBuildingHelpers.BuildingLivesInsideLair(building))
				{
					VillainLairResourcePlot villainLairResourcePlot = building as VillainLairResourcePlot;
					if (villainLairResourcePlot != null)
					{
						GoToResourcePlotFromAnywhere(villainLairResourcePlot);
					}
					else
					{
						TransitionAway(iD);
					}
					return;
				}
				if (building.State == BuildingState.Inventory)
				{
					OpenStoreFromAnywhere(iD);
					return;
				}
				if (itemDefinitionID != 0)
				{
					SetPossibleCraftingParams(building, itemDefinitionID);
				}
				TransitionAway(iD, delegate
				{
					panAndOpenModalSignal.Dispatch(building.ID, false);
				});
			}
			else
			{
				logger.Error("Unable to transition to building instance (or UI): building is null.");
			}
		}

		private void GoToResourcePlotFromAnywhere(VillainLairResourcePlot plot)
		{
			if (plot == null)
			{
				return;
			}
			if (villainLairModel.currentActiveLair != null)
			{
				openVillainLairResourcePlotBuildingSignal.Dispatch(plot);
				return;
			}
			Action onComplete = delegate
			{
				panAndOpenModalSignal.Dispatch(plot.parentLair.portalInstanceID, false);
			};
			TransitionAway(0, onComplete);
		}

		private void PanOutTikiBar(Action onComplete = null)
		{
			if (zoomCameraModel.ZoomedIn)
			{
				gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, zoomCameraModel.LastZoomBuildingType, onComplete));
			}
			else if (onComplete != null)
			{
				onComplete();
			}
		}

		private void ZoomOutToPlacementLevel()
		{
			float currentPercentage = myCamera.GetComponent<ZoomView>().GetCurrentPercentage();
			if (currentPercentage > 0.4f)
			{
				autoZoomSignal.Dispatch(0.4f);
			}
		}

		private void RunOnComplete(Action onComplete = null)
		{
			if (onComplete != null)
			{
				onComplete();
			}
		}

		private int UpdateDefinitionToUseForNullBuilding(int buildingDefinitionID)
		{
			BuildingDefinition definition = null;
			definitionService.TryGet<BuildingDefinition>(buildingDefinitionID, out definition);
			if (definition == null)
			{
				logger.Fatal(FatalCode.DS_GOTO_INVALID_DEF);
				return 0;
			}
			if (definition.Type == BuildingType.BuildingTypeIdentifier.LAIR_RESOURCEPLOT)
			{
				return 3132;
			}
			return buildingDefinitionID;
		}

		private void SetPossibleCraftingParams(Building building, int itemDefinitionID)
		{
			if (building.Definition.Type == BuildingType.BuildingTypeIdentifier.CRAFTING)
			{
				craftingModalParams.itemId = itemDefinitionID;
				craftingModalParams.highlight = true;
			}
		}

		private List<Building> GetAccessibleBuildingList(QuestStep step, IQuestStepController questStepController)
		{
			List<Building> list = new List<Building>();
			QuestStepType stepType = questStepController.StepType;
			if (stepType == QuestStepType.PlayAnyLeisure || stepType == QuestStepType.HarvestAnyLeisure)
			{
				List<LeisureBuilding> instancesByType = playerService.GetInstancesByType<LeisureBuilding>();
				foreach (LeisureBuilding item in instancesByType)
				{
					if (item.State != BuildingState.Inventory)
					{
						list.Add(item);
					}
				}
			}
			else
			{
				foreach (Building item2 in playerService.GetByDefinitionId<Building>(step.TrackedID))
				{
					if (item2.State != BuildingState.Inventory)
					{
						list.Add(item2);
					}
				}
			}
			return list;
		}

		private void GoToSuitableLeisureBuilding(bool sortByHighestPoints)
		{
			IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<LeisureBuildingDefintiion>();
			LeisureBuilding leisureBuilding = null;
			if (sortByHighestPoints)
			{
				leisureBuilding = GetHighestScoringOwnedLeisureBuilding(instancesByDefinition);
			}
			else
			{
				Building building = TryGetFirstBuildingNotInState(instancesByDefinition, BuildingState.Idle);
				if (building != null)
				{
					leisureBuilding = building as LeisureBuilding;
				}
			}
			if (leisureBuilding == null)
			{
				TransitionAndHighlightStoreTab(StoreItemType.Leisure);
			}
			else
			{
				TransitionToBuildingInstanceUIorStore(leisureBuilding);
			}
		}

		private LeisureBuilding GetHighestScoringOwnedLeisureBuilding(IList<Instance> leisureBuildings)
		{
			LeisureBuilding result = null;
			int num = 0;
			for (int i = 0; i < leisureBuildings.Count; i++)
			{
				LeisureBuilding leisureBuilding = leisureBuildings[i] as LeisureBuilding;
				if (leisureBuilding != null && leisureBuilding.Definition.PartyPointsReward > num)
				{
					num = leisureBuilding.Definition.PartyPointsReward;
					result = leisureBuilding;
				}
			}
			return result;
		}

		private Building TryGetFirstBuildingNotInState(IList<Instance> buildings, BuildingState excludedState)
		{
			Building building = null;
			for (int i = 0; i < buildings.Count; i++)
			{
				Instance instance = buildings[i];
				Building building2 = instance as Building;
				if (building2 != null && building2.State == excludedState)
				{
					building = building2;
					break;
				}
			}
			if (building == null && buildings.Count > 0)
			{
				building = buildings[0] as Building;
			}
			return building;
		}

		private bool BuildingBelongsInLair(int targetBuildingID)
		{
			if (targetBuildingID == 0)
			{
				return false;
			}
			BuildingDefinition definition;
			if (definitionService.TryGet<BuildingDefinition>(targetBuildingID, out definition))
			{
				return GotoBuildingHelpers.BuildingLivesInsideLair(definition);
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(targetBuildingID);
			if (byInstanceId != null)
			{
				return GotoBuildingHelpers.BuildingLivesInsideLair(byInstanceId);
			}
			return false;
		}
	}
}
