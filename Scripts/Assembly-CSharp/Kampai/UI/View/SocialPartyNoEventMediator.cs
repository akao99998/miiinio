using Kampai.Game;
using Kampai.Main;

namespace Kampai.UI.View
{
	public class SocialPartyNoEventMediator : UIStackMediator<SocialPartyNoEventView>
	{
		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.SetServices(timedSocialEventService, timeService, localService);
			base.view.Init();
			base.view.TitleText.text = localService.GetString("socialpartynoeventtitle");
			base.view.YesButtonText.text = localService.GetString("socialpartynoeventbutton");
			base.view.YesButton.ClickedSignal.AddListener(OkButtonPressed);
			base.view.OnMenuClose.AddListener(OnClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.YesButton.ClickedSignal.RemoveListener(OkButtonPressed);
			base.view.OnMenuClose.RemoveListener(OnClose);
		}

		protected override void Close()
		{
			OkButtonPressed();
		}

		public void OkButtonPressed()
		{
			base.view.Close();
			OnClose();
		}

		public void OnClose()
		{
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_NoEvent");
			hideSkrimSignal.Dispatch("SocialSkrim");
		}
	}
}
