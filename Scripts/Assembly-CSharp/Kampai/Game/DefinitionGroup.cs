using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DefinitionGroup : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1001;
			}
		}

		public IList<int> Group { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, Group);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Group = BinarySerializationUtil.ReadListInt32(reader, Group);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "GROUP":
				reader.Read();
				Group = ReaderUtil.PopulateListInt32(reader, Group);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
