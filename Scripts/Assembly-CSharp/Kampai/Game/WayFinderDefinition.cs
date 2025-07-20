using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class WayFinderDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1188;
			}
		}

		public string NewQuestIcon { get; set; }

		public string QuestCompleteIcon { get; set; }

		public string TaskCompleteIcon { get; set; }

		public string SpecialEventNewQuestIcon { get; set; }

		public string SpecialEventQuestCompleteIcon { get; set; }

		public string SpecialEventTaskCompleteIcon { get; set; }

		public string TikibarDefaultIcon { get; set; }

		public float TikibarZoomViewEnabledAt { get; set; }

		public string CabanaDefaultIcon { get; set; }

		public string OrderBoardDefaultIcon { get; set; }

		public string TSMDefaultIcon { get; set; }

		public string StorageBuildingDefaultIcon { get; set; }

		public string BobPointsAtStuffLandExpansionIcon { get; set; }

		public string BobPointsAtStuffDefaultIcon { get; set; }

		public float BobPointsAtStuffYWorldOffset { get; set; }

		public string MarketplaceSoldIcon { get; set; }

		public string StageBuildingIcon { get; set; }

		public string DefaultIcon { get; set; }

		public string KevinLairIcon { get; set; }

		public string MasterPlanComponentCompleteIcon { get; set; }

		public string MasterPlanComponentTaskCompleteIcon { get; set; }

		public string MignetteDefaultIcon { get; set; }

		public string ConnectableIcon { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, NewQuestIcon);
			BinarySerializationUtil.WriteString(writer, QuestCompleteIcon);
			BinarySerializationUtil.WriteString(writer, TaskCompleteIcon);
			BinarySerializationUtil.WriteString(writer, SpecialEventNewQuestIcon);
			BinarySerializationUtil.WriteString(writer, SpecialEventQuestCompleteIcon);
			BinarySerializationUtil.WriteString(writer, SpecialEventTaskCompleteIcon);
			BinarySerializationUtil.WriteString(writer, TikibarDefaultIcon);
			writer.Write(TikibarZoomViewEnabledAt);
			BinarySerializationUtil.WriteString(writer, CabanaDefaultIcon);
			BinarySerializationUtil.WriteString(writer, OrderBoardDefaultIcon);
			BinarySerializationUtil.WriteString(writer, TSMDefaultIcon);
			BinarySerializationUtil.WriteString(writer, StorageBuildingDefaultIcon);
			BinarySerializationUtil.WriteString(writer, BobPointsAtStuffLandExpansionIcon);
			BinarySerializationUtil.WriteString(writer, BobPointsAtStuffDefaultIcon);
			writer.Write(BobPointsAtStuffYWorldOffset);
			BinarySerializationUtil.WriteString(writer, MarketplaceSoldIcon);
			BinarySerializationUtil.WriteString(writer, StageBuildingIcon);
			BinarySerializationUtil.WriteString(writer, DefaultIcon);
			BinarySerializationUtil.WriteString(writer, KevinLairIcon);
			BinarySerializationUtil.WriteString(writer, MasterPlanComponentCompleteIcon);
			BinarySerializationUtil.WriteString(writer, MasterPlanComponentTaskCompleteIcon);
			BinarySerializationUtil.WriteString(writer, MignetteDefaultIcon);
			BinarySerializationUtil.WriteString(writer, ConnectableIcon);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			NewQuestIcon = BinarySerializationUtil.ReadString(reader);
			QuestCompleteIcon = BinarySerializationUtil.ReadString(reader);
			TaskCompleteIcon = BinarySerializationUtil.ReadString(reader);
			SpecialEventNewQuestIcon = BinarySerializationUtil.ReadString(reader);
			SpecialEventQuestCompleteIcon = BinarySerializationUtil.ReadString(reader);
			SpecialEventTaskCompleteIcon = BinarySerializationUtil.ReadString(reader);
			TikibarDefaultIcon = BinarySerializationUtil.ReadString(reader);
			TikibarZoomViewEnabledAt = reader.ReadSingle();
			CabanaDefaultIcon = BinarySerializationUtil.ReadString(reader);
			OrderBoardDefaultIcon = BinarySerializationUtil.ReadString(reader);
			TSMDefaultIcon = BinarySerializationUtil.ReadString(reader);
			StorageBuildingDefaultIcon = BinarySerializationUtil.ReadString(reader);
			BobPointsAtStuffLandExpansionIcon = BinarySerializationUtil.ReadString(reader);
			BobPointsAtStuffDefaultIcon = BinarySerializationUtil.ReadString(reader);
			BobPointsAtStuffYWorldOffset = reader.ReadSingle();
			MarketplaceSoldIcon = BinarySerializationUtil.ReadString(reader);
			StageBuildingIcon = BinarySerializationUtil.ReadString(reader);
			DefaultIcon = BinarySerializationUtil.ReadString(reader);
			KevinLairIcon = BinarySerializationUtil.ReadString(reader);
			MasterPlanComponentCompleteIcon = BinarySerializationUtil.ReadString(reader);
			MasterPlanComponentTaskCompleteIcon = BinarySerializationUtil.ReadString(reader);
			MignetteDefaultIcon = BinarySerializationUtil.ReadString(reader);
			ConnectableIcon = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NEWQUESTICON":
				reader.Read();
				NewQuestIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTCOMPLETEICON":
				reader.Read();
				QuestCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "TASKCOMPLETEICON":
				reader.Read();
				TaskCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "SPECIALEVENTNEWQUESTICON":
				reader.Read();
				SpecialEventNewQuestIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "SPECIALEVENTQUESTCOMPLETEICON":
				reader.Read();
				SpecialEventQuestCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "SPECIALEVENTTASKCOMPLETEICON":
				reader.Read();
				SpecialEventTaskCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "TIKIBARDEFAULTICON":
				reader.Read();
				TikibarDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "TIKIBARZOOMVIEWENABLEDAT":
				reader.Read();
				TikibarZoomViewEnabledAt = Convert.ToSingle(reader.Value);
				break;
			case "CABANADEFAULTICON":
				reader.Read();
				CabanaDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "ORDERBOARDDEFAULTICON":
				reader.Read();
				OrderBoardDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "TSMDEFAULTICON":
				reader.Read();
				TSMDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "STORAGEBUILDINGDEFAULTICON":
				reader.Read();
				StorageBuildingDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "BOBPOINTSATSTUFFLANDEXPANSIONICON":
				reader.Read();
				BobPointsAtStuffLandExpansionIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "BOBPOINTSATSTUFFDEFAULTICON":
				reader.Read();
				BobPointsAtStuffDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "BOBPOINTSATSTUFFYWORLDOFFSET":
				reader.Read();
				BobPointsAtStuffYWorldOffset = Convert.ToSingle(reader.Value);
				break;
			case "MARKETPLACESOLDICON":
				reader.Read();
				MarketplaceSoldIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "STAGEBUILDINGICON":
				reader.Read();
				StageBuildingIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "DEFAULTICON":
				reader.Read();
				DefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "KEVINLAIRICON":
				reader.Read();
				KevinLairIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASTERPLANCOMPONENTCOMPLETEICON":
				reader.Read();
				MasterPlanComponentCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASTERPLANCOMPONENTTASKCOMPLETEICON":
				reader.Read();
				MasterPlanComponentTaskCompleteIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "MIGNETTEDEFAULTICON":
				reader.Read();
				MignetteDefaultIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "CONNECTABLEICON":
				reader.Read();
				ConnectableIcon = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
