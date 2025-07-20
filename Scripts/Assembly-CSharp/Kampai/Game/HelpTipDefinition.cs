using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class HelpTipDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1003;
			}
		}

		public string Title { get; set; }

		public string Message { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Title);
			BinarySerializationUtil.WriteString(writer, Message);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Title = BinarySerializationUtil.ReadString(reader);
			Message = BinarySerializationUtil.ReadString(reader);
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
					Message = ReaderUtil.ReadString(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "TITLE":
				reader.Read();
				Title = ReaderUtil.ReadString(reader, converters);
				break;
			}
			return true;
		}
	}
}
