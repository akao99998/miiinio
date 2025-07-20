using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class QuestDefinitionConverter : CustomCreationConverter<QuestDefinition>
	{
		private QuestType questType;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			questType = QuestType.Default;
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				if (jObject.Property("type") != null)
				{
					string value = jObject.Property("type").Value.ToString();
					questType = (QuestType)(int)Enum.Parse(typeof(QuestType), value);
				}
				else if (jObject.Property("serverStartTimeUTC") != null && jObject.Property("serverStopTimeUTC") != null)
				{
					questType = QuestType.LimitedQuest;
				}
				reader = jObject.CreateReader();
			}
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override QuestDefinition Create(Type objectType)
		{
			switch (questType)
			{
			case QuestType.Default:
				return new QuestDefinition();
			case QuestType.TimedQuest:
				return new TimedQuestDefinition();
			case QuestType.LimitedQuest:
				return new LimitedQuestDefinition();
			case QuestType.DynamicQuest:
				return new DynamicQuestDefinition();
			default:
				return null;
			}
		}
	}
}
