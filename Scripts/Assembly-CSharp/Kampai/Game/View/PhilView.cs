using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class PhilView : NamedMinionView
	{
		private string TikiBarStateMachine;

		private Signal<string, Type, object> tikiBarAnimSignal = new Signal<string, Type, object>();

		internal Signal partyIntroCompleteSignal = new Signal();

		public Signal OnAnimatorParamatersResetSignal = new Signal();

		private List<Vector3> introPath;

		private Vector3 introEulers;

		private float introTime;

		public override Signal<string, Type, object> AnimSignal
		{
			get
			{
				return tikiBarAnimSignal;
			}
		}

		protected override string AttentionString
		{
			get
			{
				return "bartender_IsGetAttention";
			}
		}

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			base.Build(character, parent, logger, minionBuilder);
			PhilCharacter philCharacter = character as PhilCharacter;
			TikiBarStateMachine = philCharacter.Definition.TikiBarStateMachine;
			introPath = new List<Vector3>(philCharacter.Definition.IntroPath);
			introEulers = philCharacter.Definition.RotationEulers;
			introTime = philCharacter.Definition.IntroTime;
			return this;
		}

		public override void ResetAnimationParameters()
		{
			base.ResetAnimationParameters();
			OnAnimatorParamatersResetSignal.Dispatch();
		}

		internal void GoToStartLocation()
		{
			Animate("walk");
			GoSpline path = new GoSpline(introPath);
			GoTween tween = new GoTween(base.transform, 0.25f, new GoTweenConfig().rotation(introEulers));
			GoTween tween2 = new GoTween(base.transform, introTime, new GoTweenConfig().setEaseType(GoEaseType.Linear).positionPath(path, false, GoLookAtType.NextPathNode).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
				StopWalking();
			}));
			GoTweenFlow goTweenFlow = new GoTweenFlow();
			goTweenFlow.insert(0f, tween2).insert(1f, tween).play();
		}

		internal void SignFixed()
		{
			SetAnimTrigger("signFixed");
			tikiBarAnimSignal.Dispatch("signFixed", typeof(bool), true);
		}

		internal void Activate(bool activate)
		{
			SetAnimBool("bartender_IsActivated", activate);
			tikiBarAnimSignal.Dispatch("bartender_IsActivated", typeof(bool), activate);
			if (!activate && GetAnimBool("isSeated"))
			{
				base.IsIdle = true;
			}
			else
			{
				base.IsIdle = false;
			}
		}

		internal void SitAtBar(object sit, object teleportSignal)
		{
			bool flag = (bool)sit;
			TeleportCharacterToTikiBarSignal teleportCharacterToTikiBarSignal = (TeleportCharacterToTikiBarSignal)teleportSignal;
			teleportCharacterToTikiBarSignal.Dispatch(this, 0);
			if (flag && !GetAnimBool("isActivated"))
			{
				base.IsIdle = true;
			}
			else
			{
				base.IsIdle = false;
			}
		}

		internal void Celebrate()
		{
			SetAnimBool("bartender_OnCelebrate", true);
			tikiBarAnimSignal.Dispatch("bartender_OnCelebrate", typeof(bool), true);
		}

		internal void PlayConFetti()
		{
			SetAnimTrigger("confetti");
			tikiBarAnimSignal.Dispatch("confetti", typeof(bool), true);
		}

		internal void StartParty()
		{
			SetAnimBool("PartySkip", false);
			string text = "bartender_StartParty";
			EnqueueAction(new SetAnimatorAction(this, GetCurrentAnimController(), text, logger), true);
			tikiBarAnimSignal.Dispatch(text, typeof(bool), true);
			EnqueueAction(new DelayAction(this, 3.34f, logger));
			EnqueueAction(new DelegateAction(PartyIntroComplete, logger));
		}

		private void PartyIntroComplete()
		{
			partyIntroCompleteSignal.Dispatch();
		}

		internal void PartySkip()
		{
			SetAnimBool("PartySkip", true);
		}

		internal void SpinFireStick(bool enable)
		{
			string type = "isInParty";
			SetAnimBool(type, enable);
			tikiBarAnimSignal.Dispatch(type, typeof(bool), enable);
			if (enable)
			{
				SetAnimTrigger("StartParting");
			}
		}

		public override void AddProp(string propName, GameObject parent)
		{
			bool animBool = GetAnimBool("isInParty");
			bool animBool2 = GetAnimBool("bartender_IsActivated");
			bool animBool3 = GetAnimBool("PartySkip");
			base.AddProp(propName, parent);
			SetAnimBool("isInParty", animBool);
			SetAnimBool("bartender_IsActivated", animBool2);
			SetAnimBool("PartySkip", animBool3);
		}

		public override void RemoveProp(string propName)
		{
			bool animBool = GetAnimBool("isInParty");
			bool animBool2 = GetAnimBool("bartender_IsActivated");
			bool animBool3 = GetAnimBool("PartySkip");
			base.RemoveProp(propName);
			SetAnimBool("isInParty", animBool);
			SetAnimBool("bartender_IsActivated", animBool2);
			SetAnimBool("PartySkip", animBool3);
		}

		internal void GetAttention(bool enable)
		{
			base.IsAtAttention = enable;
			SetAnimBool(AttentionStartString, enable);
			SetAnimBool(AttentionString, enable);
			tikiBarAnimSignal.Dispatch(AttentionStartString, typeof(bool), enable);
			tikiBarAnimSignal.Dispatch(AttentionString, typeof(bool), enable);
		}

		internal void BeginIntroLoop()
		{
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.NewMinionIntro"), logger));
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Occupied"), logger));
			EnqueueAction(new DelegateAction(IntroComplete, logger));
			ResetAnimTrigger("OnNewMinionIntro");
			SetAnimBool("OnNewMinionAppear", true);
			SetAnimBool("NewMinionSequence", true);
			tikiBarAnimSignal.Dispatch("OnNewMinionAppear", typeof(bool), true);
			tikiBarAnimSignal.Dispatch("NewMinionSequence", typeof(bool), true);
		}

		internal void PlayIntro()
		{
			SetAnimBool("OnNewMinionIntro", true);
			tikiBarAnimSignal.Dispatch("OnNewMinionIntro", typeof(bool), true);
		}

		internal void Animate(string animation)
		{
			SetAnimBool(animation, true);
			tikiBarAnimSignal.Dispatch(animation, typeof(bool), true);
		}

		internal void StopWalking()
		{
			SetAnimBool("walk", false);
			tikiBarAnimSignal.Dispatch("walk", typeof(bool), false);
		}

		internal void GotoTikiBar(GameObject tikiBar, TeleportCharacterToTikiBarSignal teleportSignal)
		{
			Transform transform = tikiBar.transform.Find("route0");
			base.transform.position = transform.position;
			base.transform.eulerAngles = transform.eulerAngles;
			SetAnimBool("goToBar", true);
			StartCoroutine(WaitAFrame());
			ClearActionQueue();
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.idleAtBar"), logger));
			EnqueueAction(new DuelParameterizedDelegateAction(SitAtBar, true, teleportSignal, logger));
		}

		internal void EnableTikiBarController()
		{
			ClearActionQueue();
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(TikiBarStateMachine);
			EnqueueAction(new SetAnimatorAction(this, controller, logger));
		}

		private void IntroComplete()
		{
			SetAnimBool("NewMinionSequence", false);
			tikiBarAnimSignal.Dispatch("NewMinionSequence", typeof(bool), false);
		}

		private IEnumerator WaitAFrame()
		{
			yield return new WaitForEndOfFrame();
			UpdateBlobShadowPosition();
		}
	}
}
