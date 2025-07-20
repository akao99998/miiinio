using System.Collections.Generic;

namespace Swrve
{
	public interface ISwrvePushNotificationListener
	{
		void OnNotificationReceived(Dictionary<string, object> notificationJson);

		void OnOpenedFromPushNotification(Dictionary<string, object> notificationJson);
	}
}
