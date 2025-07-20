using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LandExpansionDefinition : TaxonomyDefinition, Locatable
	{
		public override int TypeCode
		{
			get
			{
				return 1100;
			}
		}

		public int BuildingDefinitionID { get; set; }

		public int ExpansionID { get; set; }

		public Location Location { get; set; }

		public bool Grass { get; set; }

		public int MinimumLevel { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(BuildingDefinitionID);
			writer.Write(ExpansionID);
			BinarySerializationUtil.WriteLocation(writer, Location);
			writer.Write(Grass);
			writer.Write(MinimumLevel);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			BuildingDefinitionID = reader.ReadInt32();
			ExpansionID = reader.ReadInt32();
			Location = BinarySerializationUtil.ReadLocation(reader);
			Grass = reader.ReadBoolean();
			MinimumLevel = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUILDINGDEFINITIONID":
				reader.Read();
				BuildingDefinitionID = Convert.ToInt32(reader.Value);
				break;
			case "EXPANSIONID":
				reader.Read();
				ExpansionID = Convert.ToInt32(reader.Value);
				break;
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "GRASS":
				reader.Read();
				Grass = Convert.ToBoolean(reader.Value);
				break;
			case "MINIMUMLEVEL":
				reader.Read();
				MinimumLevel = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
