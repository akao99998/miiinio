using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyPointsPerLevelDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1120;
			}
		}

		public int Level { get; set; }

		public int PointsPerTick { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Level);
			writer.Write(PointsPerTick);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Level = reader.ReadInt32();
			PointsPerTick = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					PointsPerTick = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "LEVEL":
				reader.Read();
				Level = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
