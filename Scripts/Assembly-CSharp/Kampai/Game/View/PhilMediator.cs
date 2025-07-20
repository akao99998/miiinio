using System;
using System.Collections;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class PhilMediator : Mediator
	{
		private const int cameraMask = 7;

		private bool cameraDisabled;

		private DisplayCameraControlsSignal displayCameraControlsSignal;

		[Inject]
		public PhilView view { get; set; }

		[Inject]
		public PhilCelebrateSignal celebrateSignal { get; set; }

		[Inject]
		public PhilGetAttentionSignal getAttentionSignal { get; set; }

		[Inject]
		public PhilBeginIntroLoopSignal beginIntroLoopSignal { get; set; }

		[Inject]
		public PhilPlayIntroSignal playIntroSignal { get; set; }

		[Inject]
		public PhilSignFixedSignal philSignFixedSignal { get; set; }

		[Inject]
		public PhilSitAtBarSignal sitAtBarSignal { get; set; }

		[Inject]
		public PhilActivateSignal activateSignal { get; set; }

		[Inject]
		public AnimatePhilSignal animatePhilSignal { get; set; }

		[Inject]
		public PhilGoToTikiBarSignal philGoToTikiBarSignal { get; set; }

		[Inject]
		public PhilEnableTikiBarControllerSignal enableTikiBarControllerSignal { get; set; }

		[Inject]
		public PhilPlayConfettiSignal philPlayConfettiSignal { get; set; }

		[Inject]
		public TeleportCharacterToTikiBarSignal teleportCharacterToTikiBarSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		[Inject]
		public TikiBarSetAnimParamSignal tikiBarSetAnimParamSignal { get; set; }

		[Inject]
		public TikiBarResetAnimParamSignal tikiBarResetAnimParamSignal { get; set; }

		[Inject]
		public PhilGoToStartLocationSignal goToStartLocationSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraBehaviourSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraBehaviourSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal cameraAutoMoveToBuildingSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public PostMinionPartyEndSignal postMinionPartyEndSignal { get; set; }

		[Inject]
		public TriggerPhilPartyStartSignal triggerPartyStartSignal { get; set; }

		[Inject]
		public StartMinionPartySignal startMinionPartySignal { get; set; }

		[Inject]
		public LoadPartyAssetsSignal loadPartyAssetsSignal { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject]
		public CreateDoobersFromGiftBoxSignal createDoobersSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public EndMinionPartySignal endMinionPartySignal { get; set; }

		public override void OnRegister()
		{
			celebrateSignal.AddListener(Celebrate);
			getAttentionSignal.AddListener(GetAttention);
			beginIntroLoopSignal.AddListener(BeginIntroLoop);
			playIntroSignal.AddListener(PlayIntro);
			sitAtBarSignal.AddListener(SitAtBar);
			activateSignal.AddListener(Activate);
			animatePhilSignal.AddListener(AnimatePhil);
			enableTikiBarControllerSignal.AddListener(EnableTikiBarController);
			philGoToTikiBarSignal.AddListener(GotoTikiBar);
			philSignFixedSignal.AddListener(SignFixed);
			philPlayConfettiSignal.AddListener(PlayConFetti);
			view.AnimSignal.AddListener(SendTikiBarSignal);
			view.OnAnimatorParamatersResetSignal.AddListener(ResetTikiBarAnimParams);
			view.partyIntroCompleteSignal.AddListener(PartyIntroFinished);
			goToStartLocationSignal.AddListener(GoToStartLocation);
			endMinionPartySignal.AddListener(SkipParty);
			postMinionPartyEndSignal.AddListener(StopSpinFireStick);
			triggerPartyStartSignal.AddListener(TriggerPartyStart);
			displayCameraControlsSignal = uiContext.injectionBinder.GetInstance<DisplayCameraControlsSignal>();
			displayCameraControlsSignal.AddListener(StartSpinFireStick);
		}

		public override void OnRemove()
		{
			view.AnimSignal.RemoveListener(SendTikiBarSignal);
			view.OnAnimatorParamatersResetSignal.RemoveListener(ResetTikiBarAnimParams);
			view.partyIntroCompleteSignal.RemoveListener(PartyIntroFinished);
			celebrateSignal.RemoveListener(Celebrate);
			getAttentionSignal.RemoveListener(GetAttention);
			beginIntroLoopSignal.RemoveListener(BeginIntroLoop);
			playIntroSignal.RemoveListener(PlayIntro);
			sitAtBarSignal.RemoveListener(SitAtBar);
			activateSignal.RemoveListener(Activate);
			animatePhilSignal.RemoveListener(AnimatePhil);
			enableTikiBarControllerSignal.RemoveListener(EnableTikiBarController);
			philGoToTikiBarSignal.RemoveListener(GotoTikiBar);
			philSignFixedSignal.RemoveListener(SignFixed);
			goToStartLocationSignal.RemoveListener(GoToStartLocation);
			postMinionPartyEndSignal.RemoveListener(StopSpinFireStick);
			triggerPartyStartSignal.RemoveListener(TriggerPartyStart);
			philPlayConfettiSignal.RemoveListener(PlayConFetti);
			endMinionPartySignal.RemoveListener(SkipParty);
			if (displayCameraControlsSignal != null)
			{
				displayCameraControlsSignal.RemoveListener(StartSpinFireStick);
			}
		}

		private void GoToStartLocation()
		{
			view.GoToStartLocation();
		}

		private void SignFixed()
		{
			view.SignFixed();
		}

		private void Activate(bool activate)
		{
			view.Activate(activate);
		}

		private void SitAtBar(bool sit)
		{
			view.SitAtBar(sit, teleportCharacterToTikiBarSignal);
		}

		private void Celebrate()
		{
			view.Celebrate();
		}

		private void PlayConFetti()
		{
			view.PlayConFetti();
		}

		private void TriggerPartyStart()
		{
			StopSpinFireStick();
			view.RemoveProp("Prop_FireStick_Prefab");
			view.StartParty();
		}

		private void PartyIntroFinished()
		{
			loadPartyAssetsSignal.Dispatch();
			if (!playerService.GetMinionPartyInstance().PartyPreSkip)
			{
				createDoobersSignal.Dispatch();
				customCameraPositionSignal.Dispatch(60001, new Boxed<Action>(StartTheParty));
			}
		}

		private void StartTheParty()
		{
			startMinionPartySignal.Dispatch();
			view.RemoveProp("Prop_PartyBox_Prefab");
		}

		private void SkipParty(bool isSkipping)
		{
			if (isSkipping)
			{
				view.PartySkip();
			}
		}

		private void StartSpinFireStick(bool display)
		{
			if (display)
			{
				view.SpinFireStick(display);
			}
		}

		private void StopSpinFireStick()
		{
			view.SpinFireStick(false);
		}

		private void GetAttention(bool enable)
		{
			view.GetAttention(enable);
		}

		private void BeginIntroLoop(bool showTikiBar)
		{
			view.RemoveProp("Prop_PartyBox_Prefab");
			view.RemoveProp("Prop_FireStick_Prefab");
			view.BeginIntroLoop();
			if (showTikiBar)
			{
				disableCamera();
				pickControllerModel.ForceDisabled = true;
				uiContext.injectionBinder.GetInstance<SetBuildMenuEnabledSignal>().Dispatch(false);
			}
		}

		private void PlayIntro(bool showTikiBar)
		{
			view.PlayIntro();
			if (showTikiBar)
			{
				StartCoroutine(MoveToTikiBar());
			}
		}

		private IEnumerator MoveToTikiBar()
		{
			yield return new WaitForSeconds(1f);
			TikiBarBuilding tikibarBuilding = playerService.GetByInstanceId<TikiBarBuilding>(313);
			cameraAutoMoveToBuildingSignal.Dispatch(tikibarBuilding, new PanInstructions(tikibarBuilding));
		}

		private void disableCamera()
		{
			cameraDisabled = true;
			disableCameraBehaviourSignal.Dispatch(7);
		}

		private void enableCamera()
		{
			cameraDisabled = false;
			enableCameraBehaviourSignal.Dispatch(7);
		}

		private void AnimatePhil(string animation)
		{
			if (animation.Equals("idle"))
			{
				view.StopWalking();
			}
			view.Animate(animation);
		}

		private void GotoTikiBar(bool pop)
		{
			view.GotoTikiBar(injectionBinder.GetInstance<GameObject>(StaticItem.TIKI_BAR_BUILDING_ID_DEF), teleportCharacterToTikiBarSignal);
		}

		private void EnableTikiBarController()
		{
			view.EnableTikiBarController();
		}

		private void ResetTikiBarAnimParams()
		{
			bool animBool = view.GetAnimBool("PartySkip");
			tikiBarResetAnimParamSignal.Dispatch(animBool);
		}

		private void SendTikiBarSignal(string animation, Type type, object obj)
		{
			tikiBarSetAnimParamSignal.Dispatch(animation, type, obj);
			if (animation.Equals("NewMinionSequence") && cameraDisabled)
			{
				enableCamera();
				pickControllerModel.ForceDisabled = false;
				uiContext.injectionBinder.GetInstance<SetBuildMenuEnabledSignal>().Dispatch(true);
			}
		}
	}
}
