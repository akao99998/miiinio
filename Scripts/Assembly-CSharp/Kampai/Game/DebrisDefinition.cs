using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DebrisDefinition : Definition, Locatable
	{
		public override int TypeCode
		{
			get
			{
				return 1098;
			}
		}

		public int BuildingDefinitionID { get; set; }

		public Location Location { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(BuildingDefinitionID);
			BinarySerializationUtil.WriteLocation(writer, Location);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			BuildingDefinitionID = reader.ReadInt32();
			Location = BinarySerializationUtil.ReadLocation(reader);
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
					Location = ReaderUtil.ReadLocation(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "BUILDINGDEFINITIONID":
				reader.Read();
				BuildingDefinitionID = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
