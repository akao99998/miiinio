using Newtonsoft.Json;

namespace Kampai.Util
{
	public interface FastJsonConverter<T> where T : class, IFastJSONDeserializable
	{
		T ReadJson(JsonReader reader, JsonConverters converters);

		T Create();
	}
}
