using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class QuestTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1169;
			}
		}

		public int questDefinitionID { get; set; }

		public int duration { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Quest;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(questDefinitionID);
			writer.Write(duration);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			questDefinitionID = reader.ReadInt32();
			duration = reader.ReadInt32();
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
					duration = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "QUESTDEFINITIONID":
				reader.Read();
				questDefinitionID = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IQuestService instance = gameContext.injectionBinder.GetInstance<IQuestService>();
			if (instance == null)
			{
				return false;
			}
			int actualValue = ((questDefinitionID != 0) ? instance.GetIdleQuestDuration(questDefinitionID) : instance.GetLongestIdleQuestDuration());
			return TestOperator(duration, actualValue);
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, questDefinitionID: {3}, duration: {4}", GetType(), base.conditionOp, type, questDefinitionID, duration);
		}

		public override TransactionDefinition GetDynamicTriggerTransaction(ICrossContextCapable gameContext)
		{
			IQuestService instance = gameContext.injectionBinder.GetInstance<IQuestService>();
			if (instance == null)
			{
				return null;
			}
			IQuestController questController;
			if (questDefinitionID == 0)
			{
				IQuestController longestIdleQuestController = instance.GetLongestIdleQuestController();
				questController = longestIdleQuestController;
			}
			else
			{
				questController = instance.GetQuestControllerByDefinitionID(questDefinitionID);
			}
			IQuestController questController2 = questController;
			if (questController2 != null)
			{
				IList<QuantityItem> requiredQuantityItems = questController2.GetRequiredQuantityItems();
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.Inputs = new List<QuantityItem>();
				transactionDefinition.Outputs = requiredQuantityItems;
				return transactionDefinition;
			}
			return null;
		}
	}
}
