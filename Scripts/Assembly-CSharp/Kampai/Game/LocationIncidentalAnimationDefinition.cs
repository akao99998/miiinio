using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LocationIncidentalAnimationDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1078;
			}
		}

		public int AnimationId { get; set; }

		public FloatLocation Location { get; set; }

		public Angle Rotation { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(AnimationId);
			BinarySerializationUtil.WriteFloatLocation(writer, Location);
			BinarySerializationUtil.WriteAngle(writer, Rotation);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AnimationId = reader.ReadInt32();
			Location = BinarySerializationUtil.ReadFloatLocation(reader);
			Rotation = BinarySerializationUtil.ReadAngle(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ANIMATIONID":
				reader.Read();
				AnimationId = Convert.ToInt32(reader.Value);
				break;
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadFloatLocation(reader, converters);
				break;
			case "ROTATION":
				reader.Read();
				Rotation = ReaderUtil.ReadAngle(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
