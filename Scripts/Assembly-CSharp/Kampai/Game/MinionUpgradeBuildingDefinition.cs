using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionUpgradeBuildingDefinition : RepairableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1058;
			}
		}

		public string AspirationalMessage_NeedLevel { get; set; }

		public string AspirationalMessage_NeedQuest { get; set; }

		public int UnlockAtLevel { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage_NeedLevel);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage_NeedQuest);
			writer.Write(UnlockAtLevel);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AspirationalMessage_NeedLevel = BinarySerializationUtil.ReadString(reader);
			AspirationalMessage_NeedQuest = BinarySerializationUtil.ReadString(reader);
			UnlockAtLevel = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ASPIRATIONALMESSAGE_NEEDLEVEL":
				reader.Read();
				AspirationalMessage_NeedLevel = ReaderUtil.ReadString(reader, converters);
				break;
			case "ASPIRATIONALMESSAGE_NEEDQUEST":
				reader.Read();
				AspirationalMessage_NeedQuest = ReaderUtil.ReadString(reader, converters);
				break;
			case "UNLOCKATLEVEL":
				reader.Read();
				UnlockAtLevel = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new MinionUpgradeBuilding(this);
		}
	}
}
