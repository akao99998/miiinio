using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DropLevelBandDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1002;
			}
		}

		public List<int> HarvestsPerDrop { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, HarvestsPerDrop);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			HarvestsPerDrop = BinarySerializationUtil.ReadListInt32(reader, HarvestsPerDrop);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "HARVESTSPERDROP":
				reader.Read();
				HarvestsPerDrop = ReaderUtil.PopulateListInt32(reader, HarvestsPerDrop);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
