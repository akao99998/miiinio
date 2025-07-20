using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class TSMCharacterView : NamedMinionView
	{
		private Signal<string, Type, object> animSignal = new Signal<string, Type, object>();

		internal Signal RemoveCharacterSignal = new Signal();

		internal Signal NextPartyAnimSignal = new Signal();

		internal Signal DestinationReachedSignal = new Signal();

		private Signal GrabTreasureChestSignal = new Signal();

		internal Signal ChestReadyToOpen = new Signal();

		private List<Vector3> introPath;

		private List<Vector3> reverseIntroPath;

		private float defaultIntroTime;

		private string treasureController;

		private int idleStateHash;

		private int belloStateHash;

		private Dictionary<string, object> celebrateParams;

		private Dictionary<string, object> wavingParams;

		private bool showed;

		public override Signal<string, Type, object> AnimSignal
		{
			get
			{
				return animSignal;
			}
		}

		public override NamedCharacterObject Build(NamedCharacter character, GameObject parent, IKampaiLogger logger, IMinionBuilder minionBuilder)
		{
			idleStateHash = Animator.StringToHash("Base Layer.Idle");
			belloStateHash = Animator.StringToHash("Base Layer.Bello");
			celebrateParams = new Dictionary<string, object>();
			celebrateParams.Add("isCelebrating", true);
			wavingParams = new Dictionary<string, object>();
			wavingParams.Add("isWaving", true);
			TSMCharacter tSMCharacter = character as TSMCharacter;
			TSMCharacterDefinition definition = tSMCharacter.Definition;
			introPath = new List<Vector3>(definition.IntroPath);
			reverseIntroPath = new List<Vector3>(definition.IntroPath);
			reverseIntroPath.Reverse();
			defaultIntroTime = definition.IntroTime;
			treasureController = definition.TreasureController;
			defaultController = KampaiResources.Load<RuntimeAnimatorController>(definition.CharacterAnimations.StateMachine);
			base.Build(character, parent, logger, minionBuilder);
			base.gameObject.SetLayerRecursively(8);
			GrabTreasureChestSignal.AddListener(GrabTreasureChest);
			return this;
		}

		internal void ShowTSMCharacter()
		{
			AnimatePosition(true, defaultIntroTime, DestinationReachedSignal);
		}

		internal void HideTSMCharacter(TSMCharacterHideState hideState)
		{
			switch (hideState)
			{
			case TSMCharacterHideState.Celebrate:
			case TSMCharacterHideState.CelebrateAndReturn:
				EnqueueAction(new SetAnimatorAction(this, defaultController, logger));
				EnqueueAction(new SetAnimatorAction(this, null, logger, celebrateParams));
				EnqueueAction(new WaitForMecanimStateAction(this, idleStateHash, logger));
				break;
			case TSMCharacterHideState.Chest:
				TeleportHome();
				break;
			}
			AnimatePosition(false, defaultIntroTime, RemoveCharacterSignal);
		}

		internal void SayCheese(Action callback)
		{
			EnqueueAction(new SetAnimatorAction(this, null, logger, wavingParams));
			EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			EnqueueAction(new WaitForMecanimStateAction(this, belloStateHash, logger));
			EnqueueAction(new WaitForMecanimStateAction(this, idleStateHash, logger));
			EnqueueAction(new DelegateAction(callback, logger));
		}

		private void AnimatePosition(bool show, float animationDuration, Signal callback)
		{
			if (show != showed)
			{
				showed = show;
				List<Vector3> path;
				if (show)
				{
					path = introPath;
				}
				else
				{
					path = reverseIntroPath;
					EnableCollider(false);
				}
				EnqueueAction(new PathAction(this, path, animationDuration, logger));
				EnqueueAction(new SendSignalAction(callback, logger));
			}
		}

		public void PlayPartyAnimation(MinionAnimationDefinition def)
		{
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(def.StateMachine);
			EnqueueAction(new SetAnimatorAction(this, controller, logger, def.arguments), true);
			if (def.FaceCamera)
			{
				EnqueueAction(new RotateAction(this, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			}
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.Exit"), logger));
			EnqueueAction(new SendSignalAction(NextPartyAnimSignal, logger));
		}

		public void ChestIntroPostParty()
		{
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(treasureController);
			EnqueueAction(new SetAnimatorAction(this, controller, logger).SetInstant(), true);
			EnqueueAction(new PlayMecanimStateAction(this, Animator.StringToHash("Base Layer.captain_idle_02"), logger));
		}

		public void StartChestIntro()
		{
			if (actionQueue.Count > 0)
			{
				ClearActionQueue();
				GrabTreasureChest();
			}
			else
			{
				AnimatePosition(false, defaultIntroTime / 3f, GrabTreasureChestSignal);
			}
		}

		public void TeleportHome()
		{
			EnqueueAction(new SetAnimatorAction(this, defaultController, logger));
		}

		private void GrabTreasureChest()
		{
			showed = true;
			RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(treasureController);
			EnqueueAction(new SetAnimatorAction(this, controller, logger).SetInstant());
			EnqueueAction(new TeleportAction(this, introPath[introPath.Count - 1], new Vector3(0f, 130f, 0f), logger));
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.captain_idle_02"), logger));
			EnqueueAction(new SendSignalAction(ChestReadyToOpen, logger));
		}

		public void OpenChest(Action callback)
		{
			EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "open", null));
			EnqueueAction(new WaitForMecanimStateAction(this, Animator.StringToHash("Base Layer.end"), logger));
			EnqueueAction(new DelegateAction(callback, logger));
		}
	}
}
