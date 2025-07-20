using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LeisureBuildingDefintiion : AnimatingBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1052;
			}
		}

		public int LeisureTimeDuration { get; set; }

		public int PartyPointsReward { get; set; }

		public string VFXPrefab { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(LeisureTimeDuration);
			writer.Write(PartyPointsReward);
			BinarySerializationUtil.WriteString(writer, VFXPrefab);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			LeisureTimeDuration = reader.ReadInt32();
			PartyPointsReward = reader.ReadInt32();
			VFXPrefab = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LEISURETIMEDURATION":
				reader.Read();
				LeisureTimeDuration = Convert.ToInt32(reader.Value);
				break;
			case "PARTYPOINTSREWARD":
				reader.Read();
				PartyPointsReward = Convert.ToInt32(reader.Value);
				break;
			case "VFXPREFAB":
				reader.Read();
				VFXPrefab = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new LeisureBuilding(this);
		}
	}
}
