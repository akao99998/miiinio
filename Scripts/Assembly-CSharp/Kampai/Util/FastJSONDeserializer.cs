using System.IO;
using Newtonsoft.Json;

namespace Kampai.Util
{
	public static class FastJSONDeserializer
	{
		public static T Deserialize<T>(JsonReader reader, JsonConverters converters = null) where T : IFastJSONDeserializable, new()
		{
			T result = new T();
			result.Deserialize(reader, converters);
			return result;
		}

		public static T Deserialize<T>(string json, JsonConverters converters = null) where T : IFastJSONDeserializable, new()
		{
			using (StringReader reader = new StringReader(json))
			{
				using (JsonTextReader reader2 = new JsonTextReader(reader))
				{
					T result = new T();
					result.Deserialize(reader2, converters);
					return result;
				}
			}
		}

		public static T DeserializeFromFile<T>(string path, JsonConverters converters = null) where T : IFastJSONDeserializable, new()
		{
			using (FileStream stream = File.OpenRead(path))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					using (JsonTextReader reader2 = new JsonTextReader(reader))
					{
						T result = new T();
						result.Deserialize(reader2, converters);
						return result;
					}
				}
			}
		}
	}
}
