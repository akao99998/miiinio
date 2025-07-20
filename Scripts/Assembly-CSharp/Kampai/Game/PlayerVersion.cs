using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class PlayerVersion : IFastJSONDeserializable
	{
		private const int currentVersion = 15;

		private IPlayerSerializer CurrentSerializer = new PlayerSerializerV15();

		public int Version { get; set; }

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
			case "VERSION":
				reader.Read();
				Version = Convert.ToInt32(reader.Value);
				return true;
			default:
				return false;
			}
		}

		public Player CreatePlayer(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = CurrentSerializer.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version != 15)
			{
				throw new FatalException(FatalCode.PS_UPGRADE_FAILED, "Upgrade failed");
			}
			return player;
		}

		public byte[] Serialize(Player player, IDefinitionService definitionService, IKampaiLogger logger)
		{
			return CurrentSerializer.Serialize(player, definitionService, logger);
		}
	}
}
