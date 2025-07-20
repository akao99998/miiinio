using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class RepairableBuildingDefinition : BuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1033;
			}
		}

		public string brokenPrefab { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, brokenPrefab);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			brokenPrefab = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BROKENPREFAB":
				reader.Read();
				brokenPrefab = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
