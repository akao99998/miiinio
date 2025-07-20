using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class AchievementDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1016;
			}
		}

		public AchievementType.AchievementTypeIdentifier Type { get; set; }

		public int DefinitionID { get; set; }

		public AchievementID AchievementID { get; set; }

		public int Steps { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Type);
			writer.Write(DefinitionID);
			BinarySerializationUtil.WriteAchievementID(writer, AchievementID);
			writer.Write(Steps);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadEnum<AchievementType.AchievementTypeIdentifier>(reader);
			DefinitionID = reader.ReadInt32();
			AchievementID = BinarySerializationUtil.ReadAchievementID(reader);
			Steps = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadEnum<AchievementType.AchievementTypeIdentifier>(reader);
				break;
			case "DEFINITIONID":
				reader.Read();
				DefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "ACHIEVEMENTID":
				reader.Read();
				AchievementID = ReaderUtil.ReadAchievementID(reader, converters);
				break;
			case "STEPS":
				reader.Read();
				Steps = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new Achievement(this);
		}
	}
}
