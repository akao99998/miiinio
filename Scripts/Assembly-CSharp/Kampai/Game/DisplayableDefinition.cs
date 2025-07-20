using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DisplayableDefinition : Definition, IDisplayableDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1015;
			}
		}

		public string Image { get; set; }

		public string Mask { get; set; }

		public string Description { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Image);
			BinarySerializationUtil.WriteString(writer, Mask);
			BinarySerializationUtil.WriteString(writer, Description);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Image = BinarySerializationUtil.ReadString(reader);
			Mask = BinarySerializationUtil.ReadString(reader);
			Description = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "IMAGE":
				reader.Read();
				Image = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASK":
				reader.Read();
				Mask = ReaderUtil.ReadString(reader, converters);
				break;
			case "DESCRIPTION":
				reader.Read();
				Description = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
