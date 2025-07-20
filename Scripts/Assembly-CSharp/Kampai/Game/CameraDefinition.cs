using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CameraDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1070;
			}
		}

		public float MaxZoomInLevel { get; set; }

		public float MaxZoomOutLevel { get; set; }

		public float ZoomInBounceSpeed { get; set; }

		public float ZoomOutBounceSpeed { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MaxZoomInLevel);
			writer.Write(MaxZoomOutLevel);
			writer.Write(ZoomInBounceSpeed);
			writer.Write(ZoomOutBounceSpeed);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MaxZoomInLevel = reader.ReadSingle();
			MaxZoomOutLevel = reader.ReadSingle();
			ZoomInBounceSpeed = reader.ReadSingle();
			ZoomOutBounceSpeed = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MAXZOOMINLEVEL":
				reader.Read();
				MaxZoomInLevel = Convert.ToSingle(reader.Value);
				break;
			case "MAXZOOMOUTLEVEL":
				reader.Read();
				MaxZoomOutLevel = Convert.ToSingle(reader.Value);
				break;
			case "ZOOMINBOUNCESPEED":
				reader.Read();
				ZoomInBounceSpeed = Convert.ToSingle(reader.Value);
				break;
			case "ZOOMOUTBOUNCESPEED":
				reader.Read();
				ZoomOutBounceSpeed = Convert.ToSingle(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
