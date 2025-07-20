using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class QuestChainDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1132;
			}
		}

		public string Name { get; set; }

		public string Summary { get; set; }

		public int Giver { get; set; }

		public int Level { get; set; }

		public IList<QuestChainStepDefinition> Steps { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Name);
			BinarySerializationUtil.WriteString(writer, Summary);
			writer.Write(Giver);
			writer.Write(Level);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteQuestChainStepDefinition, Steps);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Name = BinarySerializationUtil.ReadString(reader);
			Summary = BinarySerializationUtil.ReadString(reader);
			Giver = reader.ReadInt32();
			Level = reader.ReadInt32();
			Steps = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadQuestChainStepDefinition, Steps);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NAME":
				reader.Read();
				Name = ReaderUtil.ReadString(reader, converters);
				break;
			case "SUMMARY":
				reader.Read();
				Summary = ReaderUtil.ReadString(reader, converters);
				break;
			case "GIVER":
				reader.Read();
				Giver = Convert.ToInt32(reader.Value);
				break;
			case "LEVEL":
				reader.Read();
				Level = Convert.ToInt32(reader.Value);
				break;
			case "STEPS":
				reader.Read();
				Steps = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadQuestChainStepDefinition, Steps);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
