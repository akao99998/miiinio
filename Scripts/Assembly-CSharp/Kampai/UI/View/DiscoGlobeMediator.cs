using Kampai.Game;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class DiscoGlobeMediator : Mediator
	{
		private MinionParty minionParty;

		private StartMinionPartySignal startMinionPartySignal;

		private PostStartPartyBuffTimerSignal postStartPartyBuffTimerSignal;

		private bool partyIsHappening;

		private bool partyWillProduceBuff;

		[Inject]
		public DiscoGlobeView view { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public DisplayCameraControlsSignal displayCameraControlsSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHudSignal { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public DisplayDisco3DElements displayDisco3DElements { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void OnRegister()
		{
			minionParty = playerService.GetMinionPartyInstance();
			displayCameraControlsSignal.AddListener(DisplayPartyControls);
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			startMinionPartySignal = injectionBinder.GetInstance<StartMinionPartySignal>();
			startMinionPartySignal.AddListener(OnPartyStarted);
			postStartPartyBuffTimerSignal = injectionBinder.GetInstance<PostStartPartyBuffTimerSignal>();
			postStartPartyBuffTimerSignal.AddListener(OnBuffStarted);
			displayDisco3DElements.AddListener(DisplayDisco3DElements);
			showHudSignal.AddListener(OnShowHUDSignal);
			IGuestOfHonorService instance = gameContext.injectionBinder.GetInstance<IGuestOfHonorService>();
			partyWillProduceBuff = instance.PartyShouldProduceBuff();
		}

		public void DisplayDiscoGlobe()
		{
			positionService.AddHUDElementToAvoid(view.DiscoGlobeMesh, true);
			if (minionParty.IsBuffHappening && !minionParty.IsPartyHappening)
			{
				OnBuffStarted();
			}
			else
			{
				DisplayEffects(minionParty.IsPartyHappening);
			}
			view.ShowDiscoBallAwesomeness();
		}

		public override void OnRemove()
		{
			startMinionPartySignal.RemoveListener(OnPartyStarted);
			postStartPartyBuffTimerSignal.RemoveListener(OnBuffStarted);
			displayDisco3DElements.RemoveListener(DisplayDisco3DElements);
			startMinionPartySignal = null;
			postStartPartyBuffTimerSignal = null;
			positionService.RemoveHUDElementToAvoid(view.DiscoGlobeMesh);
			hideItemPopupSignal.Dispatch();
			showHudSignal.RemoveListener(OnShowHUDSignal);
			displayCameraControlsSignal.RemoveListener(DisplayPartyControls);
		}

		private void OnPartyStarted()
		{
			DisplayEffects(true);
			partyIsHappening = true;
		}

		private void OnBuffStarted()
		{
			DisplayEffects(false);
			partyIsHappening = false;
		}

		private void DisplayEffects(bool display)
		{
			if (!display)
			{
				DisplayPartyControls(display);
			}
			view.DisplayEffects(display);
		}

		private void DisplayPartyControls(bool isEnabled)
		{
			if (!isEnabled || partyIsHappening)
			{
				view.ShowCameraControlsPanel(isEnabled && partyWillProduceBuff && minionParty.IsPartyHappening);
			}
		}

		private void DisplayDisco3DElements(bool display)
		{
			view.DisplayDisco3DElements(display);
		}

		private void OnShowHUDSignal(bool display)
		{
			if (!partyIsHappening)
			{
				if (display && villainLairModel.currentActiveLair == null)
				{
					positionService.AddHUDElementToAvoid(view.DiscoGlobeMesh, true);
					view.ShowDiscoBallAwesomeness();
				}
				else
				{
					view.RemoveDiscoBallAwesomeness(null);
				}
			}
		}
	}
}
