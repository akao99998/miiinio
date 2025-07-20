using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class BuildingManagerView : KampaiView
	{
		private IKampaiLogger logger;

		private IDefinitionService definitionService;

		private IMasterPlanService masterPlanService;

		private BuildingObject selectedBuilding;

		private Dictionary<int, BuildingObjectCollection> buildings = new Dictionary<int, BuildingObjectCollection>();

		private Dictionary<string, RuntimeAnimatorController> animationControllers;

		private bool isSpecialEventActive;

		internal Signal<BuildingObject, Dictionary<string, RuntimeAnimatorController>, Building> initBuildingObject = new Signal<BuildingObject, Dictionary<string, RuntimeAnimatorController>, Building>();

		internal Signal<int, MinionTaskInfo> updateMinionSignal = new Signal<int, MinionTaskInfo>();

		internal Signal<Building, Location> addFootprintSignal = new Signal<Building, Location>();

		internal Signal<Building> updateResourceBuildingSignal = new Signal<Building>();

		internal Signal<Building> setBuildingNumberSignal = new Signal<Building>();

		private BuildingObject toInventoryBuildingObject;

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject(GameElement.LAND_EXPANSION_PARENT)]
		public GameObject landExpansionParent { get; set; }

		[Inject]
		public ConnectableBuildingPickedUpSignal connectableBuildingPickedUpSignal { get; set; }

		internal void Init(IKampaiLogger log, IDefinitionService definitionService, IMasterPlanService masterPlanService, bool isSpecialEventActive)
		{
			selectedBuilding = null;
			animationControllers = new Dictionary<string, RuntimeAnimatorController>();
			logger = log;
			this.definitionService = definitionService;
			this.masterPlanService = masterPlanService;
			this.isSpecialEventActive = isSpecialEventActive;
		}

		internal GameObject CreateBuilding(Building building, int prefabIndex = 0)
		{
			int iD = building.ID;
			Location location = building.Location;
			string prefab = building.GetPrefab(prefabIndex);
			GameObject gameObject;
			if (string.IsNullOrEmpty(prefab))
			{
				gameObject = new GameObject();
			}
			else
			{
				GameObject gameObject2 = KampaiResources.Load<GameObject>(prefab);
				if (gameObject2 != null)
				{
					gameObject = UnityEngine.Object.Instantiate(gameObject2);
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "Trying to instantiate null prefab: {0}", prefab);
					gameObject = new GameObject();
				}
			}
			if (building is LandExpansionBuilding)
			{
				gameObject.transform.parent = landExpansionParent.transform;
			}
			else
			{
				gameObject.transform.parent = base.transform;
			}
			BuildingObject buildingObject = SetupBuilding(gameObject, building);
			gameObject.transform.position = new Vector3(location.x, 0f, location.y);
			gameObject.transform.rotation = Quaternion.identity;
			if (building is MasterPlanComponentBuilding)
			{
				gameObject.transform.position += masterPlanService.GetComponentBuildingOffset(building.Definition.ID);
			}
			else if (building.IsFootprintable)
			{
				addFootprintSignal.Dispatch(building, location);
			}
			if (isSpecialEventActive)
			{
				LoadSpecialEventPaintover(building, gameObject);
			}
			buildings[iD] = new BuildingObjectCollection(buildingObject);
			return gameObject;
		}

		internal void HighlightBuilding(int buildingId, bool highlight)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				buildingObject.Highlight(highlight);
			}
		}

		private void LoadSpecialEventPaintover(Building building, GameObject parent)
		{
			if (!building.IsBuildingRepaired())
			{
				return;
			}
			string paintover = building.GetPaintover();
			if (string.IsNullOrEmpty(paintover))
			{
				return;
			}
			GameObject gameObject = KampaiResources.Load<GameObject>(paintover);
			if (!(gameObject == null))
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				if (!(gameObject2 == null))
				{
					gameObject2.transform.parent = parent.transform;
					gameObject2.transform.localPosition = Vector3.zero;
				}
			}
		}

		private BuildingObject SetupBuilding(GameObject buildingInstance, Building building)
		{
			BuildingObject buildingObject = building.AddBuildingObject(buildingInstance);
			if (!(building is DecorationBuilding) && !(building is LandExpansionBuilding))
			{
				AddAnimation(buildingObject.gameObject);
			}
			if (building is ResourceBuilding)
			{
				updateResourceBuildingSignal.Dispatch(building);
			}
			else
			{
				setBuildingNumberSignal.Dispatch(building);
			}
			AddAnimEventHandlersToChildren(buildingInstance.transform, buildingObject);
			if (!(building is MignetteBuilding))
			{
				LoadAnimationControllers(building);
			}
			IStartAudio startAudio = buildingObject as IStartAudio;
			if (startAudio != null)
			{
				startAudio.InitAudio(building.State, audioSignal);
			}
			initBuildingObject.Dispatch(buildingObject, animationControllers, building);
			buildingInstance.name = string.Format("building_{0}", building.ID);
			return buildingObject;
		}

		private BuildingObjectCollection GetBuildingObjectCollection(int buildingId)
		{
			BuildingObjectCollection result = null;
			if (buildings.ContainsKey(buildingId))
			{
				result = buildings[buildingId];
			}
			return result;
		}

		internal void SetBuildingRushed(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection != null)
			{
				buildingObjectCollection.Rushed = true;
			}
		}

		internal bool IsBuildingRushed(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection != null)
			{
				return buildingObjectCollection.Rushed;
			}
			return false;
		}

		internal BuildingObject GetBuildingObject(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection == null)
			{
				return null;
			}
			return buildingObjectCollection.BuildingObject;
		}

		internal ScaffoldingBuildingObject GetScaffoldingBuildingObject(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection == null)
			{
				return null;
			}
			return buildingObjectCollection.ScaffoldingBuildingObject;
		}

		private string GetScaffoldingPrefabName(BuildingDefinition buildingDefinition)
		{
			return buildingDefinition.ScaffoldingPrefab;
		}

		private string GetPlatformPrefabName(BuildingDefinition buildingDefinition)
		{
			return buildingDefinition.PlatformPrefab;
		}

		private string GetRibbonPrefabName(BuildingDefinition buildingDefinition)
		{
			return buildingDefinition.RibbonPrefab;
		}

		internal void RemoveAllScaffoldingParts(int buildingId)
		{
			RemoveScaffoldingBuildingObject(buildingId);
			RemoveRibbonBuildingObject(buildingId);
			RemovePlatformBuildingObject(buildingId);
		}

		internal void RemoveScaffoldingBuildingObject(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection != null && buildingObjectCollection.ScaffoldingBuildingObject != null)
			{
				UnityEngine.Object.Destroy(buildingObjectCollection.ScaffoldingBuildingObject.gameObject);
				buildingObjectCollection.ScaffoldingBuildingObject = null;
			}
		}

		internal void RemovePlatformBuildingObject(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection != null && buildingObjectCollection.PlatformBuildingObject != null)
			{
				UnityEngine.Object.Destroy(buildingObjectCollection.PlatformBuildingObject.gameObject);
				buildingObjectCollection.PlatformBuildingObject = null;
			}
		}

		internal void RemoveRibbonBuildingObject(int buildingId)
		{
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(buildingId);
			if (buildingObjectCollection != null && buildingObjectCollection.RibbonBuildingObject != null)
			{
				UnityEngine.Object.Destroy(buildingObjectCollection.RibbonBuildingObject.gameObject);
				buildingObjectCollection.RibbonBuildingObject = null;
			}
		}

		internal void RemoveBuilding(int buildingId, bool destroyObject = true)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (!(buildingObject == null))
			{
				RemoveAllScaffoldingParts(buildingId);
				buildings.Remove(buildingId);
				if (destroyObject)
				{
					buildingObject.Cleanup();
					UnityEngine.Object.Destroy(buildingObject.gameObject);
				}
			}
		}

		private bool CanCreateScaffoldingPartBuildingObject(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject == null)
			{
				logger.Warning("Building object is null, can't create a scaffolding part object");
				return false;
			}
			IRequiresBuildingScaffolding requiresBuildingScaffolding = buildingObject as IRequiresBuildingScaffolding;
			if (requiresBuildingScaffolding == null)
			{
				logger.Warning("Can't create a scaffolding part object on a building that does not require it.");
				return false;
			}
			return true;
		}

		private T CreateScaffoldingPartPrefab<T>(Building building, string prefabName, Vector3 position) where T : MonoBehaviour, IScaffoldingPart
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(KampaiResources.Load<GameObject>(prefabName));
			gameObject.name = "Building Cover";
			if (gameObject.transform.childCount > 0)
			{
				AddAnimEventHandlersToChildren(gameObject.transform.GetChild(0), null);
			}
			Transform transform = gameObject.transform;
			transform.parent = base.transform;
			transform.position = position;
			transform.eulerAngles = Vector3.zero;
			T result = gameObject.AddComponent<T>();
			result.Init(building, logger, definitionService);
			return result;
		}

		internal ScaffoldingBuildingObject CreateScaffoldingBuildingObject(Building building, Vector3 position)
		{
			int iD = building.ID;
			if (!CanCreateScaffoldingPartBuildingObject(iD))
			{
				return null;
			}
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(iD);
			if (buildingObjectCollection.ScaffoldingBuildingObject != null)
			{
				return buildingObjectCollection.ScaffoldingBuildingObject;
			}
			ScaffoldingBuildingObject scaffoldingBuildingObject2 = (buildingObjectCollection.ScaffoldingBuildingObject = CreateScaffoldingPartPrefab<ScaffoldingBuildingObject>(building, GetScaffoldingPrefabName(building.Definition), position));
			AdjustObjectPosition(scaffoldingBuildingObject2.transform, building);
			return scaffoldingBuildingObject2;
		}

		internal PlatformBuildingObject CreatePlatformBuildingObject(Building building, Vector3 position)
		{
			int iD = building.ID;
			if (!CanCreateScaffoldingPartBuildingObject(iD))
			{
				return null;
			}
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(iD);
			if (buildingObjectCollection.PlatformBuildingObject != null)
			{
				return buildingObjectCollection.PlatformBuildingObject;
			}
			return buildingObjectCollection.PlatformBuildingObject = CreateScaffoldingPartPrefab<PlatformBuildingObject>(building, GetPlatformPrefabName(building.Definition), position);
		}

		internal RibbonBuildingObject CreateRibbonBuildingObject(Building building, Vector3 position)
		{
			int iD = building.ID;
			if (!CanCreateScaffoldingPartBuildingObject(iD))
			{
				return null;
			}
			BuildingObjectCollection buildingObjectCollection = GetBuildingObjectCollection(iD);
			if (buildingObjectCollection.RibbonBuildingObject != null)
			{
				return buildingObjectCollection.RibbonBuildingObject;
			}
			RibbonBuildingObject ribbonBuildingObject2 = (buildingObjectCollection.RibbonBuildingObject = CreateScaffoldingPartPrefab<RibbonBuildingObject>(building, GetRibbonPrefabName(building.Definition), position));
			AdjustObjectPosition(ribbonBuildingObject2.transform, building);
			return ribbonBuildingObject2;
		}

		internal bool Is8x8Building(BuildingDefinition buildingDef)
		{
			string platformPrefabName = GetPlatformPrefabName(buildingDef);
			if (string.IsNullOrEmpty(platformPrefabName))
			{
				return false;
			}
			return platformPrefabName.Contains("8x8");
		}

		private void AdjustObjectPosition(Transform transform, Building building)
		{
			if (Is8x8Building(building.Definition))
			{
				foreach (Transform item in transform)
				{
					if (item.name.Contains("_LOD"))
					{
						item.localPosition = new Vector3(3.5f, 0f, -3.5f);
						break;
					}
				}
				BoxCollider boxCollider = transform.GetComponent<Collider>() as BoxCollider;
				if (boxCollider != null)
				{
					boxCollider.center = new Vector3(3.5f, 0f, -3.5f);
				}
			}
			if (building is MasterPlanComponentBuilding)
			{
				transform.position += masterPlanService.GetComponentBuildingOffset(building.Definition.ID);
			}
		}

		internal DummyBuildingObject CreateDummyBuilding(BuildingDefinition buildingDefinition, Vector3 position)
		{
			int index = 0;
			ConnectableBuildingDefinition connectableBuildingDefinition = buildingDefinition as ConnectableBuildingDefinition;
			if (connectableBuildingDefinition != null)
			{
				index = connectableBuildingDefinition.GetDefaultPrefabIndex();
			}
			GameObject original = KampaiResources.Load<GameObject>(buildingDefinition.GetPrefab(index));
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			gameObject.name = "Dummy Building";
			Transform transform = gameObject.transform;
			transform.parent = base.transform;
			transform.position = position;
			transform.eulerAngles = Vector3.zero;
			DummyBuildingObject dummyBuildingObject = gameObject.AddComponent<DummyBuildingObject>();
			dummyBuildingObject.Init(buildingDefinition, definitionService);
			return dummyBuildingObject;
		}

		private void AddAnimation(GameObject go)
		{
			Animation animation = go.AddComponent<Animation>();
			animation.playAutomatically = false;
			AnimationClip clip = KampaiResources.Load<AnimationClip>("AnimBuildingReaction");
			animation.AddClip(clip, "AnimBuildingReaction");
			animation.clip = clip;
		}

		private void AddAnimEventHandlersToChildren(Transform trans, BuildingObject buildingObject)
		{
			AddAnimEventHandler(trans, buildingObject);
			foreach (Transform tran in trans)
			{
				AddAnimEventHandler(tran, buildingObject);
			}
		}

		private void AddAnimEventHandler(Transform transform, BuildingObject buildingObject)
		{
			GameObject gameObject = transform.gameObject;
			Animator component = gameObject.GetComponent<Animator>();
			AnimEventHandler animEventHandler = gameObject.GetComponent<AnimEventHandler>();
			if (component != null && animEventHandler == null)
			{
				AnimEventHandler animEventHandler2 = gameObject.AddComponent<AnimEventHandler>();
				animEventHandler2.Init(gameObject, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
				animEventHandler = animEventHandler2;
			}
			if (animEventHandler != null && !animEventHandler.IsStopBuildingAudioSignalSet && buildingObject != null)
			{
				animEventHandler.SetStopBuildingAudioInIdleStateSignal(buildingObject.StopBuildingAudioInIdleStateSignal);
			}
		}

		internal void DestroyScaffolding(GameObject scaffolding)
		{
			if (scaffolding != null)
			{
				UnityEngine.Object.Destroy(scaffolding);
			}
		}

		internal void SelectBuilding(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null && buildingObject is ConnectableBuildingObject)
			{
				connectableBuildingPickedUpSignal.Dispatch(buildingId);
			}
			if (buildingObject != null)
			{
				selectedBuilding = buildingObject;
				Vector3 position = selectedBuilding.transform.position;
				Vector3 position2 = new Vector3(position.x, position.y, position.z);
				selectedBuilding.transform.position = position2;
				selectedBuilding.SetBlendedColor(GameConstants.Building.VALID_PLACEMENT_COLOR);
				selectedBuilding.gameObject.SetLayerRecursively(14);
			}
			else
			{
				selectedBuilding = null;
			}
		}

		internal void DeselectBuilding(int buildingId)
		{
			if (selectedBuilding != null && selectedBuilding.ID == buildingId)
			{
				selectedBuilding.SetBlendedColor(Color.clear);
				Vector3 position = selectedBuilding.transform.position;
				selectedBuilding.transform.position = new Vector3(position.x, 0f, position.z);
				selectedBuilding.gameObject.SetLayerRecursively(9);
				selectedBuilding = null;
			}
		}

		internal void MoveBuilding(int buildingID, Vector3 position, bool isValidPosition)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingID);
			if (buildingObject != null)
			{
				BuildingObject buildingObject2 = buildingObject;
				Vector3 position2 = new Vector3(Mathf.Round(position.x), buildingObject2.transform.position.y, Mathf.Round(position.z));
				buildingObject2.transform.position = position2;
				buildingObject.SetBlendedColor((!isValidPosition) ? GameConstants.Building.INVALID_PLACEMENT_COLOR : GameConstants.Building.VALID_PLACEMENT_COLOR);
			}
		}

		internal void SetBuildingPosition(int buildingId, Vector3 position)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				buildingObject.transform.position = position;
			}
		}

		internal void PrepareTaskingMinionForMinionParty(TaskableBuilding taskableBuilding)
		{
			BuildingObject buildingObject = buildings[taskableBuilding.ID].BuildingObject;
			if (buildingObject != null)
			{
				TaskableBuildingObject taskableBuildingObject = buildingObject as TaskableBuildingObject;
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.ReleaseMinionsForParty(taskableBuilding);
				}
			}
		}

		internal void StartMinionTask(TaskableBuilding building, MinionObject minionObject, bool alreadyRushed)
		{
			int iD = building.ID;
			if (building is MignetteBuilding)
			{
				MignetteBuildingObject component = buildings[iD].BuildingObject.GetComponent<MignetteBuildingObject>();
				component.LoadMignetteAnimationControllers(animationControllers);
			}
			string minionController = building.Definition.AnimationDefinitions[0].MinionController;
			logger.Info("Using controller {0}", minionController);
			RuntimeAnimatorController controller = animationControllers[minionController];
			BuildingObject buildingObject = GetBuildingObject(iD);
			bool flag = false;
			if (buildingObject != null)
			{
				TaskableBuildingObject taskableBuildingObject = buildingObject as TaskableBuildingObject;
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.TrackChild(minionObject, controller, alreadyRushed);
					flag = taskableBuildingObject.IsGFXFaded();
				}
			}
			if (selectedBuilding == GetBuildingObject(iD) || flag)
			{
				minionObject.EnableRenderers(false);
			}
			else
			{
				minionObject.EnableRenderers(true);
			}
		}

		internal bool IsGagAnimationPlaying(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				GaggableBuildingObject gaggableBuildingObject = buildingObject as GaggableBuildingObject;
				if (gaggableBuildingObject != null)
				{
					return gaggableBuildingObject.IsGagAnimationPlaying();
				}
			}
			return false;
		}

		internal void StopGagAnimation(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				GaggableBuildingObject gaggableBuildingObject = buildingObject as GaggableBuildingObject;
				if (gaggableBuildingObject != null)
				{
					gaggableBuildingObject.StopGagAnimation();
				}
			}
		}

		internal void AppendMinionTaskAnimationCompleteCallback(MinionObject minionObject, Signal<int> callback)
		{
			minionObject.EnqueueAction(new SendIDSignalAction(minionObject, callback, logger));
		}

		internal bool TriggerGagAnimation(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				GaggableBuildingObject gaggableBuildingObject = buildingObject as GaggableBuildingObject;
				if (gaggableBuildingObject != null)
				{
					return gaggableBuildingObject.TriggerGagAnimation();
				}
			}
			return false;
		}

		internal void HarvestReady(int buildingId, int minionId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				TaskableBuildingObject taskableBuildingObject = buildingObject as TaskableBuildingObject;
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.RestMinion(minionId);
				}
			}
		}

		internal void UntrackMinion(int buildingId, int minionId, TaskableBuilding taskableBuilding)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				TaskableBuildingObject taskableBuildingObject = buildingObject as TaskableBuildingObject;
				if (taskableBuildingObject != null)
				{
					taskableBuildingObject.UntrackChild(minionId, taskableBuilding);
				}
			}
		}

		internal void UpdateBuildingState(int buildingId, BuildingState newState)
		{
			if (!buildings.ContainsKey(buildingId))
			{
				logger.Warning("No such building {0}", buildingId);
				return;
			}
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				AnimatingBuildingObject animatingBuildingObject = buildingObject as AnimatingBuildingObject;
				if (animatingBuildingObject != null)
				{
					animatingBuildingObject.SetState(newState);
				}
			}
		}

		private void LoadAnimationControllers(Building building)
		{
			AnimatingBuildingDefinition animatingBuildingDefinition = building.Definition as AnimatingBuildingDefinition;
			if (animatingBuildingDefinition == null)
			{
				return;
			}
			foreach (string item in animatingBuildingDefinition.AnimationControllerKeys())
			{
				if (!animationControllers.ContainsKey(item))
				{
					RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(item);
					if (runtimeAnimatorController == null)
					{
						logger.Fatal(FatalCode.BV_ILLEGAL_ANIMATION_CONTROLLER, item);
					}
					animationControllers.Add(item, runtimeAnimatorController);
				}
			}
		}

		public void setAnimParam(GameObject building, string paramName, object value)
		{
			if (building.transform.childCount < 1)
			{
				return;
			}
			foreach (Transform item in building.transform.GetChild(0))
			{
				Animator component = item.gameObject.GetComponent<Animator>();
				if (component != null)
				{
					if (value is bool)
					{
						component.SetBool(paramName, (bool)value);
					}
					else if (value is float)
					{
						component.SetFloat(paramName, (float)value);
					}
					else if (value is int)
					{
						component.SetInteger(paramName, (int)value);
					}
				}
			}
		}

		public void DestroyScaffoldingDelay(float delay, GameObject scaffolding)
		{
			Action<GameObject> action = DestroyScaffolding;
			StartCoroutine(Delay(delay, action, scaffolding));
		}

		public ICollection<ActionableObject> GetFadedObjects()
		{
			LinkedList<ActionableObject> linkedList = new LinkedList<ActionableObject>();
			foreach (KeyValuePair<int, BuildingObjectCollection> building in buildings)
			{
				BuildingObject buildingObject = building.Value.BuildingObject;
				if (!(buildingObject == null) && !(buildingObject is DebrisBuildingObject) && !(buildingObject is LandExpansionBuildingObject) && buildingObject.IsFaded())
				{
					linkedList.AddLast(buildingObject);
				}
			}
			return linkedList;
		}

		public ICollection<ActionableObject> GetOccludingObjects(int buildingId)
		{
			LinkedList<ActionableObject> linkedList = new LinkedList<ActionableObject>();
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject == null)
			{
				return linkedList;
			}
			GetOccludingObjects(buildingObject.transform.position, buildingId, linkedList, null);
			return linkedList;
		}

		public void GetOccludingObjects(Vector3 position, int buildingId, ICollection<ActionableObject> occludingObjects, ICollection<ActionableObject> nonOccludingObjects)
		{
			if (buildingId == 313)
			{
				return;
			}
			Vector3 vector = position;
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			Vector3 forward = mainCamera.transform.forward;
			Vector2 rhs = -new Vector2(forward.x, forward.z);
			rhs.Normalize();
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
			foreach (KeyValuePair<int, BuildingObjectCollection> building in buildings)
			{
				if (building.Key == buildingId)
				{
					continue;
				}
				BuildingObject buildingObject = building.Value.BuildingObject;
				if (!(buildingObject == null) && !(buildingObject is DebrisBuildingObject) && !(buildingObject is LandExpansionBuildingObject))
				{
					Vector3 position2 = buildingObject.transform.position;
					Vector2 vector3 = new Vector2(position2.x, position2.z);
					Vector2 lhs = vector3 - vector2;
					bool flag = true;
					if (buildingObject.GetComponent<Collider>() != null)
					{
						flag = GeometryUtility.TestPlanesAABB(planes, buildingObject.GetComponent<Collider>().bounds);
					}
					if (flag && Vector2.Dot(lhs, rhs) > 0f)
					{
						occludingObjects.Add(buildingObject);
					}
					else if (nonOccludingObjects != null)
					{
						nonOccludingObjects.Add(buildingObject);
					}
				}
			}
		}

		private IEnumerator Delay(float t, Action<GameObject> action, GameObject actionArg)
		{
			yield return new WaitForSeconds(t);
			action(actionArg);
		}

		internal void TweenBuildingToMenu(GameObject scaffolding, Vector3 destination, Action<GameObject> onTweenDone)
		{
			Go.to(scaffolding.transform, 0.5f, new GoTweenConfig().setEaseType(GoEaseType.Linear).scale(0f).rotation(new Vector3(90f, 90f, 90f))
				.position(destination)
				.onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
					if (onTweenDone != null)
					{
						onTweenDone(scaffolding);
					}
				}));
		}

		internal void ToInventory(int buildingID)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingID);
			if (!(buildingObject == null))
			{
				toInventoryBuildingObject = buildingObject;
				Vector3 destination = BuildingUtil.UIToWorldCoords(mainCamera, Vector3.zero);
				TweenBuildingToMenu(toInventoryBuildingObject.gameObject, destination, DestroyBuilding);
			}
		}

		internal void DestroyBuilding(GameObject inventoryGO)
		{
			CleanupBuilding(inventoryGO.GetComponent<BuildingObject>().ID);
		}

		internal void CleanupBuilding(int buildingId)
		{
			RemoveBuilding(buildingId);
		}

		public VFXScript GetVFXScriptForBuilding(int buildingId)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingId);
			if (buildingObject != null)
			{
				return buildingObject.GetComponent<VFXScript>();
			}
			return null;
		}

		public void PreloadBuildingMinionParty(int buildingID, IMinionPartyBuilding building, Location buildingPosition, MinionPartyType partyType)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingID);
			if (buildingObject == null || buildingObject.MinionPartyDecorations != null)
			{
				return;
			}
			string partyPrefab = building.GetPartyPrefab(partyType);
			GameObject gameObject = KampaiResources.Load<GameObject>(partyPrefab);
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				if (gameObject2 != null)
				{
					gameObject2.transform.position = new Vector3(buildingPosition.x, 0f, buildingPosition.y);
					gameObject2.transform.rotation = Quaternion.identity;
					gameObject2.transform.SetParent(buildingObject.transform);
					buildingObject.MinionPartyDecorations = gameObject2;
					gameObject2.SetActive(false);
				}
			}
			else
			{
				logger.Log(KampaiLogLevel.Debug, "Trying to instantiate null prefab: {0}", partyPrefab);
			}
		}

		public void StartBuildingMinionParty(int buildingID, IMinionPartyBuilding building, Location buildingPosition, MinionPartyType partyType)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingID);
			if (buildingObject == null)
			{
				logger.Error("Trying to display an Minion Building object that is not there.");
				return;
			}
			if (buildingObject.MinionPartyDecorations == null)
			{
				PreloadBuildingMinionParty(buildingID, building, buildingPosition, partyType);
			}
			buildingObject.MinionPartyDecorations.SetActive(true);
		}

		public void EndBuildingMinionParty(int buildingID)
		{
			BuildingObject buildingObject = GetBuildingObject(buildingID);
			if (!(buildingObject == null) && !(buildingObject.MinionPartyDecorations == null))
			{
				UnityEngine.Object.Destroy(buildingObject.MinionPartyDecorations);
				buildingObject.MinionPartyDecorations = null;
			}
		}
	}
}
