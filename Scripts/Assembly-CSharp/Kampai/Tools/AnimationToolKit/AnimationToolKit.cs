using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKit
	{
		public class SimplePathFinder : IPathFinder
		{
			public IList<Vector3> FindPath(Vector3 startPos, Vector3 goalPos, int modifier, bool forceDestination = false)
			{
				IList<Vector3> list = new List<Vector3>();
				list.Add(startPos);
				list.Add(goalPos);
				return list;
			}

			public bool IsOccupiable(Location location)
			{
				return true;
			}
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("AnimationToolKit") as IKampaiLogger;

		private int activeDefintionId;

		private int activeBuildingId;

		private int animatingMinions;

		private IList<int> coordinatedAnimations;

		private IDictionary<int, CharacterObject> characterDict;

		private IDictionary<int, BuildingObject> buildingDict;

		private Signal<int> minionDoneSignal;

		private Signal<CharacterObject, int> addToTikiBarSignal;

		private RuntimeAnimatorController walk;

		private Dictionary<string, RuntimeAnimatorController> animationControllers;

		private readonly Vector3 CENTER = new Vector3(3.4688f, 0f, -3.4687f);

		private readonly Vector3 RIGHT = new Vector3(8.4136f, 0f, 1.4761f);

		private readonly Vector3 LEFT = new Vector3(-1.306f, 0f, -8.2435f);

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject ContextView { get; set; }

		[Inject(AnimationToolKitElement.CONTEXT)]
		public ICrossContextCapable Context { get; set; }

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public AnimationToolkitModel model { get; set; }

		[Inject]
		public LoadInterfaceSignal LoadInterfaceSignal { get; set; }

		[Inject]
		public GenerateMinionSignal generateMinionSignal { get; set; }

		[Inject]
		public MinionCreatedSignal minionCreatedSignal { get; set; }

		[Inject]
		public AddMinionSignal AddMinionSignal { get; set; }

		[Inject]
		public RemoveMinionSignal RemoveMinionSignal { get; set; }

		[Inject]
		public PlayMinionAnimationSignal playGachaSignal { get; set; }

		[Inject]
		public GenerateVillainSignal generateVillainSignal { get; set; }

		[Inject]
		public VillainCreatedSignal villainCreatedSignal { get; set; }

		[Inject]
		public GenerateCharacterSignal generateCharacterSignal { get; set; }

		[Inject]
		public CharacterCreatedSignal characterCreatedSignal { get; set; }

		[Inject]
		public AddCharacterSignal addCharacterSignal { get; set; }

		[Inject]
		public RemoveCharacterSignal removeCharacterSignal { get; set; }

		[Inject]
		public EnableInterfaceSignal enableInterfaceSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		public AnimationToolKit()
		{
			coordinatedAnimations = new List<int>();
			characterDict = new Dictionary<int, CharacterObject>();
			buildingDict = new Dictionary<int, BuildingObject>();
			animationControllers = new Dictionary<string, RuntimeAnimatorController>();
			walk = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			animationControllers.Add("asm_minion_movement", walk);
			minionDoneSignal = new Signal<int>();
			addToTikiBarSignal = new Signal<CharacterObject, int>();
		}

		[PostConstruct]
		public void PostConstruct()
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Transform));
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform = (Transform)array[i];
				if (!(transform.parent != null) && !(transform == ContextView.transform))
				{
					model.Mode = AnimationToolKitMode.Building;
					AddBuilding(transform);
				}
			}
			LoadInterfaceSignal.Dispatch();
			playGachaSignal.AddListener(PlayMinionAnimation);
			minionDoneSignal.AddListener(GachaFinished);
			minionCreatedSignal.AddListener(MinionCreated);
			villainCreatedSignal.AddListener(VillainCreated);
			characterCreatedSignal.AddListener(CharacterCreated);
			GameObject gameObject = new GameObject("The Light");
			Light light = gameObject.AddComponent<Light>();
			gameObject.GetComponent<Light>().color = Color.white;
			gameObject.transform.position = new Vector3(4.52576f, 0.539923f, -2.41174f);
			gameObject.transform.rotation = Quaternion.Euler(new Vector3(50f, 330f, 0f));
			light.intensity = 0.4f;
			light.type = LightType.Directional;
		}

		public void ToggleOn(int toggleId)
		{
			AnimationToolKitMode mode = model.Mode;
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(toggleId);
			if (byInstanceId is StageBuilding)
			{
				model.Mode = AnimationToolKitMode.Stage;
			}
			else if (byInstanceId is TikiBarBuilding)
			{
				model.Mode = AnimationToolKitMode.TikiBar;
			}
			else if (byInstanceId is DebrisBuilding)
			{
				model.Mode = AnimationToolKitMode.Debris;
			}
			else if (byInstanceId != null)
			{
				model.Mode = AnimationToolKitMode.Building;
			}
			if (mode != model.Mode)
			{
				LoadInterfaceSignal.Dispatch();
			}
			if (model.Mode == AnimationToolKitMode.Building || model.Mode == AnimationToolKitMode.TikiBar || model.Mode == AnimationToolKitMode.Stage || model.Mode == AnimationToolKitMode.Debris)
			{
				activeBuildingId = toggleId;
			}
			else if (model.Mode == AnimationToolKitMode.Villain || model.Mode == AnimationToolKitMode.Character)
			{
				activeDefintionId = toggleId;
			}
		}

		public void AddMinion()
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
			if (byInstanceId != null)
			{
				int routeIndex = GetRouteIndex(byInstanceId);
				int stationCount = GetStationCount(buildingDict[activeBuildingId]);
				if (routeIndex < 0 || routeIndex >= stationCount)
				{
					return;
				}
			}
			generateMinionSignal.Dispatch();
		}

		public void AddVillain()
		{
			generateVillainSignal.Dispatch(activeDefintionId);
		}

		public void AddCharacter()
		{
			if (model.Mode == AnimationToolKitMode.Character)
			{
				generateCharacterSignal.Dispatch(activeDefintionId);
			}
			else if (model.Mode == AnimationToolKitMode.Stage)
			{
				generateCharacterSignal.Dispatch(70001);
			}
			else if (model.Mode == AnimationToolKitMode.TikiBar)
			{
				Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
				int routeIndex = GetRouteIndex(byInstanceId);
				int stationCount = GetStationCount(buildingDict[activeBuildingId]);
				if (routeIndex >= 0 && routeIndex < stationCount)
				{
					int type = ((routeIndex != 0) ? 70003 : 70000);
					generateCharacterSignal.Dispatch(type);
				}
			}
		}

		private void AddMinionToBuilding(MinionObject minionObj)
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
			int routeIndex = GetRouteIndex(byInstanceId);
			int stationCount = GetStationCount(buildingDict[activeBuildingId]);
			if (routeIndex >= 0 && routeIndex < stationCount)
			{
				AddMinionSignal.Dispatch(buildingDict[activeBuildingId], minionObj, routeIndex);
				if (model.Mode == AnimationToolKitMode.Debris)
				{
					minionObj.EnqueueAction(new DelegateAction(RemoveMinionFromBuilding, logger));
				}
			}
		}

		private void AddCharacterToBuilding(NamedCharacterObject characterObj)
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
			int routeIndex = GetRouteIndex(byInstanceId);
			int stationCount = GetStationCount(buildingDict[activeBuildingId]);
			if (routeIndex < 0 || routeIndex >= stationCount)
			{
				return;
			}
			addCharacterSignal.Dispatch(buildingDict[activeBuildingId], characterObj, routeIndex);
			if (model.Mode == AnimationToolKitMode.TikiBar)
			{
				PhilView philView = characterObj as PhilView;
				if (philView != null)
				{
					philView.AnimSignal.AddListener(PlayTikiBarAnimation);
					TeleportCharacterToTikiBarSignal instance = Context.injectionBinder.GetInstance<TeleportCharacterToTikiBarSignal>();
					instance.AddListener(AddCharacterToTikiBarActions);
					philView.SitAtBar(true, instance);
				}
			}
		}

		private void AddCharacterToTikiBarActions(CharacterObject characterObject, int routeIndex)
		{
			TikiBarBuildingObjectView tikiBarBuildingObjectView = buildingDict[activeBuildingId] as TikiBarBuildingObjectView;
			if (tikiBarBuildingObjectView != null)
			{
				addToTikiBarSignal.Dispatch(characterObject, routeIndex);
			}
		}

		private void PlayTikiBarAnimation(string animation, Type type, object obj)
		{
			TikiBarBuildingObjectView tikiBarBuildingObjectView = buildingDict[activeBuildingId] as TikiBarBuildingObjectView;
			if (tikiBarBuildingObjectView != null)
			{
				tikiBarBuildingObjectView.PlayAnimation(animation, type, obj);
			}
		}

		private void MinionCreated(MinionObject minionObj)
		{
			minionObj.ShelveActionQueue();
			characterDict.Add(minionObj.ID, minionObj);
			ICollection<Building> instancesByType = PlayerService.GetInstancesByType<Building>();
			if (instancesByType.Count > 0)
			{
				AddMinionToBuilding(minionObj);
			}
			else
			{
				ResetCharacterPosition();
			}
		}

		private void VillainCreated(VillainView villainObj)
		{
			villainObj.ShelveActionQueue();
			characterDict.Add(villainObj.ID, villainObj);
			ResetCharacterPosition();
		}

		private void CharacterCreated(NamedCharacterObject characterObj)
		{
			characterObj.ShelveActionQueue();
			characterDict.Add(characterObj.ID, characterObj);
			if (model.Mode == AnimationToolKitMode.Stage || model.Mode == AnimationToolKitMode.TikiBar)
			{
				AddCharacterToBuilding(characterObj);
			}
			else if (model.Mode == AnimationToolKitMode.Character)
			{
				ResetCharacterPosition();
			}
		}

		public void RemoveMinion()
		{
			if (buildingDict.Count > 0)
			{
				RemoveMinionFromBuilding();
				return;
			}
			List<int> list = new List<int>(characterDict.Keys);
			if (list.Count > 0)
			{
				UnityEngine.Object.DestroyImmediate(characterDict[list[0]].gameObject);
				characterDict.Remove(list[0]);
				ResetCharacterPosition();
			}
		}

		public void RemoveVillain()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(activeDefintionId);
			foreach (NamedCharacter item in byDefinitionId)
			{
				CharacterObject characterObject = characterDict[item.ID];
				UnityEngine.Object.DestroyImmediate(characterObject.gameObject);
				PlayerService.Remove(item);
				characterDict.Remove(item.ID);
			}
		}

		public void RemoveCharacter()
		{
			int key = -1;
			int num = -1;
			int num2 = -1;
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
			if (buildingDict.ContainsKey(activeBuildingId))
			{
				num = GetRouteIndex(byInstanceId);
				if (model.Mode != AnimationToolKitMode.Stage)
				{
					num--;
				}
				num2 = GetStationCount(buildingDict[activeBuildingId]);
				if (num < 0 || num >= num2)
				{
					return;
				}
			}
			int definitionId = ((model.Mode != AnimationToolKitMode.Character) ? 70001 : activeDefintionId);
			if (model.Mode == AnimationToolKitMode.TikiBar)
			{
				definitionId = ((num != 0) ? 70003 : 70000);
			}
			NamedCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<NamedCharacter>(definitionId);
			if (model.Mode == AnimationToolKitMode.Stage || model.Mode == AnimationToolKitMode.TikiBar)
			{
				key = GetMinionId(byInstanceId, num);
			}
			else if (model.Mode == AnimationToolKitMode.Character && firstInstanceByDefinitionId != null)
			{
				key = firstInstanceByDefinitionId.ID;
			}
			if (!characterDict.ContainsKey(key))
			{
				return;
			}
			CharacterObject characterObject = characterDict[key];
			if (buildingDict.ContainsKey(activeBuildingId))
			{
				removeCharacterSignal.Dispatch(buildingDict[activeBuildingId], characterObject);
			}
			if (model.Mode == AnimationToolKitMode.TikiBar)
			{
				TikiBarBuildingObjectView tikiBarBuildingObjectView = buildingDict[activeBuildingId] as TikiBarBuildingObjectView;
				if (tikiBarBuildingObjectView != null)
				{
					switch (num)
					{
					case 0:
						tikiBarBuildingObjectView.SetAnimBool("bartender_IsSeated", false);
						break;
					case 1:
						tikiBarBuildingObjectView.SetAnimBool("pos1_IsSeated", false);
						break;
					case 2:
						tikiBarBuildingObjectView.SetAnimBool("pos2_IsSeated", false);
						break;
					}
				}
			}
			PlayerService.Remove(firstInstanceByDefinitionId);
			UnityEngine.Object.Destroy(characterObject.gameObject);
			characterDict.Remove(characterObject.ID);
		}

		public void RemoveMinionFromBuilding()
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(activeBuildingId);
			int num = GetRouteIndex(byInstanceId) - 1;
			int stationCount = GetStationCount(buildingDict[activeBuildingId]);
			if (num >= 0 && num < stationCount)
			{
				int minionId = GetMinionId(byInstanceId, num);
				if (characterDict.ContainsKey(minionId))
				{
					MinionObject minionObject = characterDict[minionId] as MinionObject;
					RemoveMinionSignal.Dispatch(buildingDict[activeBuildingId], minionObject);
					UnityEngine.Object.Destroy(minionObject.gameObject);
					characterDict.Remove(minionObject.ID);
				}
			}
		}

		private void ResetCharacterPosition()
		{
			float num = 1f / Convert.ToSingle(characterDict.Count + 1);
			float num2 = num;
			foreach (CharacterObject value in characterDict.Values)
			{
				value.gameObject.transform.position = Vector3.Lerp(LEFT, RIGHT, num2);
				num2 += num;
				if (model.Mode != AnimationToolKitMode.Villain)
				{
					Vector3 position = Camera.main.transform.position;
					value.gameObject.transform.LookAt(new Vector3(position.x, 0f, position.z));
				}
			}
		}

		public void LoopAnimation()
		{
			if (buildingDict.ContainsKey(activeBuildingId))
			{
				AnimatingBuildingObject animatingBuildingObject = buildingDict[activeBuildingId] as AnimatingBuildingObject;
				if (animatingBuildingObject != null)
				{
					animatingBuildingObject.StartAnimating();
				}
			}
		}

		public void PlayMinionAnimation(AnimationDefinition def)
		{
			if (animatingMinions > 0)
			{
				logger.Error("Wait for the previous animation to finish.");
				return;
			}
			if (characterDict.Count == 0)
			{
				logger.Error("Add minions first.");
				return;
			}
			GachaAnimationDefinition gachaAnimationDefinition = def as GachaAnimationDefinition;
			if (gachaAnimationDefinition != null)
			{
				if (gachaAnimationDefinition.Minions <= 0)
				{
					if (model.Mode == AnimationToolKitMode.Minion)
					{
						StartNonCoordinatedGacha(gachaAnimationDefinition, characterDict.Keys);
					}
					else if (model.Mode == AnimationToolKitMode.Character)
					{
						StartNonCoordinatedGacha(gachaAnimationDefinition, characterDict.Keys);
					}
					return;
				}
				List<ActionableObject> list = new List<ActionableObject>();
				if (model.Mode == AnimationToolKitMode.Minion)
				{
					foreach (MinionObject value in characterDict.Values)
					{
						list.Add(value);
					}
				}
				else if (model.Mode == AnimationToolKitMode.Character)
				{
					foreach (NamedCharacterObject value2 in characterDict.Values)
					{
						list.Add(value2);
					}
				}
				StartCoordinatedGacha(gachaAnimationDefinition, list);
			}
			else
			{
				MinionAnimationDefinition definition = def as MinionAnimationDefinition;
				if (model.Mode == AnimationToolKitMode.Minion)
				{
					StartMinionAnimation(definition, characterDict.Keys);
				}
				else if (model.Mode == AnimationToolKitMode.Character)
				{
					StartMinionAnimation(definition, characterDict.Keys);
				}
			}
		}

		public void GagAnimation()
		{
			if (buildingDict.ContainsKey(activeBuildingId))
			{
				GaggableBuildingObject gaggableBuildingObject = buildingDict[activeBuildingId] as GaggableBuildingObject;
				if (gaggableBuildingObject != null)
				{
					gaggableBuildingObject.TriggerGagAnimation();
				}
			}
		}

		public void WaitAnimation()
		{
			if (!buildingDict.ContainsKey(activeBuildingId))
			{
				return;
			}
			TaskableBuilding byInstanceId = PlayerService.GetByInstanceId<TaskableBuilding>(activeBuildingId);
			if (byInstanceId == null)
			{
				return;
			}
			TaskableBuildingObject taskableBuildingObject = buildingDict[activeBuildingId] as TaskableBuildingObject;
			if (!(taskableBuildingObject == null))
			{
				int num = byInstanceId.GetMinionsInBuilding() - 1;
				if (num >= 0 && num < taskableBuildingObject.GetNumberOfStations())
				{
					int minionByIndex = byInstanceId.GetMinionByIndex(num);
					taskableBuildingObject.RestMinion(minionByIndex);
				}
			}
		}

		public void VillainIntroAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(activeDefintionId);
			foreach (NamedCharacter item in byDefinitionId)
			{
				VillainView villainView = characterDict[item.ID] as VillainView;
				villainView.PlayWelcome();
			}
		}

		public void VillainCabanaAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(activeDefintionId);
			foreach (NamedCharacter item in byDefinitionId)
			{
				VillainView villainView = characterDict[item.ID] as VillainView;
				villainView.GotoCabana(0, villainView.transform);
			}
		}

		public void VillainFarewellAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(activeDefintionId);
			foreach (NamedCharacter item in byDefinitionId)
			{
				VillainView villainView = characterDict[item.ID] as VillainView;
				villainView.PlayFarewell();
			}
		}

		public void TikiBarCelebrateAnimation()
		{
			NamedCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<NamedCharacter>(70000);
			PhilView philView = characterDict[firstInstanceByDefinitionId.ID] as PhilView;
			if (!(philView == null))
			{
				philView.Celebrate();
			}
		}

		public void TikiBarAttentionAnimation()
		{
			NamedCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<NamedCharacter>(70000);
			PhilView philView = characterDict[firstInstanceByDefinitionId.ID] as PhilView;
			if (!(philView == null))
			{
				bool flag = philView.GetAnimBool("AttentionStart") || philView.GetAnimBool("bartender_IsGetAttention");
				philView.GetAttention(!flag);
			}
		}

		public void TikiBarAlternateAnimation(int alternateIndex)
		{
			NamedCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<NamedCharacter>(70000);
			PhilView philView = characterDict[firstInstanceByDefinitionId.ID] as PhilView;
			if (!(philView == null))
			{
				philView.SetAnimInteger("randomizer", alternateIndex);
				philView.AnimSignal.Dispatch("randomizer", typeof(int), alternateIndex);
				philView.SetAnimBool("PlayAlternate", true);
				philView.AnimSignal.Dispatch("PlayAlternate", typeof(bool), true);
			}
		}

		public void StuartStageIdleAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(70001);
			foreach (NamedCharacter item in byDefinitionId)
			{
				StuartView stuartView = characterDict[item.ID] as StuartView;
				if (stuartView != null)
				{
					stuartView.GetOnStage(!stuartView.GetAnimBool("goToStage"));
				}
			}
		}

		public void StuartPerformAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(70001);
			foreach (NamedCharacter item in byDefinitionId)
			{
				StuartView stuartView = characterDict[item.ID] as StuartView;
				if (stuartView != null)
				{
					stuartView.Perform(new SignalCallback<Signal>(new Signal()));
				}
			}
		}

		public void StuartCelebrateAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(70001);
			foreach (NamedCharacter item in byDefinitionId)
			{
				StuartView stuartView = characterDict[item.ID] as StuartView;
				if (stuartView != null)
				{
					stuartView.StartingState(StuartStageAnimationType.CELEBRATE);
				}
			}
		}

		public void StuartAttentionAnimation()
		{
			ICollection<NamedCharacter> byDefinitionId = PlayerService.GetByDefinitionId<NamedCharacter>(70001);
			foreach (NamedCharacter item in byDefinitionId)
			{
				StuartView stuartView = characterDict[item.ID] as StuartView;
				if (stuartView != null)
				{
					stuartView.GetAttention(!stuartView.GetAnimBool("isGetAttention"));
				}
			}
		}

		private void AddBuilding(Transform transform)
		{
			BuildingDefinition buildingDefinition = GetBuildingDefinition(transform.name);
			if (buildingDefinition == null)
			{
				logger.Error(string.Format("Building ({0}) not found! Does the prefab name match the JSON?", transform.name));
				return;
			}
			TaskableBuildingDefinition taskableBuildingDefinition = buildingDefinition as TaskableBuildingDefinition;
			if (taskableBuildingDefinition != null)
			{
				taskableBuildingDefinition.RouteToSlot = true;
			}
			LoadAnimationControllers(buildingDefinition);
			LoadAnimationEventHandler(transform);
			Building building = buildingDefinition.BuildBuilding();
			BuildingObject buildingObject = building.AddBuildingObject(transform.gameObject);
			building.Location = new Location((int)buildingObject.transform.position.x, (int)buildingObject.transform.position.z);
			PlayerService.Add(building);
			buildingObject.Init(building, logger, animationControllers, DefinitionService);
			buildingDict.Add(building.ID, buildingObject);
		}

		private BuildingDefinition GetBuildingDefinition(string prefabName)
		{
			BuildingDefinition buildingDefinition = null;
			Dictionary<int, Definition> allDefinitions = DefinitionService.GetAllDefinitions();
			foreach (KeyValuePair<int, Definition> item in allDefinitions)
			{
				Definition value = item.Value;
				buildingDefinition = value as BuildingDefinition;
				if (buildingDefinition != null && buildingDefinition.GetPrefab().Contains(prefabName))
				{
					return buildingDefinition;
				}
			}
			return buildingDefinition;
		}

		private void LoadAnimationControllers(BuildingDefinition buildingDefinition)
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

		private void LoadAnimationEventHandler(Transform transform)
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

		private void GachaFinished(int minionId)
		{
			if (characterDict.ContainsKey(minionId))
			{
				CharacterObject characterObject = characterDict[minionId];
				characterObject.ClearActionQueue();
				animatingMinions--;
				if (animatingMinions == 0)
				{
					enableInterfaceSignal.Dispatch(true);
				}
				ResetCharacterPosition();
			}
		}

		private void CacheAnimators(AnimationDefinition animationDef, bool cycle = false)
		{
			MinionAnimationDefinition minionAnimationDefinition = animationDef as MinionAnimationDefinition;
			if (minionAnimationDefinition != null && !animationControllers.ContainsKey(minionAnimationDefinition.StateMachine))
			{
				animationControllers.Add(minionAnimationDefinition.StateMachine, KampaiResources.Load<RuntimeAnimatorController>(minionAnimationDefinition.StateMachine));
			}
			GachaAnimationDefinition gachaAnimationDefinition = animationDef as GachaAnimationDefinition;
			if (gachaAnimationDefinition == null)
			{
				return;
			}
			AnimationDefinition definition;
			if (!DefinitionService.TryGet<AnimationDefinition>(gachaAnimationDefinition.AnimationID, out definition))
			{
				logger.Log(KampaiLogLevel.Error, "Undefined animation ID {0} for animation {1}", gachaAnimationDefinition.AnimationID, animationDef.ID);
			}
			else
			{
				CacheAnimators(definition);
			}
			if (gachaAnimationDefinition.AnimationAlternate == null || cycle)
			{
				return;
			}
			DefinitionGroup definition2 = null;
			if (!DefinitionService.TryGet<DefinitionGroup>(gachaAnimationDefinition.AnimationAlternate.GroupID, out definition2))
			{
				logger.Log(KampaiLogLevel.Error, "Undefined group ID {0} for animation {1}", definition2.Group, animationDef.ID);
				return;
			}
			foreach (int item in definition2.Group)
			{
				AnimationDefinition animationDef2 = DefinitionService.Get<AnimationDefinition>(item);
				CacheAnimators(animationDef2, true);
			}
		}

		private void StartMinionAnimation(MinionAnimationDefinition definition, ICollection<int> minionIds)
		{
			CacheAnimators(definition);
			foreach (int minionId in minionIds)
			{
				CharacterObject characterObject = characterDict[minionId];
				characterObject.EnqueueAction(new SetAnimatorAction(characterObject, animationControllers[definition.StateMachine], logger, definition.arguments));
				characterObject.EnqueueAction(new DelayAction(characterObject, definition.AnimationSeconds, logger));
				characterObject.EnqueueAction(new SendIDSignalAction(characterObject, minionDoneSignal, logger));
				animatingMinions++;
				if (animatingMinions == 1)
				{
					enableInterfaceSignal.Dispatch(false);
				}
			}
		}

		private void StartNonCoordinatedGacha(GachaAnimationDefinition animationDef, ICollection<int> minionIds)
		{
			CacheAnimators(animationDef);
			MinionManagerView.Knuckleheaddedness knuckleheaddedness = new MinionManagerView.Knuckleheaddedness(animationDef, minionIds, randomService);
			bool mute = false;
			foreach (int minionId in minionIds)
			{
				CharacterObject characterObject = characterDict[minionId];
				MinionObject minionObject = characterObject as MinionObject;
				if (minionObject != null)
				{
					float animationDelay = knuckleheaddedness.DelayTime(minionId, randomService);
					characterObject.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.Active, logger));
					MinionManagerView.SetupSingleMinionGacha(randomService, DefinitionService, minionObject, animationControllers, animationDef, animationDelay, null, logger, ref mute);
					characterObject.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.Inactive, logger));
				}
				characterObject.EnqueueAction(new SetAnimatorAction(characterObject, walk, logger));
				characterObject.EnqueueAction(new SendIDSignalAction(characterObject, minionDoneSignal, logger));
				animatingMinions++;
				if (animatingMinions == 1)
				{
					enableInterfaceSignal.Dispatch(false);
				}
			}
		}

		private void StartCoordinatedGacha(GachaAnimationDefinition gacha, ICollection<ActionableObject> minions)
		{
			CacheAnimators(gacha);
			if (!coordinatedAnimations.Contains(gacha.ID))
			{
				UnityEngine.Object @object = KampaiResources.Load(gacha.Prefab);
				if (@object == null)
				{
					logger.Error("Can't load prefab: " + gacha.Prefab);
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
				if (gameObject == null)
				{
					logger.Error("Can't instantiate prefab: " + gacha.Prefab);
				}
				UnityEngine.Object.Destroy(gameObject);
				coordinatedAnimations.Add(gacha.ID);
			}
			if (model.Mode == AnimationToolKitMode.Minion)
			{
				MinionManagerView.SetupCoordinatedMinionGacha(ContextView, DefinitionService, gacha, animationControllers, minions, CENTER, new SimplePathFinder(), logger);
			}
			foreach (CharacterObject minion in minions)
			{
				MinionObject minionObject = minion as MinionObject;
				if (minionObject != null)
				{
					minionObject.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.Active, logger));
					minionObject.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.Inactive, logger));
				}
				minion.EnqueueAction(new SetAnimatorAction(minion, walk, logger));
				minion.EnqueueAction(new SendIDSignalAction(minion, minionDoneSignal, logger));
				animatingMinions++;
				if (animatingMinions == 1)
				{
					enableInterfaceSignal.Dispatch(false);
				}
			}
		}

		private int GetRouteIndex(Building building)
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			LeisureBuilding leisureBuilding = building as LeisureBuilding;
			StageBuilding stageBuilding = building as StageBuilding;
			TikiBarBuilding tikiBarBuilding = building as TikiBarBuilding;
			int result = -1;
			if (taskableBuilding != null)
			{
				result = taskableBuilding.GetMinionsInBuilding();
			}
			else if (leisureBuilding != null)
			{
				result = leisureBuilding.GetMinionsInBuilding();
			}
			else if (tikiBarBuilding != null)
			{
				result = tikiBarBuilding.GetMinionsInBuilding();
			}
			else if (stageBuilding != null)
			{
				result = 0;
			}
			return result;
		}

		private int GetStationCount(BuildingObject buildingObject)
		{
			RoutableBuildingObject routableBuildingObject = buildingObject as RoutableBuildingObject;
			int num = -1;
			if (routableBuildingObject != null)
			{
				num = routableBuildingObject.GetNumberOfStations();
			}
			if (buildingObject is TikiBarBuildingObjectView)
			{
				num -= 3;
			}
			return num;
		}

		private int GetMinionId(Building building, int routeIndex)
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			LeisureBuilding leisureBuilding = building as LeisureBuilding;
			StageBuilding stageBuilding = building as StageBuilding;
			int definitionId = 70001;
			if (model.Mode == AnimationToolKitMode.TikiBar)
			{
				definitionId = ((routeIndex != 0) ? 70003 : 70000);
			}
			NamedCharacter firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<NamedCharacter>(definitionId);
			int result = -1;
			if (taskableBuilding != null)
			{
				result = taskableBuilding.GetMinionByIndex(routeIndex);
			}
			else if (leisureBuilding != null)
			{
				result = leisureBuilding.GetMinionByIndex(routeIndex);
			}
			else if (stageBuilding != null && firstInstanceByDefinitionId != null)
			{
				result = firstInstanceByDefinitionId.ID;
			}
			return result;
		}
	}
}
