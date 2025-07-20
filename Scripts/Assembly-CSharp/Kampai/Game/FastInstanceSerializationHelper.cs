using Newtonsoft.Json;

namespace Kampai.Game
{
	internal static class FastInstanceSerializationHelper
	{
		internal static void SerializeInstanceData(JsonWriter jsonWriter, Instance instance)
		{
			jsonWriter.WritePropertyName("ID");
			jsonWriter.WriteValue(instance.ID);
			jsonWriter.WritePropertyName("Definition");
			jsonWriter.WriteValue(instance.Definition.ID);
		}
	}
}
