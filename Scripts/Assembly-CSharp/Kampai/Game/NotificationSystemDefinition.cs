using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class NotificationSystemDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1012;
			}
		}

		public List<NotificationReminder> notificationReminders { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteNotificationReminder, notificationReminders);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			notificationReminders = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadNotificationReminder, notificationReminders);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NOTIFICATIONREMINDERS":
				reader.Read();
				notificationReminders = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadNotificationReminder, notificationReminders);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
