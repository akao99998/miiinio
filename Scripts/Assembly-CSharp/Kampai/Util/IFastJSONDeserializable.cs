using Newtonsoft.Json;

namespace Kampai.Util
{
	public interface IFastJSONDeserializable
	{
		object Deserialize(JsonReader reader, JsonConverters converters = null);
	}
}
