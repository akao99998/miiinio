using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Quest2xRewardDefinition : AdPlacementDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1028;
			}
		}

		public List<int> AllowedRewardItemTypes { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteListInt32(writer, AllowedRewardItemTypes);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AllowedRewardItemTypes = BinarySerializationUtil.ReadListInt32(reader, AllowedRewardItemTypes);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ALLOWEDREWARDITEMTYPES":
				reader.Read();
				AllowedRewardItemTypes = ReaderUtil.PopulateListInt32(reader, AllowedRewardItemTypes);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
