using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class TimedQuestNotificationCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TimedQuestNotificationCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ScheduleNotificationSignal notificationSignal { get; set; }

		[Inject]
		public IDevicePrefsService devicePrefsService { get; set; }

		[Inject]
		public int questId { get; set; }

		public override void Execute()
		{
			if (devicePrefsService.GetDevicePrefs().EventNotif)
			{
				NotificationDefinition notificationDefinition = definitionService.Get<NotificationDefinition>(10012);
				Quest byInstanceId = playerService.GetByInstanceId<Quest>(questId);
				if (byInstanceId == null)
				{
					logger.Log(KampaiLogLevel.Error, string.Format("Quest instance id {0} does not exist in the player service", questId));
				}
				TimedQuestDefinition timedQuestDefinition = definitionService.Get<TimedQuestDefinition>(byInstanceId.GetActiveDefinition().ID);
				NotificationDefinition notificationDefinition2 = new NotificationDefinition();
				notificationDefinition2.ID = questId;
				notificationDefinition2.Type = notificationDefinition.Type;
				notificationDefinition2.Seconds = timedQuestDefinition.PushNoteWarningTime;
				notificationDefinition2.Title = notificationDefinition.Title;
				notificationDefinition2.Text = notificationDefinition.Text;
				notificationSignal.Dispatch(notificationDefinition2);
			}
		}
	}
}
