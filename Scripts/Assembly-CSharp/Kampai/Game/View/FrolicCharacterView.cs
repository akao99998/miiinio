using System;
using System.Collections.Generic;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class FrolicCharacterView : NamedMinionView
	{
		private Signal<string, Type, object> animSignal = new Signal<string, Type, object>();

		public int IdleStateHash;

		public Dictionary<string, object> CelebrateParams;

		public Signal AnimationCallback;

		public Agent agent;

		protected MinionAnimationDefinition currentFrolicAnimation;

		protected RuntimeAnimatorController nextFrolicAnimationController;

		protected string wanderControllerName;

		protected RuntimeAnimatorController wanderAnimationController;

		protected Angle currentFrolicRotation;

		public override Signal<string, Type, object> AnimSignal
		{
			get
			{
				return animSignal;
			}
		}

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			FrolicCharacter frolicCharacter = character as FrolicCharacter;
			FrolicCharacterDefinition definition = frolicCharacter.Definition;
			wanderControllerName = definition.WanderStateMachine;
			IdleStateHash = Animator.StringToHash("Base Layer.Idle");
			CelebrateParams = new Dictionary<string, object>();
			CelebrateParams.Add("isCelebrating", true);
			base.Build(character, parent, logger, minionBuilder);
			agent = base.gameObject.GetComponent<Agent>();
			if (agent == null)
			{
				agent = base.gameObject.AddComponent<Agent>();
			}
			agent.Radius = 0.5f;
			agent.Mass = 1f;
			agent.MaxForce = 8f;
			agent.MaxSpeed = 0f;
			base.gameObject.SetLayerRecursively(8);
			SteerCharacterToFollowPath steerCharacterToFollowPath = base.gameObject.AddComponent<SteerCharacterToFollowPath>();
			steerCharacterToFollowPath.enabled = false;
			steerCharacterToFollowPath.Threshold = 0.2f;
			steerCharacterToFollowPath.FinalThreshold = 0.1f;
			steerCharacterToFollowPath.Modifier = 4;
			return this;
		}

		internal void IdleInTownHall(LocationIncidentalAnimationDefinition animationDefinition, MinionAnimationDefinition mad)
		{
			logger.Info("Frolic Character Idle InTownHall animation:{0} location:{1} => {2}", mad.ID, animationDefinition.LocalizedKey, ((Vector3)animationDefinition.Location).ToString());
			if (wanderAnimationController == null)
			{
				wanderAnimationController = KampaiResources.Load<RuntimeAnimatorController>(wanderControllerName);
			}
			if (!IsWandering())
			{
				GameObjectUtil.TryEnableBehaviour<Agent>(base.gameObject, true);
				GameObjectUtil.TryEnableBehaviour<SteerCharacterToFollowPath>(base.gameObject, true);
			}
			ClearActionQueue();
			SetAnimController(wanderAnimationController);
			currentFrolicAnimation = mad;
			currentFrolicRotation = animationDefinition.Rotation;
			SteerCharacterToFollowPath component = base.gameObject.GetComponent<SteerCharacterToFollowPath>();
			component.SetTarget((Vector3)animationDefinition.Location);
			component.enabled = true;
			nextFrolicAnimationController = KampaiResources.Load<RuntimeAnimatorController>(mad.StateMachine);
			if (nextFrolicAnimationController == null)
			{
				logger.Error("Unable to load animation controller {0} for {1}", mad.StateMachine, base.name);
			}
			agent.MaxSpeed = 1f;
		}

		internal void ArrivedAtWayPoint()
		{
			logger.Info("Frolic Minion ArrivedAtWayPoint");
			agent.MaxSpeed = 0f;
			if (currentFrolicAnimation != null)
			{
				ClearActionQueue();
				if (currentFrolicAnimation.FaceCamera)
				{
					EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
				}
				else if (currentFrolicRotation != null)
				{
					EnqueueAction(new RotateAction(this, currentFrolicRotation.Degrees, 360f, logger));
				}
				EnqueueAction(new SetAnimatorAction(this, nextFrolicAnimationController, logger, currentFrolicAnimation.arguments));
				float animationSeconds = currentFrolicAnimation.AnimationSeconds;
				if (animationSeconds > 0f)
				{
					EnqueueAction(new DelayAction(this, animationSeconds, logger));
				}
				if (nextFrolicAnimationController == wanderAnimationController)
				{
					EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "chill", false));
				}
				else
				{
					EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
					EnqueueAction(new SetAnimatorAction(this, wanderAnimationController, logger));
				}
				if (AnimationCallback != null)
				{
					EnqueueAction(new SendSignalAction(AnimationCallback, logger));
				}
				currentFrolicAnimation = null;
			}
			else
			{
				logger.Error("No animaiton to play");
			}
		}

		public void SetAnimationCallback(Signal callback)
		{
			AnimationCallback = callback;
		}

		internal bool IsWandering()
		{
			return GetCurrentAnimController() == wanderAnimationController;
		}

		internal virtual void SetIsInParty(bool enable)
		{
			agent.MaxSpeed = 0f;
			EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "isInParty", enable));
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (IsWandering())
			{
				SetAnimBool("isMoving", agent.MaxSpeed > 0.0001f);
				SetAnimFloat("speed", agent.Speed);
			}
		}
	}
}
