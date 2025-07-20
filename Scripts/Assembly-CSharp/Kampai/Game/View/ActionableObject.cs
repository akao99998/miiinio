using System;
using System.Collections.Generic;
using Kampai.Common.Service.Audio;
using Kampai.Util;
using Kampai.Util.Audio;
using Kampai.Util.Graphics;
using UnityEngine;

namespace Kampai.Game.View
{
	public abstract class ActionableObject : MonoBehaviour, IDefinitionsHotSwapHandler, Actionable, Animatable, VisuallyEffective, Audible, Identifiable
	{
		protected KampaiQueue<KampaiAction> actionQueue = new KampaiQueue<KampaiAction>();

		protected KampaiAction shelvedAction;

		protected KampaiQueue<KampaiAction> shelvedQueue;

		protected List<Animator> animators;

		protected RuntimeAnimatorController defaultController;

		protected VFXScript vfxScript;

		protected TriggerVFXOnState vfxTrigger;

		protected Dictionary<string, PropObject> activeProps;

		protected IKampaiLogger logger;

		[SerializeField]
		protected bool debug;

		[SerializeField]
		protected bool dumpQueue;

		private Dictionary<string, CustomFMOD_StudioEventEmitter> _audioEmitters;

		private bool initialized;

		internal bool gfxFaded;

		private bool sfxFaded;

		private KampaiAction fadeSfxAction;

		private RuntimeAnimatorController currentRuntimeController;

		private MaterialModifier materialModifier;

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public int ID { get; set; }

		public int DefinitionID { get; protected set; }

		public KampaiAction currentAction { get; protected set; }

		public bool IsAnimating { get; protected set; }

		public bool IsInteractable { get; protected set; }

		public Renderer[] objectRenderers { get; protected set; }

		protected Collider[] colliders { get; set; }

		public CustomFMOD_StudioEventEmitter localAudioEmitter
		{
			get
			{
				return GetAudioEmitter("LocalAudio");
			}
		}

		public CustomFMOD_StudioEventEmitter GetAudioEmitter(string id)
		{
			if (_audioEmitters == null)
			{
				_audioEmitters = new Dictionary<string, CustomFMOD_StudioEventEmitter>();
			}
			if (!_audioEmitters.ContainsKey(id))
			{
				_audioEmitters.Add(id, Kampai.Util.Audio.GetAudioEmitter.Get(base.gameObject, id));
			}
			return _audioEmitters[id];
		}

		public virtual void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			if (DefinitionID == 0)
			{
				return;
			}
			Definition definition = definitionService.Get(DefinitionID);
			if (definition != null && ID != 0 && playerService != null)
			{
				Instance byInstanceId = playerService.GetByInstanceId<Instance>(ID);
				if (byInstanceId != null)
				{
					byInstanceId.OnDefinitionHotSwap(definition);
				}
			}
		}

		internal virtual void Init()
		{
			objectRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
			colliders = base.gameObject.GetComponents<Collider>();
			IsInteractable = true;
			vfxScript = GetComponent<VFXScript>();
			if (vfxScript != null)
			{
				vfxScript.Init();
				vfxScript.TriggerState("OnStop");
			}
			InitProps();
		}

		internal void InitProps()
		{
			activeProps = new Dictionary<string, PropObject>();
		}

		private void DisableWarnings()
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.logWarnings = false;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void Update()
		{
			if (animators != null && !initialized)
			{
				DisableWarnings();
				initialized = true;
				sfxFaded = false;
				if (animators.Count > 0)
				{
					currentRuntimeController = animators[0].runtimeAnimatorController;
				}
			}
			if (dumpQueue)
			{
				LogActions();
				dumpQueue = false;
			}
			HandleActions();
			UpdateVFX();
		}

		private void HandleActions()
		{
			HandleNextAction();
			while (currentAction != null && currentAction.IsInstant() && currentAction.Done)
			{
				HandleNextAction();
			}
		}

		private void HandleNextAction()
		{
			if (currentAction == null || currentAction.Done)
			{
				currentAction = DequeueNextAction();
				if (currentAction != null)
				{
					ExecuteAction(currentAction);
				}
				else if (shelvedQueue == null)
				{
					if (debug)
					{
						logger.Info("{0} IDLE", base.name);
					}
					Idle();
				}
			}
			if (currentAction != null)
			{
				if (debug)
				{
					logger.Info("{0} - {1}.Update()", base.name, currentAction.ToString());
				}
				currentAction.Update();
			}
		}

