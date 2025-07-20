using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyMeterTierDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1118;
			}
		}

		public int Level { get; set; }

		public int Duration { get; set; }

		public int SpeedPercent { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Level);
			writer.Write(Duration);
			writer.Write(SpeedPercent);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Level = reader.ReadInt32();
			Duration = reader.ReadInt32();
			SpeedPercent = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LEVEL":
				reader.Read();
				Level = Convert.ToInt32(reader.Value);
				break;
			case "DURATION":
				reader.Read();
				Duration = Convert.ToInt32(reader.Value);
				break;
			case "SPEEDPERCENT":
				reader.Read();
				SpeedPercent = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
