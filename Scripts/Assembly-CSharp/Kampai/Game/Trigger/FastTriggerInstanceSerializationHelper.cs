using Newtonsoft.Json;

namespace Kampai.Game.Trigger
{
	internal static class FastTriggerInstanceSerializationHelper
	{
		internal static void SerializeTriggerInstanceData(JsonWriter jsonWriter, TriggerInstance instance)
		{
			jsonWriter.WritePropertyName("Definition");
			jsonWriter.WriteValue(instance.Definition.ID);
			jsonWriter.WritePropertyName("StartGameTime");
			jsonWriter.WriteValue(instance.StartGameTime);
			jsonWriter.WritePropertyName("RecievedRewardIds");
			jsonWriter.WriteStartArray();
			for (int i = 0; i < instance.RecievedRewardIds.Count; i++)
			{
				jsonWriter.WriteValue(instance.RecievedRewardIds[i]);
			}
			jsonWriter.WriteEndArray();
		}
	}
}
