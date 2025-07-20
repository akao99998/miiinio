using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game.Trigger
{
	public static class TriggerConditionType
	{
		public enum Identifier
		{
			Unknown = 0,
			QuantityItem = 1,
			Storage = 2,
			MarketplaceSaleSlot = 3,
			MarketplaceSaleItem = 4,
			Quest = 5,
			Purchase = 6,
			OrderBoard = 7,
			Prestige = 8,
			MignetteScore = 9,
			LandExpansion = 10,
			SocialOrder = 11,
			SocialTime = 12,
			Platform = 13,
			Segment = 14,
			HelpButton = 15,
			Country = 16,
			Churn = 17,
			AvailableLand = 18,
			PrestigeState = 19,
			PrestigeLevel = 20,
			HoursPlayed = 21,
			DaysSinceInstall = 22,
			SessionCount = 23,
			ConsecutiveDays = 24
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

		public static TriggerConditionDefinition CreateFromIdentifier(Identifier conditionType, IKampaiLogger logger)
		{
			switch (conditionType)
			{
			case Identifier.QuantityItem:
				return new QuantityItemTriggerConditionDefinition();
			case Identifier.Storage:
				return new StorageTriggerConditionDefinition();
			case Identifier.MarketplaceSaleSlot:
				return new SaleSlotTriggerConditionDefinition();
			case Identifier.MarketplaceSaleItem:
				return new SaleItemTriggerConditionDefinition();
			case Identifier.Quest:
				return new QuestTriggerConditionDefinition();
			case Identifier.Purchase:
				return new PurchaseTriggerConditionDefinition();
			case Identifier.OrderBoard:
				return new OrderBoardTriggerConditionDefinition();
			case Identifier.Prestige:
				return new PrestigeTriggerConditionDefinition();
			case Identifier.SocialOrder:
				return new SocialOrderTriggerConditionDefinition();
			case Identifier.SocialTime:
				return new SocialTimeTriggerConditionDefinition();
			case Identifier.Platform:
				return new PlatformTriggerConditionDefinition();
			case Identifier.Segment:
				return new SegmentTriggerConditionDefinition();
			case Identifier.Country:
				return new CountryTriggerConditionDefinition();
			case Identifier.MignetteScore:
				return new MignetteScoreTriggerConditionDefinition();
			case Identifier.LandExpansion:
				return new LandExpansionTriggerConditionDefinition();
			case Identifier.HelpButton:
				return new HelpButtonTriggerConditionDefinition();
			case Identifier.Churn:
				return new ChurnTriggerConditionDefinition();
			case Identifier.AvailableLand:
				return new AvailableLandTriggerConditionDefinition();
			case Identifier.PrestigeState:
				return new PrestigeStateTriggerConditionDefinition();
			case Identifier.PrestigeLevel:
				return new PrestigeLevelTriggerConditionDefinition();
			case Identifier.HoursPlayed:
				return new HoursPlayedTriggerConditionDefinition();
			case Identifier.DaysSinceInstall:
				return new DaysSinceInstallTriggerConditionDefinition();
			case Identifier.SessionCount:
				return new SessionCountTriggerConditionDefinition();
			case Identifier.ConsecutiveDays:
				return new ConsecutiveDaysConditionDefinition();
			default:
				logger.Error("Invalid Trigger Definition type: {0}", conditionType);
				return null;
			}
		}
	}
}
