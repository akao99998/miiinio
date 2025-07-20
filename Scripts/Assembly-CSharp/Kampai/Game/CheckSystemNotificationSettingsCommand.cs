using System;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CheckSystemNotificationSettingsCommand : Command
	{
		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			bool value = Native.AreNotificationsEnabled();
			int num = Convert.ToInt32(value);
			if (!localPersistence.HasKey("NotificationSettings"))
			{
				localPersistence.PutDataInt("NotificationSettings", num);
				return;
			}
			int dataInt = localPersistence.GetDataInt("NotificationSettings");
			if (dataInt != num)
			{
				telemetryService.Send_Telemetry_EVT_NOTE_SETTING_CHANGE("SystemNotifications", (num != 1) ? "Disabled" : "Enabled", "System");
				localPersistence.PutDataInt("NotificationSettings", num);
			}
		}
	}
}
