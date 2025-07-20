using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CurrencyStoreDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1143;
			}
		}

		public IList<CurrencyStoreCategoryDefinition> CategoryDefinitions { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, CategoryDefinitions);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			CategoryDefinitions = BinarySerializationUtil.ReadList(reader, CategoryDefinitions);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "CATEGORYDEFINITIONS":
				reader.Read();
				CategoryDefinitions = ReaderUtil.PopulateList(reader, converters, CategoryDefinitions);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
