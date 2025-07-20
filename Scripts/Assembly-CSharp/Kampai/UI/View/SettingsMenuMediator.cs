using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SettingsMenuMediator : UIStackMediator<SettingsMenuView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SettingsMenuMediator") as IKampaiLogger;

		private SettingsMenuPanel? currentPanel;

		private bool isTempHidden;

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService googleService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public ShowSocialPartyFBConnectSignal showFacebookPopupSignal { get; set; }

		[Inject]
		public SocialLoginSignal socialLoginSignal { get; set; }

		[Inject]
		public SocialLogoutSignal socialLogoutSignal { get; set; }

		[Inject]
		public UpdateFacebookStateSignal facebookStateSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public SaveDevicePrefsSignal saveSignal { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public SocialLoginFailureSignal loginFailure { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public OpenRateAppPageSignal openRateAppPageSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public TogglePopupForHUDSignal togglePopupSignal { get; set; }

		[Inject]
		public IQuestScriptService questService { get; set; }

		[Inject]
		public IVideoService videoService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public TempHideSettingsMenuSignal tempHidden { get; set; }

		[Inject]
		public DisplayDebugButtonSignal displayDebugButtonSignal { get; set; }

		[Inject]
		public IAchievementService achievementService { get; set; }

		[Inject]
		public DisplayDisco3DElements displayDisco3DElements { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.facebookButton.ClickedSignal.AddListener(FacebookButton);
			base.view.googleButton.ClickedSignal.AddListener(GoogleButton);
			base.view.rateAppButton.ClickedSignal.AddListener(RateAppButton);
			base.view.closeButton.ClickedSignal.AddListener(CloseButton);
			base.view.achievementButton.ClickedSignal.AddListener(AchievementButton);
			facebookStateSignal.AddListener(setFacebookStatus);
			setFacebookStatus(facebookService.isLoggedIn);
			closeSignal.AddListener(Close);
			loginSuccess.AddListener(LoginSuccess);
			loginFailure.AddListener(LoginFailure);
			base.view.settings.ClickedSignal.AddListener(ShowSettings);
			base.view.about.ClickedSignal.AddListener(ShowAbout);
			base.view.help.ClickedSignal.AddListener(ShowHelp);
			base.view.playMovieButton.ClickedSignal.AddListener(PlayMovieButton);
			tempHidden.AddListener(TempHide);
			init();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.facebookButton.ClickedSignal.RemoveListener(FacebookButton);
			base.view.rateAppButton.ClickedSignal.RemoveListener(RateAppButton);
			base.view.googleButton.ClickedSignal.RemoveListener(GoogleButton);
			base.view.closeButton.ClickedSignal.RemoveListener(CloseButton);
			base.view.achievementButton.ClickedSignal.RemoveListener(AchievementButton);
			closeSignal.RemoveListener(Close);
			loginSuccess.RemoveListener(LoginSuccess);
			loginFailure.RemoveListener(LoginFailure);
			base.view.settings.ClickedSignal.RemoveListener(ShowSettings);
			base.view.about.ClickedSignal.RemoveListener(ShowAbout);
			base.view.help.ClickedSignal.RemoveListener(ShowHelp);
			facebookStateSignal.RemoveListener(setFacebookStatus);
			base.view.playMovieButton.ClickedSignal.RemoveListener(PlayMovieButton);
			tempHidden.RemoveListener(TempHide);
		}

		private void init()
		{
			base.view.RateUsText.text = localService.GetString("RateUsMenu");
			base.view.playMovieText.text = localService.GetString("PlayMovie");
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (base.view != null)
			{
				Start();
			}
		}

		private void Start()
		{
			togglePopupSignal.Dispatch(true);
			logger.Info("facebook killswitch : {0}", facebookService.isKillSwitchEnabled);
			logger.Info("google+ killswitch : {0}", googleService.isKillSwitchEnabled);
			base.view.facebookButton.gameObject.SetActive(!coppaService.Restricted() && !facebookService.isKillSwitchEnabled);
			SetupButtons();
			showStoreSignal.Dispatch(false);
			SettingsMenuPanel? settingsMenuPanel = currentPanel;
			if (!settingsMenuPanel.HasValue || !isTempHidden)
			{
				ShowSettings();
			}
			isTempHidden = false;
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				questService.PauseQuestScripts();
			}
			ScreenUtils.ToggleAutoRotation(true);
		}

		private void SetupButtons()
		{
			UpdateLoginButtonText();
			base.view.googleButton.gameObject.SetActive(!coppaService.Restricted() && !googleService.isKillSwitchEnabled);
			base.view.achievementButton.gameObject.SetActive(!coppaService.Restricted() && !googleService.isKillSwitchEnabled);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			showStoreSignal.Dispatch(true);
		}

		private void TempHide()
		{
			isTempHidden = true;
		}

		private void CloseButton()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			Close();
		}

		protected override void Close()
		{
			if (base.view.gameObject.activeInHierarchy)
			{
				base.view.gameObject.SetActive(false);
				saveSignal.Dispatch();
				togglePopupSignal.Dispatch(false);
				if (playerService.GetHighestFtueCompleted() < 9 && !isTempHidden)
				{
					questService.ResumeQuestScripts();
				}
				model.ForceDisabled = false;
				displayDisco3DElements.Dispatch(true);
			}
			displayDebugButtonSignal.Dispatch(false);
		}

		private void AchievementButton()
		{
			ISocialService socialService = null;
			socialService = googleService;
			if (!ShouldLogin(socialService))
			{
				achievementService.ShowAchievements();
			}
		}

		private bool ShouldLogin(ISocialService socialService)
		{
			if (socialService != null && !socialService.isLoggedIn)
			{
				socialLoginSignal.Dispatch(socialService, new Boxed<Action>(AchievementButton));
				return true;
			}
			return false;
		}

		private void Close(GameObject ignore)
		{
			Close();
		}

		private void LoginSuccess(ISocialService socialService)
		{
			switch (socialService.type)
			{
			case SocialServices.FACEBOOK:
				popupMessageSignal.Dispatch(localService.GetString("fbLoginSuccess"), PopupMessageType.NORMAL);
				break;
			case SocialServices.GOOGLEPLAY:
				popupMessageSignal.Dispatch(localService.GetString("googleplayloginsuccess"), PopupMessageType.NORMAL);
				break;
			}
			SetupButtons();
		}

		private void LoginFailure(ISocialService socialService)
		{
			switch (socialService.type)
			{
			case SocialServices.FACEBOOK:
				popupMessageSignal.Dispatch(localService.GetString("fbLoginFailure"), PopupMessageType.NORMAL);
				break;
			case SocialServices.GOOGLEPLAY:
				popupMessageSignal.Dispatch(localService.GetString("GooglePlayLoginFailure"), PopupMessageType.NORMAL);
				break;
			}
			SetupButtons();
		}

		private void UpdateLoginButtonText()
		{
			base.view.facebookButtonText.text = localService.GetString((!facebookService.isLoggedIn) ? "facebooklogin" : "facebooklogout");
			base.view.googleButtonText.text = localService.GetString((coppaService.Restricted() || !googleService.isLoggedIn) ? "googleplaylogin" : "googleplaylogout");
		}

		private void FacebookButton()
		{
			Close();
			SocialButton(facebookService, base.view.facebookButtonText, "facebooklogin");
		}

		private void GoogleButton()
		{
			SocialButton(googleService, base.view.googleButtonText, "googleplaylogin");
		}

		private void SocialButton(ISocialService service, Text buttonTextView, string loggedInKey)
		{
			if (service.isLoggedIn)
			{
				socialLogoutSignal.Dispatch(service);
				buttonTextView.text = localService.GetString(loggedInKey);
			}
			else if (service.type == SocialServices.FACEBOOK)
			{
				facebookService.LoginSource = "Settings";
				showFacebookPopupSignal.Dispatch(delegate
				{
				});
			}
			else
			{
				socialLoginSignal.Dispatch(service, new Boxed<Action>(null));
			}
		}

		private void PlayMovieButton()
		{
			videoService.playIntro(false, true);
		}

		private void RateAppButton()
		{
			Close();
			openRateAppPageSignal.Dispatch();
		}

		private void setFacebookStatus(bool loggedOn)
		{
			base.view.facebookButtonText.text = localService.GetString((!loggedOn) ? "facebooklogin" : "facebooklogout");
		}

		private void ShowSettings()
		{
			ShowPanel(SettingsMenuPanel.SETTINGS);
		}

		private void ShowAbout()
		{
			ShowPanel(SettingsMenuPanel.ABOUT);
		}

		public void ShowHelp()
		{
			ShowPanel(SettingsMenuPanel.HELP);
		}

		private void ShowPanel(SettingsMenuPanel panel)
		{
			if (currentPanel != panel)
			{
				bool flag = panel == SettingsMenuPanel.SETTINGS;
				base.view.settingsPanel.SetActive(flag);
				base.view.settingClicked.SetActive(flag);
				bool active = panel == SettingsMenuPanel.ABOUT;
				base.view.aboutPanel.SetActive(active);
				base.view.aboutClicked.SetActive(active);
				bool active2 = panel == SettingsMenuPanel.HELP;
				base.view.helpPanel.SetActive(active2);
				base.view.helpClicked.SetActive(active2);
				displayDebugButtonSignal.Dispatch(flag);
				currentPanel = panel;
			}
		}
	}
}
