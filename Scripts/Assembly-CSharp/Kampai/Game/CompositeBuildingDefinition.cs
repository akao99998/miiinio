using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CompositeBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1041;
			}
		}

		public int MaxPieces { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MaxPieces);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MaxPieces = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MAXPIECES":
				reader.Read();
				MaxPieces = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override Building BuildBuilding()
		{
			return new CompositeBuilding(this);
		}
	}
}
