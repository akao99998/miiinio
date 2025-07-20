using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PendingRewardDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1017;
			}
		}

		public string aspirationalLocKey { get; set; }

		public int awardAtLevel { get; set; }

		public string hudReminderImage { get; set; }

		public string hudReminderMask { get; set; }

		public List<int> transactions { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, aspirationalLocKey);
			writer.Write(awardAtLevel);
			BinarySerializationUtil.WriteString(writer, hudReminderImage);
			BinarySerializationUtil.WriteString(writer, hudReminderMask);
			BinarySerializationUtil.WriteListInt32(writer, transactions);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			aspirationalLocKey = BinarySerializationUtil.ReadString(reader);
			awardAtLevel = reader.ReadInt32();
			hudReminderImage = BinarySerializationUtil.ReadString(reader);
			hudReminderMask = BinarySerializationUtil.ReadString(reader);
			transactions = BinarySerializationUtil.ReadListInt32(reader, transactions);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ASPIRATIONALLOCKEY":
				reader.Read();
				aspirationalLocKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "AWARDATLEVEL":
				reader.Read();
				awardAtLevel = Convert.ToInt32(reader.Value);
				break;
			case "HUDREMINDERIMAGE":
				reader.Read();
				hudReminderImage = ReaderUtil.ReadString(reader, converters);
				break;
			case "HUDREMINDERMASK":
				reader.Read();
				hudReminderMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "TRANSACTIONS":
				reader.Read();
				transactions = ReaderUtil.PopulateListInt32(reader, transactions);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
