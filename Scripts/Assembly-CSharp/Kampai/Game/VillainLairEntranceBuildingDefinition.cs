using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class VillainLairEntranceBuildingDefinition : RepairableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1067;
			}
		}

		public string AspirationalMessage_NeedLevel { get; set; }

		public string AspirationalMessage_NeedKevinsQuest { get; set; }

		public int UnlockAtLevel { get; set; }

		public int UpgradedMinionsNeeded { get; set; }

		public int TransactionID { get; set; }

		public Vector3 HarvestableIconOffset { get; set; }

		public int VillainLairDefinitionID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage_NeedLevel);
			BinarySerializationUtil.WriteString(writer, AspirationalMessage_NeedKevinsQuest);
			writer.Write(UnlockAtLevel);
			writer.Write(UpgradedMinionsNeeded);
			writer.Write(TransactionID);
			BinarySerializationUtil.WriteVector3(writer, HarvestableIconOffset);
			writer.Write(VillainLairDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AspirationalMessage_NeedLevel = BinarySerializationUtil.ReadString(reader);
			AspirationalMessage_NeedKevinsQuest = BinarySerializationUtil.ReadString(reader);
			UnlockAtLevel = reader.ReadInt32();
			UpgradedMinionsNeeded = reader.ReadInt32();
			TransactionID = reader.ReadInt32();
			HarvestableIconOffset = BinarySerializationUtil.ReadVector3(reader);
			VillainLairDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ASPIRATIONALMESSAGE_NEEDLEVEL":
				reader.Read();
				AspirationalMessage_NeedLevel = ReaderUtil.ReadString(reader, converters);
				break;
			case "ASPIRATIONALMESSAGE_NEEDKEVINSQUEST":
				reader.Read();
				AspirationalMessage_NeedKevinsQuest = ReaderUtil.ReadString(reader, converters);
				break;
			case "UNLOCKATLEVEL":
				reader.Read();
				UnlockAtLevel = Convert.ToInt32(reader.Value);
				break;
			case "UPGRADEDMINIONSNEEDED":
				reader.Read();
				UpgradedMinionsNeeded = Convert.ToInt32(reader.Value);
				break;
			case "TRANSACTIONID":
				reader.Read();
				TransactionID = Convert.ToInt32(reader.Value);
				break;
			case "HARVESTABLEICONOFFSET":
				reader.Read();
				HarvestableIconOffset = ReaderUtil.ReadVector3(reader, converters);
				break;
			case "VILLAINLAIRDEFINITIONID":
				reader.Read();
				VillainLairDefinitionID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new VillainLairEntranceBuilding(this);
		}
	}
}
