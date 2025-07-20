using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game.Trigger
{
	public static class TriggerDefinitionType
	{
		public enum Identifier
		{
			Unknown = 0,
			TSM = 1,
			Upsell = 2
		}

		public static Identifier ParseIdentifier(string identifier)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				return Identifier.Unknown;
			}
			return (Identifier)(int)Enum.Parse(typeof(Identifier), identifier, true);
		}

		public static Identifier ReadFromJson(ref JsonReader reader)
		{
			Identifier result = Identifier.Unknown;
			if (reader.TokenType == JsonToken.Null)
			{
				return result;
			}
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("type") != null)
			{
				string identifier = jObject.Property("type").Value.ToString();
				result = ParseIdentifier(identifier);
			}
			reader = jObject.CreateReader();
			return result;
		}

		public static TriggerDefinition CreateDefinitionFromIdentifier(Identifier triggerType, IKampaiLogger logger)
		{
			switch (triggerType)
			{
			case Identifier.TSM:
				return new TSMTriggerDefinition();
			case Identifier.Upsell:
				return new UpsellTriggerDefinition();
			default:
				logger.Error("Invalid Trigger Definition type: {0}", triggerType);
				return null;
			}
		}
	}
}
