using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestPanelProgressBarView : KampaiView
	{
		public Text TimeRemainingText;

		private int endTime;

		private ITimeService timeService;

		private int timeRemaining;

		private ILocalizationService localizationService;

		internal void Init(int UTCEndTime, ITimeService timeService, ILocalizationService localizationService)
		{
			endTime = UTCEndTime;
			this.timeService = timeService;
			this.localizationService = localizationService;
		}

		public void Update()
		{
			if (timeService != null)
			{
				UpdateTime(timeService.CurrentTime());
			}
		}

		internal void UpdateTime(int currentTime)
		{
			timeRemaining = endTime - currentTime;
			TimeRemainingText.text = UIUtils.FormatTime(timeRemaining, localizationService);
		}
	}
}
