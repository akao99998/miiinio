using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public abstract class PlotDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1128;
			}
		}

		public int X { get; set; }

		public int Y { get; set; }

		public int FootprintID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(X);
			writer.Write(Y);
			writer.Write(FootprintID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			FootprintID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "X":
				reader.Read();
				X = Convert.ToInt32(reader.Value);
				break;
			case "Y":
				reader.Read();
				Y = Convert.ToInt32(reader.Value);
				break;
			case "FOOTPRINTID":
				reader.Read();
				FootprintID = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public abstract Plot Instantiate();
	}
}
