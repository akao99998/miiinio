using System;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class OnTheGlassDailyRewardTier : IFastJSONDeserializable
	{
		public int Weight { get; set; }

		public WeightedDefinition PredefinedRewards { get; set; }

		public int CraftableRewardMinTier { get; set; }

		public int CraftableRewardMaxQuantity { get; set; }

		public int CraftableRewardWeight { get; set; }

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
			case "WEIGHT":
				reader.Read();
				Weight = Convert.ToInt32(reader.Value);
				break;
			case "PREDEFINEDREWARDS":
				reader.Read();
				PredefinedRewards = FastJSONDeserializer.Deserialize<WeightedDefinition>(reader, converters);
				break;
			case "CRAFTABLEREWARDMINTIER":
				reader.Read();
				CraftableRewardMinTier = Convert.ToInt32(reader.Value);
				break;
			case "CRAFTABLEREWARDMAXQUANTITY":
				reader.Read();
				CraftableRewardMaxQuantity = Convert.ToInt32(reader.Value);
				break;
			case "CRAFTABLEREWARDWEIGHT":
				reader.Read();
				CraftableRewardWeight = Convert.ToInt32(reader.Value);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
