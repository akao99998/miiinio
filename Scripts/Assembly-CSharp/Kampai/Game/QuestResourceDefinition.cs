using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class QuestResourceDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1133;
			}
		}

		public string resourcePath { get; set; }

		public string maskPath { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, resourcePath);
			BinarySerializationUtil.WriteString(writer, maskPath);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			resourcePath = BinarySerializationUtil.ReadString(reader);
			maskPath = BinarySerializationUtil.ReadString(reader);
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
					maskPath = ReaderUtil.ReadString(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "RESOURCEPATH":
				reader.Read();
				resourcePath = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
