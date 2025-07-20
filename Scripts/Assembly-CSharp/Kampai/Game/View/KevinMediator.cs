using UnityEngine;
using strange.extensions.injector.api;

namespace Kampai.Game.View
{
	public class KevinMediator : FrolicCharacterMediator
	{
		[Inject]
		public KevinView kevinView { get; set; }

		[Inject]
		public KevinGreetVillainSignal greetVillainSignal { get; set; }

		[Inject]
		public AnimateKevinSignal animateKevinSignal { get; set; }

		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		[Inject]
		public ReleaseMinionFromTikiBarSignal releaseMinionFromTikiBarSignal { get; set; }

		[Inject]
		public SetKevinAnimatorCullingModeSignal setKevinAnimatorCullingModeSignal { get; set; }

		[Inject]
		public SetVillainLairAnimationTriggerSignal setAnimTriggerSignal { get; set; }

		[Inject]
		public KevinGoToWelcomeHutSignal gotoWelcomeHutSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			greetVillainSignal.AddListener(HandleGreeting);
			animateKevinSignal.AddListener(AnimateKevin);
			setKevinAnimatorCullingModeSignal.AddListener(kevinView.SetAnimatorCullingMode);
			setAnimTriggerSignal.AddListener(kevinView.SetAnimTrigger);
			gotoWelcomeHutSignal.AddListener(GotoWelcomeHut);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			greetVillainSignal.RemoveListener(HandleGreeting);
			animateKevinSignal.RemoveListener(AnimateKevin);
			setKevinAnimatorCullingModeSignal.RemoveListener(kevinView.SetAnimatorCullingMode);
			setAnimTriggerSignal.RemoveListener(kevinView.SetAnimTrigger);
			gotoWelcomeHutSignal.RemoveListener(GotoWelcomeHut);
		}

		private void AnimateKevin(string animation)
		{
			switch (animation)
			{
			case "walk":
				kevinView.Walk(true);
				break;
			case "idle":
				kevinView.Walk(false);
				break;
			}
		}

		private void HandleGreeting(bool shouldGreet)
		{
			kevinView.GreetVillain(shouldGreet);
			releaseMinionFromTikiBarSignal.Dispatch(base.playerService.GetByInstanceId<Character>(base.view.ID), true);
		}

		private void GotoWelcomeHut(bool pop)
		{
			kevinView.GotoWelcomeHut(injectionBinder.GetInstance<GameObject>(StaticItem.WELCOME_BOOTH_BUILDING_ID_DEF), pop);
		}
	}
}
