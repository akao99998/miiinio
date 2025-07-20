using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class FeatureAccess : IFastJSONDeserializable
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int accessPercentage;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public IList<string> userIdWhitelist;

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
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					userIdWhitelist = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, userIdWhitelist);
					break;
				}
				return false;
			}
			case "ACCESSPERCENTAGE":
				reader.Read();
				accessPercentage = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}
	}
}
