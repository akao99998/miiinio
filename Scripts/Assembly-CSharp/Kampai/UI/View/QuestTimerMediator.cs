using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class QuestTimerMediator : Mediator
	{
		[Inject]
		public QuestTimerView view { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public QuestDetailIDSignal questIdSignal { get; set; }

		[Inject]
		public QuestTimeoutSignal timeoutSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void OnRegister()
		{
			view.timeEventService = timeEventService;
			view.playerService = playerService;
			view.localizationService = localizationService;
			questIdSignal.AddListener(EnableQuestTimer);
			timeoutSignal.AddListener(DisableQuestTimer);
		}

		public override void OnRemove()
		{
			questIdSignal.RemoveListener(EnableQuestTimer);
			timeoutSignal.RemoveListener(DisableQuestTimer);
		}

		private void EnableQuestTimer(int questId)
		{
			view.EnableQuestTimer(questId);
		}

		private void DisableQuestTimer(int questId)
		{
			view.DisableQuestTimer(questId);
		}
	}
}
