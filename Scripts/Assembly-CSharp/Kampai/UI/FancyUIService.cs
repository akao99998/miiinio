using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using Kampai.Util.AI;
using Kampai.Util.Graphics;
using UnityEngine;

namespace Kampai.UI
{
	public class FancyUIService : IFancyUIService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("FancyUIService") as IKampaiLogger;

		private Dictionary<string, RuntimeAnimatorController> animationControllers;

		[Inject]
		public IDummyCharacterBuilder builder { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public NetworkLostOpenSignal networkLostOpenSignal { get; set; }

		[Inject]
		public NetworkLostCloseSignal networkLostCloseSignal { get; set; }

		public DummyCharacterObject CreateCharacter(DummyCharacterType type, DummyCharacterAnimationState startingState, Transform parent, Vector3 villainScale, Vector3 villainPositionOffset, int prestigeDefinitionID = 0, bool isHighLOD = true, bool isAudible = true, bool adjustMaterial = false)
		{
			PrestigeDefinition prestigeDefinition = null;
			if (prestigeDefinitionID != 0)
			{
				prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionID);
			}
			switch (type)
			{
			case DummyCharacterType.Minion:
			{
				IList<MinionDefinition> all = definitionService.GetAll<MinionDefinition>();
				int count = all.Count;
				int index = randomService.NextInt(count);
				MinionDefinition def = all[index];
				Minion minion = new Minion(def);
				int num = 99;
				if (prestigeDefinition != null)
				{
					Prestige prestige = prestigeService.GetPrestige(prestigeDefinitionID);
					if (prestige != null)
					{
						minion.PrestigeId = prestige.ID;
					}
					num = prestigeDefinition.TrackedDefinitionID;
				}
				CostumeItemDefinition costumeItemDefinition = definitionService.Get<CostumeItemDefinition>(num);
				if (costumeItemDefinition == null)
				{
					logger.Fatal(FatalCode.PS_MISSING_MINION_COSTUME, "ERROR: Minion costume ID: {0} - Could not create costume!!!", num);
				}
				DummyCharacterObject dummyCharacterObject2 = builder.BuildMinion(minion, costumeItemDefinition, parent, isHighLOD, villainScale, villainPositionOffset);
				if (!isAudible)
				{
					dummyCharacterObject2.ExecuteAction(new MuteAction(dummyCharacterObject2, true, logger));
				}
				if (adjustMaterial)
				{
					dummyCharacterObject2.SetStenciledShader();
				}
				dummyCharacterObject2.StartingState(startingState);
				dummyCharacterObject2.SetUpWifiListeners(networkLostOpenSignal, networkLostCloseSignal);
				return dummyCharacterObject2;
			}
			case DummyCharacterType.NamedCharacter:
			{
				NamedCharacterDefinition namedCharacterDefinition = definitionService.Get<NamedCharacterDefinition>(prestigeDefinition.TrackedDefinitionID);
				if (namedCharacterDefinition != null)
				{
					DummyCharacterObject dummyCharacterObject = CreateNamedCharacter(namedCharacterDefinition, parent, villainScale, villainPositionOffset, isHighLOD);
					if (!isAudible)
					{
						dummyCharacterObject.ExecuteAction(new MuteAction(dummyCharacterObject, true, logger));
					}
					if (adjustMaterial)
					{
						dummyCharacterObject.SetStenciledShader();
					}
					dummyCharacterObject.StartingState(startingState);
					dummyCharacterObject.SetUpWifiListeners(networkLostOpenSignal, networkLostCloseSignal);
					return dummyCharacterObject;
				}
				break;
			}
			}
			return null;
		}

