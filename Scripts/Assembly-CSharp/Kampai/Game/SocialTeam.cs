using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class SocialTeam : IFastJSONDeserializable
	{
		public long ID { get; set; }

		public int SocialEventId { get; set; }

		public TimedSocialEventDefinition Definition { get; protected set; }

		public IList<UserIdentity> Members { get; set; }

		public IList<SocialOrderProgress> OrderProgress { get; set; }

		public SocialTeam(TimedSocialEventDefinition def)
		{
			Definition = def;
		}

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
				ID = Convert.ToInt64(reader.Value);
				break;
			case "SOCIALEVENTID":
				reader.Read();
				SocialEventId = Convert.ToInt32(reader.Value);
				break;
			case "MEMBERS":
				reader.Read();
				Members = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadUserIdentity, Members);
				break;
			case "ORDERPROGRESS":
				reader.Read();
				OrderProgress = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadSocialOrderProgress, OrderProgress);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
