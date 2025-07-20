using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MissingResourcesRewardDefinition : AdPlacementDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1026;
			}
		}

		public int MaxCostPremiumCurrency { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MaxCostPremiumCurrency);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MaxCostPremiumCurrency = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "MAXCOSTPREMIUMCURRENCY":
				reader.Read();
				MaxCostPremiumCurrency = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
