using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DebrisBuildingDefinition : TaskableBuildingDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1048;
			}
		}

		public int TransactionID { get; set; }

		public IList<string> VFXPrefabs { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(TransactionID);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, VFXPrefabs);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			TransactionID = reader.ReadInt32();
			VFXPrefabs = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, VFXPrefabs);
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
					VFXPrefabs = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, VFXPrefabs);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "TRANSACTIONID":
				reader.Read();
				TransactionID = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new DebrisBuilding(this);
		}
	}
}
