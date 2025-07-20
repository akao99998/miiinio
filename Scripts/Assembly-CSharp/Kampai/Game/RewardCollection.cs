using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class RewardCollection : Instance<RewardCollectionDefinition>
	{
		public int CollectionScoreProgress { get; set; }

		public int CollectionScorePreReset { get; set; }

		public int NumRewardsCollected { get; set; }

		public int NumTimesPlayed { get; set; }

		public RewardCollection(RewardCollectionDefinition definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "COLLECTIONSCOREPROGRESS":
				reader.Read();
				CollectionScoreProgress = Convert.ToInt32(reader.Value);
				break;
			case "COLLECTIONSCOREPRERESET":
				reader.Read();
				CollectionScorePreReset = Convert.ToInt32(reader.Value);
				break;
			case "NUMREWARDSCOLLECTED":
				reader.Read();
				NumRewardsCollected = Convert.ToInt32(reader.Value);
				break;
			case "NUMTIMESPLAYED":
				reader.Read();
				NumTimesPlayed = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			writer.WritePropertyName("CollectionScoreProgress");
			writer.WriteValue(CollectionScoreProgress);
			writer.WritePropertyName("CollectionScorePreReset");
			writer.WriteValue(CollectionScorePreReset);
			writer.WritePropertyName("NumRewardsCollected");
			writer.WriteValue(NumRewardsCollected);
			writer.WritePropertyName("NumTimesPlayed");
			writer.WriteValue(NumTimesPlayed);
		}

		public void IncreaseScore(int scoreIncrease)
		{
			CollectionScoreProgress += scoreIncrease;
		}

		public int GetMaxScore()
		{
			int num = 0;
			for (int i = 0; i < base.Definition.Rewards.Count; i++)
			{
				num = Mathf.Max(num, base.Definition.Rewards[i].RequiredPoints);
			}
			return num;
		}

		public int GetPointTotalForNextReward()
		{
			int num = GetMaxScore();
			for (int i = 0; i < base.Definition.Rewards.Count; i++)
			{
				if (CollectionScoreProgress < base.Definition.Rewards[i].RequiredPoints)
				{
					num = Mathf.Min(num, base.Definition.Rewards[i].RequiredPoints);
				}
			}
			return num;
		}

		public bool HasRewardReadyForCollection()
		{
			return GetTransactionIDReadyForCollection() != -1;
		}

		public bool IsRewardReadyForCollection(CollectionReward reward)
		{
			int num = base.Definition.Rewards.IndexOf(reward);
			return reward.RequiredPoints <= CollectionScoreProgress && NumRewardsCollected <= num;
		}

		public int GetTransactionIDReadyForCollection()
		{
			for (int i = 0; i < base.Definition.Rewards.Count; i++)
			{
				CollectionReward collectionReward = base.Definition.Rewards[i];
				if (collectionReward.RequiredPoints <= CollectionScoreProgress && NumRewardsCollected <= i)
				{
					return collectionReward.TransactionID;
				}
			}
			return -1;
		}

		public bool IsCompleted()
		{
			return NumRewardsCollected >= base.Definition.Rewards.Count;
		}

		public void ResetProgress()
		{
			CollectionScoreProgress = 0;
			NumRewardsCollected = 0;
		}
	}
}
