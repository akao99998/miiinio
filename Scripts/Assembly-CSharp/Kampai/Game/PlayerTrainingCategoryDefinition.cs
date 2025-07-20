using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PlayerTrainingCategoryDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1124;
			}
		}

		public string categoryTitleLocalizedKey { get; set; }

		public List<int> trainingDefinitionIDs { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, categoryTitleLocalizedKey);
			BinarySerializationUtil.WriteListInt32(writer, trainingDefinitionIDs);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			categoryTitleLocalizedKey = BinarySerializationUtil.ReadString(reader);
			trainingDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, trainingDefinitionIDs);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					trainingDefinitionIDs = ReaderUtil.PopulateListInt32(reader, trainingDefinitionIDs);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "CATEGORYTITLELOCALIZEDKEY":
				reader.Read();
				categoryTitleLocalizedKey = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
