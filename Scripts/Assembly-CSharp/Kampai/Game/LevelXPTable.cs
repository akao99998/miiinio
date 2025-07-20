using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LevelXPTable : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1010;
			}
		}

		public IList<int> xpNeededList { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, xpNeededList);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			xpNeededList = BinarySerializationUtil.ReadListInt32(reader, xpNeededList);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "XPNEEDEDLIST":
				reader.Read();
				xpNeededList = ReaderUtil.PopulateListInt32(reader, xpNeededList);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
