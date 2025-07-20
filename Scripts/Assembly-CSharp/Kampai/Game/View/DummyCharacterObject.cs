using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using Kampai.Util.Graphics;
using UnityEngine;
using UnityEngine.Rendering;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class DummyCharacterObject : ActionableObject
	{
		private struct LOD
		{
			public int Number;

			private List<GameObject> Renderers;

			internal LOD(int number, GameObject go)
			{
				Number = number;
				Renderers = new List<GameObject>(2);
				Renderers.Add(go);
			}

			internal void addRendererGameObject(GameObject go)
			{
				Renderers.Add(go);
			}

			internal void Enable()
			{
				for (int i = 0; i < Renderers.Count; i++)
				{
					Renderers[i].SetActive(true);
				}
			}
		}

		private const int EXPECTED_LOD_COUNT = 3;

		private const int EXPECTED_RENDERERS_PER_LOD = 2;

		protected RuntimeAnimatorController cachedDefaultController;

		protected RuntimeAnimatorController cachedRuntimeController;

		protected bool animatorControllersAreEqual;

		private IRandomService randomService;

		private IDefinitionService definitionService;

		private bool markedForDestory;

		private KampaiImage shadowImage;

		private int idleAnimationCount;

		private WeightedInstance idleWeightedInstance;

		private int selectedAnimationCount;

		private WeightedInstance selectedWeightedInstance;

		private int happyAnimationCount;

		private WeightedInstance happyWeightedInstance;

		private List<WeightedInstance> animationPoolWeightedInstanceList;

		private int lastIndex = -1;

		private Signal networkOpenSignal;

		private Signal networkCloseSignal;

		private DummyCharacterAnimationState animationState;

		public void Init(Character character, IKampaiLogger logger, IRandomService randomService, IDefinitionService definitinoService, List<WeightedInstance> weightedInstanceList)
		{
			base.Init();
			ID = character.ID;
			base.name = character.Name;
			base.logger = logger;
			this.randomService = randomService;
			definitionService = definitinoService;
			animationPoolWeightedInstanceList = weightedInstanceList;
			SetName(character);
			animators = new List<Animator>(base.gameObject.GetComponentsInChildren<Animator>());
			Minion minion = character as Minion;
			if (minion != null && !minion.HasPrestige)
			{
				MinionObject.SetEyes(base.transform, minion.Definition.Eyes);
				MinionObject.SetBody(base.transform, minion.Definition.Body);
				MinionObject.SetHair(base.transform, minion.Definition.Hair);
			}
			else if (character is KevinCharacter)
			{
				MinionObject.SetEyes(base.transform, 2u);
				MinionObject.SetBody(base.transform, MinionBody.TALL);
				MinionObject.SetHair(base.transform, MinionHair.SPROUT);
			}
		}

		private WeightedInstance CreateWeightedInstance(int weightedCount)
		{
			WeightedDefinition weightedDefinition = new WeightedDefinition();
			weightedDefinition.Entities = new List<WeightedQuantityItem>();
			for (int i = 0; i < weightedCount; i++)
			{
				weightedDefinition.Entities.Add(new WeightedQuantityItem(i, 0u, 1u));
			}
			return new WeightedInstance(weightedDefinition);
		}

		public DummyCharacterObject Build(Character character, CharacterUIAnimationDefinition characterUIAnimationDefinition, Transform parent, IKampaiLogger logger, bool forceHighLOD, Vector3 villainScale, Vector3 villainPositionOffset, IMinionBuilder minionBuilder)
		{
			if (parent != null)
			{
				base.gameObject.transform.SetParent(parent, false);
			}
			base.gameObject.transform.localEulerAngles = Vector3.zero;
			if (character is Villain)
			{
				base.gameObject.transform.localPosition = villainPositionOffset;
				base.gameObject.transform.localScale = villainScale;
			}
			else
			{
				base.gameObject.transform.localPosition = Vector3.zero;
				base.gameObject.transform.localScale = Vector3.one;
				SetupLODs(forceHighLOD);
			}
			string stateMachine = characterUIAnimationDefinition.StateMachine;
			Animator component = base.gameObject.GetComponent<Animator>();
			component.applyRootMotion = false;
			component.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(stateMachine);
			component.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			if (component.runtimeAnimatorController == null)
			{
				logger.Error("Failed to get default runtime animator controller for {0}: asm name: {1}", character.Name, stateMachine);
			}
			Transform rootBone = base.gameObject.transform.Find(GetRootJointPath());
			SkinnedMeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				skinnedMeshRenderer.rootBone = rootBone;
			}
			SetupBlobShadow(minionBuilder);
			base.gameObject.SetLayerRecursively(5);
			SetupCharacterObject(component.runtimeAnimatorController);
			SetupCharacterAnimation(component, characterUIAnimationDefinition);
			return this;
		}

		private void SetupCharacterAnimation(Animator anim, CharacterUIAnimationDefinition characterUIAnimationDefinition)
		{
			if (characterUIAnimationDefinition.UseLegacy)
			{
				idleAnimationCount = characterUIAnimationDefinition.IdleCount;
				happyAnimationCount = characterUIAnimationDefinition.HappyCount;
				selectedAnimationCount = characterUIAnimationDefinition.SelectedCount;
			}
			else
			{
				SetupOverrideAnimatorController(anim);
			}
			idleWeightedInstance = CreateWeightedInstance(idleAnimationCount);
			selectedWeightedInstance = CreateWeightedInstance(selectedAnimationCount);
			happyWeightedInstance = CreateWeightedInstance(happyAnimationCount);
		}

		private void SetupOverrideAnimatorController(Animator anim)
		{
			AnimatorOverrideController overrideController = new AnimatorOverrideController();
			overrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
			ReplacingClips(ref overrideController);
			anim.runtimeAnimatorController = overrideController;
		}

		private void ReplacingClips(ref AnimatorOverrideController overrideController)
		{
			AnimationClipPair[] clips = overrideController.clips;
			for (int i = 0; i < clips.Length; i++)
			{
				GrabClipBasedOnName(ref overrideController, clips[i].originalClip.name);
			}
		}

		private void GrabClipBasedOnName(ref AnimatorOverrideController overrideController, string name)
		{
			if (name.Contains("Idle"))
			{
				idleAnimationCount++;
				overrideController[name] = GetAnimationClip(0);
			}
			else if (name.Contains("Happy"))
			{
				happyAnimationCount++;
				overrideController[name] = GetAnimationClip(1);
			}
			else if (name.Contains("Selected"))
			{
				selectedAnimationCount++;
				overrideController[name] = GetAnimationClip(2);
			}
		}

		private AnimationClip GetAnimationClip(int weightedAnimationIndex)
		{
			int count = animationPoolWeightedInstanceList.Count;
			if (count > weightedAnimationIndex)
			{
				QuantityItem quantityItem = animationPoolWeightedInstanceList[weightedAnimationIndex].NextPick(randomService);
				UIAnimationDefinition uIAnimationDefinition = definitionService.Get<UIAnimationDefinition>(quantityItem.ID);
				return KampaiResources.Load<AnimationClip>(uIAnimationDefinition.AnimationClipName);
			}
			if (count > 0)
			{
				logger.Error("Index out of range for animationPoolWeightedInstanceList in DummyCharacterObject, picking index 0");
				return GetAnimationClip(0);
			}
			logger.Error("No animation weighted instance found for this character {0}", ID);
			return null;
		}

		public void SetUpWifiListeners(Signal openSignal, Signal closeSignal)
		{
			networkOpenSignal = openSignal;
			networkCloseSignal = closeSignal;
			networkOpenSignal.AddListener(HideDummy);
			networkCloseSignal.AddListener(ShowDummy);
		}

		private void HideDummy()
		{
			base.gameObject.SetActive(false);
		}

		private void ShowDummy()
		{
			base.gameObject.SetActive(true);
			StartingState(animationState, true);
		}

		public void SetStenciledShader()
		{
			int stencilRef = 2;
			int num = 1;
			Renderer componentInChildren = GetComponentInChildren<Renderer>();
			if (componentInChildren != null)
			{
				Material[] materials = componentInChildren.materials;
				foreach (Material material in materials)
				{
					if (!(material == null) && !(material.shader == null) && !string.IsNullOrEmpty(material.shader.name))
					{
						switch (material.shader.name)
						{
						case "Kampai/Standard/Texture":
						case "Kampai/Standard/Minion":
						case "Kampai/Standard/Minion_LOD1":
							ShaderUtils.EnableStencilShader(material, stencilRef, num++);
							break;
						}
					}
				}
			}
			if (shadowImage != null)
			{
				shadowImage.SetStencilMaterial();
			}
		}

		private void SetupBlobShadow(IMinionBuilder minionBuilder)
		{
			if (minionBuilder.GetLOD() != TargetPerformance.LOW && minionBuilder.GetLOD() != 0)
			{
				GameObject original = KampaiResources.Load("MinionBlobShadow_UI") as GameObject;
				GameObject gameObject = UnityEngine.Object.Instantiate(original);
				shadowImage = gameObject.GetComponent<KampaiImage>();
				gameObject.transform.SetParent(base.gameObject.transform, false);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
			}
		}

		public void RemoveCoroutine(bool isDestorying = true)
		{
			if (isDestorying)
			{
				markedForDestory = true;
			}
		}

		public virtual void OnDestroy()
		{
			if (networkOpenSignal != null)
			{
				networkOpenSignal.RemoveListener(HideDummy);
			}
			if (networkCloseSignal != null)
			{
				networkCloseSignal.RemoveListener(ShowDummy);
			}
			RemoveCoroutine();
		}

		public void StartingState(DummyCharacterAnimationState targetState, bool clearQueue = false)
		{
			if (!markedForDestory)
			{
				int num = 0;
				animationState = targetState;
				switch (targetState)
				{
				case DummyCharacterAnimationState.Idle:
					num = idleWeightedInstance.NextPick(randomService).ID;
					num = ((num == lastIndex) ? idleWeightedInstance.NextPick(randomService).ID : num);
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "IsIdle", true, "IsHappy", false), clearQueue);
					EnqueueAction(new DelayAction(this, 0.5f, logger));
					EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Idle"), logger));
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "IdleRandomizer", num));
					EnqueueAction(new PickNextDummyUIAnimationAction(this, targetState, logger));
					lastIndex = num;
					break;
				case DummyCharacterAnimationState.Happy:
					num = happyWeightedInstance.NextPick(randomService).ID;
					num = ((num == lastIndex) ? happyWeightedInstance.NextPick(randomService).ID : num);
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "IsIdle", false, "IsHappy", true), clearQueue);
					EnqueueAction(new DelayAction(this, 0.5f, logger));
					EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Happy"), logger));
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "HappyRandomizer", num));
					EnqueueAction(new PickNextDummyUIAnimationAction(this, targetState, logger));
					lastIndex = num;
					break;
				case DummyCharacterAnimationState.SelectedIdle:
					num = selectedWeightedInstance.NextPick(randomService).ID;
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "SelectedRandomizer", num, "IsSelected", null, "IsIdle", true, "IsHappy", false), true);
					EnqueueAction(new DelayAction(this, 0.5f, logger));
					EnqueueAction(new PickNextDummyUIAnimationAction(this, DummyCharacterAnimationState.Idle, logger));
					lastIndex = -1;
					break;
				case DummyCharacterAnimationState.SelectedHappy:
					num = selectedWeightedInstance.NextPick(randomService).ID;
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "SelectedRandomizer", num, "IsSelected", null, "IsIdle", false, "IsHappy", true), true);
					EnqueueAction(new DelayAction(this, 0.5f, logger));
					EnqueueAction(new PickNextDummyUIAnimationAction(this, DummyCharacterAnimationState.Happy, logger));
					lastIndex = -1;
					break;
				}
			}
		}

		public void StartBundlePackAnimation()
		{
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>("asm_UI_minion_bundle");
			EnqueueAction(new SetAnimatorAction(this, controller, logger), true);
		}

		public void MakePhilDance()
		{
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>("asm_UI_minion_solo_dance_loop");
			EnqueueAction(new SetAnimatorAction(this, controller, logger), true);
		}

		private void addRenderToLodList(ref List<LOD> lodRenderers, Renderer renderer)
		{
			GameObject gameObject = renderer.gameObject;
			string text = gameObject.name;
			int startIndex = text.IndexOf("LOD", StringComparison.Ordinal) + 3;
			int result = 0;
			int.TryParse(text.Substring(startIndex), out result);
			for (int i = 0; i < lodRenderers.Count; i++)
			{
				if (lodRenderers[i].Number == result)
				{
					lodRenderers[i].addRendererGameObject(gameObject);
					return;
				}
			}
			lodRenderers.Add(new LOD(result, gameObject));
		}

		protected void SetupLODs(bool forceHigh)
		{
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			List<LOD> lodRenderers = new List<LOD>(3);
			foreach (Renderer renderer in componentsInChildren)
			{
				addRenderToLodList(ref lodRenderers, renderer);
				renderer.gameObject.SetActive(false);
				renderer.shadowCastingMode = ShadowCastingMode.Off;
				renderer.receiveShadows = false;
			}
			lodRenderers.Sort((LOD x, LOD y) => x.Number - y.Number);
			int index = ((!forceHigh) ? (lodRenderers.Count - 1) : 0);
			lodRenderers[index].Enable();
		}

		public override void SetAnimController(RuntimeAnimatorController controller)
		{
			animators[0].runtimeAnimatorController = controller;
		}

		private void SetupCharacterObject(RuntimeAnimatorController defaultController)
		{
			SetDefaultAnimController(defaultController);
		}

		protected string GetRootJointPath()
		{
			return "minion:ROOT/minion:pelvis_jnt";
		}

		protected void SetName(Character character)
		{
			if (base.gameObject.name.Length == 0)
			{
				base.gameObject.name = string.Format("Minion_{0}", character.ID);
			}
		}
	}
}
