using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LocalizedTextDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1101;
			}
		}

		public string Language { get; set; }

		public string Translation { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Language);
			BinarySerializationUtil.WriteString(writer, Translation);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Language = BinarySerializationUtil.ReadString(reader);
			Translation = BinarySerializationUtil.ReadString(reader);
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
					Translation = ReaderUtil.ReadString(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "LANGUAGE":
				reader.Read();
				Language = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
