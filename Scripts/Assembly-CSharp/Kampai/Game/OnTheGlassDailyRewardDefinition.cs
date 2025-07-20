using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class OnTheGlassDailyRewardDefinition : AdPlacementDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1021;
			}
		}

		public RewardTiers RewardTiers { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteRewardTiers(writer, RewardTiers);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			RewardTiers = BinarySerializationUtil.ReadRewardTiers(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "REWARDTIERS":
				reader.Read();
				RewardTiers = FastJSONDeserializer.Deserialize<RewardTiers>(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}
	}
}
