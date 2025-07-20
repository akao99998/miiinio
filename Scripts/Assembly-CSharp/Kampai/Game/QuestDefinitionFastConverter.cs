using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class QuestDefinitionFastConverter : FastJsonCreationConverter<QuestDefinition>
	{
		private QuestType questType;

		public override QuestDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			questType = QuestType.Default;
			if (reader.TokenType != JsonToken.Null)
			{
				JObject jObject = JObject.Load(reader);
				JProperty jProperty = jObject.Property("type");
				if (jProperty != null)
				{
					string value = jProperty.Value.ToString();
					questType = (QuestType)(int)Enum.Parse(typeof(QuestType), value);
				}
				else if (jObject.Property("serverStartTimeUTC") != null && jObject.Property("serverStopTimeUTC") != null)
				{
					questType = QuestType.LimitedQuest;
				}
				reader = jObject.CreateReader();
			}
			return base.ReadJson(reader, converters);
		}

		public override QuestDefinition Create()
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
				throw new JsonSerializationException(string.Format("Unexpected QuestDefinition type: {0}", questType));
			}
		}
	}
}
