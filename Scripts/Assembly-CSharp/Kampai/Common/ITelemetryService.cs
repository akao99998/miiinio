using System.Collections.Generic;
using Kampai.Common.Service.Telemetry;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Util;

namespace Kampai.Common
{
	public interface ITelemetryService : IIapTelemetryService
	{
		void AddTelemetrySender(ITelemetrySender sender);

		void AddIapTelemetryService(IIapTelemetryService service);

		void SharingUsage(ITelemetrySender sender, bool enabled);

		void GameStarted();

		int SecondsSinceGameStart();

		void COPPACompliance();

		void SharingUsageCompliance();

		void SharingUsage(bool enabled);

		bool SharingUsageEnabled();

		void LogGameEvent(TelemetryEvent gameEvent);

		void Send_Telemetry_EVT_GAME_ERROR_GAMEPLAY(string nameOfError, string errorDetails, bool userFacing);

		void Send_Telemetry_EVT_GAME_ERROR_CONNECTIVITY(string nameOfError, string errorDetails, bool userFacing);

		void Send_Telemetry_EVT_GAME_ERROR_CRASH(string nameOfError, string crashReason, string crashTime, string errorDetails);

		void Send_Telemetry_EVT_GAME_BUTTON_PRESSED_GENERIC(GameConstants.TrackedGameButton buttonName, string optionalParam2 = "", ParameterName param2Name = ParameterName.NONE);

		void Send_Telemetry_EVT_GAME_XPROMO_BUTTON_PRESSED(GameConstants.TrackedGameButton buttonName, bool petsInstalled);

		void Send_Telemetry_EVT_IGE_FREE_CREDITS_EARNED(int grindEarned, string eventName, bool purchasedCurrencySpent);

		void Send_Telemetry_EVT_IGE_PAID_CREDITS_EARNED(int premiumEarned, string eventName, bool purchasedCurrencySpent);

		void Send_Telemetry_EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE(int grindSpent, string itemPurchased, bool purchasedCurrencySpent, string highLevel, string specific, string type);

		void Send_Telemetry_EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE(int premiumSpent, string itemPurchased, bool purchasedCurrencySpent, string highLevel, string specific, string type);

		void Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_EARNED(int amount, string itemName, string itemType, string highLevel, string specific, string type);

		void Send_Telemetry_EVT_IGE_RESOURCE_CRAFTABLE_SPENT(int amount, string sourceName, string itemName, string highLevel, string specific, string type);

		void Send_Telemetry_EVT_IGE_STORE_VISIT(string trafficSource, string storeVisited);

		void Send_Telemetry_EVT_USER_TUTORIAL_FUNNEL_EAL(string tutorialName, string step);

		void Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL(string step, string swrveGroup, string performance);

		void Send_Telemetry_EVT_USER_GAME_DOWNLOAD_FUNNEL(string bundleName, int duration, long size, bool isNetworkWifi);

		void Send_Telemetry_EVT_GP_LEVEL_PROMOTION();

		void Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(string achievementName, TelemetryAchievementType type, int PartyPointsEarned, string questGiver = "");

		void Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL_ProceduralQuest(string achievementName, ProceduralQuestEndState endState, int PartyPointsEarned);

		void Send_Telemetry_EVT_GP_ACHIEVEMENTS_STARTED_EAL(string achievementName, TelemetryAchievementType type, string questGiver = "");

		void Send_Telemetry_EVT_EBISU_LOGIN_GAMECENTER(string loginLocation);

		void Send_Telemetry_EVT_EBISU_LOGIN_GOOGLEPLAY(string loginLocation);

		void Send_Telemetry_EVT_EBISU_LOGIN_FACEBOOK(string loginLocation, string loginSource);

		void Send_Telemetry_EVT_AGE_GATE_SET(int year, int month);

		void Send_TelemetryCharacterPrestiged(Prestige prestige);

		void Send_TelemetryOrderBoard(bool isFillingOrder, TransactionDefinition transactionDef, int characterDefinitionId);

		void Send_Telemetry_EVT_IN_APP_MESSAGE_DISPLAYED(string inAppMessageName, HindsightCampaign.DismissType choice);

		void Send_Telemetry_EVT_USER_TRACKING_OPTOUT();

