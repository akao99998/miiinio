using Newtonsoft.Json;

namespace Kampai.Util
{
	public abstract class FastJsonCreationConverter<T> : FastJsonConverter<T> where T : class, IFastJSONDeserializable
	{
		public virtual T ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return (T)null;
			}
			T val = Create();
			if (val == null)
			{
				throw new JsonSerializationException("No object created.");
			}
			val.Deserialize(reader, converters);
			return val;
		}

		public abstract T Create();
	}
}
