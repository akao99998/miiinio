using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class LeisureBuildingObjectView : AnimatingBuildingObject, IView
	{
		public LeisureBuilding leisureBuilding;

		private RuntimeAnimatorController minionController;

		private RuntimeAnimatorController minionWalkStateMachine;

		protected Dictionary<int, TaskingCharacterObject> childAnimators = new Dictionary<int, TaskingCharacterObject>();

		protected List<TaskingCharacterObject> childQueue = new List<TaskingCharacterObject>();

		private MinionStateChangeSignal stateChangeSignal;

		private RelocateCharacterSignal relocateCharacterSignal;

		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		private StartLeisurePartyPointsSignal startLeisurePartyPointsSignal { get; set; }

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
			leisureBuilding = building as LeisureBuilding;
			LeisureBuildingDefintiion leisureBuildingDefintiion = building.Definition as LeisureBuildingDefintiion;
			if (leisureBuildingDefintiion == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_TASKABLE_DEFINITION, building.Definition.ID.ToString());
			}
			buildingState = building.State;
			if (buildingState != BuildingState.Construction && buildingState != BuildingState.Complete)
			{
				SetupAnimationControllers(leisureBuildingDefintiion);
			}
		}

		internal void SetupInjections(MinionStateChangeSignal minionStateChangeSignal, StartLeisurePartyPointsSignal startLeisurePartyPointsSignal, RelocateCharacterSignal relocateCharacterSignal)
		{
			stateChangeSignal = minionStateChangeSignal;
			this.startLeisurePartyPointsSignal = startLeisurePartyPointsSignal;
			this.relocateCharacterSignal = relocateCharacterSignal;
		}

		public virtual void FadeMinions(ToggleMinionRendererSignal toggleMinionSignal, bool fadeIn)
		{
			foreach (int key in childAnimators.Keys)
			{
				toggleMinionSignal.Dispatch(key, fadeIn);
			}
		}

		private void SetupAnimationControllers(LeisureBuildingDefintiion def)
		{
			minionWalkStateMachine = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			if (def.AnimationDefinitions == null || def.AnimationDefinitions.Count == 0 || string.IsNullOrEmpty(def.AnimationDefinitions[0].MinionController))
			{
				logger.Error("Animation Definition NOT defined for the leisure Building");
			}
			else
			{
				minionController = KampaiResources.Load<RuntimeAnimatorController>(def.AnimationDefinitions[0].MinionController);
			}
		}

		internal bool IsMinionInBuilding(int minionID)
		{
			return childAnimators.ContainsKey(minionID);
		}

		internal void FreeAllMinions(int selectedMinionID = 0, MinionState targetState = MinionState.Idle)
		{
			Dictionary<int, TaskingCharacterObject>.Enumerator enumerator = childAnimators.GetEnumerator();
			try
			{
				int num = 0;
				while (enumerator.MoveNext())
				{
					TaskingCharacterObject value = enumerator.Current.Value;
					if (value.Character is MinionObject)
					{
						value.Character.gameObject.SetLayerRecursively(8);
					}
					UntrackChild(value.ID, (selectedMinionID != value.ID) ? MinionState.Idle : targetState, num);
					num++;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			childAnimators.Clear();
			EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger), true);
		}

		internal void UntrackChild(int minionId, MinionState targetState = MinionState.Idle, int index = 0)
		{
			if (childAnimators.ContainsKey(minionId))
			{
				TaskingCharacterObject taskingCharacterObject = childAnimators[minionId];
				CharacterObject character = taskingCharacterObject.Character;
				character.ApplyRootMotion(false);
				character.UnshelveActionQueue();
				character.EnableBlobShadow(true);
				character.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
				UnlinkChild(minionId);
				NamedCharacterObject namedCharacterObject = character as NamedCharacterObject;
				if (namedCharacterObject != null)
				{
					character.ResetAnimationController();
				}
				else if (character is MinionObject)
				{
					character.EnqueueAction(new StateChangeAction(character.ID, stateChangeSignal, targetState, logger));
					character.EnqueueAction(new SetAnimatorAction(character, minionWalkStateMachine, logger));
					character.EnqueueAction(new GotoSideWalkAction(character, leisureBuilding, logger, definitionService, relocateCharacterSignal, index));
				}
			}
		}

		protected void UnlinkChild(int minionId)
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
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Not found");
			}
		}

		internal void PathMinionToLeisureBuilding(CharacterObject characterObject, IList<Vector3> path, float rotation, int routeIndex, Signal<CharacterObject, int> addSignal)
		{
			float speed = 4.5f;
			characterObject.EnqueueAction(new SetAnimatorAction(characterObject, minionWalkStateMachine, logger), true);
			characterObject.EnqueueAction(new ConstantSpeedPathAction(characterObject, path, speed, logger));
			characterObject.EnqueueAction(new RotateAction(characterObject, rotation, 720f, logger));
			characterObject.EnqueueAction(new PathToBuildingCompleteAction(characterObject, routeIndex, addSignal, logger));
		}

		internal void AddCharacterToBuildingActions(CharacterObject mo, int routeIndex)
		{
			TrackChild(mo, minionController, routeIndex);
			startLeisurePartyPointsSignal.Dispatch(leisureBuilding.ID);
		}

		public void TrackChild(CharacterObject child, RuntimeAnimatorController controller, int routeIndex)
		{
			if (!IsAnimating)
			{
				Dictionary<string, object> animationParams = OnlyStateEnabled("OnLoop");
				EnqueueAction(new TriggerBuildingAnimationAction(this, animationParams, logger));
			}
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
			character.ExecuteAction(new SetAnimatorAction(character, null, logger, new Dictionary<string, object> { { "minionPosition", routingIndex } }));
			Dictionary<string, object> animationParams = OnlyStateEnabled("OnLoop");
			int hashAnimationState = GetHashAnimationState("Base Layer.Loop_Pos" + (routingIndex + 1));
			int hashAnimationState2 = GetHashAnimationState("Base Layer.Loop");
			character.EnqueueAction(new SetAnimatorAction(character, null, logger, OnlyStateEnabled("OnStop")), true);
			character.EnqueueAction(new WaitForMecanimStateAction(character, GetHashAnimationState("Base Layer.Idle"), logger));
			character.EnqueueAction(new SetAnimatorAction(character, null, logger, animationParams));
			character.EnqueueAction(new SkipToTimeAction(character, new SkipToTime(0, hashAnimationState, GetCurrentAnimationTimeForState(hashAnimationState2)), logger));
			MoveToRoutingPosition(character, routingIndex);
		}

		public override void Update()
		{
			base.Update();
			bool flag = false;
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Renderer renderer = base.objectRenderers[i];
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

		protected void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext);
		}

		protected void Start()
		{
			KampaiView.BubbleToContextOnStart(this, ref currentContext);
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContextOnDestroy(this, ref currentContext);
		}
	}
}
