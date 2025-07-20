using System;
using System.IO;
using Kampai.Game;
using Newtonsoft.Json;

namespace Kampai.Splash
{
	public class LoadinTipBucketDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1191;
			}
		}

		public int Min { get; set; }

		public int Max { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Min);
			writer.Write(Max);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Min = reader.ReadInt32();
			Max = reader.ReadInt32();
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
					Max = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "MIN":
				reader.Read();
				Min = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
