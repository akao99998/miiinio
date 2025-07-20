using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class QuestDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1130;
			}
		}

		public int QuestLineID { get; set; }

		public virtual QuestType type { get; set; }

		public int NarrativeOrder { get; set; }

		public bool ProgressiveGoto { get; set; }

		public bool ShowRewardsPopupByDefault { get; set; }

		public QuestSurfaceType SurfaceType { get; set; }

		public int SurfaceID { get; set; }

		public int UnlockLevel { get; set; }

		public int UnlockQuestId { get; set; }

		public int QuestPriority { get; set; }

		public int QuestVersion { get; set; }

		public string QuestBookIcon { get; set; }

		public string QuestBookMask { get; set; }

		public int QuestCompletePlayerTrainingCategoryItemId { get; set; }

		public int QuestModalClosePlayerTrainingCategoryItemId { get; set; }

		public IList<QuestStepDefinition> QuestSteps { get; set; }

		public int RewardTransaction { get; set; }

		public virtual int RewardDisplayCount { get; set; }

		public string WayFinderIcon { get; set; }

		public string QuestIntro { get; set; }

		public string QuestVoice { get; set; }

		public string QuestOutro { get; set; }

		public string QuestIntroMood { get; set; }

		public string QuestVoiceMood { get; set; }

		public string QuestOutroMood { get; set; }

		public bool ForceEnableRewardedAd2xReward { get; set; }

		public bool ForceDisableRewardedAd2xReward { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(QuestLineID);
			BinarySerializationUtil.WriteEnum(writer, type);
			writer.Write(NarrativeOrder);
			writer.Write(ProgressiveGoto);
			writer.Write(ShowRewardsPopupByDefault);
			BinarySerializationUtil.WriteEnum(writer, SurfaceType);
			writer.Write(SurfaceID);
			writer.Write(UnlockLevel);
			writer.Write(UnlockQuestId);
			writer.Write(QuestPriority);
			writer.Write(QuestVersion);
			BinarySerializationUtil.WriteString(writer, QuestBookIcon);
			BinarySerializationUtil.WriteString(writer, QuestBookMask);
			writer.Write(QuestCompletePlayerTrainingCategoryItemId);
			writer.Write(QuestModalClosePlayerTrainingCategoryItemId);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteQuestStepDefinition, QuestSteps);
			writer.Write(RewardTransaction);
			writer.Write(RewardDisplayCount);
			BinarySerializationUtil.WriteString(writer, WayFinderIcon);
			BinarySerializationUtil.WriteString(writer, QuestIntro);
			BinarySerializationUtil.WriteString(writer, QuestVoice);
			BinarySerializationUtil.WriteString(writer, QuestOutro);
			BinarySerializationUtil.WriteString(writer, QuestIntroMood);
			BinarySerializationUtil.WriteString(writer, QuestVoiceMood);
			BinarySerializationUtil.WriteString(writer, QuestOutroMood);
			writer.Write(ForceEnableRewardedAd2xReward);
			writer.Write(ForceDisableRewardedAd2xReward);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			QuestLineID = reader.ReadInt32();
			type = BinarySerializationUtil.ReadEnum<QuestType>(reader);
			NarrativeOrder = reader.ReadInt32();
			ProgressiveGoto = reader.ReadBoolean();
			ShowRewardsPopupByDefault = reader.ReadBoolean();
			SurfaceType = BinarySerializationUtil.ReadEnum<QuestSurfaceType>(reader);
			SurfaceID = reader.ReadInt32();
			UnlockLevel = reader.ReadInt32();
			UnlockQuestId = reader.ReadInt32();
			QuestPriority = reader.ReadInt32();
			QuestVersion = reader.ReadInt32();
			QuestBookIcon = BinarySerializationUtil.ReadString(reader);
			QuestBookMask = BinarySerializationUtil.ReadString(reader);
			QuestCompletePlayerTrainingCategoryItemId = reader.ReadInt32();
			QuestModalClosePlayerTrainingCategoryItemId = reader.ReadInt32();
			QuestSteps = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadQuestStepDefinition, QuestSteps);
			RewardTransaction = reader.ReadInt32();
			RewardDisplayCount = reader.ReadInt32();
			WayFinderIcon = BinarySerializationUtil.ReadString(reader);
			QuestIntro = BinarySerializationUtil.ReadString(reader);
			QuestVoice = BinarySerializationUtil.ReadString(reader);
			QuestOutro = BinarySerializationUtil.ReadString(reader);
			QuestIntroMood = BinarySerializationUtil.ReadString(reader);
			QuestVoiceMood = BinarySerializationUtil.ReadString(reader);
			QuestOutroMood = BinarySerializationUtil.ReadString(reader);
			ForceEnableRewardedAd2xReward = reader.ReadBoolean();
			ForceDisableRewardedAd2xReward = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "QUESTLINEID":
				reader.Read();
				QuestLineID = Convert.ToInt32(reader.Value);
				break;
			case "TYPE":
				reader.Read();
				type = ReaderUtil.ReadEnum<QuestType>(reader);
				break;
			case "NARRATIVEORDER":
				reader.Read();
				NarrativeOrder = Convert.ToInt32(reader.Value);
				break;
			case "PROGRESSIVEGOTO":
				reader.Read();
				ProgressiveGoto = Convert.ToBoolean(reader.Value);
				break;
			case "SHOWREWARDSPOPUPBYDEFAULT":
				reader.Read();
				ShowRewardsPopupByDefault = Convert.ToBoolean(reader.Value);
				break;
			case "SURFACETYPE":
				reader.Read();
				SurfaceType = ReaderUtil.ReadEnum<QuestSurfaceType>(reader);
				break;
			case "SURFACEID":
				reader.Read();
				SurfaceID = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKLEVEL":
				reader.Read();
				UnlockLevel = Convert.ToInt32(reader.Value);
				break;
			case "UNLOCKQUESTID":
				reader.Read();
				UnlockQuestId = Convert.ToInt32(reader.Value);
				break;
			case "QUESTPRIORITY":
				reader.Read();
				QuestPriority = Convert.ToInt32(reader.Value);
				break;
			case "QUESTVERSION":
				reader.Read();
				QuestVersion = Convert.ToInt32(reader.Value);
				break;
			case "QUESTBOOKICON":
				reader.Read();
				QuestBookIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTBOOKMASK":
				reader.Read();
				QuestBookMask = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTCOMPLETEPLAYERTRAININGCATEGORYITEMID":
				reader.Read();
				QuestCompletePlayerTrainingCategoryItemId = Convert.ToInt32(reader.Value);
				break;
			case "QUESTMODALCLOSEPLAYERTRAININGCATEGORYITEMID":
				reader.Read();
				QuestModalClosePlayerTrainingCategoryItemId = Convert.ToInt32(reader.Value);
				break;
			case "QUESTSTEPS":
				reader.Read();
				QuestSteps = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadQuestStepDefinition, QuestSteps);
				break;
			case "REWARDTRANSACTION":
				reader.Read();
				RewardTransaction = Convert.ToInt32(reader.Value);
				break;
			case "REWARDDISPLAYCOUNT":
				reader.Read();
				RewardDisplayCount = Convert.ToInt32(reader.Value);
				break;
			case "WAYFINDERICON":
				reader.Read();
				WayFinderIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTINTRO":
				reader.Read();
				QuestIntro = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTVOICE":
				reader.Read();
				QuestVoice = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTOUTRO":
				reader.Read();
				QuestOutro = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTINTROMOOD":
				reader.Read();
				QuestIntroMood = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTVOICEMOOD":
				reader.Read();
				QuestVoiceMood = ReaderUtil.ReadString(reader, converters);
				break;
			case "QUESTOUTROMOOD":
				reader.Read();
				QuestOutroMood = ReaderUtil.ReadString(reader, converters);
				break;
			case "FORCEENABLEREWARDEDAD2XREWARD":
				reader.Read();
				ForceEnableRewardedAd2xReward = Convert.ToBoolean(reader.Value);
				break;
			case "FORCEDISABLEREWARDEDAD2XREWARD":
				reader.Read();
				ForceDisableRewardedAd2xReward = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual TransactionDefinition GetReward(IDefinitionService definitionService)
		{
			if (RewardTransaction == 0 || definitionService == null)
			{
				return null;
			}
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(RewardTransaction);
			if (RewardDisplayCount == 0)
			{
				RewardDisplayCount = transactionDefinition.GetOutputCount();
			}
			return transactionDefinition;
		}

		public static string GetProceduralQuestIcon(QuestStepType type)
		{
			switch (type)
			{
			case QuestStepType.Delivery:
				return "tempCharQuestIcon";
			case QuestStepType.OrderBoard:
				return "tempCharQuestIcon";
			case QuestStepType.MinionTask:
				return "tempCharQuestIcon";
			case QuestStepType.Mignette:
				return "tempCharQuestIcon";
			default:
				return string.Empty;
			}
		}

		public static string GetProceduralQuestDescription(QuestStepType type)
		{
			switch (type)
			{
			case QuestStepType.Delivery:
				return "deliveryTaskName";
			case QuestStepType.OrderBoard:
				return "orderBoardTaskName";
			case QuestStepType.MinionTask:
				return "minionTaskName";
			case QuestStepType.Mignette:
				return "mignetteTaskName";
			default:
				return string.Empty;
			}
		}

		public virtual Instance Build()
		{
			return new Quest(this);
		}
	}
}
