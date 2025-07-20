using System;
using System.Collections.Generic;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class TikiBarBuildingObjectView : AnimatingBuildingObject, IView
	{
		public TikiBarBuilding tikiBar;

		private Renderer glowRenderer;

		private Animation glowAnimation;

		private int routeIndex;

		internal bool didSkipParty;

		private Dictionary<int, RuntimeAnimatorController> unlockedMinionControllers;

		private RuntimeAnimatorController bartenderStateMachine;

		private RuntimeAnimatorController minionWalkStateMachine;

		private int[] layerIndicies;

		private Renderer[] renderers;

		protected Dictionary<int, TaskingCharacterObject> childAnimators = new Dictionary<int, TaskingCharacterObject>();

		protected List<TaskingCharacterObject> childQueue = new List<TaskingCharacterObject>();

		private MinionStateChangeSignal stateChangeSignal;

		private NamedCharacterRemovedFromTikiBarSignal removedFromTikibarSignal;

		private CharacterIntroCompleteSignal introCompleteSignal;

		private CharacterDrinkingCompleteSignal drinkingCompelteSignal;

		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		public bool requiresContext
		{
			get
			{
				return _requiresContext;
			}
			set
			{
				_requiresContext = value;
			}
		}

		public bool registeredWithContext { get; set; }

		public virtual bool autoRegisterWithContext
		{
			get
			{
				return registerWithContext;
			}
			set
			{
				registerWithContext = value;
			}
		}

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			tikiBar = building as TikiBarBuilding;
			TaskableBuildingDefinition taskableBuildingDefinition = building.Definition as TaskableBuildingDefinition;
			if (taskableBuildingDefinition == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_TASKABLE_DEFINITION, building.Definition.ID.ToString());
			}
			if (building.IsBuildingRepaired())
			{
				if (tikiBar.State != BuildingState.MissingTikiSign)
				{
					Transform transform = base.transform.Find("Unique_TikiBar_LOD0/Unique_TikiBar:Unique_TikiBar/Unique_TikiBar:Unique_TikiBar_Glow_mesh");
					glowRenderer = transform.GetComponent<Renderer>();
					glowRenderer.enabled = false;
					glowAnimation = transform.GetComponent<Animation>();
				}
				SetupAnimationControllers();
			}
			renderers = base.gameObject.GetComponentsInChildren<Renderer>();
		}

		internal void SetupInjections(MinionStateChangeSignal stateChangeSignal, NamedCharacterRemovedFromTikiBarSignal removedFromTikibarSignal, CharacterIntroCompleteSignal introCompleteSingal, CharacterDrinkingCompleteSignal drinkingCompleteSingal)
		{
			this.stateChangeSignal = stateChangeSignal;
			this.removedFromTikibarSignal = removedFromTikibarSignal;
			introCompleteSignal = introCompleteSingal;
			drinkingCompelteSignal = drinkingCompleteSingal;
		}

		private void SetupAnimationControllers()
		{
			minionWalkStateMachine = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			bartenderStateMachine = KampaiResources.Load<RuntimeAnimatorController>("asm_unique_tikibar_bartender");
			unlockedMinionControllers = new Dictionary<int, RuntimeAnimatorController>();
			for (int i = 0; i < 3; i++)
			{
				unlockedMinionControllers.Add(i, KampaiResources.Load<RuntimeAnimatorController>(string.Format("{0}{1}", "asm_animIntro_newMinion", i + 1)));
				unlockedMinionControllers.Add(i + 3, KampaiResources.Load<RuntimeAnimatorController>(string.Format("{0}{1}", "asm_unique_tikibar_newMinion_Fun", i + 1)));
			}
		}

		public override void ResetAnimationParameters()
		{
			if (!didSkipParty)
			{
				base.ResetAnimationParameters();
			}
			else
			{
				SetAnimTrigger("SkipParty");
			}
			didSkipParty = false;
		}

		internal void SetupLayers()
		{
			if (animators.Count == 0)
			{
				return;
			}
			int layerCount = animators[0].layerCount;
			layerIndicies = new int[3];
			for (int i = 0; i < layerIndicies.Length; i++)
			{
				layerIndicies[i] = -1;
			}
			for (int j = 0; j < layerCount; j++)
			{
				string layerName = animators[0].GetLayerName(j);
				if (layerName.StartsWith("Pos"))
				{
					string value = layerName.Substring("Pos".Length);
					int num = Convert.ToInt32(value) - 1;
					if (num < 0 || num >= stations)
					{
						logger.Fatal(FatalCode.BV_NO_SUCH_WEIGHT_FOR_STATION);
					}
					layerIndicies[num] = j;
				}
			}
			if (stations <= 1)
			{
				return;
			}
			for (int k = 0; k < layerIndicies.Length; k++)
			{
				if (layerIndicies[k] == -1)
				{
					logger.Fatal(FatalCode.BV_MISSING_LAYER);
				}
			}
		}

		internal void SetupCharacter(CharacterObject characterObject, IPlayerService playerService, IPrestigeService prestigeService)
		{
			int num = routeIndex % 3;
			routeIndex++;
			MoveToRoutingPosition(characterObject, num + 3);
			characterObject.ShelveActionQueue();
			characterObject.SetAnimController(GetMinionArrivalAnimation(characterObject, playerService, prestigeService, num));
			characterObject.SetAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);
			characterObject.EnqueueAction(new WaitForMecanimStateAction(characterObject, GetHashAnimationState("Base Layer.NewMinionIntro"), logger));
			characterObject.EnqueueAction(new SetCullingModeAction(characterObject, AnimatorCullingMode.CullUpdateTransforms, logger));
			stateChangeSignal.Dispatch(characterObject.ID, MinionState.Questing);
		}

		internal RuntimeAnimatorController GetMinionArrivalAnimation(CharacterObject characterObject, IPlayerService playerService, IPrestigeService prestigeService, int routeIndex)
		{
			Character byInstanceId = playerService.GetByInstanceId<Character>(characterObject.ID);
			if (byInstanceId != null)
			{
				Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(byInstanceId);
				if (prestigeFromMinionInstance != null)
				{
					PrestigeDefinition definition = prestigeFromMinionInstance.Definition;
					if (definition != null && definition.UniqueArrivalStateMachine != null)
					{
						return KampaiResources.Load<RuntimeAnimatorController>(prestigeFromMinionInstance.Definition.UniqueArrivalStateMachine);
					}
				}
			}
			int minionCount = playerService.GetMinionCount();
			if (minionCount > 4)
			{
				return unlockedMinionControllers[routeIndex];
			}
			return unlockedMinionControllers[routeIndex + 3];
		}

		internal void BeginCharacterIntroLoop(bool waitForLoop, CharacterObject characterObject)
		{
			if (waitForLoop)
			{
				SetAnimTrigger("OnNewMinionAppear");
				characterObject.SetAnimTrigger("OnNewMinionAppear");
			}
			characterObject.SetAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);
		}

		internal void BeginCharacterIntro(bool waitForLoop, CharacterObject characterObject, int minionRouteIndex)
		{
			routeIndex = 0;
			characterObject.ClearActionQueue();
			characterObject.EnqueueAction(new SetCullingModeAction(characterObject, AnimatorCullingMode.AlwaysAnimate, logger));
			if (waitForLoop)
			{
				SetAnimTrigger("OnNewMinionIntro");
				characterObject.EnqueueAction(new WaitForMecanimStateAction(characterObject, GetHashAnimationState("Base Layer.Loop"), logger));
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("OnNewMinionIntro", true);
				characterObject.EnqueueAction(new SetAnimatorAction(characterObject, null, logger, dictionary));
			}
			characterObject.EnqueueAction(new WaitForMecanimStateAction(characterObject, GetHashAnimationState("Base Layer.Exit"), logger));
			characterObject.EnqueueAction(new CharacterIntroCompleteAction(characterObject, minionRouteIndex, minionWalkStateMachine, introCompleteSignal, logger));
			characterObject.EnqueueAction(new SetCullingModeAction(characterObject, AnimatorCullingMode.CullUpdateTransforms, logger));
		}

		internal void EndCharacterIntro(CharacterObject characterObject, int slotIndex)
		{
			characterObject.ApplyRootMotion(false);
			characterObject.EnableBlobShadow(true);
			characterObject.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
			characterObject.UnshelveActionQueue();
			if (slotIndex < 0)
			{
				characterObject.EnqueueAction(new StateChangeAction(characterObject.ID, stateChangeSignal, MinionState.Idle, logger), true);
			}
		}

		internal void RemoveCharacterFromTikiBar(int minionId)
		{
			if (childAnimators.ContainsKey(minionId))
			{
				TaskingCharacterObject taskingCharacterObject = childAnimators[minionId];
				int routingIndex = taskingCharacterObject.RoutingIndex;
				CharacterObject character = taskingCharacterObject.Character;
				switch (routingIndex)
				{
				case 1:
					SetAnimBool("pos1_IsSeated", false);
					break;
				case 2:
					SetAnimBool("pos2_IsSeated", false);
					break;
				}
				character.SetAnimBool("isSeated", false);
				character.IsSeatedInTikiBar = false;
				character.EnqueueAction(new SetAnimatorAction(character, character.GetCurrentAnimController(), logger), true);
				character.EnqueueAction(new WaitForMecanimStateAction(character, GetHashAnimationState("Base Layer.Idle"), logger));
				character.EnqueueAction(new CharacterDrinkingCompleteAction(character, drinkingCompelteSignal, logger));
			}
		}

		internal void UntrackChild(int minionId)
		{
			if (childAnimators.ContainsKey(minionId))
			{
				TaskingCharacterObject taskingCharacterObject = childAnimators[minionId];
				int routingIndex = taskingCharacterObject.RoutingIndex;
				CharacterObject character = taskingCharacterObject.Character;
				character.ApplyRootMotion(false);
				character.UnshelveActionQueue();
				character.EnableBlobShadow(true);
				character.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
				UnlinkChild(minionId);
				SetEnabledStation(routingIndex, false);
				NamedCharacterObject namedCharacterObject = character as NamedCharacterObject;
				if (namedCharacterObject != null)
				{
					character.ResetAnimationController();
					removedFromTikibarSignal.Dispatch(namedCharacterObject);
				}
				else if (character is MinionObject)
				{
					character.EnqueueAction(new StateChangeAction(character.ID, stateChangeSignal, MinionState.Idle, logger), true);
					character.EnqueueAction(new SetAnimatorAction(character, minionWalkStateMachine, logger));
					character.EnqueueAction(new GotoSideWalkAction(character, tikiBar, logger, definitionService, null));
				}
			}
		}

		internal void UnlinkChild(int minionId)
		{
			int num = -1;
			for (int i = 0; i < childQueue.Count; i++)
			{
				if (childQueue[i].ID == minionId)
				{
					num = i;
					break;
				}
			}
			if (num > -1)
			{
				childQueue.RemoveAt(num);
				childAnimators.Remove(minionId);
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Not found");
			}
		}

		internal void PathCharacterToTikiBar(CharacterObject characterObject, IList<Vector3> path, float rotation, int routeIndex, Signal<CharacterObject, int> addSignal)
		{
			float speed = 4.5f;
			characterObject.EnqueueAction(new SetAnimatorAction(characterObject, minionWalkStateMachine, logger), true);
			characterObject.EnqueueAction(new ConstantSpeedPathAction(characterObject, path, speed, logger));
			characterObject.EnqueueAction(new RotateAction(characterObject, rotation, 720f, logger));
			characterObject.EnqueueAction(new PathToBuildingCompleteAction(characterObject, routeIndex, addSignal, logger));
		}

		internal bool ContainsCharacter(int instanceID)
		{
			return childAnimators.ContainsKey(instanceID);
		}

		internal void AddCharacterToBuildingActions(CharacterObject characterObject, IPlayerService playerService, int routeIndex, IPrestigeService prestigeService, GetNewQuestSignal getNewQuestSignal)
		{
			switch (routeIndex)
			{
			case 1:
				SetAnimBool("pos1_IsSeated", true);
				break;
			case 2:
				SetAnimBool("pos2_IsSeated", true);
				break;
			}
			EnqueueAction(new TikibarTrackChildAction(this, characterObject, routeIndex, GetMinionBarstoolAnimation(characterObject, playerService, routeIndex, prestigeService), getNewQuestSignal, logger));
			if (!(characterObject is PhilView))
			{
				EnqueueAction(new SetAnimatorArgumentsAction(characterObject, logger, "isSeated", true));
			}
			characterObject.IsSeatedInTikiBar = true;
			Agent component = characterObject.GetComponent<Agent>();
			if (component != null)
			{
				component.MaxSpeed = 0f;
			}
		}

		internal RuntimeAnimatorController GetMinionBarstoolAnimation(CharacterObject characterObject, IPlayerService playerService, int routeIndex, IPrestigeService prestigeService)
		{
			if (routeIndex == 0)
			{
				return bartenderStateMachine;
			}
			Character byInstanceId = playerService.GetByInstanceId<Character>(characterObject.ID);
			Prestige prestigeFromMinionInstance = prestigeService.GetPrestigeFromMinionInstance(byInstanceId);
			if (prestigeFromMinionInstance == null)
			{
				logger.Fatal(FatalCode.PS_NO_SUCH_PRESTIGE, characterObject.ID);
				return null;
			}
			if (routeIndex == 1)
			{
				return KampaiResources.Load<RuntimeAnimatorController>(prestigeFromMinionInstance.Definition.UniqueTikiBarstoolASMPatron1);
			}
			return KampaiResources.Load<RuntimeAnimatorController>(prestigeFromMinionInstance.Definition.UniqueTikiBarstoolASMPatron2);
		}

		public void TrackChild(CharacterObject child, RuntimeAnimatorController controller, int routeIndex)
		{
			TaskingCharacterObject taskingCharacterObject = new TaskingCharacterObject(child, routeIndex);
			if (!childAnimators.ContainsKey(child.ID))
			{
				childAnimators.Add(child.ID, taskingCharacterObject);
				childQueue.Add(taskingCharacterObject);
				child.ShelveActionQueue();
			}
			SetupChild(routeIndex, taskingCharacterObject, controller);
		}

		protected virtual void SetupChild(int routingIndex, TaskingCharacterObject taskingChild, RuntimeAnimatorController controller = null)
		{
			taskingChild.RoutingIndex = routingIndex;
			CharacterObject character = taskingChild.Character;
			if (controller != null)
			{
				character.SetAnimController(controller);
			}
			character.ApplyRootMotion(false);
			character.EnableRenderers(true);
			MoveToRoutingPosition(character, routingIndex);
			SetEnabledStation(routingIndex, true);
		}

		public override void Update()
		{
			base.Update();
			bool flag = false;
			for (int i = 0; i < renderers.Length; i++)
			{
				Renderer renderer = renderers[i];
				if (renderer.isVisible)
				{
					flag = true;
					break;
				}
			}
			AnimatorCullingMode animatorCullingMode = ((!flag) ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.AlwaysAnimate);
			Dictionary<int, TaskingCharacterObject>.Enumerator enumerator = childAnimators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskingCharacterObject value = enumerator.Current.Value;
					value.Character.SetAnimatorCullingMode(animatorCullingMode);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		protected void SetEnabledStation(int station, bool isEnabled)
		{
			foreach (Animator animator in animators)
			{
				SetStationState(animator, station, isEnabled);
			}
		}

		private void SetStationState(Animator animator, int station, bool isEnabled)
		{
			if (GetNumberOfStations() > 1 && station < GetNumberOfStations())
			{
				animator.SetLayerWeight(layerIndicies[station], (!isEnabled) ? 0f : 1f);
			}
		}

		public void ToggleHitbox(bool enable)
		{
			Collider[] components = GetComponents<Collider>();
			foreach (Collider collider in components)
			{
				collider.enabled = enable;
			}
		}

		internal void ToggleStickerbookGlow(bool enable)
		{
			if (enable)
			{
				glowRenderer.enabled = true;
				glowAnimation.Play();
			}
			else
			{
				glowAnimation.Stop();
				glowRenderer.enabled = false;
			}
		}

		protected override Vector3 GetIndicatorPosition(bool centerY)
		{
			if (tikiBar != null && tikiBar.State == BuildingState.MissingTikiSign)
			{
				return GameConstants.TIKI_BAR_MISSING_SIGN_INDICATOR_POSITION;
			}
			return base.GetIndicatorPosition(centerY);
		}

		protected void Awake()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, false, ref currentContext);
			}
		}

		protected void Start()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, true, ref currentContext);
			}
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContext(this, false, false, ref currentContext);
		}

		internal void PlayAnimation(string animation, Type type, object obj)
		{
			if (type == typeof(int))
			{
				SetAnimInteger(animation, (int)obj);
			}
			else if (type == typeof(float))
			{
				SetAnimFloat(animation, (float)obj);
			}
			else if (type == typeof(bool))
			{
				SetAnimBool(animation, (bool)obj);
			}
		}

		public override bool CanFadeGFX()
		{
			return false;
		}

		public override bool CanFadeSFX()
		{
			return false;
		}
	}
}
