namespace Kampai.Game
{
	public interface INotificationService
	{
		void Initialize();

		void ScheduleLocalNotification(Notification notification);

		void CancelLocalNotification(string type);

		void CancelAllNotifications();
	}
}
