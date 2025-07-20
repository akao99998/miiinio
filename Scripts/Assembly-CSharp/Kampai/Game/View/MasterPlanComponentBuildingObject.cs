using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using Kampai.Util.Audio;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace Kampai.Game.View
{
	public class MasterPlanComponentBuildingObject : BuildingObject, IView, IRequiresBuildingScaffolding, IStartAudio
	{
		internal CleanupMasterPlanComponentsSignal cleanupComponentSignal;

		private PlayLocalAudioSignal playLocalAudioSignal;

		private bool audioPlaying;

		private MasterPlanComponentBuilding planBuilding;

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
			planBuilding = building as MasterPlanComponentBuilding;
			string animationController = planBuilding.Definition.animationController;
			if (!string.IsNullOrEmpty(animationController))
			{
				RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animationController);
				if (runtimeAnimatorController == null)
				{
					logger.Error("MasterPlanComponentDefinition has bad animationController value: {0}", animationController);
				}
				else
				{
					InitAnimators();
					SetAnimController(runtimeAnimatorController);
				}
			}
		}

		private void InitAnimators()
		{
			animators = new List<Animator>();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.name.Contains("LOD"))
				{
					Animator component = child.GetComponent<Animator>();
					if (component != null)
					{
						animators.Add(component);
					}
				}
			}
		}

		public void TriggerVFX()
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				ParticleSystem component = child.GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.Play();
				}
			}
		}

		public void TriggerMasterPlanCompleteAnimation(string controllerName)
		{
			Animator animator = base.gameObject.GetComponent<Animator>();
			if (animator == null)
			{
				animator = base.gameObject.AddComponent<Animator>();
			}
			animators.Insert(0, animator);
			animator.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(controllerName);
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
			EnqueueAction(new SendIDSignalAction(this, cleanupComponentSignal, logger));
			EnqueueAction(new DestroyObjectAction(this, logger));
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

		public void InitAudio(BuildingState creationState, PlayLocalAudioSignal playLocalAudioSignal)
		{
			this.playLocalAudioSignal = playLocalAudioSignal;
			StartCoroutine(WaitForGameToStart(creationState));
		}

		public void NotifyBuildingState(BuildingState newState)
		{
			if (newState != BuildingState.Broken && newState != BuildingState.Inaccessible && newState != BuildingState.Complete)
			{
				PlaySFX();
			}
		}

		private void PlaySFX()
		{
			string environmentalAudio = planBuilding.Definition.environmentalAudio;
			if (!string.IsNullOrEmpty(environmentalAudio) && playLocalAudioSignal != null && !audioPlaying)
			{
				playLocalAudioSignal.Dispatch(Kampai.Util.Audio.GetAudioEmitter.Get(base.gameObject, "MasterPlanComponentEmitter"), environmentalAudio, null);
				audioPlaying = true;
			}
		}

		private IEnumerator WaitForGameToStart(BuildingState creationState)
		{
			yield return new WaitForEndOfFrame();
			NotifyBuildingState(creationState);
		}
	}
}
