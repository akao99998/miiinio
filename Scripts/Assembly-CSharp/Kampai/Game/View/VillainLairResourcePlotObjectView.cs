using System.Collections.Generic;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class VillainLairResourcePlotObjectView : AnimatingBuildingObject, IView
	{
		public VillainLairResourcePlot resourcePlot;

		internal bool isGagable;

		internal Signal<int> gagSignal = new Signal<int>();

		internal Signal<CharacterObject, int> addToBuildingSignal = new Signal<CharacterObject, int>();

		protected Dictionary<int, TaskingCharacterObject> childAnimators = new Dictionary<int, TaskingCharacterObject>();

		protected List<TaskingCharacterObject> childQueue = new List<TaskingCharacterObject>();

		private RuntimeAnimatorController buildingController;

		private RuntimeAnimatorController minionController;

		private RuntimeAnimatorController minionWalkStateMachine;

		private MinionStateChangeSignal stateChangeSignal;

		private readonly List<Material> usableMaterials = new List<Material>();

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
			resourcePlot = building as VillainLairResourcePlot;
			VillainLairResourcePlotDefinition villainLairResourcePlotDefinition = building.Definition as VillainLairResourcePlotDefinition;
			if (villainLairResourcePlotDefinition == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_TASKABLE_DEFINITION, building.Definition.ID.ToString());
			}
			buildingState = building.State;
		}

		public void InitializeControllers(IRandomService randomService, MinionStateChangeSignal minionStateChangeSignal)
		{
			stateChangeSignal = minionStateChangeSignal;
			VillainLairResourcePlotDefinition definition = resourcePlot.Definition;
			minionWalkStateMachine = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
			if (definition.AnimationDefinitions == null || definition.AnimationDefinitions.Count == 0)
			{
				logger.Error("Animation Definition NOT defined for a resource plot");
				return;
			}
			int num = randomService.NextInt(definition.AnimationDefinitions.Count);
			isGagable = num > 0;
			BuildingAnimationDefinition buildingAnimationDefinition = definition.AnimationDefinitions[num];
			buildingController = buildingControllers[buildingAnimationDefinition.CostumeId];
			minionController = KampaiResources.Load<RuntimeAnimatorController>(buildingAnimationDefinition.MinionController);
			InitializeAnimators();
		}

		public void InitializeAnimators()
		{
			ClearAnimators();
			Animator[] componentsInChildren = base.gameObject.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].name.Contains("LOD"))
				{
					animators.Add(componentsInChildren[i]);
				}
			}
			SetAnimController(buildingController);
			InitializeVFX();
			if (childAnimators.Count > 0)
			{
				EnqueueAction(new SetAnimatorAction(this, null, logger, OnlyStateEnabled("OnLoop")));
				int hashAnimationState = GetHashAnimationState("Base Layer.Loop");
				int hashAnimationState2 = GetHashAnimationState("Base Layer.Loop");
				CharacterObject character = childAnimators[resourcePlot.MinionIDInBuilding].Character;
				character.EnqueueAction(new SetAnimatorAction(character, null, logger, OnlyStateEnabled("OnLoop")));
				character.EnqueueAction(new SkipToTimeAction(character, new SkipToTime(0, hashAnimationState2, GetCurrentAnimationTimeForState(hashAnimationState)), logger));
				SetSiblingVFXScript(character.gameObject, vfxScript);
			}
		}

		public override void SetAnimController(RuntimeAnimatorController controller)
		{
			if (animators.Count == 0)
			{
				return;
			}
			using (List<Animator>.Enumerator enumerator2 = animators.GetEnumerator())
			{
				List<string> list = new List<string>(activeProps.Keys);
				foreach (string item in list)
				{
					RemoveProp(item);
				}
				while (enumerator2.MoveNext())
				{
					Animator current2 = enumerator2.Current;
					current2.runtimeAnimatorController = controller;
				}
			}
			vfxTrigger = null;
		}

		internal void TriggerGagAnimation()
		{
			if (animators.Count == 0)
			{
				return;
			}
			IList<ActionableObject> list = new List<ActionableObject>();
			list.Add(this);
			foreach (TaskingCharacterObject value in childAnimators.Values)
			{
				list.Add(value.Character);
			}
			SyncAction action = new SyncAction(list, logger);
			SyncAction action2 = new SyncAction(list, logger);
			foreach (TaskingCharacterObject value2 in childAnimators.Values)
			{
				int hashAnimationState = GetHashAnimationState("Base Layer.Loop");
				int hashAnimationState2 = GetHashAnimationState("Base Layer.Loop");
				SkipToTime skipToTime = new SkipToTime(0, hashAnimationState, GetCurrentAnimationTimeForState(hashAnimationState2));
				CharacterObject character = value2.Character;
				character.EnqueueAction(new SkipToTimeAction(character, skipToTime, logger));
				character.EnqueueAction(action);
				character.EnqueueAction(new SetAnimatorAction(character, null, logger, OnlyStateEnabled("OnGag")));
				character.EnqueueAction(action2);
				character.EnqueueAction(new SetAnimatorAction(character, null, logger, OnlyStateEnabled("OnLoop")));
			}
			EnqueueAction(action);
			EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnGag"), logger));
			EnqueueAction(new WaitForMecanimStateAction(this, GetHashAnimationState("Base Layer.Gag"), logger));
			EnqueueAction(action2);
			EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnLoop"), logger, "Base Layer.Loop"));
		}

		internal override void Highlight(bool enabled)
		{
		}

		internal void AddCharacterToBuildingActions(CharacterObject mo, int routeIndex)
		{
			TrackChild(mo, minionController, routeIndex);
			Dictionary<string, object> animationParams = OnlyStateEnabled("OnLoop");
			EnqueueAction(new SetAnimatorAction(this, null, logger, OnlyStateEnabled("OnStop")), true);
			EnqueueAction(new WaitForMecanimStateAction(this, GetHashAnimationState("Base Layer.Idle"), logger));
			EnqueueAction(new SetAnimatorAction(this, null, logger, animationParams));
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
			InitializeVFX();
			SetSiblingVFXScript(child.gameObject, vfxScript);
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
			character.ApplyRootMotion(true);
			character.EnableRenderers(true);
			character.SetAnimatorCullingMode(AnimatorCullingMode.AlwaysAnimate);
			character.ExecuteAction(new SetAnimatorAction(character, null, logger, new Dictionary<string, object> { { "minionPosition", routingIndex } }));
			Dictionary<string, object> animationParams = OnlyStateEnabled("OnLoop");
			character.EnqueueAction(new SetAnimatorAction(character, null, logger, OnlyStateEnabled("OnStop")), true);
			character.EnqueueAction(new WaitForMecanimStateAction(character, GetHashAnimationState("Base Layer.Idle"), logger));
			character.EnqueueAction(new SetAnimatorAction(character, null, logger, animationParams));
			MoveToRoutingPosition(character, routingIndex);
		}

		internal void PathMinionToPlot(CharacterObject characterObject, Signal<CharacterObject, int> addSignal)
		{
			characterObject.EnqueueAction(new TeleportAction(characterObject, routes[0].position, routes[0].eulerAngles, logger), true);
			characterObject.EnqueueAction(new PathToBuildingCompleteAction(characterObject, 0, addSignal, logger));
		}

		internal void FreeAllMinions(Vector3 newPos)
		{
			Dictionary<int, TaskingCharacterObject>.Enumerator enumerator = childAnimators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskingCharacterObject value = enumerator.Current.Value;
					UntrackChild(value.ID, newPos);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			childAnimators.Clear();
			EnqueueAction(new TriggerBuildingAnimationAction(this, OnlyStateEnabled("OnStop"), logger), true);
		}

		internal void UntrackChild(int minionId, Vector3 newPos, MinionState targetState = MinionState.Idle)
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
					character.EnqueueAction(new TeleportAction(character, newPos, Vector3.zero, logger));
					character.EnqueueAction(new SetAnimatorAction(character, minionWalkStateMachine, logger));
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

		public void UpdateRoutes(GameObject newInstance, MinionObject mo = null)
		{
			GameObject gameObject = newInstance.FindChild("route0");
			if (gameObject != null)
			{
				routes[0] = gameObject.transform;
			}
			if (mo != null)
			{
				mo.transform.position = routes[0].position;
				mo.transform.rotation = routes[0].rotation;
			}
		}

		public void UpdateRenderers()
		{
			base.objectRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
			if (base.objectRenderers == null)
			{
				return;
			}
			for (int i = 0; i < base.objectRenderers.Length; i++)
			{
				Renderer renderer = base.objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					if (!material.name.Contains("Platform") && !material.name.Contains("Shadow") && material.HasProperty("_BlendedColor"))
					{
						usableMaterials.Add(material);
					}
				}
			}
		}

		private void InitializeVFX()
		{
			VFXScript[] componentsInChildren = base.transform.gameObject.GetComponentsInChildren<VFXScript>();
			VFXScript[] array = componentsInChildren;
			foreach (VFXScript vFXScript in array)
			{
				vFXScript.Init();
				vFXScript.TriggerState("OnStop");
			}
			if (componentsInChildren.Length == 1)
			{
				TrackVFX(componentsInChildren[0]);
				return;
			}
			logger.Warning("Invalid number of VFX Scripts on this object: {0}", componentsInChildren.Length);
		}

		private void SetSiblingVFXScript(GameObject sibling, VFXScript vfxScript)
		{
			AnimEventHandler component = sibling.GetComponent<AnimEventHandler>();
			if (component != null)
			{
				component.SetSiblingVFXScript(vfxScript);
			}
		}

		protected void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext);
		}

		protected void Start()
		{
			KampaiView.BubbleToContextOnStart(this, ref currentContext);
			UpdateRenderers();
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContextOnDestroy(this, ref currentContext);
		}
	}
}
