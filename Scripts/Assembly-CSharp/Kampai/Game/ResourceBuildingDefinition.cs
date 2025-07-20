using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ResourceBuildingDefinition : TaskableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1060;
			}
		}

		public int ItemId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ItemId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ItemId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ITEMID":
				reader.Read();
				ItemId = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override Building BuildBuilding()
		{
			return new ResourceBuilding(this);
		}
	}
}
