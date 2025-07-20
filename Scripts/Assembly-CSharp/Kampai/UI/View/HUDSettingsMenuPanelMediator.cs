using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class HUDSettingsMenuPanelMediator : Mediator
	{
		private GameObject settingsMenuPanelGO;

		[Inject]
		public HUDSettingsMenuPanelView view { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UpdateFacebookStateSignal updateFacebookDialogState { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllMenuSignal { get; set; }

		[Inject]
		public StopAutopanSignal stopAutopanSignal { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public DisplaySettingsMenuSignal displaySettingsMenuSignal { get; set; }

		[Inject]
		public DisplayDebugButtonSignal displayDebugButtonSignal { get; set; }

		[Inject]
		public DisplayDisco3DElements displayDisco3DElements { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		[Inject]
		public TempHideSettingsMenuSignal tempHideSettingsMenuSignal { get; set; }

		public override void OnRegister()
		{
			settingsMenuPanelGO = Object.Instantiate(KampaiResources.Load<GameObject>("screen_HUD_Panel_Settings_Menu"));
			settingsMenuPanelGO.transform.SetParent(base.transform, false);
			settingsMenuPanelGO.SetActive(false);
			if (GameConstants.StaticConfig.DEBUG_ENABLED)
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>("DebugConsoleButton"));
				gameObject.transform.SetParent(base.transform, false);
			}
			displaySettingsMenuSignal.AddListener(Display);
			view.ButtonToListenTo.ClickedSignal.AddListener(ButtonClicked);
			networkConnectionLostSignal.AddListener(OnNetworkLost);
		}

		public override void OnRemove()
		{
			view.ButtonToListenTo.ClickedSignal.RemoveListener(ButtonClicked);
			displaySettingsMenuSignal.RemoveListener(Display);
			networkConnectionLostSignal.RemoveListener(OnNetworkLost);
		}

		private void Display(bool display)
		{
			displayDisco3DElements.Dispatch(!display);
			if (!display && settingsMenuPanelGO.activeInHierarchy)
			{
				closeAllMenuSignal.Dispatch(null);
				displayDebugButtonSignal.Dispatch(false);
				return;
			}
			model.ForceDisabled = true;
			stopAutopanSignal.Dispatch();
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			updateFacebookDialogState.Dispatch(facebookService.isLoggedIn);
			closeAllMenuSignal.Dispatch(settingsMenuPanelGO);
			settingsMenuPanelGO.SetActive(true);
		}

		private void ButtonClicked()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			if (minionPartyInstance == null || (!minionPartyInstance.CharacterUnlocking && !minionPartyInstance.IsPartyHappening))
			{
				Display(true);
				displayDebugButtonSignal.Dispatch(true);
			}
		}

		private void OnNetworkLost()
		{
			if (settingsMenuPanelGO.activeInHierarchy)
			{
				tempHideSettingsMenuSignal.Dispatch();
				Display(false);
				resumeNetworkOperationSignal.AddOnce(delegate
				{
					Display(true);
				});
			}
		}
	}
}
