using System;
using System.Collections.Generic;
using Kampai.Game.Transaction;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class StuartView : FrolicCharacterView
	{
		private const string Base_Layer_OnStage_Idle = "Base Layer.OnStage_Idle";

		private const string Base_Layer_OnStage_Celebrate = "Base Layer.OnStage_Celebrate";

		private const string Base_Layer_OnStage_Perform = "Base Layer.OnStage_Perform";

		private const string BOOL_ISPERFORMING = "IsPerforming";

		private const string BOOL_ISCELEBRATING = "IsCelebrating";

		private const int SPIN_MIX_INDEX = 0;

		private string stageControllerName;

		private RuntimeAnimatorController stageController;

		private MinionAnimationDefinition onStageAnimation;

		private bool onStage;

		private WeightedInstance onStageIdleWeightedInstance;

		private WeightedInstance onStageTicketFilledWeightedInstance;

		private WeightedInstance onStagePerformWeightedInstance;

		private int lastIndex = -1;

		public Action OnSpinMic { get; set; }

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			base.Build(character, parent, logger, minionBuilder);
			StuartCharacter stuartCharacter = character as StuartCharacter;
			StuartCharacterDefinition definition = stuartCharacter.Definition;
			onStageAnimation = definition.OnStageAnimation;
			stageControllerName = definition.OnStageAnimation.StateMachine;
			int onStageIdleAnimationCount = definition.OnStageIdleAnimationCount;
			int onStageTicketFilledAnimationCount = definition.OnStageTicketFilledAnimationCount;
			int onStagePerformAnimationCount = definition.OnStagePerformAnimationCount;
			onStageIdleWeightedInstance = CreateWeightedInstance(onStageIdleAnimationCount);
			onStageTicketFilledWeightedInstance = CreateWeightedInstance(onStageTicketFilledAnimationCount);
			onStagePerformWeightedInstance = CreateWeightedInstance(onStagePerformAnimationCount);
			return this;
		}

		internal void AddToStage(Vector3 position, Quaternion rotation, StuartStageAnimationType animType)
		{
			GameObjectUtil.TryEnableBehaviour<Agent>(base.gameObject, false);
			GameObjectUtil.TryEnableBehaviour<SteerCharacterToFollowPath>(base.gameObject, false);
			base.transform.position = position;
			base.transform.localRotation = rotation;
			if (stageController == null)
			{
				stageController = KampaiResources.Load<RuntimeAnimatorController>(stageControllerName);
			}
			switch (animType)
			{
			case StuartStageAnimationType.CELEBRATE:
				EnqueueAction(new SetAnimatorAction(this, stageController, logger, onStageAnimation.arguments), true);
				GetOnStage(true);
				StartingState(StuartStageAnimationType.CELEBRATE);
				break;
			case StuartStageAnimationType.IDLEONSTAGE:
				GetOnStage(true);
				EnqueueAction(new SetAnimatorAction(this, stageController, logger, onStageAnimation.arguments), true);
				StartingState(StuartStageAnimationType.IDLEONSTAGE);
				break;
			default:
				GetOnStage(false);
				EnqueueAction(new SetAnimatorAction(this, stageController, logger), true);
				break;
			}
			base.IsIdle = true;
		}

		internal void GetOnStage(bool enable, bool clear = false)
		{
			if (onStage != enable)
			{
				onStage = enable;
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "goToStage", enable), clear);
				base.IsIdle = !enable;
			}
		}

		internal void GetOnStageImmediate(bool enable)
		{
			if (onStage != enable)
			{
				onStage = enable;
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "Immediate", null, "goToStage", enable), true);
				base.IsIdle = !enable;
				if (enable)
				{
					StartingState(StuartStageAnimationType.IDLEONSTAGE);
				}
			}
		}

		internal void Perform(SignalCallback<Signal> finishedCallback)
		{
			base.IsIdle = false;
			SetAnimController(stageController);
			SetAnimBool("goToStage", true);
			StartingState(StuartStageAnimationType.PERFORM, true);
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.OffStage_Idle"), logger));
			EnqueueAction(new DelegateAction(SetToIdle, logger));
			EnqueueAction(new SendSignalAction(finishedCallback.PromiseDispatch(), logger));
		}

		private void SetToIdle()
		{
			base.IsIdle = true;
		}

		internal void GetAttention(bool enable)
		{
			EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "isGetAttention", enable));
		}

		internal override void SetIsInParty(bool enable)
		{
			if (IsWandering())
			{
				base.SetIsInParty(enable);
				return;
			}
			SetAnimBool("isInParty", enable);
			SetAnimBool("stageIsBuilt", onStage);
		}

		internal void TuneGuitar(Action onDone)
		{
			StartingState(StuartStageAnimationType.IDLEONSTAGE, false, onDone);
		}

		public bool IsOnStage()
		{
			return onStage;
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

		public void StartingState(StuartStageAnimationType targetState, bool clearQueue = false, Action onDone = null)
		{
			int num = 0;
			switch (targetState)
			{
			case StuartStageAnimationType.IDLEONSTAGE:
				num = onStageIdleWeightedInstance.NextPick(base.randomService).ID;
				num = ((num == lastIndex) ? onStageIdleWeightedInstance.NextPick(base.randomService).ID : num);
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "IsPerforming", false, "IsCelebrating", false), clearQueue);
				EnqueueAction(new DelayAction(this, 0.5f, logger));
				EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.OnStage_Idle"), logger));
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "randomizer", num, "TriggerIdle", null));
				if (num == 0)
				{
					EnqueueAction(new DelegateAction(OnSpinMic, logger));
				}
				if (onDone != null)
				{
					EnqueueAction(new DelegateAction(onDone, logger));
				}
				EnqueueAction(new PickNextStuartAnimationAction(this, targetState, logger));
				lastIndex = num;
				break;
			case StuartStageAnimationType.CELEBRATE:
				num = onStageTicketFilledWeightedInstance.NextPick(base.randomService).ID;
				num = ((num == lastIndex) ? onStageTicketFilledWeightedInstance.NextPick(base.randomService).ID : num);
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "IsPerforming", false, "IsCelebrating", true), clearQueue);
				EnqueueAction(new DelayAction(this, 0.5f, logger));
				EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.OnStage_Celebrate"), logger));
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "randomizer", num));
				if (onDone != null)
				{
					EnqueueAction(new DelegateAction(onDone, logger));
				}
				EnqueueAction(new PickNextStuartAnimationAction(this, StuartStageAnimationType.IDLEONSTAGE, logger));
				lastIndex = num;
				break;
			case StuartStageAnimationType.PERFORM:
				num = onStagePerformWeightedInstance.NextPick(base.randomService).ID;
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "randomizer", num, "IsPerforming", true, "IsCelebrating", false), true);
				EnqueueAction(new PlayMecanimStateAction(this, Animator.StringToHash("Base Layer.OnStage_Perform"), logger));
				EnqueueAction(new DelayAction(this, 0.5f, logger));
				EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "goToStage", false, "IsPerforming", false));
				lastIndex = -1;
				break;
			case StuartStageAnimationType.IDLEOFFSTAGE:
				break;
			}
		}
	}
}
