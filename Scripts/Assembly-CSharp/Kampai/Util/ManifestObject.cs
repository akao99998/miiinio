using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kampai.Util
{
	public class ManifestObject : IFastJSONDeserializable
	{
		public string id { get; set; }

		public string baseURL { get; set; }

		public List<BundleInfo> bundles { get; set; }

		[Deserializer("ReaderUtil.ReadStringDictionary")]
		public Dictionary<string, string> assets { get; set; }

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
			case "ID":
				reader.Read();
				id = ReaderUtil.ReadString(reader, converters);
				break;
			case "BASEURL":
				reader.Read();
				baseURL = ReaderUtil.ReadString(reader, converters);
				break;
			case "BUNDLES":
				reader.Read();
				bundles = ReaderUtil.PopulateList(reader, converters, bundles);
				break;
			case "ASSETS":
				reader.Read();
				assets = ReaderUtil.ReadStringDictionary(reader, converters);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
