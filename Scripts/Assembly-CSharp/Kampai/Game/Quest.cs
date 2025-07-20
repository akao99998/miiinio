using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Quest : Instance<QuestDefinition>, IGameTimeTracker, IComparable<Quest>
	{
		public Dictionary<string, QuestScriptInstance> questScriptInstances = new Dictionary<string, QuestScriptInstance>();

		public virtual IList<QuestStep> Steps { get; set; }

		public int UTCQuestStartTime { get; set; }

		public int QuestIconTrackedInstanceId { get; set; }

		public int QuestVersion { get; set; }

		public int StartGameTime { get; set; }

		public virtual QuestState state { get; set; }

		[JsonIgnore]
		public virtual bool AutoGrantReward { get; set; }

		public DynamicQuestDefinition dynamicDefinition { get; set; }

		public Quest(QuestDefinition def)
			: base(def)
		{
			CheckDynamicDefinition(def);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STEPS":
				reader.Read();
				Steps = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadQuestStep, Steps);
				break;
			case "UTCQUESTSTARTTIME":
				reader.Read();
				UTCQuestStartTime = Convert.ToInt32(reader.Value);
				break;
			case "QUESTICONTRACKEDINSTANCEID":
				reader.Read();
				QuestIconTrackedInstanceId = Convert.ToInt32(reader.Value);
				break;
			case "QUESTVERSION":
				reader.Read();
				QuestVersion = Convert.ToInt32(reader.Value);
				break;
			case "STARTGAMETIME":
				reader.Read();
				StartGameTime = Convert.ToInt32(reader.Value);
				break;
			case "STATE":
				reader.Read();
				state = ReaderUtil.ReadEnum<QuestState>(reader);
				break;
			case "DYNAMICDEFINITION":
				reader.Read();
				dynamicDefinition = ((converters.questDefinitionConverter == null) ? FastJSONDeserializer.Deserialize<DynamicQuestDefinition>(reader, converters) : ((DynamicQuestDefinition)converters.questDefinitionConverter.ReadJson(reader, converters)));
				break;
			case "QUESTSCRIPTINSTANCES":
				reader.Read();
				questScriptInstances = ReaderUtil.ReadDictionary<QuestScriptInstance>(reader, converters);
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
			if (Steps != null)
			{
				writer.WritePropertyName("Steps");
				writer.WriteStartArray();
				IEnumerator<QuestStep> enumerator = Steps.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						QuestStep current = enumerator.Current;
						writer.WriteStartObject();
						writer.WritePropertyName("state");
						writer.WriteValue((int)current.state);
						writer.WritePropertyName("AmountCompleted");
						writer.WriteValue(current.AmountCompleted);
						writer.WritePropertyName("AmountReady");
						writer.WriteValue(current.AmountReady);
						writer.WritePropertyName("TrackedID");
						writer.WriteValue(current.TrackedID);
						writer.WriteEndObject();
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				writer.WriteEndArray();
			}
			writer.WritePropertyName("UTCQuestStartTime");
			writer.WriteValue(UTCQuestStartTime);
			writer.WritePropertyName("QuestIconTrackedInstanceId");
			writer.WriteValue(QuestIconTrackedInstanceId);
			writer.WritePropertyName("QuestVersion");
			writer.WriteValue(QuestVersion);
			writer.WritePropertyName("StartGameTime");
			writer.WriteValue(StartGameTime);
			writer.WritePropertyName("state");
			writer.WriteValue((int)state);
			if (dynamicDefinition != null)
			{
				writer.WritePropertyName("dynamicDefinition");
				writer.WriteStartObject();
				if (dynamicDefinition.LocalizedKey != null)
				{
					writer.WritePropertyName("LocalizedKey");
					writer.WriteValue(dynamicDefinition.LocalizedKey);
				}
				writer.WritePropertyName("ID");
				writer.WriteValue(dynamicDefinition.ID);
				writer.WritePropertyName("Disabled");
				writer.WriteValue(dynamicDefinition.Disabled);
				writer.WritePropertyName("QuestLineID");
				writer.WriteValue(dynamicDefinition.QuestLineID);
				writer.WritePropertyName("type");
				writer.WriteValue((int)dynamicDefinition.type);
				writer.WritePropertyName("NarrativeOrder");
				writer.WriteValue(dynamicDefinition.NarrativeOrder);
				writer.WritePropertyName("ProgressiveGoto");
				writer.WriteValue(dynamicDefinition.ProgressiveGoto);
				writer.WritePropertyName("ShowRewardsPopupByDefault");
				writer.WriteValue(dynamicDefinition.ShowRewardsPopupByDefault);
				writer.WritePropertyName("SurfaceType");
				writer.WriteValue((int)dynamicDefinition.SurfaceType);
				writer.WritePropertyName("SurfaceID");
				writer.WriteValue(dynamicDefinition.SurfaceID);
				writer.WritePropertyName("UnlockLevel");
				writer.WriteValue(dynamicDefinition.UnlockLevel);
				writer.WritePropertyName("UnlockQuestId");
				writer.WriteValue(dynamicDefinition.UnlockQuestId);
				writer.WritePropertyName("QuestPriority");
				writer.WriteValue(dynamicDefinition.QuestPriority);
				writer.WritePropertyName("QuestVersion");
				writer.WriteValue(dynamicDefinition.QuestVersion);
				if (dynamicDefinition.QuestBookIcon != null)
				{
					writer.WritePropertyName("QuestBookIcon");
					writer.WriteValue(dynamicDefinition.QuestBookIcon);
				}
				if (dynamicDefinition.QuestBookMask != null)
				{
					writer.WritePropertyName("QuestBookMask");
					writer.WriteValue(dynamicDefinition.QuestBookMask);
				}
				writer.WritePropertyName("QuestCompletePlayerTrainingCategoryItemId");
				writer.WriteValue(dynamicDefinition.QuestCompletePlayerTrainingCategoryItemId);
				writer.WritePropertyName("QuestModalClosePlayerTrainingCategoryItemId");
				writer.WriteValue(dynamicDefinition.QuestModalClosePlayerTrainingCategoryItemId);
				if (dynamicDefinition.QuestSteps != null)
				{
					writer.WritePropertyName("QuestSteps");
					writer.WriteStartArray();
					IEnumerator<QuestStepDefinition> enumerator2 = dynamicDefinition.QuestSteps.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							QuestStepDefinition current2 = enumerator2.Current;
							writer.WriteStartObject();
							writer.WritePropertyName("Type");
							writer.WriteValue((int)current2.Type);
							writer.WritePropertyName("ItemAmount");
							writer.WriteValue(current2.ItemAmount);
							writer.WritePropertyName("ItemDefinitionID");
							writer.WriteValue(current2.ItemDefinitionID);
							writer.WritePropertyName("CostumeDefinitionID");
							writer.WriteValue(current2.CostumeDefinitionID);
							writer.WritePropertyName("ShowWayfinder");
							writer.WriteValue(current2.ShowWayfinder);
							writer.WritePropertyName("QuestStepCompletePlayerTrainingCategoryItemId");
							writer.WriteValue(current2.QuestStepCompletePlayerTrainingCategoryItemId);
							writer.WritePropertyName("UpgradeLevel");
							writer.WriteValue(current2.UpgradeLevel);
							writer.WriteEndObject();
						}
					}
					finally
					{
						enumerator2.Dispose();
					}
					writer.WriteEndArray();
				}
				writer.WritePropertyName("RewardTransaction");
				writer.WriteValue(dynamicDefinition.RewardTransaction);
				writer.WritePropertyName("RewardDisplayCount");
				writer.WriteValue(dynamicDefinition.RewardDisplayCount);
				if (dynamicDefinition.WayFinderIcon != null)
				{
					writer.WritePropertyName("WayFinderIcon");
					writer.WriteValue(dynamicDefinition.WayFinderIcon);
				}
				if (dynamicDefinition.QuestIntro != null)
				{
					writer.WritePropertyName("QuestIntro");
					writer.WriteValue(dynamicDefinition.QuestIntro);
				}
				if (dynamicDefinition.QuestVoice != null)
				{
					writer.WritePropertyName("QuestVoice");
					writer.WriteValue(dynamicDefinition.QuestVoice);
				}
				if (dynamicDefinition.QuestOutro != null)
				{
					writer.WritePropertyName("QuestOutro");
					writer.WriteValue(dynamicDefinition.QuestOutro);
				}
				if (dynamicDefinition.QuestIntroMood != null)
				{
					writer.WritePropertyName("QuestIntroMood");
					writer.WriteValue(dynamicDefinition.QuestIntroMood);
				}
				if (dynamicDefinition.QuestVoiceMood != null)
				{
					writer.WritePropertyName("QuestVoiceMood");
					writer.WriteValue(dynamicDefinition.QuestVoiceMood);
				}
				if (dynamicDefinition.QuestOutroMood != null)
				{
					writer.WritePropertyName("QuestOutroMood");
					writer.WriteValue(dynamicDefinition.QuestOutroMood);
				}
				writer.WritePropertyName("ForceEnableRewardedAd2xReward");
				writer.WriteValue(dynamicDefinition.ForceEnableRewardedAd2xReward);
				writer.WritePropertyName("ForceDisableRewardedAd2xReward");
				writer.WriteValue(dynamicDefinition.ForceDisableRewardedAd2xReward);
				if (dynamicDefinition.RewardTransactionInstance != null)
				{
					writer.WritePropertyName("RewardTransactionInstance");
					writer.WriteStartObject();
					writer.WritePropertyName("ID");
					writer.WriteValue(dynamicDefinition.RewardTransactionInstance.ID);
					if (dynamicDefinition.RewardTransactionInstance.Inputs != null)
					{
						writer.WritePropertyName("Inputs");
						writer.WriteStartArray();
						IEnumerator<QuantityItem> enumerator3 = dynamicDefinition.RewardTransactionInstance.Inputs.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								QuantityItem current3 = enumerator3.Current;
								writer.WriteStartObject();
								writer.WritePropertyName("ID");
								writer.WriteValue(current3.ID);
								writer.WritePropertyName("Quantity");
								writer.WriteValue(current3.Quantity);
								writer.WriteEndObject();
							}
						}
						finally
						{
							enumerator3.Dispose();
						}
						writer.WriteEndArray();
					}
					if (dynamicDefinition.RewardTransactionInstance.Outputs != null)
					{
						writer.WritePropertyName("Outputs");
						writer.WriteStartArray();
						IEnumerator<QuantityItem> enumerator4 = dynamicDefinition.RewardTransactionInstance.Outputs.GetEnumerator();
						try
						{
							while (enumerator4.MoveNext())
							{
								QuantityItem current4 = enumerator4.Current;
								writer.WriteStartObject();
								writer.WritePropertyName("ID");
								writer.WriteValue(current4.ID);
								writer.WritePropertyName("Quantity");
								writer.WriteValue(current4.Quantity);
								writer.WriteEndObject();
							}
						}
						finally
						{
							enumerator4.Dispose();
						}
						writer.WriteEndArray();
					}
					writer.WriteEndObject();
				}
				writer.WritePropertyName("DropStep");
				writer.WriteValue(dynamicDefinition.DropStep);
				writer.WriteEndObject();
			}
			if (questScriptInstances == null)
			{
				return;
			}
			writer.WritePropertyName("questScriptInstances");
			writer.WriteStartObject();
			Dictionary<string, QuestScriptInstance>.Enumerator enumerator5 = questScriptInstances.GetEnumerator();
			try
			{
				while (enumerator5.MoveNext())
				{
					KeyValuePair<string, QuestScriptInstance> current5 = enumerator5.Current;
					writer.WritePropertyName(Convert.ToString(current5.Key));
					current5.Value.Serialize(writer);
				}
			}
			finally
			{
				enumerator5.Dispose();
			}
			writer.WriteEndObject();
		}

		private void CheckDynamicDefinition(QuestDefinition def)
		{
			DynamicQuestDefinition dynamicQuestDefinition = def as DynamicQuestDefinition;
			if (dynamicQuestDefinition != null)
			{
				base.Definition = new QuestDefinition();
				base.Definition.ID = 77777;
				dynamicDefinition = dynamicQuestDefinition;
				dynamicDefinition.ID = base.Definition.ID;
			}
		}

		public new void OnDefinitionHotSwap(Definition definition)
		{
			base.OnDefinitionHotSwap(definition);
			CheckDynamicDefinition(definition as QuestDefinition);
		}

		public void Initialize()
		{
			if (Steps != null)
			{
				return;
			}
			QuestDefinition activeDefinition = GetActiveDefinition();
			if (activeDefinition.QuestSteps != null)
			{
				Steps = new List<QuestStep>(activeDefinition.QuestSteps.Count);
				for (int i = 0; i < activeDefinition.QuestSteps.Count; i++)
				{
					QuestStep item = new QuestStep();
					Steps.Add(item);
				}
			}
			else
			{
				Steps = new List<QuestStep>();
			}
			QuestVersion = GetActiveDefinition().QuestVersion;
		}

		public void Clear()
		{
			state = QuestState.Notstarted;
			UTCQuestStartTime = 0;
			IList<QuestStep> steps = Steps;
			if (steps != null)
			{
				steps.Clear();
				QuestDefinition activeDefinition = GetActiveDefinition();
				if (activeDefinition.QuestSteps != null)
				{
					for (int i = 0; i < activeDefinition.QuestSteps.Count; i++)
					{
						QuestStep item = new QuestStep();
						steps.Add(item);
					}
				}
			}
			if (questScriptInstances != null)
			{
				questScriptInstances.Clear();
			}
		}

		public QuestDefinition GetActiveDefinition()
		{
			if (dynamicDefinition != null)
			{
				if (dynamicDefinition.ID == 0)
				{
					dynamicDefinition.ID = base.Definition.ID;
				}
				return dynamicDefinition;
			}
			return base.Definition;
		}

		public bool IsDynamic()
		{
			return dynamicDefinition != null;
		}

		public bool IsProcedurallyGenerated()
		{
			return GetActiveDefinition().SurfaceType == QuestSurfaceType.ProcedurallyGenerated;
		}

		public int CompareTo(Quest other)
		{
			QuestDefinition activeDefinition = GetActiveDefinition();
			QuestDefinition activeDefinition2 = other.GetActiveDefinition();
			if (activeDefinition.SurfaceID == activeDefinition2.SurfaceID)
			{
				return activeDefinition2.QuestPriority.CompareTo(activeDefinition.QuestPriority);
			}
			return activeDefinition.SurfaceID.CompareTo(activeDefinition2.SurfaceID);
		}
	}
}
