using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using UnityEngine.Rendering;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public abstract class NamedCharacterObject : CharacterObject
	{
		private bool isIdle;

		private bool isAtAttention;

		private float spreadMin;

		private float spreadMax;

		private float attentionDuration;

		private int idleCount;

		private float currentTime;

		private float timer;

		private bool randomize;

		private bool transitionComplete;

		private List<int> knuthDeck = new List<int>();

		[Inject]
		public IRandomService randomService { get; set; }

		protected virtual string ActionsLayer
		{
			get
			{
				return "Base Layer.Actions";
			}
		}

		protected virtual string RandomizerString
		{
			get
			{
				return "randomizer";
			}
		}

		protected virtual string AttentionString
		{
			get
			{
				return "IsGetAttention";
			}
		}

		protected virtual string PlayAlternateString
		{
			get
			{
				return "PlayAlternate";
			}
		}

		protected virtual string AttentionStartString
		{
			get
			{
				return "AttentionStart";
			}
		}

		protected bool IsIdle
		{
			get
			{
				return isIdle;
			}
			set
			{
				isIdle = value;
				if (isIdle)
				{
					timer = randomService.NextFloat(spreadMin, spreadMax);
				}
			}
		}

		protected bool IsAtAttention
		{
			get
			{
				return isAtAttention;
			}
			set
			{
				isAtAttention = value;
				if (isAtAttention)
				{
					StartCoroutine(ReturnToIdle());
				}
				else
				{
					StopCoroutine(ReturnToIdle());
				}
			}
		}

		public abstract Signal<string, Type, object> AnimSignal { get; }

		public virtual NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			if (parent != null)
			{
				base.gameObject.transform.parent = parent.transform;
			}
			base.gameObject.transform.localPosition = Vector3.zero;
			base.gameObject.transform.localEulerAngles = Vector3.zero;
			NamedCharacterDefinition definition = character.Definition;
			Location location = definition.Location;
			if (location != null)
			{
				base.gameObject.transform.position = (Vector3)location;
			}
			Vector3 rotationEulers = definition.RotationEulers;
			if (rotationEulers != Vector3.zero)
			{
				base.gameObject.transform.localEulerAngles = rotationEulers;
			}
			SetupLODs();
			string stateMachine = definition.CharacterAnimations.StateMachine;
			Animator component = base.gameObject.GetComponent<Animator>();
			component.applyRootMotion = false;
			component.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(stateMachine);
			component.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			if (component.runtimeAnimatorController == null)
			{
				logger.Error("Failed to get default runtime animator controller for {0}: asm name: {1}", character.Name, stateMachine);
			}
			Transform transform = base.gameObject.transform.Find(GetRootJointPath(character));
			SkinnedMeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				skinnedMeshRenderer.rootBone = transform;
			}
			SetupCharacterObject(transform, component.runtimeAnimatorController, minionBuilder);
			return this;
		}

		protected virtual void SetupLODs()
		{
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			Array.Sort(componentsInChildren, (Renderer x, Renderer y) => x.gameObject.name.CompareTo(y.gameObject.name));
			LOD[] array = new LOD[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				array[i] = new LOD(GameConstants.GetLODHeightsArray()[i], new Renderer[1] { componentsInChildren[i] });
				componentsInChildren[i].shadowCastingMode = ShadowCastingMode.Off;
				componentsInChildren[i].receiveShadows = false;
			}
			LODGroup lODGroup = base.gameObject.AddComponent<LODGroup>();
			lODGroup.SetLODs(array);
			lODGroup.RecalculateBounds();
		}

		private void SetupCharacterObject(Transform pelvis, RuntimeAnimatorController defaultController, IMinionBuilder minionBuilder)
		{
			Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
			if (minionBuilder.GetLOD() != TargetPerformance.LOW && minionBuilder.GetLOD() != 0)
			{
				GameObject original = KampaiResources.Load("MinionBlobShadow") as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(original);
				gameObject.transform.parent = base.gameObject.transform;
				gameObject.GetComponent<MinionBlobShadowView>().SetToTrack(pelvis);
				SetBlobShadow(gameObject);
			}
			SetDefaultAnimController(defaultController);
		}

		public void SetupRandomizer(NamedCharacterAnimationDefinition animDefinition)
		{
			spreadMin = animDefinition.SpreadMin;
			spreadMax = animDefinition.SpreadMax;
			idleCount = animDefinition.IdleCount;
			attentionDuration = animDefinition.AttentionDuration;
		}

		protected virtual void SetNextAnimation(int next)
		{
			SetAnimInteger(RandomizerString, next);
			AnimSignal.Dispatch(RandomizerString, typeof(int), next);
		}

		private IEnumerator ReturnToIdle()
		{
			yield return new WaitForSeconds(attentionDuration);
			SetAnimBool(AttentionString, false);
			AnimSignal.Dispatch(AttentionString, typeof(bool), false);
		}

		public override void Update()
		{
			base.Update();
			if (!isIdle)
			{
				return;
			}
			string playAlternateString = PlayAlternateString;
			if (!randomize)
			{
				currentTime += Time.deltaTime;
				if (currentTime > timer)
				{
					randomize = true;
					if (knuthDeck.Count == 0)
					{
						PopulateKnuthDeck();
					}
					int nextAnimation = DrawFromKnuthDeck();
					SetNextAnimation(nextAnimation);
					SetAnimTrigger(playAlternateString);
					AnimSignal.Dispatch(playAlternateString, typeof(bool), true);
				}
			}
			else if (!transitionComplete && !IsInAnimatorState(Animator.StringToHash(ActionsLayer)))
			{
				transitionComplete = true;
				TransitionComplete();
			}
			else if (!GetAnimBool(playAlternateString) && IsInAnimatorState(Animator.StringToHash(ActionsLayer)))
			{
				currentTime = 0f;
				randomize = false;
				transitionComplete = false;
				timer = randomService.NextFloat(spreadMin, spreadMax);
			}
		}

		private void PopulateKnuthDeck()
		{
			for (int i = 0; i < idleCount; i++)
			{
				knuthDeck.Add(i);
			}
			for (int j = 0; j < knuthDeck.Count; j++)
			{
				int index = randomService.NextInt(j, knuthDeck.Count);
				int value = knuthDeck[j];
				knuthDeck[j] = knuthDeck[index];
				knuthDeck[index] = value;
			}
		}

		private int DrawFromKnuthDeck()
		{
			int result = knuthDeck[knuthDeck.Count - 1];
			knuthDeck.RemoveAt(knuthDeck.Count - 1);
			return result;
		}

		protected virtual void TransitionComplete()
		{
		}

		protected override void SetName(Character character)
		{
			if (base.gameObject.name.Length == 0)
			{
				base.gameObject.name = string.Format("NamedCharacter_{0}_{1}", character.Name, character.ID);
			}
		}
	}
}
