using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MignetteNotificationCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MignetteNotificationCommand") as IKampaiLogger;

		[Inject]
		public bool cancelNotification { get; set; }

		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CancelNotificationSignal cancelNotificationSignal { get; set; }

		[Inject]
		public ScheduleNotificationSignal scheduleNotificationSignal { get; set; }

		public override void Execute()
		{
			IBuildingWithCooldown byInstanceId = playerService.GetByInstanceId<IBuildingWithCooldown>(buildingId);
			if (cancelNotification)
			{
				cancelNotificationSignal.Dispatch(byInstanceId.Definition.ID.ToString());
				return;
			}
			if (byInstanceId == null)
			{
				logger.Error("No IBuildingWithCooldown exists for buildingId: {0}", buildingId);
				return;
			}
			IList<NotificationDefinition> all = definitionService.GetAll<NotificationDefinition>();
			foreach (NotificationDefinition item in all)
			{
				if (item.Type.Equals(NotificationType.MignetteCooldownComplete.ToString()))
				{
					cancelNotificationSignal.Dispatch(item.Track.ToString());
				}
			}
			foreach (NotificationDefinition item2 in all)
			{
				if (item2.Type.Equals(NotificationType.MignetteCooldownComplete.ToString()) && item2.Track == byInstanceId.Definition.ID)
				{
					item2.Seconds = byInstanceId.GetCooldown();
					scheduleNotificationSignal.Dispatch(item2);
				}
			}
		}
	}
}
