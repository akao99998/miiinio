using System.Collections.Generic;
using System.Text;
using Elevation.Logging;
using Kampai.Common.Service.Telemetry;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Common
{
	public class TelemetryService : IIapTelemetryService, ITelemetryService
	{
		private const string EVT_KEYTYPE_ENUMERATION = "EVT_KEYTYPE_ENUMERATION";

		private const string EVT_KEYTYPE_SCORE = "EVT_KEYTYPE_SCORE";

		private const string EVT_KEYTYPE_DURATION = "EVT_KEYTYPE_DURATION";

		private const string EVT_KEYTYPE_NONE = "EVT_KEYTYPE_NONE";

		private IPlayerService playerService;

		private IPlayerDurationService playerDurationService;

		private IDefinitionService definitionService;

		public IKampaiLogger logger = LogManager.GetClassLogger("TelemetryService") as IKampaiLogger;

		private List<ITelemetrySender> telemetrySenders = new List<ITelemetrySender>();

		private List<IIapTelemetryService> iapTelemetryServices = new List<IIapTelemetryService>();

		private int gameStartTime;

		private float lastFunnelEalTime;

		private float lastGameLoadFunnelTime;

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		public void AddTelemetrySender(ITelemetrySender sender)
		{
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				if (object.ReferenceEquals(telemetrySender, sender))
				{
					return;
				}
			}
			telemetrySenders.Add(sender);
		}

		public void AddIapTelemetryService(IIapTelemetryService service)
		{
			foreach (IIapTelemetryService iapTelemetryService in iapTelemetryServices)
			{
				if (object.ReferenceEquals(iapTelemetryService, service))
				{
					return;
				}
			}
			iapTelemetryServices.Add(service);
		}

		public void SharingUsage(ITelemetrySender sender, bool enabled)
		{
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				if (object.ReferenceEquals(telemetrySender.GetType(), sender.GetType()))
				{
					telemetrySender.SharingUsage(SharingUsageEnabled() && enabled);
					break;
				}
			}
		}

		public void GameStarted()
		{
			gameStartTime = timeService.Uptime();
		}

		public int SecondsSinceGameStart()
		{
			return timeService.Uptime() - gameStartTime;
		}

		public virtual void LogGameEvent(TelemetryEvent gameEvent)
		{
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				telemetrySender.SendEvent(gameEvent);
			}
		}

		public virtual void COPPACompliance()
		{
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				telemetrySender.COPPACompliance();
			}
		}

		public void SharingUsageCompliance()
		{
			bool enabled = SharingUsageEnabled();
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				telemetrySender.SharingUsage(enabled);
			}
		}

		public void SharingUsage(bool enabled)
		{
			localPersistService.PutDataIntPlayer("SharingUsage", enabled ? 1 : 0);
			foreach (ITelemetrySender telemetrySender in telemetrySenders)
			{
				telemetrySender.SharingUsage(enabled);
			}
		}

		public bool SharingUsageEnabled()
		{
			if (localPersistService.HasKeyPlayer("SharingUsage"))
			{
				return localPersistService.GetDataIntPlayer("SharingUsage") != 0;
			}
			return true;
		}

		public IList<TelemetryParameter> GetLevelGrindPremium()
		{
			IList<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(getPlayerItem(StaticItem.LEVEL_ID, ParameterName.LEVEL));
			list.Add(getPlayerItem(StaticItem.GRIND_CURRENCY_ID, ParameterName.GRIND_CURRENCY_BALANCE));
			list.Add(getPlayerItem(StaticItem.PREMIUM_CURRENCY_ID, ParameterName.PREMIUM_CURRENCY_BALANCE));
			return list;
		}

		public TelemetryParameter getPlayerItem(StaticItem item, ParameterName name)
		{
			object value = getPlayerItemValue(item);
			return new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, name);
		}

		private uint getPlayerItemValue(StaticItem item)
		{
			return (playerService != null && playerService.IsPlayerInitialized()) ? playerService.GetQuantity(item) : 0u;
		}

		public IList<TelemetryParameter> GetTaxonomyParameters(TaxonomyDefinition taxonomyDefinition)
		{
			IList<TelemetryParameter> list = new List<TelemetryParameter>();
			string value = string.Empty;
			string value2 = string.Empty;
			string value3 = string.Empty;
			if (taxonomyDefinition != null)
			{
				value = taxonomyDefinition.TaxonomyHighLevel;
				value2 = taxonomyDefinition.TaxonomySpecific;
				value3 = taxonomyDefinition.TaxonomyType;
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.TAXONOMY_HIGH_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value2, ParameterName.TAXONOMY_SPECIFIC));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value3, ParameterName.TAXONOMY_TYPE));
			return list;
		}

		public void GetTaxonomyParameters(TaxonomyDefinition taxonomyDefinition, out string highLevel, out string specific, out string type)
		{
			highLevel = string.Empty;
			specific = string.Empty;
			type = string.Empty;
			if (taxonomyDefinition != null)
			{
				highLevel = taxonomyDefinition.TaxonomyHighLevel;
				specific = taxonomyDefinition.TaxonomySpecific;
				type = taxonomyDefinition.TaxonomyType;
			}
		}

		private string GetDefinitionLocalizedKey(Definition definition)
		{
			return (!string.IsNullOrEmpty(definition.LocalizedKey)) ? definition.LocalizedKey : definition.ID.ToString();
		}

		public void Send_Telemetry_EVT_GAME_ERROR_GAMEPLAY(string nameOfError, string errorDetails, bool userFacing)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GAME_ERROR_GAMEPLAY);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", nameOfError, ParameterName.ERROR_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", errorDetails, ParameterName.ERROR_DETAILS));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", getUserFacingString(userFacing), ParameterName.USER_FACING));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public string getUserFacingString(bool userFacing)
		{
			if (userFacing)
			{
				return "USER_FACING";
			}
			return "NOT_USER_FACING";
		}

		public void Send_Telemetry_EVT_GAME_ERROR_CRASH(string nameOfError, string crashReason, string crashTime, string errorDetails)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GAME_ERROR_CRASH);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			long num = 0L;
			if (playerService != null)
			{
				num = playerService.ID;
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", nameOfError, ParameterName.ERROR_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", crashReason, ParameterName.ERROR_REASON));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", errorDetails, ParameterName.ERROR_DETAILS));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", crashTime, ParameterName.EVENT_TIMESTAMP));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", num, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GAME_ERROR_CONNECTIVITY(string nameOfError, string errorDetails, bool userFacing)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GAME_ERROR_CONNECTIVITY);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", nameOfError, ParameterName.ERROR_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", errorDetails, ParameterName.ERROR_DETAILS));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", getUserFacingString(userFacing), ParameterName.USER_FACING));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_IGE_FREE_CREDITS_EARNED(int grindEarned, string eventName, bool purchasedCurrencySpent)
		{
			if (grindEarned != 0)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_EARNED);
				List<TelemetryParameter> list = new List<TelemetryParameter>();
				list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", grindEarned, ParameterName.GRIND_CURRENCY_EARNED));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", eventName, ParameterName.EVENT_NAME));
				list.AddRange(GetLevelGrindPremium());
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PremiumPurchaseArgument(purchasedCurrencySpent), ParameterName.CURRENCY_EARN_SPEND_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
				telemetryEvent.Parameters = list;
				LogGameEvent(telemetryEvent);
			}
		}

		public void Send_Telemetry_EVT_IGE_PAID_CREDITS_EARNED(int premiumEarned, string eventName, bool purchasedCurrencySpent)
		{
			if (premiumEarned != 0)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_EARNED);
				List<TelemetryParameter> list = new List<TelemetryParameter>();
				list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", premiumEarned, ParameterName.PREMIUM_CURRENCY_EARNED));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", eventName, ParameterName.EVENT_NAME));
				list.AddRange(GetLevelGrindPremium());
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PremiumPurchaseArgument(purchasedCurrencySpent), ParameterName.CURRENCY_EARN_SPEND_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
				telemetryEvent.Parameters = list;
				LogGameEvent(telemetryEvent);
			}
		}

		public void Send_Telemetry_EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE(int grindSpent, string itemPurchased, bool purchasedCurrencySpent, string highLevel, string specific, string type)
		{
			if (grindSpent != 0)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE);
				List<TelemetryParameter> list = new List<TelemetryParameter>();
				list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", grindSpent, ParameterName.GRIND_CURRENCY_SPENT));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemPurchased, ParameterName.ITEM_PURCHASED));
				list.AddRange(GetLevelGrindPremium());
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PremiumPurchaseArgument(purchasedCurrencySpent), ParameterName.CURRENCY_EARN_SPEND_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
				telemetryEvent.Parameters = list;
				LogGameEvent(telemetryEvent);
			}
		}

		public void Send_Telemetry_EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE(int premiumSpent, string itemPurchased, bool purchasedCurrencySpent, string highLevel, string specific, string type)
		{
			if (premiumSpent != 0)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE);
				List<TelemetryParameter> list = new List<TelemetryParameter>();
				list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", premiumSpent, ParameterName.PREMIUM_CURRENCY_SPENT));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemPurchased, ParameterName.ITEM_PURCHASED));
				list.AddRange(GetLevelGrindPremium());
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PremiumPurchaseArgument(purchasedCurrencySpent), ParameterName.CURRENCY_EARN_SPEND_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
				telemetryEvent.Parameters = list;
				LogGameEvent(telemetryEvent);
			}
		}

		public void Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_EARNED(int amount, string itemName, string itemType, string highLevel, string specific, string type)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_RESOURCE_CRAFTABLE_EARNED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", amount, ParameterName.AMOUNT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemName, ParameterName.ITEM_NAME));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemType, ParameterName.ITEM_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_SPENT(int amount, string sourceName, string itemName, string highLevel, string specific, string type)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_RESOURCE_CRAFTABLE_SPENT);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", amount, ParameterName.AMOUNT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemName, ParameterName.ITEM_NAME));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", sourceName, ParameterName.SOURCE_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_IGE_STORE_VISIT(string trafficSource, string storeVisited)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IGE_STORE_VISIT);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", trafficSource, ParameterName.TRAFFIC_SOURCE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", storeVisited, ParameterName.STORE_VISITED));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_TUTORIAL_FUNNEL_EAL(string tutorialName, string step)
		{
			int num = Mathf.RoundToInt(Time.realtimeSinceStartup - lastFunnelEalTime);
			lastFunnelEalTime = Time.realtimeSinceStartup;
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_TUTORIAL_FUNNEL_EAL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", tutorialName, ParameterName.TUTORIAL_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", step, ParameterName.STEP));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", num, ParameterName.DURATION));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL(string step, string swrveGroup, string performance)
		{
			float num = timeService.RealtimeSinceStartup();
			int num2 = Mathf.RoundToInt(num - lastGameLoadFunnelTime);
			lastGameLoadFunnelTime = num;
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_GAME_LOAD_FUNNEL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", num2, ParameterName.DURATION));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", step, ParameterName.STEP));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", swrveGroup, ParameterName.SWRVE_GROUP));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", performance, ParameterName.PERFORMANCE));
			telemetryEvent.Parameters = list;
			logger.Debug(string.Format("Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL: {0} - {1}", step, swrveGroup));
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MINION_POPULATION_BENEFIT(string benefit)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_POPULATION_GOAL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", benefit, ParameterName.POPULATION_GOAL_UNLOCKED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MINION_UPGRADE(int newLevel, int tokensUsed, uint tokensBeforeUpgrade)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MINION_LEVEL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", newLevel, ParameterName.NEW_MINION_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", tokensBeforeUpgrade, ParameterName.MINION_UPGRADE_TOKENS));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", tokensUsed, ParameterName.UPGRADE_TOKENS_CONSUMED));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL(string bundleName, int duration, long size, bool isNetworkWifi)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_GAME_DOWNLOAD_FUNNEL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", bundleName, ParameterName.DLC_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", duration, ParameterName.DURATION));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", size, ParameterName.DLC_SIZE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", isNetworkWifi.ToString(), ParameterName.DLC_IS_WIFI));
			telemetryEvent.Parameters = list;
			logger.Debug(string.Format("Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL: {0} in {1}", bundleName, duration));
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GP_LEVEL_PROMOTION()
		{
			int totalSecondsSinceLevelUp = playerDurationService.TotalSecondsSinceLevelUp;
			int gameplaySecondsSinceLevelUp = playerDurationService.GameplaySecondsSinceLevelUp;
			logger.Info(string.Format("LEVELUP TELEMETRY: TOTAL SECONDS: {0}", totalSecondsSinceLevelUp));
			logger.Info(string.Format("LEVELUP TELEMETRY: GAMEPLAY SECONDS: {0}", gameplaySecondsSinceLevelUp));
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GP_LEVEL_PROMOTION);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", totalSecondsSinceLevelUp, ParameterName.DURATION_TOTAL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", gameplaySecondsSinceLevelUp, ParameterName.DURATION_GAMEPLAY));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		private IEnumerable<TelemetryParameter> GetFtueParameters()
		{
			int ftueLevel = playerService.GetHighestFtueCompleted();
			if (ftueLevel < 999999)
			{
				yield return new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "FTUE", ParameterName.FTUE_LEVEL);
				yield return new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", ftueLevel.ToString(), ParameterName.FTUE_LEVEL);
			}
		}

		public void Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(string achievementName, TelemetryAchievementType type, int PartyPointsEarned, string questGiver = "")
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", achievementName, ParameterName.ACHIEVEMENT_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type.ToString(), ParameterName.ARCHIVEMENT_TYPE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PartyPointsEarned, ParameterName.AMOUNT_PARTY_POINTS_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", questGiver, ParameterName.QUEST_GIVER));
			list.AddRange(GetFtueParameters());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL_ProceduralQuest(string achievementName, ProceduralQuestEndState endState, int PartyPointsEarned)
		{
			TelemetryAchievementType telemetryAchievementType = TelemetryAchievementType.ProceduralQuest;
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", achievementName, ParameterName.ACHIEVEMENT_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", telemetryAchievementType.ToString(), ParameterName.ARCHIVEMENT_TYPE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", PartyPointsEarned, ParameterName.AMOUNT_PARTY_POINTS_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.QUEST_GIVER));
			list.AddRange(GetFtueParameters());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GP_ACHIEVEMENTS_STARTED_EAL(string achievementName, TelemetryAchievementType type, string questGiver = "")
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.ACHIEVEMENT_OR_QUEST_STARTED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", achievementName, ParameterName.ACHIEVEMENT_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type.ToString(), ParameterName.ARCHIVEMENT_TYPE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.PROCEDURAL_QUEST_END_STATE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", questGiver, ParameterName.QUEST_GIVER));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_EBISU_LOGIN_GAMECENTER(string loginLocation)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GC_SIGNON);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", loginLocation, ParameterName.LOGIN_LOCATION));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_EBISU_LOGIN_GOOGLEPLAY(string loginLocation)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GP_SIGNON);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", loginLocation, ParameterName.LOGIN_LOCATION));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_EBISU_LOGIN_FACEBOOK(string loginLocation, string loginSource)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_FB_SIGNON);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", loginLocation, ParameterName.LOGIN_LOCATION));
			if (string.IsNullOrEmpty(loginSource))
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			}
			else
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", loginSource, ParameterName.LOGIN_SOURCE));
			}
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_AGE_GATE_SET(int year, int month)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_COPPA_AGE_GATE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", year, ParameterName.YEAR));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", month, ParameterName.MONTH));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_TelemetryCharacterPrestiged(Prestige prestige)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_CHARACTER_PRESTIGED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", prestige.Definition.LocalizedKey, ParameterName.CHARACTER_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", prestige.CurrentOrdersCompleted, ParameterName.ORDERS_COMPLETED));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_TelemetryOrderBoard(bool isFillingOrder, TransactionDefinition transactionDef, int characterDefinitionId)
		{
			SynergyTrackingEventType type = ((!isFillingOrder) ? SynergyTrackingEventType.EVT_ORDER_CANCEL_SUM : SynergyTrackingEventType.EVT_ORDER_COMPLETED_SUM);
			TelemetryEvent telemetryEvent = new TelemetryEvent(type);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.NONE));
			string value = "none";
			if (characterDefinitionId != 0 && definitionService != null)
			{
				PrestigeDefinition definition = null;
				if (definitionService.TryGet<PrestigeDefinition>(characterDefinitionId, out definition))
				{
					value = string.Format("Prestiged with {0}", (definition == null) ? "uknown" : definition.LocalizedKey);
				}
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.PRESTIGED_WITH));
			list.AddRange(GetLevelGrindPremium());
			uint num = 0u;
			uint num2 = 0u;
			foreach (QuantityItem output in transactionDef.Outputs)
			{
				if (output.ID == 0)
				{
					num = output.Quantity;
				}
				else if (output.ID == 2)
				{
					num2 = output.Quantity;
				}
			}
			int count = transactionDef.Inputs.Count;
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", num, ParameterName.GRIND_CURRENCY_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", num2, ParameterName.XP_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", count, ParameterName.INPUTS_COUNT));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
			type = ((!isFillingOrder) ? SynergyTrackingEventType.EVT_ORDER_CANCEL_DET : SynergyTrackingEventType.EVT_ORDER_COMPLETED_DET);
			telemetryEvent = new TelemetryEvent(type);
			list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.NONE));
			if (count > 0)
			{
				AddIngredient(transactionDef.Inputs[0], list);
			}
			else
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "none:0", ParameterName.INGREDIENT));
			}
			list.AddRange(GetLevelGrindPremium());
			for (int i = 1; i < count; i++)
			{
				AddIngredient(transactionDef.Inputs[i], list);
			}
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		private void AddIngredient(QuantityItem item, List<TelemetryParameter> parameters)
		{
			ItemDefinition definition;
			if (definitionService != null && definitionService.TryGet<ItemDefinition>(item.ID, out definition))
			{
				string value = string.Format("{0}:{1}", (definition != null) ? definition.LocalizedKey : "unknown", item.Quantity);
				parameters.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.INGREDIENT));
			}
		}

		public void Send_Telemetry_EVT_IN_APP_MESSAGE_DISPLAYED(string inAppMessageName, HindsightCampaign.DismissType choice)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_IPAD_UPSELL_MESSAGE_DISPLAYED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", inAppMessageName, ParameterName.IN_APP_MESSAGE_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", choice, ParameterName.USER_CHOICE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_TRACKING_OPTOUT()
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_TRACKING_OPTOUT);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MINI_GAME_PLAYED(string mignetteName, int score, float timePlayed, int xpReward)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MINI_GAME_PLAYED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mignetteName, ParameterName.MIGNETTE_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", score, ParameterName.SCORE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", timePlayed, ParameterName.TIME_PLAYED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", xpReward, ParameterName.AMOUNT_PARTY_POINTS_EARNED));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		private TelemetryParameter IsSpender()
		{
			string dataPlayer = localPersistService.GetDataPlayer("IsSpender");
			if (dataPlayer.Equals("true"))
			{
				return new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "spender", ParameterName.SPENDER);
			}
			return new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "non-spender", ParameterName.SPENDER);
		}

		public void Send_Telemetry_EVT_USER_DATA_AT_APP_START(int seconds, int tokenCount, int minions, string swrveGroup, string expansions)
		{
			logger.Debug(string.Format("Send_Telemetry_EVT_USER_DATA_AT_APP_START: {0} {1} {2} {3} {4}", seconds, tokenCount, minions, swrveGroup, expansions));
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_DATA_AT_APP_START);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			long iD = playerService.ID;
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", seconds, ParameterName.TIME_SINCE_LAST_PLAYED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", tokenCount, ParameterName.MINION_UPGRADE_TOKENS));
			list.AddRange(GetLevelGrindPremium());
			list.Add(IsSpender());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", minions, ParameterName.NUM_MINIONS));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", swrveGroup, ParameterName.SWRVE_GROUP));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", expansions, ParameterName.LAND_EXPANSIONS));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", iD, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_DATA_AT_APP_CLOSE()
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_DATA_AT_APP_CLOSE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			long iD = playerService.ID;
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", iD, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_STORAGE_LIMIT_HIT(int storageLimit)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_STORAGE_LIMIT_HIT);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", storageLimit, ParameterName.STORAGE_LIMIT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_SOCIAL_EVENT_COMPLETION(int teamSize)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_SOCIAL_EVENT_COMPLETION);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			if (teamSize == 1)
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "solo", ParameterName.SOLO_TEAM));
			}
			else
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "team", ParameterName.SOLO_TEAM));
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", teamSize, ParameterName.TEAM_SIZE));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_SOCIAL_EVENT_CONTRIBUTION(string item, int quantity, int teamSize, int xpReward)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_SOCIAL_EVENT_CONTRIBUTION);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", item, ParameterName.ITEM_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", quantity, ParameterName.AMOUNT));
			list.AddRange(GetLevelGrindPremium());
			if (teamSize == 1)
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "solo", ParameterName.SOLO_TEAM));
			}
			else
			{
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", "team", ParameterName.SOLO_TEAM));
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", teamSize, ParameterName.TEAM_SIZE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", xpReward, ParameterName.AMOUNT_PARTY_POINTS_EARNED));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemtry_EVT_MINI_TIER_REACHED(string mignetteName, int tier, int plays)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MINI_TIER_REACHED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mignetteName, ParameterName.MIGNETTE_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", tier, ParameterName.REWARD_TIER));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", plays, ParameterName.TIMES_PLAYED));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemtry_EVT_MARKETPLACE_ITEM_LISTED(string itemName, int quantity, int price, string highLevel, string specific, string type, string other)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MARKETPLACE_ITEM_LISTED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemName, ParameterName.ITEM_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", quantity, ParameterName.AMOUNT));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", price, ParameterName.PRICE_LISTED_AT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", other, ParameterName.TAXONOMY_OTHER));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemtry_EVT_MARKETPLACE_VIEWED(string viewType)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MARKETPLACE_VIEWED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", viewType, ParameterName.VIEW_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MINION_PARTY_STARTED(int totalPartyPoints, string buffSelected, string guestOfHonor, bool isInspiredParty)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MINION_PARTY_STARTED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", totalPartyPoints, ParameterName.TOTAL_PARTY_POINTS_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", buffSelected, ParameterName.PARTY_BUFF_TYPE));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", guestOfHonor, ParameterName.PARTY_GUEST_OF_HONOR));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", isInspiredParty ? 1 : 0, ParameterName.PARTY_IS_INSPIRED));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_PARTY_POINTS_EARNED(int amountOfPartyPoints, string sourceName)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_PARTY_POINTS_EARNED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", amountOfPartyPoints, ParameterName.AMOUNT_PARTY_POINTS_EARNED));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", sourceName, ParameterName.PARTY_POINTS_EARNED_FROM));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_NOTE_SETTING_CHANGE(string settingName, string enabled, string sourceName)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_NOTE_SETTING_CHANGE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", settingName, ParameterName.NOTIFICATION_CHANGED_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", enabled, ParameterName.NOTIFICATION_ENABLED));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", sourceName, ParameterName.NOTIFICATION_CHANGED_FROM));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_PLAYER_TRAINING(int triggeredID, int fromSettings, int timeOpen)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_PLAYER_TRAINING);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", triggeredID, ParameterName.TRIGGERED_DEF_ID));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", fromSettings, ParameterName.SOURCE_OF_TRAINING));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", timeOpen, ParameterName.TIME_SPENT_ON_TRAINING));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void SetPlayerServiceReference(IPlayerService playerService)
		{
			this.playerService = playerService;
		}

		public void SetPlayerDurationServiceReference(IPlayerDurationService playerDurationService)
		{
			this.playerDurationService = playerDurationService;
		}

		public void SetDefinitionServiceReference(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public void Send_Telemetry_EVT_RATE_MY_APP(string promptType, bool? userAccepted)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_RATE_MY_APP);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			string value = "Cancel";
			if (userAccepted.HasValue)
			{
				value = ((!userAccepted.Value) ? "No" : "Yes");
			}
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", promptType, ParameterName.PROMPT_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.USER_CHOICE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void SendInAppPurchaseEventOnPurchaseComplete(IapTelemetryEvent iapTelemetryEvent)
		{
			foreach (IIapTelemetryService iapTelemetryService in iapTelemetryServices)
			{
				iapTelemetryService.SendInAppPurchaseEventOnPurchaseComplete(iapTelemetryEvent);
			}
		}

		public void SendInAppPurchaseEventOnProductDelivery(string sku, TransactionDefinition reward)
		{
			foreach (IIapTelemetryService iapTelemetryService in iapTelemetryServices)
			{
				iapTelemetryService.SendInAppPurchaseEventOnProductDelivery(sku, reward);
			}
		}

		private string PremiumPurchaseArgument(bool isPremium)
		{
			return (!isPremium) ? "FREE" : "TRUE";
		}

		public void Send_Telemetry_EVT_PARTY_SKIPPED()
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_PARTY_SKIPPED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_PINCH_PROMPT(string sourceName, PendingCurrencyTransaction pct, IList<QuantityItem> requiredItems, string action)
		{
			if (pct == null || requiredItems == null || requiredItems.Count == 0)
			{
				return;
			}
			TransactionArg transactionArg = pct.GetTransactionArg();
			if (transactionArg != null && transactionArg.InstanceId != 0)
			{
				Instance byInstanceId = playerService.GetByInstanceId<Instance>(transactionArg.InstanceId);
				if (byInstanceId != null)
				{
					sourceName = byInstanceId.Definition.LocalizedKey;
				}
			}
			IList<QuantityItem> outputs = pct.GetPendingTransaction().Outputs;
			IList<QuantityItem> inputs = pct.GetPendingTransaction().Inputs;
			TaxonomyDefinition definition = null;
			if (outputs != null && outputs.Count > 0)
			{
				definitionService.TryGet<TaxonomyDefinition>(outputs[0].ID, out definition);
			}
			foreach (QuantityItem requiredItem in requiredItems)
			{
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(requiredItem.ID);
				int quantity = (int)requiredItem.Quantity;
				foreach (QuantityItem item in inputs)
				{
					if (item.ID == requiredItem.ID)
					{
						quantity = (int)item.Quantity;
						break;
					}
				}
				Send_Telemetry_EVT_PINCH_PROMPT(sourceName, itemDefinition.LocalizedKey, quantity, (definition == null) ? string.Empty : definition.TaxonomyHighLevel, (definition == null) ? string.Empty : definition.TaxonomySpecific, (definition == null) ? string.Empty : definition.TaxonomyType, action);
			}
		}

		public void Send_Telemetry_EVT_PINCH_PROMPT(string sourceName, string itemName, int amount, string highLevel, string specific, string type, string action)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_PINCH_PROMPT);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_SCORE", amount, ParameterName.AMOUNT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", itemName, ParameterName.ITEM_NAME));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", sourceName, ParameterName.SOURCE_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", highLevel, ParameterName.TAXONOMY_HIGH_LEVEL));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", specific, ParameterName.TAXONOMY_SPECIFIC));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", type, ParameterName.TAXONOMY_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", action, ParameterName.PINCH_PROMPT_ACTION));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_DCN(string buttonPressed, string url, string name)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_DCN);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", buttonPressed, ParameterName.DCN_BUTTON));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", url, ParameterName.DCN_URL));
			list.AddRange(GetLevelGrindPremium());
			list.Add(IsSpender());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", name, ParameterName.DCN_NAME));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_UPSELL(string mtxSellID, UpsellStatus status)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_UPSELL_NOTIFICATION);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mtxSellID, ParameterName.MTX_SELL_ID));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", status.ToString(), ParameterName.UPSELL_STATUS));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MASTER_PLAN_COMPLETE(string masterPlanName, string villainName, int duration)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MASTER_PLAN_COMPLETE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", masterPlanName, ParameterName.MASTER_PLAN_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_DURATION", duration, ParameterName.MASTER_PLAN_DURATION));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", villainName, ParameterName.MASTER_PLAN_VILLAIN_NAME));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MASTER_PLAN_COMPONENT_COMPLETE(string masterPlanName, string villainName, string componentName, int orderComplete)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MASTER_PLAN_COMPONENT_COMPLETE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", componentName, ParameterName.MASTER_PLAN_COMPONENT_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", orderComplete, ParameterName.MASTER_PLAN_COMPONENT_ORDER));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", masterPlanName, ParameterName.MASTER_PLAN_NAME));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", villainName, ParameterName.MASTER_PLAN_VILLAIN_NAME));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_MASTER_PLAN_TASK_COMPLETE(string componentName, string taskType, string requiredItem, int requiredQuantity)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MASTER_PLAN_TASK_COMPLETE);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", taskType, ParameterName.MASTER_PLAN_TASK_TYPE));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", componentName, ParameterName.MASTER_PLAN_COMPONENT_NAME));
			list.AddRange(GetLevelGrindPremium());
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", requiredItem, ParameterName.MASTER_PLAN_TASK_REQUIRED_ITEM));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", requiredQuantity, ParameterName.MASTER_PLAN_TASK_REQUIRED_QUANTITY));
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_CONTACT_US_CLICKED()
		{
			long iD = playerService.ID;
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_CONTACT_US_CLICKED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", iD, ParameterName.PLAYER_ID));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_NONE", string.Empty, ParameterName.NONE));
			list.AddRange(GetLevelGrindPremium());
			telemetryEvent.Parameters = list;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(string source, int buildingDefId, int sourceDefId)
		{
			BuildingDefinition definition;
			if (definitionService.TryGet<BuildingDefinition>(buildingDefId, out definition))
			{
				Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(source, definition, sourceDefId);
			}
		}

		public void Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(string source, BuildingDefinition buildingDefinition, int sourceDefId)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_USER_ACQUIRES_BUILDING);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", GetDefinitionLocalizedKey(buildingDefinition), ParameterName.BUILDING));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", source, ParameterName.SOURCE_NAME));
			List<TelemetryParameter> list2 = list;
			list2.AddRange(GetLevelGrindPremium());
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", (sourceDefId != 0) ? sourceDefId.ToString() : "NULL", ParameterName.SOURCE_DEF_ID));
			list2.AddRange(GetTaxonomyParameters(buildingDefinition));
			telemetryEvent.Parameters = list2;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_TSM_TRIGGER_ACTION(TriggerDefinition triggerDefinition, TriggerRewardDefinition reward)
		{
			if (reward == null)
			{
				return;
			}
			UpsellTriggerRewardDefinition upsellTriggerRewardDefinition = reward as UpsellTriggerRewardDefinition;
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_TSM_TRIGGER_ACTION);
			string value = string.Empty;
			string value2 = TelemetryTriggerRewardCost(reward);
			if (!string.IsNullOrEmpty(value2))
			{
				value = "Transaction";
			}
			else if (upsellTriggerRewardDefinition != null)
			{
				value2 = string.Format("Upsell: {0}", upsellTriggerRewardDefinition.upsellId);
				value = "Upsell";
			}
			else if (reward.IsFree)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < reward.transaction.GetOutputCount(); i++)
				{
					QuantityItem outputItem = reward.transaction.GetOutputItem(i);
					BuildingDefinition definition;
					if (definitionService.TryGet<BuildingDefinition>(outputItem.ID, out definition))
					{
						Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(definition.TaxonomyType, definition, reward.ID);
					}
					if (i == 0)
					{
						stringBuilder.Append(outputItem.ToString(definitionService));
					}
					else
					{
						stringBuilder.AppendFormat(", {0}", outputItem.ToString(definitionService));
					}
				}
				value2 = stringBuilder.ToString();
				value = "Award";
			}
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value2, ParameterName.TSM_COST_AWARD_AMOUNT));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.TSM_TRIGGER_ACTION));
			List<TelemetryParameter> list2 = list;
			list2.AddRange(GetLevelGrindPremium());
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", (upsellTriggerRewardDefinition == null) ? string.Empty : upsellTriggerRewardDefinition.upsellId.ToString(), ParameterName.OFFER_ID));
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", triggerDefinition.ID, ParameterName.TSM_TRIGGER_TYPE));
			telemetryEvent.Parameters = list2;
			LogGameEvent(telemetryEvent);
			Send_Telemetry_EVT_TSM_TRIGGER_BUY_SELL(triggerDefinition, reward);
		}

		public void Send_Telemetry_EVT_TSM_TRIGGER_BUY_SELL(TriggerDefinition triggerDefinition, TriggerRewardDefinition reward)
		{
			if (reward != null && !reward.IsFree)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_TSM_TRIGGER_BUY_SELL);
				List<TelemetryParameter> list = new List<TelemetryParameter>();
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", TelemetryTriggerRewardCost(reward), ParameterName.COST));
				list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", triggerDefinition.ID, ParameterName.TRIGGER_DEF_ID));
				List<TelemetryParameter> list2 = list;
				list2.AddRange(GetLevelGrindPremium());
				for (int i = 0; i < 5; i++)
				{
					QuantityItem outputItem = reward.transaction.GetOutputItem(i);
					string value = ((outputItem != null) ? outputItem.ToString(definitionService) : string.Empty);
					list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", value, ParameterName.ITEM_QUANTITY));
				}
				telemetryEvent.Parameters = list2;
				LogGameEvent(telemetryEvent);
			}
		}

		private string TelemetryTriggerRewardCost(TriggerRewardDefinition reward)
		{
			if (!reward.HasInputs)
			{
				if (!reward.IsCash)
				{
					return string.Empty;
				}
				PremiumCurrencyItemDefinition premiumCurrencyItemDefinition = definitionService.Get<PremiumCurrencyItemDefinition>(reward.SKUId);
				return string.Format("MTX: {0}", premiumCurrencyItemDefinition.SKU);
			}
			QuantityItem inputItem = reward.transaction.GetInputItem(0);
			return inputItem.ToString(definitionService);
		}

		public void Send_Telemetry_EVT_AD_INTERACTION(AdPlacementName placementName, IList<QuantityItem> rewards, int timesRedeemedInCurrentDay)
		{
			ItemDefinition itemDefinition;
			int rewardAmount;
			if (RewardedAdUtil.GetFirstItemDefintion(rewards, out itemDefinition, out rewardAmount, definitionService))
			{
				string surfaceLocation = RewardedAdTelemetryUtil.GetSurfaceLocation(placementName);
				string rewardType = RewardedAdTelemetryUtil.GetRewardType(placementName, itemDefinition);
				Send_Telemetry_EVT_AD_INTERACTION(surfaceLocation, rewardType, timesRedeemedInCurrentDay);
			}
		}

		public void Send_Telemetry_EVT_AD_INTERACTION(AdPlacementName placementName, ItemDefinition reward, int timesRedeemedInCurrentDay)
		{
			string surfaceLocation = RewardedAdTelemetryUtil.GetSurfaceLocation(placementName);
			string rewardType = RewardedAdTelemetryUtil.GetRewardType(placementName, reward);
			Send_Telemetry_EVT_AD_INTERACTION(surfaceLocation, rewardType, timesRedeemedInCurrentDay);
		}

		public void Send_Telemetry_EVT_AD_INTERACTION(string surfaceLocation, string rewardType, int timesRedeemedInCurrentDay)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_AD_INTERACTION);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", surfaceLocation, ParameterName.AD_SURFACE_LOCATION));
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", rewardType, ParameterName.AD_REWARD_TYPE));
			List<TelemetryParameter> list2 = list;
			list2.AddRange(GetLevelGrindPremium());
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", timesRedeemedInCurrentDay.ToString(), ParameterName.AD_TIMES_REDEEMED_IN_CURRENT_DAY));
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.TAXONOMY_HIGH_LEVEL));
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.TAXONOMY_SPECIFIC));
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.TAXONOMY_TYPE));
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list2;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GAME_BUTTON_PRESSED_GENERIC(GameConstants.TrackedGameButton buttonName, string optionalParam2 = "", ParameterName param2Name = ParameterName.NONE)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_GAME_BUTTON_PRESSED);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", buttonName.ToString(), ParameterName.BUTTON_NAME));
			List<TelemetryParameter> list2 = list;
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", optionalParam2, param2Name));
			list2.AddRange(GetLevelGrindPremium());
			for (int i = 0; i < 4; i++)
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.NONE));
			}
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list2;
			LogGameEvent(telemetryEvent);
		}

		public void Send_Telemetry_EVT_GAME_XPROMO_BUTTON_PRESSED(GameConstants.TrackedGameButton buttonName, bool petsInstalled)
		{
			Send_Telemetry_EVT_GAME_BUTTON_PRESSED_GENERIC(buttonName, (!petsInstalled) ? "Pets Not Installed" : "Pets Installed", ParameterName.XPROMO_PETS_INSTALLED);
		}

		public void Send_Telemetry_EVT_MTX_BOOKEND_EVENT(MtxBookendTelemetryInfo mtxInfo)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(SynergyTrackingEventType.EVT_MTX_BOOKEND_INFO);
			List<TelemetryParameter> list = new List<TelemetryParameter>();
			list.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mtxInfo.productId, ParameterName.MTX_SELL_ID));
			List<TelemetryParameter> list2 = list;
			if (mtxInfo.purchaseStage != MTXPurchaseStage.Complete_Fail)
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mtxInfo.purchaseStage.ToString(), ParameterName.STAGE_OF_PURCHASE));
			}
			else
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mtxInfo.purchaseStage.ToString() + " " + mtxInfo.failedReason, ParameterName.STAGE_OF_PURCHASE));
			}
			list2.AddRange(GetLevelGrindPremium());
			if (mtxInfo.purchaseStage == MTXPurchaseStage.Initiate)
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.PURCHASE_COMPLETION_TIME));
			}
			else
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", mtxInfo.timeToComplete.ToString(), ParameterName.PURCHASE_COMPLETION_TIME));
			}
			for (int i = 0; i < 3; i++)
			{
				list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", string.Empty, ParameterName.NONE));
			}
			list2.Add(new TelemetryParameter("EVT_KEYTYPE_ENUMERATION", playerService.ID, ParameterName.PLAYER_ID));
			telemetryEvent.Parameters = list2;
			LogGameEvent(telemetryEvent);
		}
	}
}
