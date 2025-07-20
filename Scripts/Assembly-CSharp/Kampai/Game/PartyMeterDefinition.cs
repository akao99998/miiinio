using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PartyMeterDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1117;
			}
		}

		public IList<PartyMeterTierDefinition> Tiers { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, Tiers);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Tiers = BinarySerializationUtil.ReadList(reader, Tiers);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TIERS":
				reader.Read();
				Tiers = ReaderUtil.PopulateList(reader, converters, Tiers);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
