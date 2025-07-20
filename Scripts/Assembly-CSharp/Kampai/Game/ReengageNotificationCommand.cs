using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ReengageNotificationCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ScheduleNotificationSignal notificationSignal { get; set; }

		[Inject]
		public IDevicePrefsService devicePrefsService { get; set; }

		public override void Execute()
		{
			if (!devicePrefsService.GetDevicePrefs().MinionsParadiseNotif)
			{
				return;
			}
			IList<NotificationDefinition> all = definitionService.GetAll<NotificationDefinition>();
			for (int i = 0; i < all.Count; i++)
			{
				NotificationDefinition notificationDefinition = all[i];
				if (notificationDefinition.Type == NotificationType.Reengage.ToString())
				{
					notificationSignal.Dispatch(notificationDefinition);
				}
			}
		}
	}
}
