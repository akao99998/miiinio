using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionPartyLevelBandDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1119;
			}
		}

		public int MinLevel { get; set; }

		public int PointsTotal { get; set; }

		public int Delta { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MinLevel);
			writer.Write(PointsTotal);
			writer.Write(Delta);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MinLevel = reader.ReadInt32();
			PointsTotal = reader.ReadInt32();
			Delta = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MINLEVEL":
				reader.Read();
				MinLevel = Convert.ToInt32(reader.Value);
				break;
			case "POINTSTOTAL":
				reader.Read();
				PointsTotal = Convert.ToInt32(reader.Value);
				break;
			case "DELTA":
				reader.Read();
				Delta = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
