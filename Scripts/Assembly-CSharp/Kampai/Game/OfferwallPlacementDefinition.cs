using System;
using System.IO;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class OfferwallPlacementDefinition : AdPlacementDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1029;
			}
		}

		public int RewardItemId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(RewardItemId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			RewardItemId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REWARDITEMID":
				reader.Read();
				RewardItemId = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
