using System;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class BobView : NamedMinionView
	{
		private Signal<string, Type, object> townSquareAnimSignal = new Signal<string, Type, object>();

		private Signal delaySignal = new Signal();

		private MinionAnimationDefinition currentFrolicAnimation;

		private RuntimeAnimatorController nextFrolicAnimationController;

		private RuntimeAnimatorController defaultAnimationController;

		private MinionAnimationDefinition celebrateAnimation;

		private MinionAnimationDefinition attentionAnimation;

		private Signal animationCallback;

		private Agent agent;

		private Vector3 defaultPosition;

		private Angle currentFrolicRotation;

		private bool waitingOnSignClear;

		public override Signal<string, Type, object> AnimSignal
		{
			get
			{
				return townSquareAnimSignal;
			}
		}

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			base.Build(character, parent, logger, minionBuilder);
			SteerCharacterToFollowPath steerCharacterToFollowPath = base.gameObject.AddComponent<SteerCharacterToFollowPath>();
			steerCharacterToFollowPath.enabled = false;
			steerCharacterToFollowPath.Threshold = 0.2f;
			steerCharacterToFollowPath.FinalThreshold = 0.1f;
			steerCharacterToFollowPath.Modifier = 4;
			agent = base.gameObject.GetComponent<Agent>();
			if (agent == null)
			{
				agent = base.gameObject.AddComponent<Agent>();
			}
			agent.Radius = 0.5f;
			agent.Mass = 1f;
			agent.MaxForce = 8f;
			agent.MaxSpeed = 0f;
			defaultAnimationController = base.gameObject.GetComponent<Animator>().runtimeAnimatorController;
			if (defaultAnimationController == null)
			{
				logger.Error("No default animation controller found for {0}", base.name);
			}
			BobCharacter bobCharacter = character as BobCharacter;
			celebrateAnimation = bobCharacter.Definition.CelebrateAnimation;
			attentionAnimation = bobCharacter.Definition.AttentionAnimation;
			base.gameObject.transform.position = (defaultPosition = (Vector3)bobCharacter.Definition.WanderAnimations[0].Location);
			return this;
		}

		internal void PointAtSign(Vector3 position)
		{
			if (!waitingOnSignClear)
			{
				logger.Info("Bob PointAtSign");
				SetAnimController(defaultAnimationController);
				base.transform.position = position;
				Agent component = base.gameObject.GetComponent<Agent>();
				component.MaxSpeed = 0f;
				ClearActionQueue();
				if (attentionAnimation.FaceCamera)
				{
					EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
				}
				RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(attentionAnimation.StateMachine);
				EnqueueAction(new SetAnimatorAction(this, controller, logger, attentionAnimation.arguments));
			}
		}

		internal void CelebrateLandExpansion(Signal callback)
		{
			waitingOnSignClear = true;
			logger.Info("Bob CelebrateLandExpansion");
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(celebrateAnimation.StateMachine);
			if (celebrateAnimation.FaceCamera)
			{
				EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			}
			EnqueueAction(new SetAnimatorAction(this, controller, logger, celebrateAnimation.arguments));
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
			EnqueueAction(new SendSignalAction(delaySignal, logger));
			EnqueueAction(new SendSignalAction(callback, logger));
			delaySignal.AddOnce(ToggleBool);
		}

		private void ToggleBool()
		{
			waitingOnSignClear = false;
		}

		internal void IdleInTownHall(LocationIncidentalAnimationDefinition animationDefinition, MinionAnimationDefinition mad)
		{
			logger.Info("Bob Idle InTownHall animation:{0} location:{1} => {2}", mad.ID, animationDefinition.LocalizedKey, ((Vector3)animationDefinition.Location).ToString());
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
			logger.Info("Bob ArrivedAtWayPoint");
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
				EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
				EnqueueAction(new SetAnimatorAction(this, defaultAnimationController, logger));
				if (animationCallback != null)
				{
					EnqueueAction(new SendSignalAction(animationCallback, logger));
				}
				currentFrolicAnimation = null;
			}
			else
			{
				logger.Error("No animaiton to play");
			}
		}

		internal void ReturnToTown()
		{
			logger.Info("Bob ReturnToTown");
			agent.MaxSpeed = 1f;
			base.gameObject.transform.position = defaultPosition;
			SetAnimController(defaultAnimationController);
		}

		public void SetAnimationCallback(Signal callback)
		{
			animationCallback = callback;
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (GetCurrentAnimControllerName() == defaultAnimationController.name)
			{
				SetAnimBool("isMoving", agent.MaxSpeed > 0.0001f);
				SetAnimFloat("speed", agent.Speed);
			}
		}
	}
}
