using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class VillainLairResourcePlotDefinition : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1068;
			}
		}

		public string brokenPrefab_loaded { get; set; }

		public string prefab_loaded { get; set; }

		public int randomGagMin { get; set; }

		public int randomGagMax { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, brokenPrefab_loaded);
			BinarySerializationUtil.WriteString(writer, prefab_loaded);
			writer.Write(randomGagMin);
			writer.Write(randomGagMax);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			brokenPrefab_loaded = BinarySerializationUtil.ReadString(reader);
			prefab_loaded = BinarySerializationUtil.ReadString(reader);
			randomGagMin = reader.ReadInt32();
			randomGagMax = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BROKENPREFAB_LOADED":
				reader.Read();
				brokenPrefab_loaded = ReaderUtil.ReadString(reader, converters);
				break;
			case "PREFAB_LOADED":
				reader.Read();
				prefab_loaded = ReaderUtil.ReadString(reader, converters);
				break;
			case "RANDOMGAGMIN":
				reader.Read();
				randomGagMin = Convert.ToInt32(reader.Value);
				break;
			case "RANDOMGAGMAX":
				reader.Read();
				randomGagMax = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new VillainLairResourcePlot(this);
		}
	}
}
