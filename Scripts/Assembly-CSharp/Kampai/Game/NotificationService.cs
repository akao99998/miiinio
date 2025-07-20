using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;

namespace Kampai.Game
{
	public class NotificationService : INotificationService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("NotificationService") as IKampaiLogger;

		[Inject]
		public ICoppaService coppaService { get; set; }

		public void Initialize()
		{
		}

		public void ScheduleLocalNotification(Notification notification)
		{
			if (!coppaService.Restricted())
			{
				string sound = null;
				if (!string.IsNullOrEmpty(notification.sound))
				{
					sound = notification.sound;
				}
				Native.ScheduleLocalNotification(notification.type, notification.secondsFromNow, notification.title, notification.text, notification.stackedTitle, notification.stackedText, string.Empty, sound, string.Empty, notification.badgeNumber);
			}
		}

		public void CancelLocalNotification(string type)
		{
			Native.CancelLocalNotification(type);
		}

		public void CancelAllNotifications()
		{
			Native.CancelAllLocalNotifications();
		}
	}
}
