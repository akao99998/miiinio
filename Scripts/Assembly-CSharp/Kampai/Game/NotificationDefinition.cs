using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class NotificationDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1011;
			}
		}

		public string Type { get; set; }

		public int Seconds { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public int Track { get; set; }

		public string Sound { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Type);
			writer.Write(Seconds);
			BinarySerializationUtil.WriteString(writer, Title);
			BinarySerializationUtil.WriteString(writer, Text);
			writer.Write(Track);
			BinarySerializationUtil.WriteString(writer, Sound);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Type = BinarySerializationUtil.ReadString(reader);
			Seconds = reader.ReadInt32();
			Title = BinarySerializationUtil.ReadString(reader);
			Text = BinarySerializationUtil.ReadString(reader);
			Track = reader.ReadInt32();
			Sound = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TYPE":
				reader.Read();
				Type = ReaderUtil.ReadString(reader, converters);
				break;
			case "SECONDS":
				reader.Read();
				Seconds = Convert.ToInt32(reader.Value);
				break;
			case "TITLE":
				reader.Read();
				Title = ReaderUtil.ReadString(reader, converters);
				break;
			case "TEXT":
				reader.Read();
				Text = ReaderUtil.ReadString(reader, converters);
				break;
			case "TRACK":
				reader.Read();
				Track = Convert.ToInt32(reader.Value);
				break;
			case "SOUND":
				reader.Read();
				Sound = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
