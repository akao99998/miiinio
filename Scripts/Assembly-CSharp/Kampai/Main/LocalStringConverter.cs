using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Main
{
	public class LocalStringConverter : JsonConverter
	{
		private const string SINGLE_KEY = "Single";

		private const string MULTIPLE_KEY = "Multiple";

		private const string UNDEFINED = "UNDEFINED";

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ILocalString);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken jToken = JToken.Load(reader);
			if (jToken.Type == JTokenType.String)
			{
				return new LocalString(jToken.Value<string>());
			}
			if (jToken.Type == JTokenType.Object)
			{
				string single = jToken.Value<string>("Single");
				string multiple = jToken.Value<string>("Multiple");
				return new LocalQuantityString(single, multiple);
			}
			return "UNDEFINED";
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}
	}
}
