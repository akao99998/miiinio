using System;
using Kampai.Main;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ScheduleNotificationCommand : Command
	{
		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public NotificationDefinition notification { get; set; }

		[Inject]
		public INotificationService service { get; set; }

		public override void Execute()
		{
			Notification notification = new Notification();
			notification.type = this.notification.Type;
			notification.secondsFromNow = this.notification.Seconds;
			notification.title = localService.GetString(this.notification.Title);
			notification.text = localService.GetString(this.notification.Text);
			notification.sound = this.notification.Sound;
			Notification notification2 = notification;
			notification2.stackedTitle = notification2.title;
			notification2.stackedText = notification2.text;
			switch ((NotificationType)(int)Enum.Parse(typeof(NotificationType), this.notification.Type))
			{
			case NotificationType.MignetteCooldownComplete:
				notification2.type = this.notification.Track.ToString();
				break;
			case NotificationType.MarketplaceSaleComplete:
				notification2.type = string.Format("{0}_{1}", this.notification.Type, this.notification.Track);
				break;
			case NotificationType.DebugConsole:
				notification2.sound = "bob_booya";
				break;
			}
			if (string.IsNullOrEmpty(notification2.sound))
			{
				notification2.sound = string.Empty;
			}
			service.ScheduleLocalNotification(notification2);
		}
	}
}
