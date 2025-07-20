using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionBenefitLevelBandDefintion : Definition
	{
		private static readonly MinionBenefitLevel defaultBenefit = new MinionBenefitLevel
		{
			doubleDropPercentage = 0f,
			premiumDropPercentage = 0f,
			rareDropPercentage = 0f,
			costumeId = -1
		};

		public override int TypeCode
		{
			get
			{
				return 1121;
			}
		}

		public List<MinionBenefit> benefitDescriptions { get; set; }

		public List<MinionBenefitLevel> minionBenefitLevelBands { get; set; }

		public int BonusPremiumRewardValue { get; set; }

		public int FirstBuildingId { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMinionBenefit, benefitDescriptions);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMinionBenefitLevel, minionBenefitLevelBands);
			writer.Write(BonusPremiumRewardValue);
			writer.Write(FirstBuildingId);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			benefitDescriptions = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMinionBenefit, benefitDescriptions);
			minionBenefitLevelBands = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMinionBenefitLevel, minionBenefitLevelBands);
			BonusPremiumRewardValue = reader.ReadInt32();
			FirstBuildingId = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BENEFITDESCRIPTIONS":
				reader.Read();
				benefitDescriptions = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMinionBenefit, benefitDescriptions);
				break;
			case "MINIONBENEFITLEVELBANDS":
				reader.Read();
				minionBenefitLevelBands = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMinionBenefitLevel, minionBenefitLevelBands);
				break;
			case "BONUSPREMIUMREWARDVALUE":
				reader.Read();
				BonusPremiumRewardValue = Convert.ToInt32(reader.Value);
				break;
			case "FIRSTBUILDINGID":
				reader.Read();
				FirstBuildingId = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public MinionBenefitLevel GetMinionBenefit(int level)
		{
			if (minionBenefitLevelBands != null && level < minionBenefitLevelBands.Count)
			{
				return minionBenefitLevelBands[level];
			}
			return defaultBenefit;
		}
	}
}
