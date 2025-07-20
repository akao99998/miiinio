using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class QuestScriptInstance : IFastJSONDeserializable, IFastJSONSerializable
	{
		[JsonIgnore]
		public int QuestID { get; set; }

		[JsonIgnore]
		public int QuestStepID { get; set; }

		[JsonIgnore]
		public string QuestLocalizedKey { get; set; }

		[JsonIgnore]
		public string Key { get; set; }

		public object Deserialize(JsonReader reader, JsonConverters converters)
		{
			reader.Skip();
			return null;
		}

		public void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteEndObject();
		}
	}
}