		void Send_Telemetry_EVT_MINI_GAME_PLAYED(string mignetteName, int score, float timePlayed, int xpReward);

		void Send_Telemetry_EVT_USER_DATA_AT_APP_START(int seconds, int tokenCount, int minions, string swrveGroup, string expansions);

		void Send_Telemetry_EVT_USER_DATA_AT_APP_CLOSE();

		void Send_Telemetry_EVT_STORAGE_LIMIT_HIT(int storageLimit);

		void Send_Telemetry_EVT_RATE_MY_APP(string promptType, bool? userAccepted);

		void Send_Telemetry_EVT_SOCIAL_EVENT_COMPLETION(int teamSize);

		void Send_Telemetry_EVT_SOCIAL_EVENT_CONTRIBUTION(string item, int quantity, int teamSize, int xpReward);

		void Send_Telemtry_EVT_MINI_TIER_REACHED(string mignetteName, int tier, int plays);

		void Send_Telemtry_EVT_MARKETPLACE_ITEM_LISTED(string itemName, int quantity, int price, string highLevel, string specific, string type, string other);

		void Send_Telemtry_EVT_MARKETPLACE_VIEWED(string viewType);

		void Send_Telemetry_EVT_PLAYER_TRAINING(int triggeredID, int fromSettings, int timeOpen);

		void Send_Telemetry_EVT_MINION_PARTY_STARTED(int totalPartyPoints, string buffSelected, string guestOfHonor, bool isInspiredParty);

		void Send_Telemetry_EVT_PARTY_POINTS_EARNED(int amountOfPartyPoints, string sourceName);

		void Send_Telemetry_EVT_NOTE_SETTING_CHANGE(string settingName, string enabled, string sourceName);

		void Send_Telemetry_EVT_PARTY_SKIPPED();

		void Send_Telemetry_EVT_PINCH_PROMPT(string sourceName, PendingCurrencyTransaction pct, IList<QuantityItem> requiredItems, string action);

		void Send_Telemetry_EVT_PINCH_PROMPT(string sourceName, string itemName, int amount, string highLevel, string specific, string type, string action);

		void Send_Telemetry_EVT_DCN(string buttonPressed, string url, string name);

		void Send_Telemetry_EVT_UPSELL(string mtxSellID, UpsellStatus status);

		void Send_Telemetry_EVT_TSM_TRIGGER_ACTION(TriggerDefinition triggerDefinition, TriggerRewardDefinition reward);

		void Send_Telemetry_EVT_MINION_POPULATION_BENEFIT(string benefit);

		void Send_Telemetry_EVT_MINION_UPGRADE(int newLevel, int tokensUsed, uint tokensBeforeUpgrade);

		void Send_Telemetry_EVT_TSM_TRIGGER_BUY_SELL(TriggerDefinition triggerDefinition, TriggerRewardDefinition reward);

		void Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(string source, int buildingDefId, int sourceDefId);

		void Send_Telemetry_EVT_USER_ACQUIRES_BUILDING(string source, BuildingDefinition buildingDefinition, int sourceDefId);

		void Send_Telemetry_EVT_MASTER_PLAN_COMPLETE(string masterPlanName, string villainName, int duration);

		void Send_Telemetry_EVT_MASTER_PLAN_COMPONENT_COMPLETE(string masterPlanName, string villainName, string componentName, int orderComplete);

		void Send_Telemetry_EVT_MASTER_PLAN_TASK_COMPLETE(string componentName, string taskType, string requiredItem, int requiredQuantity);

		void Send_Telemetry_EVT_MTX_BOOKEND_EVENT(MtxBookendTelemetryInfo mtxInfo);

		void Send_Telemetry_CONTACT_US_CLICKED();

		void Send_Telemetry_EVT_AD_INTERACTION(AdPlacementName placementName, ItemDefinition reward, int timesRedeemedInCurrentDay);

		void Send_Telemetry_EVT_AD_INTERACTION(AdPlacementName placementName, IList<QuantityItem> rewards, int timesRedeemedInCurrentDay);

		void Send_Telemetry_EVT_AD_INTERACTION(string surfaceLocation, string rewardType, int timesRedeemedInCurrentDay);
	}
}