		private KampaiAction DequeueNextAction()
		{
			KampaiAction kampaiAction = null;
			while ((kampaiAction == null || kampaiAction.Done) && actionQueue.Count > 0)
			{
				kampaiAction = actionQueue.Dequeue();
			}
			return kampaiAction;
		}

		public virtual void LateUpdate()
		{
			if (currentAction != null && !currentAction.Done)
			{
				if (debug)
				{
					logger.Info("{0} - {1}.LateUpdate()", base.name, currentAction.ToString());
				}
				currentAction.LateUpdate();
			}
		}

		public virtual void Idle()
		{
			currentAction = null;
		}

		public virtual void EnqueueAction(KampaiAction action, bool clear = false)
		{
			if (debug)
			{
				logger.Info("{0} - EnqueueAction({1})", base.name, action.ToString());
			}
			if (clear)
			{
				ClearActionQueue();
			}
			actionQueue.Enqueue(action);
		}

		public void Abort<T>() where T : KampaiAction
		{
			LinkedList<KampaiAction> linkedList = new LinkedList<KampaiAction>();
			Type typeFromHandle = typeof(T);
			if (currentAction is T)
			{
				linkedList.AddLast(currentAction);
				currentAction = null;
			}
			using (IEnumerator<KampaiAction> enumerator = actionQueue.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetType().IsAssignableFrom(typeFromHandle))
					{
						linkedList.AddLast(enumerator.Current);
					}
				}
			}
			using (LinkedList<KampaiAction>.Enumerator enumerator2 = linkedList.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					enumerator2.Current.Abort();
					actionQueue.Remove(enumerator2.Current);
				}
			}
		}

		public virtual void InjectAction(KampaiAction action)
		{
			if (debug)
			{
				logger.Info("InjectAction({0})", action.ToString());
			}
			actionQueue.AddFirst(action);
		}

		public virtual void ReplaceCurrentAction(KampaiAction action)
		{
			if (debug)
			{
				logger.Info("ReplaceCurrentAction: {0} -> {1}", currentAction.GetType(), action.ToString());
			}
			InjectAction(action);
			currentAction.Abort();
		}

		public virtual void ReplaceActionsOfType(KampaiAction action)
		{
			if (debug)
			{
				logger.Info("ReplaceActionsOfType: {0} -> {1}", currentAction.GetType(), action.ToString());
			}
			Type type = action.GetType();
			foreach (KampaiAction item in actionQueue)
			{
				if (type.IsAssignableFrom(item.GetType()))
				{
					item.Abort();
				}
			}
			if (currentAction != null && type.IsAssignableFrom(currentAction.GetType()))
			{
				currentAction.Abort();
			}
			InjectAction(action);
		}

		public virtual void ExecuteAction(KampaiAction action)
		{
			if (debug)
			{
				logger.Info("{0} - {1}.Execute()", base.name, action.ToString());
			}
			if (action != null)
			{
				action.Execute();
			}
		}

		public virtual int GetActionQueueCount()
		{
			return actionQueue.Count;
		}

		public virtual void ClearActionQueue()
		{
			if (debug)
			{
				logger.Info("{0} - ClearActionQueue {1}", base.name, actionQueue.Count);
			}
			if (currentAction != null)
			{
				currentAction.Abort();
				currentAction = null;
			}
			IEnumerator<KampaiAction> enumerator = actionQueue.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Abort();
				}
				actionQueue.Clear();
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public KampaiAction GetNextAction()
		{
			if (actionQueue.Count > 0)
			{
				return actionQueue.Peek();
			}
			return null;
		}

		public virtual T GetAction<T>() where T : KampaiAction
		{
			IEnumerator<KampaiAction> enumerator = actionQueue.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KampaiAction current = enumerator.Current;
					T val = current as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			return (T)null;
		}

		public virtual void ShelveActionQueue()
		{
			if (debug)
			{
				logger.Info("ShelveActionQueue");
			}
			shelvedQueue = actionQueue;
			shelvedAction = currentAction;
			currentAction = null;
			actionQueue = new KampaiQueue<KampaiAction>();
		}

		public virtual void UnshelveActionQueue()
		{
			if (debug)
			{
				logger.Info("UnshelveActionQueue");
			}
			if (shelvedQueue != null)
			{
				ClearActionQueue();
				actionQueue = shelvedQueue;
				currentAction = shelvedAction;
				shelvedQueue = null;
				shelvedAction = null;
			}
		}

		public void LogActions()
		{
			logger.Log(KampaiLogLevel.Debug, "{0} {1} - currentAction:{2}", ID, base.name, (currentAction != null) ? currentAction.ToString() : "null");
			foreach (KampaiAction item in actionQueue)
			{
				logger.Log(KampaiLogLevel.Debug, "{0} {1} - nextAction:{2}", ID, base.name, item.ToString());
			}
			logger.Log(KampaiLogLevel.Debug, "{0} {1} - shelvedAction:{2}", ID, base.name, (shelvedAction != null) ? shelvedAction.ToString() : "null");
			if (shelvedQueue == null)
			{
				return;
			}
			foreach (KampaiAction item2 in shelvedQueue)
			{
				logger.Log(KampaiLogLevel.Debug, "{0} {1} - nextShelved:{2}", ID, base.name, item2.GetType());
			}
		}

		public virtual void PlayAnimation(int stateHash, int layer, float time)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					List<string> list = new List<string>(activeProps.Keys);
					foreach (string item in list)
					{
						RemoveProp(item);
					}
					Animator current2 = enumerator.Current;
					current2.Play(stateHash, layer, time);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void ResetAnimationParameters()
		{
			Animator animator = animators[0];
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			float normalizedTime = currentAnimatorStateInfo.normalizedTime;
			animator.Rebind();
			animator.Play(currentAnimatorStateInfo.fullPathHash, 0, normalizedTime);
		}

		public virtual void AddProp(string propName, GameObject parent)
		{
			if (activeProps.ContainsKey(propName))
			{
				return;
			}
			PropObject propObject = new PropObject();
			GameObject gameObject = KampaiResources.Load<GameObject>(propName);
			if (!(gameObject == null))
			{
				propObject.gameObject = UnityEngine.Object.Instantiate(gameObject);
				Transform parent2 = base.transform.Find("minion:ROOT");
				Transform transform = propObject.gameObject.transform.Find("minion:ROOT/minion:R_prop");
				if (transform != null)
				{
					transform.parent = parent2;
					propObject.transforms.Add(transform);
				}
				Transform transform2 = propObject.gameObject.transform.Find("minion:ROOT/minion:L_prop");
				if (transform2 != null)
				{
					transform2.parent = parent2;
					propObject.transforms.Add(transform2);
				}
				propObject.gameObject.transform.parent = base.transform;
				activeProps.Add(propName, propObject);
				ResetAnimationParameters();
			}
		}

		public virtual void RemoveProp(string propName)
		{
			PropObject value = null;
			if (!activeProps.TryGetValue(propName, out value))
			{
				return;
			}
			activeProps.Remove(propName);
			UnityEngine.Object.Destroy(value.gameObject);
			foreach (Transform transform in value.transforms)
			{
				transform.parent = value.gameObject.transform;
				UnityEngine.Object.Destroy(transform);
			}
			UnityEngine.Object.Destroy(value);
			ResetAnimationParameters();
		}

		public virtual AnimatorStateInfo? GetAnimatorStateInfo(int layer)
		{
			if (animators.Count > 0 && !animators[0].IsInTransition(layer))
			{
				return animators[0].GetCurrentAnimatorStateInfo(layer);
			}
			return null;
		}

		public virtual bool IsInAnimatorState(int mecanimStateHash, int layer = 0)
		{
			AnimatorStateInfo? animatorStateInfo = GetAnimatorStateInfo(layer);
			if (animatorStateInfo.HasValue)
			{
				int fullPathHash = animatorStateInfo.Value.fullPathHash;
				if (fullPathHash == mecanimStateHash)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void SetAnimTrigger(string name)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.SetTrigger(name);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void ResetAnimTrigger(string name)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.ResetTrigger(name);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void SetAnimBool(string name, bool state)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.SetBool(name, state);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void SetAnimFloat(string name, float state)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.SetFloat(name, state);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void SetAnimInteger(string name, int state)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.SetInteger(name, state);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public bool GetAnimBool(string name)
		{
			if (animators.Count > 0)
			{
				return animators[0].GetBool(name);
			}
			logger.Error("Error retrieving bool {0} from {1}", name, base.gameObject.name);
			return false;
		}

		public float GetAnimFloat(string name)
		{
			if (animators.Count > 0)
			{
				return animators[0].GetFloat(name);
			}
			logger.Error("Error retrieving float {0} from {1}", name, base.gameObject.name);
			return 0f;
		}

		public int GetAnimInteger(string name)
		{
			if (animators.Count > 0)
			{
				return animators[0].GetInteger(name);
			}
			logger.Error("Error retrieving integer {0} from {1}", name, base.gameObject.name);
			return 0;
		}

		public virtual void SetDefaultAnimController(RuntimeAnimatorController controller)
		{
			defaultController = controller;
		}

		public string GetDefaultAnimControllerName()
		{
			return defaultController.name;
		}

		public string GetCurrentAnimControllerName()
		{
			if (animators.Count > 0)
			{
				return animators[0].runtimeAnimatorController.name;
			}
			return string.Empty;
		}

		public virtual void SetAnimController(RuntimeAnimatorController controller)
		{
			if (animators.Count == 0 || GetCurrentAnimController() == controller)
			{
				return;
			}
			using (List<Animator>.Enumerator enumerator2 = animators.GetEnumerator())
			{
				currentRuntimeController = controller;
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

		public virtual RuntimeAnimatorController GetCurrentAnimController()
		{
			return currentRuntimeController;
		}

		public virtual void ClearAnimators()
		{
			animators.Clear();
		}

		public virtual void SetAnimatorCullingMode(AnimatorCullingMode mode)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.cullingMode = mode;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void SetAnimatorsActive(bool active)
		{
			if (animators == null)
			{
				return;
			}
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.enabled = active;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public void ApplyRootMotion(bool enabled)
		{
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Animator current = enumerator.Current;
					current.applyRootMotion = enabled;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public virtual void SetRenderLayer(int layer)
		{
			base.gameObject.SetLayerRecursively(layer);
		}

		public bool getMuteStatus()
		{
			AnimEventHandler[] componentsInChildren = base.transform.gameObject.GetComponentsInChildren<AnimEventHandler>();
			if (componentsInChildren.Length > 0)
			{
				return componentsInChildren[0].mute;
			}
			return false;
		}

		private CustomFMOD_StudioEventEmitter GetEmitter(string emitterName)
		{
			CustomFMOD_StudioEventEmitter[] components = base.transform.gameObject.GetComponents<CustomFMOD_StudioEventEmitter>();
			CustomFMOD_StudioEventEmitter result = null;
			CustomFMOD_StudioEventEmitter[] array = components;
			foreach (CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter in array)
			{
				if (customFMOD_StudioEventEmitter.id == emitterName)
				{
					result = customFMOD_StudioEventEmitter;
				}
			}
			return result;
		}

		public void StartLocalAudio(string path)
		{
			if (!getMuteStatus())
			{
				CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter = GetEmitter("minionPersistentAudio");
				if (customFMOD_StudioEventEmitter == null)
				{
					customFMOD_StudioEventEmitter = base.transform.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
					customFMOD_StudioEventEmitter.id = "minionPersistentAudio";
					customFMOD_StudioEventEmitter.staticSound = false;
				}
				else
				{
					customFMOD_StudioEventEmitter.Stop();
				}
				if (fmodService != null)
				{
					customFMOD_StudioEventEmitter.path = fmodService.GetGuid(path);
					customFMOD_StudioEventEmitter.Play();
				}
			}
		}

		public void StopLocalAudio()
		{
			if (!getMuteStatus())
			{
				CustomFMOD_StudioEventEmitter emitter = GetEmitter("minionPersistentAudio");
				if (!(emitter == null))
				{
					emitter.Stop();
					UnityEngine.Object.Destroy(emitter);
				}
			}
		}

		public virtual void TrackVFX(VFXScript vfxScript)
		{
			this.vfxScript = vfxScript;
		}

		public virtual void UntrackVFX()
		{
			vfxScript = null;
		}

		public virtual void AnimVFX(string eventName)
		{
			if (vfxScript != null)
			{
				vfxScript.AnimVFX(eventName);
			}
		}

		public virtual void SetVFXState(string name, string desiredState = null)
		{
			if (vfxScript != null)
			{
				if (desiredState != null)
				{
					vfxTrigger = new TriggerVFXOnState();
					vfxTrigger.StateName = name;
					vfxTrigger.MecanimStateHash = Animator.StringToHash(desiredState);
				}
				else
				{
					vfxScript.TriggerState(name);
				}
			}
		}

		public virtual void UpdateVFX()
		{
			if (vfxTrigger != null && IsInAnimatorState(vfxTrigger.MecanimStateHash))
			{
				vfxScript.TriggerState(vfxTrigger.StateName);
				vfxTrigger = null;
			}
		}

		public virtual void SetMaterialColor(Color color)
		{
			materialModifier = materialModifier ?? new MaterialModifier(objectRenderers);
			materialModifier.SetMaterialColor(color);
		}

		public virtual void SetBlendedColor(Color color)
		{
			for (int i = 0; i < objectRenderers.Length; i++)
			{
				Renderer renderer = objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					if (material.HasProperty(GameConstants.ShaderProperties.Procedural.BlendedColor) && (!material.HasProperty(GameConstants.ShaderProperties.Blend.DstBlend) || material.GetFloat(GameConstants.ShaderProperties.Blend.DstBlend) != 1f))
					{
						material.SetColor(GameConstants.ShaderProperties.Procedural.BlendedColor, color);
					}
				}
			}
		}

		public virtual void EnableRenderers(bool enabled)
		{
			if (objectRenderers != null)
			{
				for (int i = 0; i < objectRenderers.Length; i++)
				{
					Renderer renderer = objectRenderers[i];
					renderer.enabled = enabled;
				}
			}
		}

		public virtual void SetMaterialShaderFloat(string name, float value)
		{
			materialModifier = materialModifier ?? new MaterialModifier(objectRenderers);
			materialModifier.SetFloat(name, value);
			materialModifier.Update();
		}

		public virtual void IncrementMaterialRenderQueue(int delta)
		{
			for (int i = 0; i < objectRenderers.Length; i++)
			{
				objectRenderers[i].material.renderQueue += delta;
			}
		}

		public virtual void SetZTestFunction(CompareFunction func)
		{
			for (int i = 0; i < objectRenderers.Length; i++)
			{
				Renderer renderer = objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					if (material.HasProperty(GameConstants.ShaderProperties.ZTest))
					{
						material.SetFloat(GameConstants.ShaderProperties.ZTest, (float)func);
					}
				}
			}
		}

		public virtual void UpdateMinRenderQueue(int minQueue)
		{
			for (int i = 0; i < objectRenderers.Length; i++)
			{
				Renderer renderer = objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					int renderQueue = material.renderQueue;
					int num = minQueue + (renderQueue - 1000) / 1000;
					if (num > renderQueue)
					{
						material.renderQueue = num;
					}
				}
			}
		}

		public virtual void ResetRenderQueue()
		{
			for (int i = 0; i < objectRenderers.Length; i++)
			{
				Renderer renderer = objectRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					material.renderQueue = ShaderUtils.GetMaterialRenderQueue(material);
				}
			}
		}

		public void UpdateColliders(bool enableState)
		{
			if (colliders != null)
			{
				for (int i = 0; i < colliders.Length && !(colliders[i] == null); i++)
				{
					colliders[i].enabled = IsInteractable && enableState;
				}
			}
		}

		public void DebugActionableObject(bool debug)
		{
			this.debug = debug;
		}

		public void DumpQueue()
		{
			dumpQueue = true;
		}

		public virtual bool CanFadeGFX()
		{
			return false;
		}

		public virtual bool CanFadeSFX()
		{
			return false;
		}

		public virtual void FadeSFX(float duration, bool fadeIn)
		{
			if (fadeSfxAction != null)
			{
				fadeSfxAction.Abort();
			}
			fadeSfxAction = new FadeAudioAction(this, duration, fadeIn, logger);
			if (fadeIn && sfxFaded)
			{
				sfxFaded = false;
				fadeSfxAction.Execute();
			}
			else if (!fadeIn && !sfxFaded)
			{
				sfxFaded = true;
				fadeSfxAction.Execute();
			}
		}

		public bool IsFaded()
		{
			return sfxFaded || gfxFaded;
		}

		public bool IsGFXFaded()
		{
			return gfxFaded;
		}
	}
}
