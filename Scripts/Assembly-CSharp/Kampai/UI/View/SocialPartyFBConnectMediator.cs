using System;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class SocialPartyFBConnectMediator : UIStackMediator<SocialPartyFBConnectView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyFBConnectMediator") as IKampaiLogger;

		private Action<bool> returnAction;

		private bool loginSucceeded;

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public SocialLoginFailureSignal loginFailure { get; set; }

		[Inject]
		public SocialLoginSignal socialLoginSignal { get; set; }

		[Inject]
		public SocialLogoutSignal socialLogoutSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.connectButton.ClickedSignal.AddListener(ConnectButton);
			base.view.quitButton.ClickedSignal.AddListener(QuitButton);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
			loginSuccess.AddListener(OnFBLoginSuccess);
			loginFailure.AddListener(OnFBLoginFailure);
			base.view.connectButtonText.text = localService.GetString("socialpartyfbconnectbutton");
			base.view.txtHeadline.text = localService.GetString("socialpartyfbconnecttitle");
			base.view.txtDescription.text = localService.GetString("socialpartyfbconnectdetails");
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
			base.view.connectButton.ClickedSignal.RemoveListener(ConnectButton);
			base.view.quitButton.ClickedSignal.RemoveListener(QuitButton);
			loginSuccess.RemoveListener(OnFBLoginSuccess);
			loginFailure.RemoveListener(OnFBLoginFailure);
		}

		public override void Initialize(GUIArguments args)
		{
			returnAction = args.Get<Action<bool>>();
		}

		public void ConnectButton()
		{
			if (!facebookService.isLoggedIn)
			{
				socialLoginSignal.Dispatch(facebookService, new Boxed<Action>(null));
			}
			else
			{
				socialLogoutSignal.Dispatch(facebookService);
			}
		}

		public void QuitButton()
		{
			loginSucceeded = false;
			Close();
		}

		public void OnFBLoginSuccess(ISocialService socialService)
		{
			logger.Log(KampaiLogLevel.Info, "FB Login Success");
			loginSucceeded = true;
			Close();
		}

		public void OnFBLoginFailure(ISocialService socialService)
		{
			logger.Log(KampaiLogLevel.Info, "FB Login Failure");
		}

		protected override void Close()
		{
			globalSFX.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		public void CloseAnimationComplete()
		{
			hideSignal.Dispatch("StageSkrimFB");
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_FBConnect");
			if (returnAction != null)
			{
				returnAction(loginSucceeded);
			}
		}
	}
}
