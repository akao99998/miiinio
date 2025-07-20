using System.Collections.Generic;
using System.Linq;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI
{
	public class GhostComponentService : IGhostComponentService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GhostComponentService") as IKampaiLogger;

		private Dictionary<string, RuntimeAnimatorController> animationControllers;

		private Dictionary<int, GhostComponentFadeHelperObject> displayedFadableObjects = new Dictionary<int, GhostComponentFadeHelperObject>();

		private GhostBuildingDisplayType displayType_ZoomedInToComponentInProgress = GhostBuildingDisplayType.Glowing;

		private GhostBuildingDisplayType displayType_ZoomedInToRegularComponent = GhostBuildingDisplayType.Ghosted;

		private GhostBuildingDisplayType displayType_DisplayingAllNonSelectedComponents = GhostBuildingDisplayType.Glowing;

		private GhostBuildingDisplayType displayType_DisplayBuildingWithAutoClose;

		private bool popOutOverwrittenGhostComponents;

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllMessageDialogsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSoundFXSignal { get; set; }

		public BuildingObject DisplayGhostBuilding(int componentDefID, GhostBuildingDisplayType displayType)
		{
			if (lairModel.currentActiveLair == null)
			{
				logger.Error("lairModel.currentActiveLair is null, returning a null building");
				return null;
			}
			int buildingDefID = masterPlanService.CurrentMasterPlan.Definition.BuildingDefID;
			bool isRegularBuilding = componentDefID != buildingDefID;
			return GetGhostBuildingFromComponentDefID(componentDefID, displayType, isRegularBuilding);
		}

		public void DisplayComponentMarkedAsInProgress(MasterPlanComponent component)
		{
			if (component != null)
			{
				GetGhostBuildingFromComponentDefID(component.Definition.ID, displayType_ZoomedInToComponentInProgress);
			}
		}

		public void DisplayZoomedInComponent(int componentID, bool isRegularBuilding = true)
		{
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentID);
			if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State < MasterPlanComponentState.Scaffolding)
			{
				GhostBuildingDisplayType displayType = displayType_ZoomedInToRegularComponent;
				if (firstInstanceByDefinitionId.State > MasterPlanComponentState.NotStarted)
				{
					displayType = displayType_ZoomedInToComponentInProgress;
				}
				GetGhostBuildingFromComponentDefID(componentID, displayType, isRegularBuilding);
			}
		}

		public void DisplayAllSelectablePlanComponents()
		{
			if (lairModel.currentActiveLair == null)
			{
				return;
			}
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			MasterPlanDefinition definition = currentMasterPlan.Definition;
			for (int i = 0; i < definition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(definition.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId == null || firstInstanceByDefinitionId.State == MasterPlanComponentState.NotStarted)
				{
					MasterPlanComponentBuildingDefinition componentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(definition.CompBuildingDefinitionIDs[i]);
					GetGhostBuildingFromComponentBldgDef(componentBuildingDefinition, displayType_DisplayingAllNonSelectedComponents);
				}
			}
		}

		private void ShowAllBuildingsForFTUE()
		{
			if (lairModel.currentActiveLair != null)
			{
				MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
				MasterPlanDefinition definition = currentMasterPlan.Definition;
				for (int i = 0; i < definition.ComponentDefinitionIDs.Count; i++)
				{
					MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(definition.CompBuildingDefinitionIDs[i]);
					GhostBuildingDisplayType displayType = GhostBuildingDisplayType.Normal;
					int iD = masterPlanComponentBuildingDefinition.ID;
					string prefab = masterPlanComponentBuildingDefinition.GetPrefab();
					Vector3 componentBuildingPosition = masterPlanService.GetComponentBuildingPosition(iD);
					MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = CreateBuilding(prefab, displayType, masterPlanComponentBuildingDefinition, componentBuildingPosition);
					GhostComponentFadeHelperObject ghostComponentFadeHelperObject = masterPlanComponentBuildingObject.gameObject.AddComponent<GhostComponentFadeHelperObject>();
					ghostComponentFadeHelperObject.SetupAndDisplay(this, masterPlanComponentBuildingObject, displayType, false);
					displayedFadableObjects[iD] = ghostComponentFadeHelperObject;
					ghostComponentFadeHelperObject.TriggerFTUEDropAnimation(masterPlanComponentBuildingDefinition.dropAnimationController, playGlobalSoundFXSignal);
				}
			}
		}

		public bool DisplayAutoCloseGhostComponent(int componentBuildingDefID, float fadeTime, float openDuration)
		{
			BuildingObject ghostBuildingFromComponentDefID = GetGhostBuildingFromComponentDefID(componentBuildingDefID, displayType_DisplayBuildingWithAutoClose, false, false);
			if (ghostBuildingFromComponentDefID == null)
			{
				return false;
			}
			GhostComponentFadeHelperObject ghostComponentFadeHelperObject = ghostBuildingFromComponentDefID.gameObject.AddComponent<GhostComponentFadeHelperObject>();
			ghostComponentFadeHelperObject.SetupAndAutoFadeWithMessage(fadeTime, openDuration, popupMessageSignal, closeAllMessageDialogsSignal, this, displayType_DisplayBuildingWithAutoClose, ghostBuildingFromComponentDefID);
			displayedFadableObjects[ghostBuildingFromComponentDefID.DefinitionID] = ghostComponentFadeHelperObject;
			return true;
		}

		public void RunBeginGhostComponentFunctionFromDefinition(GhostComponentFunctionType functionType, int defID = 0)
		{
			switch (functionType)
			{
			case GhostComponentFunctionType.ShowAllSelectableBuildings:
				DisplayAllSelectablePlanComponents();
				break;
			case GhostComponentFunctionType.ShowAllBuildingsForFTUE:
				ShowAllBuildingsForFTUE();
				break;
			case GhostComponentFunctionType.DisplayGhostBuilding:
				if (defID == 0)
				{
					logger.Error("Trying to zoom in on a building without the correct buildingDefinitionID");
				}
				DisplayGhostBuilding(defID, GhostBuildingDisplayType.Ghosted);
				break;
			case GhostComponentFunctionType.DisplayGlowBuilding:
				if (defID == 0)
				{
					logger.Error("Trying to zoom in on a building without the correct buildingDefinitionID");
				}
				DisplayGhostBuilding(defID, GhostBuildingDisplayType.Glowing);
				break;
			case GhostComponentFunctionType.DisplayNormalBuilding:
				if (defID == 0)
				{
					logger.Error("Trying to zoom in on a building without the correct buildingDefinitionID");
				}
				DisplayGhostBuilding(defID, GhostBuildingDisplayType.Normal);
				break;
			}
		}

		public void RunEndGhostComponentFunctionFromDefinition(GhostFunctionCloseType closeType)
		{
			if (closeType == GhostFunctionCloseType.ClearAllBuildings)
			{
				ClearGhostComponentBuildings();
			}
		}

		public MasterPlanComponentBuildingObject CreateBuilding(string prefabName, GhostBuildingDisplayType displayType, MasterPlanComponentBuildingDefinition buildingDefinition, Vector3 position, bool isAudible = true)
		{
			if (animationControllers == null)
			{
				animationControllers = new Dictionary<string, RuntimeAnimatorController>();
			}
			GameObject original = KampaiResources.Load<GameObject>(prefabName);
			GameObject gameObject = Object.Instantiate(original);
			Building building = buildingDefinition.BuildBuilding();
			if (gameObject == null)
			{
				logger.Error("Could not create dummy building object from building definition id: {0}", buildingDefinition.ID);
				return null;
			}
			gameObject.name = string.Format("{0}_{1}", displayType, prefabName);
			Transform transform = gameObject.transform;
			Vector3 localEulerAngles = (transform.localPosition = Vector3.zero);
			transform.localEulerAngles = localEulerAngles;
			localEulerAngles = (transform.eulerAngles = Vector3.one);
			transform.localScale = localEulerAngles;
			transform.position = position;
			gameObject.SetLayerRecursively(9);
			MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = building.AddBuildingObject(gameObject.gameObject) as MasterPlanComponentBuildingObject;
			LoadBuildingAnimationControllers(buildingDefinition);
			LoadBuildingAnimationEventHandler(gameObject.transform);
			if (!isAudible)
			{
				masterPlanComponentBuildingObject.ExecuteAction(new MuteAction(masterPlanComponentBuildingObject, true, logger));
			}
			masterPlanComponentBuildingObject.Init(building, logger, animationControllers, definitionService);
			return masterPlanComponentBuildingObject;
		}

		private void LoadBuildingAnimationControllers(BuildingDefinition buildingDefinition)
		{
			AnimatingBuildingDefinition animatingBuildingDefinition = buildingDefinition as AnimatingBuildingDefinition;
			if (animatingBuildingDefinition == null)
			{
				return;
			}
			foreach (string item in animatingBuildingDefinition.AnimationControllerKeys())
			{
				if (!animationControllers.ContainsKey(item))
				{
					RuntimeAnimatorController value = KampaiResources.Load<RuntimeAnimatorController>(item);
					animationControllers.Add(item, value);
				}
			}
		}

		private void LoadBuildingAnimationEventHandler(Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				GameObject gameObject = child.gameObject;
				Animator component = gameObject.GetComponent<Animator>();
				AnimEventHandler component2 = gameObject.GetComponent<AnimEventHandler>();
				if (component != null && component2 == null)
				{
					AnimEventHandler animEventHandler = gameObject.AddComponent<AnimEventHandler>();
					animEventHandler.Init(gameObject, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
				}
			}
		}

		private BuildingObject GetGhostBuildingFromComponentDefID(int componentDefID, GhostBuildingDisplayType displayType, bool isRegularBuilding = true, bool doNotAutoClose = true)
		{
			int id = componentDefID;
			if (isRegularBuilding)
			{
				MasterPlanDefinition definition = masterPlanService.CurrentMasterPlan.Definition;
				for (int i = 0; i < definition.ComponentDefinitionIDs.Count; i++)
				{
					if (definition.ComponentDefinitionIDs[i] == componentDefID)
					{
						id = definition.CompBuildingDefinitionIDs[i];
						break;
					}
				}
			}
			MasterPlanComponentBuildingDefinition componentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(id);
			return GetGhostBuildingFromComponentBldgDef(componentBuildingDefinition, displayType, doNotAutoClose);
		}

		private BuildingObject GetGhostBuildingFromComponentBldgDef(MasterPlanComponentBuildingDefinition componentBuildingDefinition, GhostBuildingDisplayType displayType, bool doNotAutoClose = true)
		{
			MasterPlanComponentBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(componentBuildingDefinition.ID);
			if (firstInstanceByDefinitionId != null)
			{
				return null;
			}
			int iD = componentBuildingDefinition.ID;
			if (displayedFadableObjects.ContainsKey(iD) && displayedFadableObjects[iD] != null)
			{
				if (displayedFadableObjects[iD].ghostDisplayType == displayType)
				{
					logger.Info("Did not create a new ghost component: one of the same type already exists.");
					if (doNotAutoClose)
					{
						return displayedFadableObjects[iD].buildingObject;
					}
					return null;
				}
				ReleaseSingleBuildingObject(displayedFadableObjects[iD], popOutOverwrittenGhostComponents);
			}
			string ghostBuildingPrefabByType = GetGhostBuildingPrefabByType(componentBuildingDefinition.GetPrefab(), displayType);
			Vector3 componentBuildingPosition = masterPlanService.GetComponentBuildingPosition(iD);
			MasterPlanComponentBuildingObject masterPlanComponentBuildingObject = CreateBuilding(ghostBuildingPrefabByType, displayType, componentBuildingDefinition, componentBuildingPosition);
			if (doNotAutoClose)
			{
				GhostComponentFadeHelperObject ghostComponentFadeHelperObject = masterPlanComponentBuildingObject.gameObject.AddComponent<GhostComponentFadeHelperObject>();
				ghostComponentFadeHelperObject.SetupAndDisplay(this, masterPlanComponentBuildingObject, displayType);
				displayedFadableObjects[iD] = ghostComponentFadeHelperObject;
			}
			return masterPlanComponentBuildingObject;
		}

		private void ReleaseSingleBuildingObject(GhostComponentFadeHelperObject helper, bool immediate = false)
		{
			if (helper != null && helper.gameObject != null && displayedFadableObjects.ContainsKey(helper.buildingObject.DefinitionID))
			{
				displayedFadableObjects[helper.buildingObject.DefinitionID].StartFadeOut(immediate);
				displayedFadableObjects.Remove(helper.buildingObject.DefinitionID);
			}
		}

		public void GhostBuildingAutoRemoved(int id, GhostComponentFadeHelperObject helper)
		{
			if (displayedFadableObjects.ContainsKey(id) && displayedFadableObjects[id] == helper)
			{
				displayedFadableObjects.Remove(id);
			}
		}

		public void ClearGhostComponentBuildings(bool alsoClearComponentsInProgress = false, bool immediate = false)
		{
			if (alsoClearComponentsInProgress)
			{
				WipeEverything(immediate);
				return;
			}
			MasterPlanComponent componentCurrentlyInProgress = GetComponentCurrentlyInProgress();
			if (componentCurrentlyInProgress == null)
			{
				WipeEverything(immediate);
				return;
			}
			int buildingDefID = componentCurrentlyInProgress.buildingDefID;
			List<int> list = displayedFadableObjects.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (buildingDefID != num)
				{
					ReleaseSingleBuildingObject(displayedFadableObjects[num], immediate);
				}
			}
		}

		public void ClearAllGhostBuildingsExceptCurrent(int excludedComponentDefID, bool keepSelectedComponents = false, bool immediate = false)
		{
			int num = 0;
			if (!keepSelectedComponents)
			{
				WipeEverything(immediate);
				return;
			}
			if (keepSelectedComponents)
			{
				MasterPlanComponent componentCurrentlyInProgress = GetComponentCurrentlyInProgress();
				if (componentCurrentlyInProgress != null)
				{
					num = componentCurrentlyInProgress.Definition.ID;
				}
				return;
			}
			List<int> list = displayedFadableObjects.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				int num2 = list[i];
				if (num2 != excludedComponentDefID && num2 != num)
				{
					ReleaseSingleBuildingObject(displayedFadableObjects[num2], immediate);
				}
			}
		}

		private void WipeEverything(bool immediate = false)
		{
			List<GhostComponentFadeHelperObject> list = displayedFadableObjects.Values.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				ReleaseSingleBuildingObject(list[i], immediate);
			}
		}

		private string GetGhostBuildingPrefabByType(string prefab, GhostBuildingDisplayType displayType)
		{
			int num = prefab.IndexOf("_Prefab");
			string text = prefab;
			if (num == -1 || displayType == GhostBuildingDisplayType.Normal)
			{
				return text;
			}
			switch (displayType)
			{
			case GhostBuildingDisplayType.Ghosted:
				return text.Insert(num, "_Ghost");
			case GhostBuildingDisplayType.Glowing:
				return text.Insert(num, "_Glow");
			default:
				return prefab;
			}
		}

		private MasterPlanComponent GetComponentCurrentlyInProgress()
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				return null;
			}
			MasterPlanComponent result = null;
			if (lairModel.currentActiveLair != null)
			{
				result = masterPlanService.GetActiveComponentFromPlanDefinition(currentMasterPlan.Definition.ID);
			}
			return result;
		}
	}
}
