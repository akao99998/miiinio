using Kampai.Game;
using Kampai.Main;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SocialPartyNoEventView : PopupMenuView
	{
		public Text TitleText;

		public Text NoEventDescriptionText;

		public Text UpcomingEventDescriptionText;

		public Text YesButtonText;

		public ButtonView YesButton;

		public Text remainingTime;

		private ITimedSocialEventService timedSocialEventService;

		private ILocalizationService localService;

		private ITimeService timeService;

		private int startTime;

		public override void Init()
		{
			base.Init();
			base.Open();
			TimedSocialEventDefinition nextSocialEvent = timedSocialEventService.GetNextSocialEvent();
			if (nextSocialEvent != null)
			{
				startTime = nextSocialEvent.StartTime;
				NoEventDescriptionText.gameObject.SetActive(false);
				UpcomingEventDescriptionText.text = localService.GetString("socialpartyupcomingeventdescription");
			}
			else
			{
				UpcomingEventDescriptionText.gameObject.SetActive(false);
				remainingTime.gameObject.SetActive(false);
				NoEventDescriptionText.text = localService.GetString("socialpartynoeventdescription");
			}
		}

		public void SetServices(ITimedSocialEventService timedSocialEventService, ITimeService timeService, ILocalizationService localService)
		{
			this.timedSocialEventService = timedSocialEventService;
			this.timeService = timeService;
			this.localService = localService;
		}

		public void Update()
		{
			int num = startTime - timeService.CurrentTime();
			if (num > 0)
			{
				remainingTime.text = UIUtils.FormatSocialTime(num);
			}
		}
	}
}
