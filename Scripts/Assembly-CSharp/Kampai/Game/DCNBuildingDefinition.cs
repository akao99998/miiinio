using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DCNBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1047;
			}
		}

		public int UnlockLevel { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(UnlockLevel);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			UnlockLevel = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "UNLOCKLEVEL":
				reader.Read();
				UnlockLevel = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override Building BuildBuilding()
		{
			return new DCNBuilding(this);
		}
	}
}
