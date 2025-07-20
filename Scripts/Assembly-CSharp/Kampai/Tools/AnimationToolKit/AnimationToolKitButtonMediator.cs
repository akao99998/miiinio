using strange.extensions.mediation.impl;

namespace Kampai.Tools.AnimationToolKit
{
	internal sealed class AnimationToolKitButtonMediator : Mediator
	{
		[Inject]
		public AnimationToolKitButtonView view { get; set; }

		[Inject]
		public AnimationToolkitModel Model { get; set; }

		[Inject]
		public AnimationToolKit AnimationToolKit { get; set; }

		[Inject]
		public LoadInterfaceSignal loadInterfaceSignal { get; set; }

		[Inject]
		public ToggleInterfaceSignal toggleInterfaceSignal { get; set; }

		[Inject]
		public ToggleMeshSignal toggleMeshSignal { get; set; }

		public override void OnRegister()
		{
			view.ButtonPressSignal.AddListener(OnButtonPress);
		}

		public override void OnRemove()
		{
			view.ButtonPressSignal.RemoveListener(OnButtonPress);
		}

		private void OnButtonPress(AnimationToolKitButtonType buttonType)
		{
			switch (buttonType)
			{
			case AnimationToolKitButtonType.MinionMode:
				Model.Mode = AnimationToolKitMode.Minion;
				loadInterfaceSignal.Dispatch();
				break;
			case AnimationToolKitButtonType.VillainMode:
				Model.Mode = AnimationToolKitMode.Villain;
				loadInterfaceSignal.Dispatch();
				break;
			case AnimationToolKitButtonType.CharacterMode:
				Model.Mode = AnimationToolKitMode.Character;
				loadInterfaceSignal.Dispatch();
				break;
			case AnimationToolKitButtonType.AddMinion:
				AnimationToolKit.AddMinion();
				break;
			case AnimationToolKitButtonType.RemoveMinion:
				AnimationToolKit.RemoveMinion();
				break;
			case AnimationToolKitButtonType.AddVillain:
				AnimationToolKit.AddVillain();
				break;
			case AnimationToolKitButtonType.RemoveVillain:
				AnimationToolKit.RemoveVillain();
				break;
			case AnimationToolKitButtonType.AddCharacter:
				AnimationToolKit.AddCharacter();
				break;
			case AnimationToolKitButtonType.RemoveCharacter:
				AnimationToolKit.RemoveCharacter();
				break;
			case AnimationToolKitButtonType.LoopAnimation:
				AnimationToolKit.LoopAnimation();
				break;
			case AnimationToolKitButtonType.GagAnimation:
				AnimationToolKit.GagAnimation();
				break;
			case AnimationToolKitButtonType.WaitAnimation:
				AnimationToolKit.WaitAnimation();
				break;
			case AnimationToolKitButtonType.VillainIntroAnimation:
				AnimationToolKit.VillainIntroAnimation();
				break;
			case AnimationToolKitButtonType.VillainCabanaAnimation:
				AnimationToolKit.VillainCabanaAnimation();
				break;
			case AnimationToolKitButtonType.VillainFarewellAnimation:
				AnimationToolKit.VillainFarewellAnimation();
				break;
			case AnimationToolKitButtonType.StuartStageIdleAnimation:
				AnimationToolKit.StuartStageIdleAnimation();
				break;
			case AnimationToolKitButtonType.StuartPerformAnimation:
				AnimationToolKit.StuartPerformAnimation();
				break;
			case AnimationToolKitButtonType.StuartCelebrateAnimation:
				AnimationToolKit.StuartCelebrateAnimation();
				break;
			case AnimationToolKitButtonType.StuartAttentionAnimation:
				AnimationToolKit.StuartAttentionAnimation();
				break;
			case AnimationToolKitButtonType.TikiBarCelebrateAnimation:
				AnimationToolKit.TikiBarCelebrateAnimation();
				break;
			case AnimationToolKitButtonType.TikiBarAttentionAnimation:
				AnimationToolKit.TikiBarAttentionAnimation();
				break;
			case AnimationToolKitButtonType.TikiBarSpyGlassAnimation:
				AnimationToolKit.TikiBarAlternateAnimation(1);
				break;
			case AnimationToolKitButtonType.TikiBarMixDrinkAnimation:
				AnimationToolKit.TikiBarAlternateAnimation(0);
				break;
			case AnimationToolKitButtonType.ToggleMesh:
				toggleMeshSignal.Dispatch();
				break;
			case AnimationToolKitButtonType.ToggleInterface:
				toggleInterfaceSignal.Dispatch();
				break;
			}
		}
	}
}
