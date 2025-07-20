using Kampai.Main;

namespace Kampai.UI.View
{
	public class SocialPartyEventCompletedMediator : UIStackMediator<SocialPartyEventCompletedView>
	{
		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			base.view.TitleText.text = localService.GetString("socialpartycompletedtitle");
			base.view.MessageText.text = localService.GetString("socialpartycompleteddescription");
			base.view.YesButtonText.text = localService.GetString("socialpartycompletedbutton");
			base.view.YesButton.ClickedSignal.AddListener(OkButtonPressed);
			base.view.OnMenuClose.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.YesButton.ClickedSignal.RemoveListener(OkButtonPressed);
			base.view.OnMenuClose.RemoveListener(Close);
		}

		public void OkButtonPressed()
		{
			base.view.Close();
		}

		protected override void Close()
		{
			hideSkrim.Dispatch("SocialCompleteSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_EventCompleted");
		}
	}
}
