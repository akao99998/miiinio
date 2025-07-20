using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class SpecialEventCharacterView : FrolicCharacterView
	{
		internal Signal RemoveCharacterSignal = new Signal();

		internal Signal NextPartyAnimSignal = new Signal();

		private List<Vector3> introPath;

		private List<Vector3> reverseIntroPath;

		private float introTime;

		internal SpecialEventCharacter eventCharacter;

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			eventCharacter = character as SpecialEventCharacter;
			SpecialEventCharacterDefinition definition = eventCharacter.Definition;
			introPath = new List<Vector3>(definition.IntroPath);
			reverseIntroPath = new List<Vector3>(definition.IntroPath);
			reverseIntroPath.Reverse();
			introTime = definition.IntroTime;
			base.Build(character, parent, logger, minionBuilder);
			return this;
		}

		internal void ShowSpecialEventCharacter()
		{
			AnimatePosition(true);
		}

		internal void HideSpecialEventCharacter(bool completedQuest)
		{
			if (completedQuest)
			{
				EnqueueAction(new SetAnimatorAction(this, null, logger, CelebrateParams));
				EnqueueAction(new WaitForMecanimStateAction(this, IdleStateHash, logger));
			}
			AnimatePosition(false);
		}

		private void AnimatePosition(bool show)
		{
			List<Vector3> path = ((!show) ? reverseIntroPath : introPath);
			EnqueueAction(new PathAction(this, path, introTime, logger));
			if (!show)
			{
				EnqueueAction(new SendSignalAction(RemoveCharacterSignal, logger));
			}
			else if (AnimationCallback != null)
			{
				EnqueueAction(new SendSignalAction(AnimationCallback, logger));
			}
		}

		public void PlayPartyAnimation(MinionAnimationDefinition def)
		{
			agent.MaxSpeed = 0f;
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(def.StateMachine);
			EnqueueAction(new SetAnimatorAction(this, controller, logger, def.arguments), true);
			if (def.FaceCamera)
			{
				EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			}
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
			EnqueueAction(new SendSignalAction(NextPartyAnimSignal, logger));
		}
	}
}
