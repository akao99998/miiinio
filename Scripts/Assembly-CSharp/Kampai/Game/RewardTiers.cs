using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class RewardTiers : IFastJSONDeserializable
	{
		public OnTheGlassDailyRewardTier Tier1 { get; set; }

		public OnTheGlassDailyRewardTier Tier2 { get; set; }

		public OnTheGlassDailyRewardTier Tier3 { get; set; }

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
			case "TIER1":
				reader.Read();
				Tier1 = FastJSONDeserializer.Deserialize<OnTheGlassDailyRewardTier>(reader, converters);
				break;
			case "TIER2":
				reader.Read();
				Tier2 = FastJSONDeserializer.Deserialize<OnTheGlassDailyRewardTier>(reader, converters);
				break;
			case "TIER3":
				reader.Read();
				Tier3 = FastJSONDeserializer.Deserialize<OnTheGlassDailyRewardTier>(reader, converters);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
