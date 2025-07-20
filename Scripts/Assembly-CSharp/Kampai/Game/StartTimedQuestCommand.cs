using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartTimedQuestCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartTimedQuestCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public QuestTimeoutSignal timeoutSignal { get; set; }

		[Inject]
		public TimedQuestNotificationSignal questNoteSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public int questId { get; set; }

		public override void Execute()
		{
			Quest byInstanceId = playerService.GetByInstanceId<Quest>(questId);
			if (byInstanceId == null)
			{
				logger.Error("Quest doesn't exist for quest instance id: {0}", questId);
				return;
			}
			TimedQuestDefinition timedQuestDefinition = byInstanceId.GetActiveDefinition() as TimedQuestDefinition;
			if (timedQuestDefinition != null)
			{
				byInstanceId.UTCQuestStartTime = timeService.CurrentTime();
				questNoteSignal.Dispatch(byInstanceId.ID);
				timeEventService.AddEvent(byInstanceId.ID, byInstanceId.UTCQuestStartTime, timedQuestDefinition.Duration, timeoutSignal);
			}
		}
	}
}
