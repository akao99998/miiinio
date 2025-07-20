using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LevelUpDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1009;
			}
		}

		public IList<int> transactionList { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, transactionList);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			transactionList = BinarySerializationUtil.ReadListInt32(reader, transactionList);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TRANSACTIONLIST":
				reader.Read();
				transactionList = ReaderUtil.PopulateListInt32(reader, transactionList);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
