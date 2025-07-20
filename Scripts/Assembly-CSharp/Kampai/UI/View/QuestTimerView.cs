using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestTimerView : KampaiView
	{
		public Image timerEmpty;

		public Image timerFill;

		public Text timerText;

		internal ITimeEventService timeEventService;

		internal IPlayerService playerService;

		internal ILocalizationService localizationService;

		private int questId;

		private void Update()
		{
			UpdateTimer();
		}

		internal void EnableQuestTimer(int questId)
		{
			this.questId = questId;
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questId);
			bool flag = false;
			if (byInstanceId != null)
			{
				TimedQuestDefinition timedQuestDefinition = byInstanceId.GetActiveDefinition() as TimedQuestDefinition;
				LimitedQuestDefinition limitedQuestDefinition = byInstanceId.GetActiveDefinition() as LimitedQuestDefinition;
				flag = timedQuestDefinition != null || limitedQuestDefinition != null;
			}
			SetEnabled(flag);
		}

		internal void DisableQuestTimer(int questId)
		{
			if (this.questId == questId)
			{
				SetEnabled(false);
			}
		}

		private void UpdateTimer()
		{
			int timeRemaining = timeEventService.GetTimeRemaining(questId);
			if (timeRemaining < 0)
			{
				SetEnabled(false);
			}
			int eventDuration = timeEventService.GetEventDuration(questId);
			timerText.text = UIUtils.FormatTime(timeRemaining, localizationService);
			timerFill.fillAmount = (float)timeRemaining / (float)eventDuration;
		}

		private void SetEnabled(bool isEnabled)
		{
			GetComponent<Image>().enabled = isEnabled;
			timerEmpty.enabled = isEnabled;
			timerFill.enabled = isEnabled;
			timerText.enabled = isEnabled;
		}
	}
}