		public DummyCharacterType GetCharacterType(int prestigeDefinitionID)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(prestigeDefinitionID);
			DummyCharacterType result = DummyCharacterType.Minion;
			if (prestigeDefinition.Type == PrestigeType.Minion)
			{
				Definition definition = definitionService.Get<Definition>(prestigeDefinition.TrackedDefinitionID);
				if (definition is NamedCharacterDefinition)
				{
					result = DummyCharacterType.NamedCharacter;
				}
			}
			else
			{
				result = DummyCharacterType.NamedCharacter;
			}
			return result;
		}

		private DummyCharacterObject CreateNamedCharacter(NamedCharacterDefinition namedCharacterDefinition, Transform parent, Vector3 villainScale, Vector3 villainPositionOffset, bool isHighLOD)
		{
			NamedCharacter namedCharacter = null;
			PhilCharacterDefinition philCharacterDefinition = namedCharacterDefinition as PhilCharacterDefinition;
			if (philCharacterDefinition != null)
			{
				namedCharacter = new PhilCharacter(philCharacterDefinition);
			}
			BobCharacterDefinition bobCharacterDefinition = namedCharacterDefinition as BobCharacterDefinition;
			if (bobCharacterDefinition != null)
			{
				namedCharacter = new BobCharacter(bobCharacterDefinition);
			}
			KevinCharacterDefinition kevinCharacterDefinition = namedCharacterDefinition as KevinCharacterDefinition;
			if (kevinCharacterDefinition != null)
			{
				namedCharacter = new KevinCharacter(kevinCharacterDefinition);
			}
			StuartCharacterDefinition stuartCharacterDefinition = namedCharacterDefinition as StuartCharacterDefinition;
			if (stuartCharacterDefinition != null)
			{
				namedCharacter = new StuartCharacter(stuartCharacterDefinition);
			}
			SpecialEventCharacterDefinition specialEventCharacterDefinition = namedCharacterDefinition as SpecialEventCharacterDefinition;
			if (specialEventCharacterDefinition != null)
			{
				namedCharacter = new SpecialEventCharacter(specialEventCharacterDefinition);
			}
			VillainDefinition villainDefinition = namedCharacterDefinition as VillainDefinition;
			if (villainDefinition != null)
			{
				namedCharacter = new Villain(villainDefinition);
			}
			TSMCharacterDefinition tSMCharacterDefinition = namedCharacterDefinition as TSMCharacterDefinition;
			if (tSMCharacterDefinition != null)
			{
				namedCharacter = new TSMCharacter(tSMCharacterDefinition);
			}
			DummyCharacterObject dummyCharacterObject = builder.BuildNamedChacter(namedCharacter, parent, isHighLOD, villainScale, villainPositionOffset);
			dummyCharacterObject.SetUpWifiListeners(networkLostOpenSignal, networkLostCloseSignal);
			return dummyCharacterObject;
		}

		public DummyCharacterObject BuildMinion(int minionId, DummyCharacterAnimationState startingState, Transform parent, bool isHighLOD = true, bool isAudible = true, int minionLevel = 0)
		{
			MinionDefinition def = definitionService.Get<MinionDefinition>(minionId);
			Minion minion = new Minion(def);
			minion.Level = minionLevel;
			CostumeItemDefinition costumeItemDefinition = definitionService.Get<CostumeItemDefinition>(minion.GetCostumeId(playerService, definitionService));
			if (costumeItemDefinition == null)
			{
				logger.Fatal(FatalCode.PS_MISSING_DEFAULT_COSTUME, "ERROR: Minion costume ID: {0} - Could not create default costume!!!", 99);
			}
			DummyCharacterObject dummyCharacterObject = builder.BuildMinion(minion, costumeItemDefinition, parent, isHighLOD, Vector3.one, Vector3.one);
			if (!isAudible)
			{
				dummyCharacterObject.ExecuteAction(new MuteAction(dummyCharacterObject, true, logger));
			}
			dummyCharacterObject.StartBundlePackAnimation();
			dummyCharacterObject.SetUpWifiListeners(networkLostOpenSignal, networkLostCloseSignal);
			return dummyCharacterObject;
		}

		public void SetKampaiImage(KampaiImage image, string iconPath, string maskPath)
		{
			if (string.IsNullOrEmpty(iconPath))
			{
				iconPath = "btn_Main01_fill";
			}
			image.sprite = UIUtils.LoadSpriteFromPath(iconPath);
			if (string.IsNullOrEmpty(maskPath))
			{
				maskPath = "btn_Main01_mask";
			}
			image.maskSprite = UIUtils.LoadSpriteFromPath(maskPath);
		}

		public BuildingObject CreateDummyBuildingObject(BuildingDefinition buildingDefinition, GameObject parent, out Building building, IList<MinionObject> minionsList = null, bool isAudible = true)
		{
			string prefab = buildingDefinition.GetPrefab();
			int iD = buildingDefinition.ID;
			if (string.IsNullOrEmpty(prefab))
			{
				logger.Error("Building Definition {0} doesn't have a Prefab defined, returning null", iD);
				building = null;
				return null;
			}
			if (animationControllers == null)
			{
				animationControllers = new Dictionary<string, RuntimeAnimatorController>();
			}
			GameObject gameObject = KampaiResources.Load<GameObject>(prefab);
			if (gameObject == null)
			{
				logger.Error("Building Prefab doesn't exist for buildingDefinition {0}, Prefab: {1}", iD, prefab);
				building = null;
				return null;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			building = buildingDefinition.BuildBuilding();
			if (gameObject2 == null)
			{
				logger.Error("Could not create dummy building object from building definition id: {0}", iD);
				return null;
			}
			gameObject2.name = buildingDefinition.LocalizedKey;
			Transform transform = gameObject2.transform;
			Vector3 localEulerAngles = (transform.localPosition = Vector3.zero);
			transform.localEulerAngles = localEulerAngles;
			localEulerAngles = (transform.eulerAngles = Vector3.one);
			transform.localScale = localEulerAngles;
			transform.SetParent(parent.transform, false);
			gameObject2.SetLayerRecursively(5);
			return BuildingObjectSetup(gameObject2, buildingDefinition, parent, minionsList, building, isAudible);
		}

		public void ReleaseBuildingObject(BuildingObject buildingObj, Building building, IList<MinionObject> minionsList = null)
		{
			if (!(buildingObj != null) || !(buildingObj.gameObject != null))
			{
				return;
			}
			LeisureBuildingObjectView component = buildingObj.GetComponent<LeisureBuildingObjectView>();
			if (component != null)
			{
				component.FreeAllMinions();
			}
			if (minionsList != null)
			{
				TaskableBuilding taskableBuilding = building as TaskableBuilding;
				for (int i = 0; i < minionsList.Count; i++)
				{
					if (taskableBuilding != null)
					{
						taskableBuilding.RemoveMinion(minionsList[i].ID, 0);
					}
					Object.Destroy(minionsList[i].gameObject);
				}
			}
			buildingObj.Cleanup();
			Object.Destroy(buildingObj.gameObject);
		}

		private BuildingObject BuildingObjectSetup(GameObject prefabInstance, BuildingDefinition buildingDefinition, GameObject parent, IList<MinionObject> minionsList, Building building, bool isAudible)
		{
			BuildingObject buildingObject = building.AddBuildingObject(prefabInstance.gameObject);
			LoadBuildingAnimationControllers(buildingDefinition);
			LoadBuildingAnimationEventHandler(prefabInstance.transform);
			if (!isAudible)
			{
				buildingObject.ExecuteAction(new MuteAction(buildingObject, true, logger));
			}
			buildingObject.Init(building, logger, animationControllers, definitionService);
			RoutableBuildingObject routableBuildingObject = buildingObject as RoutableBuildingObject;
			if (minionsList != null && routableBuildingObject != null)
			{
				for (int i = 0; i < routableBuildingObject.GetNumberOfStations(); i++)
				{
					MinionObject minionObject = BuildBuildingMinionObject(parent);
					if (!isAudible)
					{
						minionObject.ExecuteAction(new MuteAction(minionObject, true, logger));
					}
					minionObject.ID = i;
					minionsList.Add(minionObject);
					AnimateBuildingsMinion(buildingObject, building, minionObject);
				}
			}
			AnimatingBuildingObject animatingBuildingObject = buildingObject as AnimatingBuildingObject;
			if (animatingBuildingObject != null)
			{
				animatingBuildingObject.StartAnimating();
			}
			TaskableBuildingObject taskableBuildingObj = buildingObject as TaskableBuildingObject;
			if (taskableBuildingObj != null)
			{
				taskableBuildingObj.EnqueueAction(new DelegateAction(delegate
				{
					taskableBuildingObj.SetEnabledAllStations(true);
				}, logger));
			}
			animationControllers = null;
			buildingObject.SetUpWifiListeners(networkLostOpenSignal, networkLostCloseSignal);
			return buildingObject;
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
					component.applyRootMotion = false;
					AnimEventHandler animEventHandler = gameObject.AddComponent<AnimEventHandler>();
					animEventHandler.Init(gameObject, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
				}
			}
		}

		private void AnimateBuildingsMinion(BuildingObject buildingObj, Building building, MinionObject minionObj)
		{
			int routeIndex = GetRouteIndex(building);
			AnimatingBuildingDefinition animatingBuildingDefinition = null;
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			LeisureBuilding leisureBuilding = building as LeisureBuilding;
			if (taskableBuilding != null)
			{
				taskableBuilding.AddMinion(minionObj.ID, 0);
				animatingBuildingDefinition = taskableBuilding.Definition;
			}
			else if (leisureBuilding != null)
			{
				leisureBuilding.AddMinion(minionObj.ID, 0);
				animatingBuildingDefinition = leisureBuilding.Definition;
			}
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animatingBuildingDefinition.AnimationDefinitions[0].MinionController);
			minionObj.SetAnimController(runtimeAnimatorController);
			TaskableBuildingObject taskableBuildingObject = buildingObj as TaskableBuildingObject;
			LeisureBuildingObjectView leisureBuildingObjectView = buildingObj as LeisureBuildingObjectView;
			if (taskableBuildingObject != null)
			{
				taskableBuildingObject.MoveToRoutingPosition(minionObj, routeIndex);
				taskableBuildingObject.TrackChild(minionObj, runtimeAnimatorController, false);
			}
			else if (leisureBuildingObjectView != null)
			{
				leisureBuildingObjectView.TrackChild(minionObj, runtimeAnimatorController, routeIndex);
			}
		}

		private int GetRouteIndex(Building building)
		{
			TaskableBuilding taskableBuilding = building as TaskableBuilding;
			LeisureBuilding leisureBuilding = building as LeisureBuilding;
			int result = -1;
			if (taskableBuilding != null)
			{
				result = taskableBuilding.GetMinionsInBuilding();
			}
			else if (leisureBuilding != null)
			{
				result = leisureBuilding.GetMinionsInBuilding();
			}
			return result;
		}

		private MinionObject BuildBuildingMinionObject(GameObject parent)
		{
			int id = GameConstants.MINION_DEFINITION_IDS[Random.Range(0, GameConstants.MINION_DEFINITION_IDS.Length)];
			MinionDefinition def = definitionService.Get<MinionDefinition>(id);
			Minion minion = new Minion(def);
			CostumeItemDefinition costume = definitionService.Get<CostumeItemDefinition>(99);
			minionBuilder.SetLOD(minionBuilder.GetLOD());
			MinionObject minionObject = minionBuilder.BuildMinion(costume, "asm_minion_movement", parent, false);
			minionObject.transform.localScale = Vector3.one;
			minion.Name = minionObject.name;
			minionObject.Init(minion, logger);
			Agent component = minionObject.GetComponent<Agent>();
			component.enabled = false;
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			minionObject.SetDefaultAnimController(runtimeAnimatorController);
			minionObject.SetAnimController(runtimeAnimatorController);
			minionObject.gameObject.SetLayerRecursively(5);
			return minionObject;
		}

		public void SetStenciledShaderOnBuilding(GameObject buildingObject)
		{
			int stencilRef = 2;
			int num = 1;
			Renderer[] componentsInChildren = buildingObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren == null)
			{
				return;
			}
			foreach (Renderer renderer in componentsInChildren)
			{
				List<Material> list = new List<Material>();
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material item = renderer.materials[j];
					list.Add(item);
				}
				list.Sort((Material x, Material y) => x.renderQueue.CompareTo(y.renderQueue));
				for (int k = 0; k < list.Count; k++)
				{
					Material item = list[k];
					ShaderUtils.EnableStencilShader(item, stencilRef, num++);
				}
			}
		}
	}
}
