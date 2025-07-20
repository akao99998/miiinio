using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ConfigurationDefinitionsDictionary : IFastJSONDeserializable
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, ConfigurationDefinition> allConfigs { get; set; }

		public virtual object Deserialize(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string propertyName = ((string)reader.Value).ToUpper();
					if (!DeserializeProperty(propertyName, reader, converters))
					{
						reader.Skip();
					}
					break;
				}
				case JsonToken.EndObject:
					return this;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, ReaderUtil.GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		protected virtual bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "ALLCONFIGS":
				reader.Read();
				allConfigs = ReaderUtil.ReadDictionary<ConfigurationDefinition>(reader, converters);
				return true;
			default:
				return false;
			}
		}
	}
}
