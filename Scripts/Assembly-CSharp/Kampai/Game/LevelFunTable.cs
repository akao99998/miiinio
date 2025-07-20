using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class LevelFunTable : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1005;
			}
		}

		public List<PartyUpDefinition> partiesNeededList { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, partiesNeededList);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			partiesNeededList = BinarySerializationUtil.ReadList(reader, partiesNeededList);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PARTIESNEEDEDLIST":
				reader.Read();
				partiesNeededList = ReaderUtil.PopulateList(reader, converters, partiesNeededList);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
