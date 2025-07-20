using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class SpecialEventItemDefinition : ItemDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1095;
			}
		}

		public bool IsActive { get; set; }

		public string Paintover { get; set; }

		public int EventMinionCostumeId { get; set; }

		public string EventMinionController { get; set; }

		public int AwardCostumeId { get; set; }

		public int PrestigeDefinitionID { get; set; }

		public int UnlockQuestID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(IsActive);
			BinarySerializationUtil.WriteString(writer, Paintover);
			writer.Write(EventMinionCostumeId);
			BinarySerializationUtil.WriteString(writer, EventMinionController);
			writer.Write(AwardCostumeId);
			writer.Write(PrestigeDefinitionID);
			writer.Write(UnlockQuestID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			IsActive = reader.ReadBoolean();
			Paintover = BinarySerializationUtil.ReadString(reader);
			EventMinionCostumeId = reader.ReadInt32();
			EventMinionController = BinarySerializationUtil.ReadString(reader);
			AwardCostumeId = reader.ReadInt32();
			PrestigeDefinitionID = reader.ReadInt32();
			UnlockQuestID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ISACTIVE":
				reader.Read();
				IsActive = Convert.ToBoolean(reader.Value);
				break;
			case "PAINTOVER":
				reader.Read();
				Paintover = ReaderUtil.ReadString(reader, converters);
				break;
			case "EVENTMINIONCOSTUMEID":
				reader.Read();
				EventMinionCostumeId = Convert.ToInt32(reader.Value);
				break;
			case "EVENTMINIONCONTROLLER":
				reader.Read();
				EventMinionController = ReaderUtil.ReadString(reader, converters);
				break;
			case "AWARDCOSTUMEID":
				reader.Read();
				AwardCostumeId = Convert.ToInt32(reader.Value);
				break;
			case "PRESTIGEDEFINITIONID":
				reader.Read();
				PrestigeDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKQUESTID":
				reader.Read();
				UnlockQuestID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Instance Build()
		{
			return new SpecialEventItem(this);
		}
	}
}
