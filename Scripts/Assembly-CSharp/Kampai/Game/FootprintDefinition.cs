using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FootprintDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1126;
			}
		}

		public string Footprint { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Footprint);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Footprint = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "FOOTPRINT":
				reader.Read();
				Footprint = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
