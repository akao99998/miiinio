using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialEventNotificationsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialEventNotificationsCommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ScheduleNotificationSignal scheduleNotificationSignal { get; set; }

		public override void Execute()
		{
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId == null || !firstInstanceByDefinitionId.IsBuildingRepaired())
			{
				logger.Error("The Stage {0} isn't repaired yet!", firstInstanceByDefinitionId);
				return;
			}
			TimedSocialEventDefinition nextSocialEvent = timedSocialEventService.GetNextSocialEvent();
			if (nextSocialEvent != null)
			{
				NotificationDefinition definition = null;
				if (definitionService.TryGet<NotificationDefinition>(10020, out definition))
				{
					int num = timeService.CurrentTime();
					if (nextSocialEvent.StartTime > num)
					{
						definition.Seconds = nextSocialEvent.StartTime - num;
						scheduleNotificationSignal.Dispatch(definition);
					}
					else
					{
						logger.Error("The current time is not greater than the next start time! This indicates an error. Don't schedule the new notification until it is.");
					}
				}
			}
			TimedSocialEventDefinition currentSocialEvent = timedSocialEventService.GetCurrentSocialEvent();
			if (currentSocialEvent == null)
			{
				return;
			}
			NotificationDefinition definition2 = null;
			if (definitionService.TryGet<NotificationDefinition>(10019, out definition2))
			{
				int num2 = timeService.CurrentTime();
				int num3 = 900;
				int num4 = currentSocialEvent.FinishTime - num3;
				if (num4 > num2)
				{
					definition2.Seconds = num4 - num2;
					scheduleNotificationSignal.Dispatch(definition2);
				}
				else
				{
					logger.Info("The current event will be ending in less than 15 minutes. Cancel the notification.");
				}
			}
		}
	}
}
