using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class VillainView : NamedCharacterObject, IView
	{
		private const string ASM_PARAM_INTRO = "Intro";

		private const string ASM_PARAM_ACTIVE = "Active";

		private const string ASM_PARAM_MOVE_INTO_CABANA = "MoveIntoCabana";

		private const string ASM_PARAM_CABANA_INDEX = "CabanaIndex";

		private Signal<string, Type, object> villainAnimSignal = new Signal<string, Type, object>();

		private int loopCountMin;

		private int loopCountMax;

		private Vector3 cabanaPosition;

		private RuntimeAnimatorController cabanaController;

		private RuntimeAnimatorController farewellController;

		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		public override Signal<string, Type, object> AnimSignal
		{
			get
			{
				return villainAnimSignal;
			}
		}

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

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			base.Build(character, parent, logger, minionBuilder);
			VillainDefinition villainDefinition = character.Definition as VillainDefinition;
			if (!string.IsNullOrEmpty(villainDefinition.AsmCabana))
			{
				cabanaController = KampaiResources.Load<RuntimeAnimatorController>(villainDefinition.AsmCabana);
			}
			if (!string.IsNullOrEmpty(villainDefinition.AsmFarewell))
			{
				farewellController = KampaiResources.Load<RuntimeAnimatorController>(villainDefinition.AsmFarewell);
			}
			VillainDefinition villainDefinition2 = character.Definition as VillainDefinition;
			loopCountMin = villainDefinition2.LoopCountMin;
			loopCountMax = villainDefinition2.LoopCountMax;
			return this;
		}

		public override Vector3 GetIndicatorPosition()
		{
			if (cabanaPosition == Vector3.zero)
			{
				return base.GetIndicatorPosition();
			}
			return cabanaPosition + GameConstants.UI.VILLAIN_UI_OFFSET;
		}

		public void PlayWelcome()
		{
			SetAnimController(defaultController);
		}

		public void GotoCabana(int index, Transform transform)
		{
			cabanaPosition = transform.position;
			setLocation(transform.position);
			setRotation(transform.eulerAngles);
			SetAnimController(cabanaController);
			SetAnimInteger("CabanaIndex", index);
		}

		public void PlayFarewell()
		{
			SetAnimController(farewellController);
		}

		internal void SetMasterPlanRewardAnimation(MasterPlanDefinition planDefinition)
		{
			if (planDefinition.VillainCharacterDefID == base.DefinitionID)
			{
				RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(planDefinition.RewardStateMachine);
				if (runtimeAnimatorController != null)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("isKevin", false);
					EnqueueAction(new SetAnimatorAction(this, runtimeAnimatorController, logger, dictionary));
					EnqueueAction(new DelayAction(this, 4f, logger));
					EnqueueAction(new SetAnimatorAction(this, defaultController, logger));
				}
			}
		}

		internal void DisplayVillain(int charDefID, bool enable)
		{
			if (charDefID == base.DefinitionID)
			{
				EnableRenderers(enable);
			}
		}

		internal void InitializeVillain(VillainLair lair, int charDefID)
		{
			if (charDefID == base.DefinitionID)
			{
				base.transform.position = (Vector3)lair.Definition.Location + lair.Definition.VillainOffset;
				base.transform.rotation = Quaternion.Euler(0f, lair.Definition.VillainRotation, 0f);
				Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				if (lair.hasVisited)
				{
					SetAnimController(defaultController);
					return;
				}
				LoadAnimationController(lair.Definition.IntroAnimController);
				SetAnimBool("isKevin", false);
				string text = string.Format("Base Layer.Exit_{0}", "Villain");
				EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash(text), logger), true);
				EnqueueAction(new SetAnimatorAction(this, defaultController, logger));
			}
		}

		protected override void TransitionComplete()
		{
			AnimatorStateInfo? animatorStateInfo = GetAnimatorStateInfo(0);
			if (animatorStateInfo.HasValue)
			{
				int num = base.randomService.NextInt(loopCountMin, loopCountMax + 1);
				StartCoroutine(ReturnToRelaxation(animatorStateInfo.Value.length * (float)num));
			}
		}

		protected override void SetupLODs()
		{
		}

		protected override string GetRootJointPath(Character character)
		{
			return string.Format("{0}:{0}/{0}:ROOT/{0}:Pelvis_M", (character as NamedCharacter).Definition.Prefab);
		}

		private IEnumerator ReturnToRelaxation(float time)
		{
			yield return new WaitForSeconds(time);
			SetAnimBool(PlayAlternateString, false);
		}

		private void LoadAnimationController(string animationController)
		{
			RuntimeAnimatorController runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animationController);
			if (runtimeAnimatorController == null)
			{
				logger.Error("Failed to load Villain's animation controller: {0}", animationController);
			}
			else
			{
				SetAnimController(runtimeAnimatorController);
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
