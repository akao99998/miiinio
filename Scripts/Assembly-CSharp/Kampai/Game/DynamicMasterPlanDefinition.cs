using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DynamicMasterPlanDefinition : Definition
	{
		public IList<MasterPlanComponentDefinition> DynamicComponents = new List<MasterPlanComponentDefinition>();

		public override int TypeCode
		{
			get
			{
				return 1106;
			}
		}

		public int ItemCategoryCount { get; set; }

		public uint EarnSandDollarMin { get; set; }

		public uint EarnSandDollarMax { get; set; }

		public uint FillOrderRangeMin { get; set; }

		public uint FillOrderRangeMax { get; set; }

		public uint MinProductionCount { get; set; }

		public uint MaxProductionCount { get; set; }

		public uint MaxProductionTime { get; set; }

		public float MaxStorageCapactiy { get; set; }

		public uint PartyPointsRangeMin { get; set; }

		public uint PartyPointsRangeMax { get; set; }

		public uint PlayMiniGameRangeMin { get; set; }

		public uint PlayMiniGameRangeMax { get; set; }

		public uint MinGrindReward { get; set; }

		public uint MinPremiumReward { get; set; }

		public IList<Reward> RewardTableCompleteOrders { get; set; }

		public IList<Reward> RewardTablePlayMiniGame { get; set; }

		public IList<Reward> RewardTableEarnPartyPoints { get; set; }

		public IList<Reward> RewardTableEarnSandDollars { get; set; }

		public IList<MiniGameScoreReward> RewardTableMiniGameScore { get; set; }

		public IList<MiniGameScoreRange> RequirementTableMiniGameScore { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(ItemCategoryCount);
			writer.Write(EarnSandDollarMin);
			writer.Write(EarnSandDollarMax);
			writer.Write(FillOrderRangeMin);
			writer.Write(FillOrderRangeMax);
			writer.Write(MinProductionCount);
			writer.Write(MaxProductionCount);
			writer.Write(MaxProductionTime);
			writer.Write(MaxStorageCapactiy);
			writer.Write(PartyPointsRangeMin);
			writer.Write(PartyPointsRangeMax);
			writer.Write(PlayMiniGameRangeMin);
			writer.Write(PlayMiniGameRangeMax);
			writer.Write(MinGrindReward);
			writer.Write(MinPremiumReward);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteReward, RewardTableCompleteOrders);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteReward, RewardTablePlayMiniGame);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteReward, RewardTableEarnPartyPoints);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteReward, RewardTableEarnSandDollars);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMiniGameScoreReward, RewardTableMiniGameScore);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMiniGameScoreRange, RequirementTableMiniGameScore);
			BinarySerializationUtil.WriteList(writer, DynamicComponents);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			ItemCategoryCount = reader.ReadInt32();
			EarnSandDollarMin = reader.ReadUInt32();
			EarnSandDollarMax = reader.ReadUInt32();
			FillOrderRangeMin = reader.ReadUInt32();
			FillOrderRangeMax = reader.ReadUInt32();
			MinProductionCount = reader.ReadUInt32();
			MaxProductionCount = reader.ReadUInt32();
			MaxProductionTime = reader.ReadUInt32();
			MaxStorageCapactiy = reader.ReadSingle();
			PartyPointsRangeMin = reader.ReadUInt32();
			PartyPointsRangeMax = reader.ReadUInt32();
			PlayMiniGameRangeMin = reader.ReadUInt32();
			PlayMiniGameRangeMax = reader.ReadUInt32();
			MinGrindReward = reader.ReadUInt32();
			MinPremiumReward = reader.ReadUInt32();
			RewardTableCompleteOrders = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadReward, RewardTableCompleteOrders);
			RewardTablePlayMiniGame = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadReward, RewardTablePlayMiniGame);
			RewardTableEarnPartyPoints = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadReward, RewardTableEarnPartyPoints);
			RewardTableEarnSandDollars = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadReward, RewardTableEarnSandDollars);
			RewardTableMiniGameScore = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMiniGameScoreReward, RewardTableMiniGameScore);
			RequirementTableMiniGameScore = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMiniGameScoreRange, RequirementTableMiniGameScore);
			DynamicComponents = BinarySerializationUtil.ReadList(reader, DynamicComponents);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ITEMCATEGORYCOUNT":
				reader.Read();
				ItemCategoryCount = Convert.ToInt32(reader.Value);
				break;
			case "EARNSANDDOLLARMIN":
				reader.Read();
				EarnSandDollarMin = Convert.ToUInt32(reader.Value);
				break;
			case "EARNSANDDOLLARMAX":
				reader.Read();
				EarnSandDollarMax = Convert.ToUInt32(reader.Value);
				break;
			case "FILLORDERRANGEMIN":
				reader.Read();
				FillOrderRangeMin = Convert.ToUInt32(reader.Value);
				break;
			case "FILLORDERRANGEMAX":
				reader.Read();
				FillOrderRangeMax = Convert.ToUInt32(reader.Value);
				break;
			case "MINPRODUCTIONCOUNT":
				reader.Read();
				MinProductionCount = Convert.ToUInt32(reader.Value);
				break;
			case "MAXPRODUCTIONCOUNT":
				reader.Read();
				MaxProductionCount = Convert.ToUInt32(reader.Value);
				break;
			case "MAXPRODUCTIONTIME":
				reader.Read();
				MaxProductionTime = Convert.ToUInt32(reader.Value);
				break;
			case "MAXSTORAGECAPACTIY":
				reader.Read();
				MaxStorageCapactiy = Convert.ToSingle(reader.Value);
				break;
			case "PARTYPOINTSRANGEMIN":
				reader.Read();
				PartyPointsRangeMin = Convert.ToUInt32(reader.Value);
				break;
			case "PARTYPOINTSRANGEMAX":
				reader.Read();
				PartyPointsRangeMax = Convert.ToUInt32(reader.Value);
				break;
			case "PLAYMINIGAMERANGEMIN":
				reader.Read();
				PlayMiniGameRangeMin = Convert.ToUInt32(reader.Value);
				break;
			case "PLAYMINIGAMERANGEMAX":
				reader.Read();
				PlayMiniGameRangeMax = Convert.ToUInt32(reader.Value);
				break;
			case "MINGRINDREWARD":
				reader.Read();
				MinGrindReward = Convert.ToUInt32(reader.Value);
				break;
			case "MINPREMIUMREWARD":
				reader.Read();
				MinPremiumReward = Convert.ToUInt32(reader.Value);
				break;
			case "REWARDTABLECOMPLETEORDERS":
				reader.Read();
				RewardTableCompleteOrders = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadReward, RewardTableCompleteOrders);
				break;
			case "REWARDTABLEPLAYMINIGAME":
				reader.Read();
				RewardTablePlayMiniGame = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadReward, RewardTablePlayMiniGame);
				break;
			case "REWARDTABLEEARNPARTYPOINTS":
				reader.Read();
				RewardTableEarnPartyPoints = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadReward, RewardTableEarnPartyPoints);
				break;
			case "REWARDTABLEEARNSANDDOLLARS":
				reader.Read();
				RewardTableEarnSandDollars = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadReward, RewardTableEarnSandDollars);
				break;
			case "REWARDTABLEMINIGAMESCORE":
				reader.Read();
				RewardTableMiniGameScore = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMiniGameScoreReward, RewardTableMiniGameScore);
				break;
			case "REQUIREMENTTABLEMINIGAMESCORE":
				reader.Read();
				RequirementTableMiniGameScore = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMiniGameScoreRange, RequirementTableMiniGameScore);
				break;
			case "DYNAMICCOMPONENTS":
				reader.Read();
				DynamicComponents = ReaderUtil.PopulateList(reader, converters, DynamicComponents);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
