using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DecorationBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1044;
			}
		}

		public int Cost { get; set; }

		public int XPReward { get; set; }

		public bool AutoPlace { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Cost);
			writer.Write(XPReward);
			writer.Write(AutoPlace);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Cost = reader.ReadInt32();
			XPReward = reader.ReadInt32();
			AutoPlace = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "COST":
				reader.Read();
				Cost = Convert.ToInt32(reader.Value);
				break;
			case "XPREWARD":
				reader.Read();
				XPReward = Convert.ToInt32(reader.Value);
				break;
			case "AUTOPLACE":
				reader.Read();
				AutoPlace = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new DecorationBuilding(this);
		}
	}
}
