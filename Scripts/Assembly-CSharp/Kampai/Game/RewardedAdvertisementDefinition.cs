using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class RewardedAdvertisementDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1018;
			}
		}

		public int MaxRewardsPerDayGlobal { get; set; }

		public List<AdPlacementDefinition> PlacementDefinitions { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MaxRewardsPerDayGlobal);
			BinarySerializationUtil.WriteList(writer, PlacementDefinitions);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MaxRewardsPerDayGlobal = reader.ReadInt32();
			PlacementDefinitions = BinarySerializationUtil.ReadList(reader, PlacementDefinitions);
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
					PlacementDefinitions = ((converters.adPlacementDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, PlacementDefinitions) : ReaderUtil.PopulateList(reader, converters, converters.adPlacementDefinitionConverter, PlacementDefinitions));
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "MAXREWARDSPERDAYGLOBAL":
				reader.Read();
				MaxRewardsPerDayGlobal = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
