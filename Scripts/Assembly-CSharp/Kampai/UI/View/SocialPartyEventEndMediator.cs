using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SocialPartyEventEndMediator : UIStackMediator<SocialPartyEventEndView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyEventEndMediator") as IKampaiLogger;

		private bool eventComplete;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ShowSocialPartyRewardSignal socialPartyRewardSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init();
			eventComplete = false;
			base.view.TitleText.text = localService.GetString("socialpartyendtitle");
			base.view.MessageText.text = localService.GetString("socialpartyenddescription");
			base.view.YesButtonText.text = localService.GetString("socialpartyendbutton");
			base.view.YesButton.ClickedSignal.AddListener(OkButtonPressed);
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(OnGetSocialEventStateResponse);
			base.view.OnMenuClose.AddListener(CloseAnimationComplete);
			timedSocialEventService.GetSocialEventState(timedSocialEventService.GetCurrentSocialEvent().ID, signal);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.YesButton.ClickedSignal.RemoveListener(OkButtonPressed);
			base.view.OnMenuClose.RemoveListener(CloseAnimationComplete);
		}

		protected override void Close()
		{
			OkButtonPressed();
		}

		public void OkButtonPressed()
		{
			base.view.Close();
		}

		public void OnGetSocialEventStateResponse(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null && response.UserEvent != null && !response.UserEvent.RewardClaimed && response.Team != null && response.Team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
			{
				eventComplete = true;
			}
			else
			{
				logger.Error("OnGetSocialEventStateResponse unexpected result");
			}
		}

		public void CloseAnimationComplete()
		{
			guiService.Execute(GUIOperation.Unload, "popup_SocialParty_End");
			if (eventComplete)
			{
				socialPartyRewardSignal.Dispatch(timedSocialEventService.GetCurrentSocialEvent().ID);
			}
		}
	}
}
