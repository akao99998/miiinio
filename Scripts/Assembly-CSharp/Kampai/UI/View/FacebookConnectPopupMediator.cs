using System;
using Kampai.Game;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class FacebookConnectPopupMediator : UIStackMediator<FacebookConnectPopupView>
	{
		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public SocialLoginSignal socialLoginSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.ConnectButton.ClickedSignal.AddListener(ConnectButtonPressed);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.ConnectButton.ClickedSignal.RemoveListener(ConnectButtonPressed);
		}

		public void ConnectButtonPressed()
		{
			if (!facebookService.isLoggedIn)
			{
				socialLoginSignal.Dispatch(facebookService, new Boxed<Action>(null));
			}
			Close();
		}

		protected override void Close()
		{
			hideSkrim.Dispatch("StageSkrimFB");
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_FBConnect");
		}
	}
}
