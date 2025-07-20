using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class OrderBoardTicketTimerView : KampaiView
	{
		public DoubleConfirmButtonView RushButton;

		public Text GemCountText;

		public Text CountDownClockText;

		internal int rushCost;

		private bool inProgress;

		private int index;

		private int duration;

		private ILocalizationService localizationService;

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		internal void Init(ILocalizationService localization)
		{
			localizationService = localization;
		}

		internal void StartTimer(int index, int duration)
		{
			inProgress = true;
			this.index = -index;
			this.duration = duration;
			Update();
		}

		public void Update()
		{
			if (inProgress && duration >= 0 && timeEventService != null)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(index);
				switch (timeRemaining)
				{
				case 0:
					inProgress = false;
					break;
				case -1:
					inProgress = false;
					return;
				}
				CountDownClockText.text = UIUtils.FormatTime(timeRemaining, localizationService);
				rushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.COOLDOWN);
				GemCountText.text = rushCost.ToString();
			}
		}
	}
}
