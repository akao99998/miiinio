using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game.Trigger
{
	public static class TriggerRewardType
	{
		public enum Identifier
		{
			Unknown = 0,
			QuantityItem = 1,
			MignetteScore = 2,
			MarketplaceSaleSlot = 3,
			MarketplaceSaleItem = 4,
			PartyPoints = 5,
			Upsell = 6,
			SocialOrder = 7,
			TSMMesssage = 8,
			CaptainTease = 9
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

		public static TriggerRewardDefinition CreateFromIdentifier(Identifier conditionType, IKampaiLogger logger)
		{
			switch (conditionType)
			{
			case Identifier.QuantityItem:
				return new QuantityItemTriggerRewardDefinition();
			case Identifier.MignetteScore:
				return new MignetteScoreTriggerRewardDefinition();
			case Identifier.MarketplaceSaleSlot:
				return new SaleSlotTriggerRewardDefinition();
			case Identifier.MarketplaceSaleItem:
				return new SaleItemTriggerRewardDefinition();
			case Identifier.PartyPoints:
				return new PartyPointsTriggerRewardDefinition();
			case Identifier.Upsell:
				return new UpsellTriggerRewardDefinition();
			case Identifier.SocialOrder:
				return new SocialOrderTriggerRewardDefinition();
			case Identifier.TSMMesssage:
				return new TSMMessageTriggerRewardDefinition();
			case Identifier.CaptainTease:
				return new CaptainTeaseTriggerRewardDefinition();
			default:
				logger.Error("Invalid Trigger Definition type: {0}", conditionType);
				return null;
			}
		}
	}
}
