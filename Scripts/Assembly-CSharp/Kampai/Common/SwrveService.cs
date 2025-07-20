using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Util;
using Swrve.ResourceManager;
using UnityEngine;

namespace Kampai.Common
{
	public class SwrveService : ISwrveService, IIapTelemetryService, ITelemetrySender
	{
		public delegate string SwrveEventConverter(TelemetryEvent gameEvent);

		private const string TAG = "Swrve: ";

		private const int SWRVE_RESOURCE_REQUEST_TIMEOUT_SEC = 5;

		private const string UNKNOWN = "unknown";

		public IKampaiLogger logger = LogManager.GetClassLogger("SwrveService") as IKampaiLogger;

		private bool resourcesResponseReceived;

		private bool resourcesResponseWaitTimerExpired;

		private IPlayerService playerService;

		private IapTelemetryEvent pendingIapTelemetryEvent;

		private Dictionary<SynergyTrackingEventType, SwrveEventConverter> eventNameConverters;

		[Inject]
		public ABTestSignal abTestSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ABTestResourcesUpdatedSignal abTestResourcesUpdatedSignal { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			eventNameConverters = PrepareEventConverters();
		}

		public void UpdateResources()
		{
			logger.Debug("{0}UpdateResources(): initiate Swrve resources update request", "Swrve: ");
			resourcesResponseReceived = false;
			resourcesResponseWaitTimerExpired = false;
			SwrveComponent.Instance.SDK.ResourcesUpdatedCallback = ResourcesUpdatedCallback;
			SwrveComponent.Instance.SDK.RefreshUserResourcesAndCampaigns();
			logger.Debug("SwrveService:UpdateResources - Starting SwrveResourceRequestExpiration routine");
			routineRunner.StartCoroutine(SwrveResourceRequestExpiration());
		}

		public void SendInAppPurchaseEventOnPurchaseComplete(IapTelemetryEvent iapTelemetryEvent)
		{
			logger.Debug("{0}SendInAppPurchaseEventOnPurchaseComplete()...: iapTelemetryEvent = {1}", "Swrve: ", (iapTelemetryEvent == null) ? "null" : iapTelemetryEvent.ToString());
			pendingIapTelemetryEvent = iapTelemetryEvent;
		}

		public void SendInAppPurchaseEventOnProductDelivery(string sku, TransactionDefinition reward)
		{
			logger.Debug("{0}SendInAppPurchaseEventOnProductDelivery()...: sku = {1}", "Swrve: ", sku);
			if (sku == null)
			{
				logger.Log(KampaiLogLevel.Error, "{0}Skip sending IAP telemetry because of sku is null", "Swrve: ");
				return;
			}
			if (pendingIapTelemetryEvent == null)
			{
				logger.Log(KampaiLogLevel.Error, "{0}Skip sending IAP telemetry because pendingIapTelemetryEvent is null for sku: {1}", "Swrve: ", sku);
				return;
			}
			if (!sku.Equals(pendingIapTelemetryEvent.productId))
			{
				logger.Log(KampaiLogLevel.Error, "{0}Skip sending IAP telemetry because pendingIapTelemetryEvent's sku [{1}] does not match current sku: [{2}]", "Swrve: ", pendingIapTelemetryEvent.productId ?? "null", sku);
			}
			IapRewards rewards = GetRewards(reward);
			DebugLogRewards(rewards);
			IapTelemetryEvent iapTelemetryEvent = pendingIapTelemetryEvent;
			SwrveComponent.Instance.SDK.IapGooglePlay(iapTelemetryEvent.productId, iapTelemetryEvent.productPrice, iapTelemetryEvent.currency, rewards, iapTelemetryEvent.googlePurchaseData, iapTelemetryEvent.googleDataSignature);
			pendingIapTelemetryEvent = null;
			logger.Debug("{0}...SendInAppPurchaseEventOnProductDelivery()", "Swrve: ");
		}

		public void COPPACompliance()
		{
		}

		public void SharingUsage(bool enabled)
		{
		}

		public void SendEvent(TelemetryEvent gameEvent)
		{
			invoker.Add(delegate
			{
				SendEventHandler(gameEvent);
			});
		}

		private void SendEventHandler(TelemetryEvent gameEvent)
		{
			SwrveSDK sDK = SwrveComponent.Instance.SDK;
			if (!sDK.Initialised || sDK.Container == null)
			{
				logger.Warning("Unable to send Swrve event [{0}]", gameEvent.GetType().ToString());
				return;
			}
			string eventName;
			Dictionary<string, string> swrvePayload = GetSwrvePayload(gameEvent, out eventName);
			if (swrvePayload == null)
			{
				logger.Debug("{0} Skip sending telemetry event {1} to Swrve", "Swrve: ", gameEvent.Type);
				return;
			}
			DebugLogTelemetryEvent(gameEvent);
			DebugSwrveEventPayload(eventName, swrvePayload);
			switch (gameEvent.Type)
			{
			case SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_EARNED:
			{
				int value6 = GetValue(gameEvent.Parameters, ParameterName.GRIND_CURRENCY_EARNED, 0);
				sDK.CurrencyGiven("GRIND", value6);
				sDK.NamedEvent(eventName, swrvePayload);
				SendUserStatsUpdate();
				break;
			}
			case SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_EARNED:
			{
				int value5 = GetValue(gameEvent.Parameters, ParameterName.PREMIUM_CURRENCY_EARNED, 0);
				sDK.CurrencyGiven("PREMIUM", value5);
				sDK.NamedEvent(eventName, swrvePayload);
				SendUserStatsUpdate();
				break;
			}
			case SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE:
			{
				int value3 = GetValue(gameEvent.Parameters, ParameterName.GRIND_CURRENCY_SPENT, 0);
				string value4 = GetValue(gameEvent.Parameters, ParameterName.ITEM_PURCHASED, string.Empty);
				sDK.Purchase(value4, "GRIND", value3, 1);
				sDK.NamedEvent(eventName, swrvePayload);
				SendUserStatsUpdate();
				break;
			}
			case SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE:
			{
				int value = GetValue(gameEvent.Parameters, ParameterName.PREMIUM_CURRENCY_SPENT, 0);
				string value2 = GetValue(gameEvent.Parameters, ParameterName.ITEM_PURCHASED, string.Empty);
				sDK.Purchase(value2, "PREMIUM", value, 1);
				sDK.NamedEvent(eventName, swrvePayload);
				SendUserStatsUpdate();
				break;
			}
			case SynergyTrackingEventType.EVT_GP_LEVEL_PROMOTION:
				SendUserStatsUpdate();
				break;
			default:
				sDK.NamedEvent(eventName, swrvePayload);
				break;
			}
		}

		private T GetValue<T>(IList<TelemetryParameter> parameters, ParameterName name, T defaultValue)
		{
			T value;
			if (TryGetValue<T>(parameters, name, out value))
			{
				return value;
			}
			return defaultValue;
		}

		private bool TryGetValue<T>(IList<TelemetryParameter> parameters, ParameterName name, out T value)
		{
			foreach (TelemetryParameter parameter in parameters)
			{
				if (parameter.name == name)
				{
					try
					{
						value = (T)parameter.value;
						return true;
					}
					catch (Exception ex)
					{
						logger.Error("{0}: GetValue error: {1}", "Swrve: ", ex);
					}
					break;
				}
			}
			value = default(T);
			return false;
		}

		private void DebugLogTelemetryEvent(TelemetryEvent gameEvent)
		{
			KampaiLogLevel level = KampaiLogLevel.Debug;
			logger.Log(level, "{0}TelemetryEvent()... event {1}: params count: {2}", "Swrve: ", gameEvent.Type.ToString(), gameEvent.Parameters.Count);
			foreach (TelemetryParameter parameter in gameEvent.Parameters)
			{
				logger.Log(level, "{0} name {1}, value {2}, keyType {3}", "Swrve: ", parameter.name, parameter.value, parameter.keyType);
			}
			logger.Log(level, "{0}...TelemetryEvent()", "Swrve: ");
		}

		private Dictionary<string, string> GetSwrvePayload(TelemetryEvent gameEvent, out string eventName)
		{
			eventName = null;
			SwrveEventConverter value;
			if (eventNameConverters.TryGetValue(gameEvent.Type, out value))
			{
				eventName = value(gameEvent);
				if (eventName == null)
				{
					return null;
				}
			}
			if (eventName == null)
			{
				logger.Error("{0} GetSwrvePayload(): unhandled event type {1}. Skip sending event to Swrve.", "Swrve: ", gameEvent.Type);
				return null;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < gameEvent.Parameters.Count; i++)
			{
				TelemetryParameter telemetryParameter = gameEvent.Parameters[i];
				string swrveParameterName = GetSwrveParameterName(telemetryParameter.name);
				string text = ((telemetryParameter.value != null) ? telemetryParameter.value.ToString() : string.Empty);
				string value2;
				if (!dictionary.TryGetValue(swrveParameterName, out value2))
				{
					dictionary.Add(swrveParameterName, text);
				}
				else
				{
					dictionary[swrveParameterName] = string.Format("{0}{1}{2}", value2, ",", text);
				}
			}
			return dictionary;
		}

		private Dictionary<SynergyTrackingEventType, SwrveEventConverter> PrepareEventConverters()
		{
			Dictionary<SynergyTrackingEventType, SwrveEventConverter> dictionary = new Dictionary<SynergyTrackingEventType, SwrveEventConverter>();
			dictionary.Add(SynergyTrackingEventType.EVT_GAME_ERROR_GAMEPLAY, (TelemetryEvent gameEvent) => "error.gameplay");
			dictionary.Add(SynergyTrackingEventType.EVT_GAME_ERROR_CONNECTIVITY, (TelemetryEvent gameEvent) => "error.connectivity");
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_EARNED, delegate(TelemetryEvent gameEvent)
			{
				string value23 = GetValue(gameEvent.Parameters, ParameterName.EVENT_NAME, "unknown");
				return string.Format("economy.grind.earned.{0}", value23);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_EARNED, delegate(TelemetryEvent gameEvent)
			{
				string value22 = GetValue(gameEvent.Parameters, ParameterName.EVENT_NAME, "unknown");
				return string.Format("economy.premium.earned.{0}", value22);
			});
			SwrveEventConverter value = delegate(TelemetryEvent gameEvent)
			{
				string value21 = GetValue(gameEvent.Parameters, ParameterName.ITEM_PURCHASED, "unknown");
				return string.Format("economy.item.{0}.purchased", value21);
			};
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_FREE_CREDITS_PURCHASE_REVENUE, value);
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_PAID_CREDITS_PURCHASE_REVENUE, value);
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_RESOURCE_CRAFTABLE_EARNED, delegate(TelemetryEvent gameEvent)
			{
				string value20 = GetValue(gameEvent.Parameters, ParameterName.ITEM_NAME, "unknown");
				return string.Format("economy.resource.craftable.earned.{0}", value20);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_RESOURCE_CRAFTABLE_SPENT, delegate(TelemetryEvent gameEvent)
			{
				string value19 = GetValue(gameEvent.Parameters, ParameterName.ITEM_NAME, "unknown");
				return string.Format("economy.resource.craftable.spent.{0}", value19);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_IGE_STORE_VISIT, delegate(TelemetryEvent gameEvent)
			{
				string value18 = GetValue(gameEvent.Parameters, ParameterName.STORE_VISITED, "unknown");
				return string.Format("economy.store.{0}.visit", value18);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_USER_TUTORIAL_FUNNEL_EAL, delegate(TelemetryEvent gameEvent)
			{
				string value16 = GetValue(gameEvent.Parameters, ParameterName.TUTORIAL_NAME, "unknown");
				string value17 = GetValue(gameEvent.Parameters, ParameterName.STEP, "unknown");
				return string.Format("tutorial.{0}.step.{1}", value16, value17);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_USER_GAME_LOAD_FUNNEL, delegate(TelemetryEvent gameEvent)
			{
				string value15 = GetValue(gameEvent.Parameters, ParameterName.STEP, "unknown");
				return string.Format("game.load.step.{0}", value15);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL, delegate(TelemetryEvent gameEvent)
			{
				string value12 = GetValue(gameEvent.Parameters, ParameterName.ACHIEVEMENT_NAME, "unknown");
				string value13 = GetValue(gameEvent.Parameters, ParameterName.ARCHIVEMENT_TYPE, "unknown");
				string value14;
				return TryGetValue<string>(gameEvent.Parameters, ParameterName.PROCEDURAL_QUEST_END_STATE, out value14) ? string.Format("achievement.checkpoint.procedural.{0}.{1}.{2}", value13, value12, value14) : string.Format("achievement.checkpoint.{0}.{1}", value13, value12);
			});
			dictionary.Add(SynergyTrackingEventType.ACHIEVEMENT_OR_QUEST_STARTED, delegate(TelemetryEvent gameEvent)
			{
				string value10 = GetValue(gameEvent.Parameters, ParameterName.ACHIEVEMENT_NAME, "unknown");
				string value11 = GetValue(gameEvent.Parameters, ParameterName.ARCHIVEMENT_TYPE, "unknown");
				return string.Format("achievements.started.{0}.{1}", value11, value10);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_GP_LEVEL_PROMOTION, (TelemetryEvent gameEvent) => "game.progression.levelup");
			dictionary.Add(SynergyTrackingEventType.EVT_GC_SIGNON, (TelemetryEvent gameEvent) => "social.login.gamecenter");
			dictionary.Add(SynergyTrackingEventType.EVT_GP_SIGNON, (TelemetryEvent gameEvent) => "social.login.googleplay");
			dictionary.Add(SynergyTrackingEventType.EVT_FB_SIGNON, (TelemetryEvent gameEvent) => "social.login.facebook");
			dictionary.Add(SynergyTrackingEventType.EVT_IPAD_UPSELL_MESSAGE_DISPLAYED, delegate(TelemetryEvent gameEvent)
			{
				string value9 = GetValue(gameEvent.Parameters, ParameterName.IN_APP_MESSAGE_NAME, "unknown");
				return string.Format("engagement.inappmessage.{0}", value9);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_USER_TRACKING_OPTOUT, (TelemetryEvent gameEvent) => "app.usageSharing.optout");
			dictionary.Add(SynergyTrackingEventType.EVT_MINI_GAME_PLAYED, delegate(TelemetryEvent gameEvent)
			{
				string value8 = GetValue(gameEvent.Parameters, ParameterName.MIGNETTE_NAME, "unknown");
				return string.Format("gameplay.minigameplayed.{0}", value8);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_MINI_TIER_REACHED, delegate(TelemetryEvent gameEvent)
			{
				string value7 = GetValue(gameEvent.Parameters, ParameterName.MIGNETTE_NAME, "unknown");
				return string.Format("gameplay.minigamereward.{0}", value7);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_USER_DATA_AT_APP_START, (TelemetryEvent gameEvent) => "app.started");
			dictionary.Add(SynergyTrackingEventType.EVT_USER_DATA_AT_APP_CLOSE, (TelemetryEvent gameEvent) => "app.closed");
			dictionary.Add(SynergyTrackingEventType.EVT_STORAGE_LIMIT_HIT, (TelemetryEvent gameEvent) => "economy.storageLimit");
			dictionary.Add(SynergyTrackingEventType.EVT_SOCIAL_EVENT_COMPLETION, (TelemetryEvent gameEvent) => "social.event.complete");
			dictionary.Add(SynergyTrackingEventType.EVT_SOCIAL_EVENT_CONTRIBUTION, (TelemetryEvent gameEvent) => "social.event.contribution");
			dictionary.Add(SynergyTrackingEventType.EVT_MINION_PARTY_STARTED, (TelemetryEvent gameEvent) => "party.event.partystarted");
			dictionary.Add(SynergyTrackingEventType.EVT_PARTY_POINTS_EARNED, (TelemetryEvent gameEvent) => "party.event.pointsearned");
			dictionary.Add(SynergyTrackingEventType.EVT_NOTE_SETTING_CHANGE, (TelemetryEvent gameEvent) => "notifications.change");
			dictionary.Add(SynergyTrackingEventType.EVT_MARKETPLACE_ITEM_LISTED, delegate(TelemetryEvent gameEvent)
			{
				string value6 = GetValue(gameEvent.Parameters, ParameterName.ITEM_NAME, "unknown");
				return string.Format("markeplace.itemListed.{0}", value6);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_MARKETPLACE_VIEWED, (TelemetryEvent gameEvent) => "markeplace.viewed");
			dictionary.Add(SynergyTrackingEventType.EVT_RATE_MY_APP, delegate(TelemetryEvent gameEvent)
			{
				string value5 = GetValue(gameEvent.Parameters, ParameterName.USER_CHOICE, "unknown");
				return string.Format("gameplay.rateapp.userchoise.{0}", value5);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_ORDER_COMPLETED_SUM, delegate(TelemetryEvent gameEvent)
			{
				string value4 = GetValue(gameEvent.Parameters, ParameterName.PRESTIGED_WITH, "unknown");
				return string.Format("gameplay.orderboard.{0}.completed", value4);
			});
			dictionary.Add(SynergyTrackingEventType.EVT_ORDER_CANCEL_SUM, delegate(TelemetryEvent gameEvent)
			{
				string value3 = GetValue(gameEvent.Parameters, ParameterName.PRESTIGED_WITH, "unknown");
				return string.Format("gameplay.orderboard.{0}.canceled", value3);
			});
			SwrveEventConverter value2 = (TelemetryEvent gameEvent) => (string)null;
			dictionary.Add(SynergyTrackingEventType.EVT_ORDER_COMPLETED_DET, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_ORDER_CANCEL_DET, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_DCN, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_PARTY_SKIPPED, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_UPSELL_NOTIFICATION, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_CHARACTER_PRESTIGED, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_PLAYER_TRAINING, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_PINCH_PROMPT, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_COPPA_AGE_GATE, value2);
			dictionary.Add(SynergyTrackingEventType.EVT_USER_GAME_DOWNLOAD_FUNNEL, value2);
			return dictionary;
		}

		private void DebugSwrveEventPayload(string eventName, Dictionary<string, string> payload)
		{
			KampaiLogLevel level = KampaiLogLevel.Debug;
			logger.Log(level, "{0}DebugSwrvePayload()...: eventName {1}, params count: {2}", "Swrve: ", eventName, payload.Count);
			foreach (KeyValuePair<string, string> item in payload)
			{
				logger.Log(level, "{0} key {1}, value {2}", "Swrve: ", item.Key, item.Value);
			}
			logger.Log(level, "{0}...DebugSwrvePayload()", "Swrve: ");
		}

		private string GetSwrveParameterName(ParameterName name)
		{
			return name.ToString().ToLower().Replace('_', '.');
		}

		public void SendUserStatsUpdate()
		{
			if (!PlayServiceReady())
			{
				logger.Log(KampaiLogLevel.Debug, "SwrveSendUserStatsUpdate - PlayService is not Ready Yet");
				return;
			}
			if (SwrveComponent.Instance == null)
			{
				logger.Log(KampaiLogLevel.Debug, "SwrveComponent.Instance is null");
				return;
			}
			Dictionary<string, string> levelPremiumGrind = GetLevelPremiumGrind();
			SwrveComponent.Instance.SDK.UserUpdate(levelPremiumGrind);
		}

		public void SetPlayerServiceReference(IPlayerService playerService)
		{
			this.playerService = playerService;
		}

		private Dictionary<string, string> GetLevelPremiumGrind()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["level"] = GetPlayerItemString(StaticItem.LEVEL_ID);
			dictionary["GRIND"] = GetPlayerItemString(StaticItem.GRIND_CURRENCY_ID);
			dictionary["PREMIUM"] = GetPlayerItemString(StaticItem.PREMIUM_CURRENCY_ID);
			return dictionary;
		}

		private string GetPlayerItemString(StaticItem item)
		{
			return GetPlayerItem(item).ToString();
		}

		private uint GetPlayerItem(StaticItem item)
		{
			return PlayServiceReady() ? playerService.GetQuantity(item) : 0u;
		}

		private bool PlayServiceReady()
		{
			return playerService != null && playerService.IsPlayerInitialized();
		}

		private void ResourcesUpdatedCallback()
		{
			logger.Debug("{0}ResourcesUpdatedCallback(): resourcesResponseReceived: {1}, resourcesResponseWaitTimerExpired = {2}", "Swrve: ", resourcesResponseReceived, resourcesResponseWaitTimerExpired);
			SwrveComponent.Instance.SDK.ResourcesUpdatedCallback = null;
			if (!resourcesResponseWaitTimerExpired)
			{
				resourcesResponseReceived = true;
				OnResourcesUpdated(true);
			}
		}

		private IEnumerator SwrveResourceRequestExpiration()
		{
			yield return new WaitForSeconds(5f);
			logger.Debug("{0}SwrveResourceRequestExpiration(): resourcesResponseReceived: {1}", "Swrve: ", resourcesResponseReceived);
			resourcesResponseWaitTimerExpired = true;
			if (!resourcesResponseReceived)
			{
				OnResourcesUpdated(false);
			}
		}

		private void OnResourcesUpdated(bool succeed)
		{
			SetupABTestModel();
			abTestResourcesUpdatedSignal.Dispatch(succeed);
		}

		private void SetupABTestModel()
		{
			logger.Debug("{0}SetupABTestModel()", "Swrve: ");
			if (!ABTestModel.debugConsoleTest)
			{
				logger.Debug("{0}Init(): setup definitions based on A/B test.", "Swrve: ");
				string definitionsVariants = GetDefinitionsVariants();
				if (!string.IsNullOrEmpty(definitionsVariants))
				{
					ABTestCommand.GameMetaData gameMetaData = new ABTestCommand.GameMetaData();
					gameMetaData.definitionVariants = definitionsVariants;
					abTestSignal.Dispatch(gameMetaData);
				}
				logger.Debug("{0}Init(): definition variants string [{1}]", "Swrve: ", definitionsVariants);
			}
			else
			{
				logger.Debug("{0}Init(): Skip live A/B test because of debug console test is in progress.", "Swrve: ");
			}
		}

		private string GetDefinitionsVariants()
		{
			logger.Debug("{0}GetDefinitionsPath()", "Swrve: ");
			DebugLogDeleteMe();
			List<string> variantValuesOfAllResources = GetVariantValuesOfAllResources();
			return FormatVariantRequest(variantValuesOfAllResources);
		}

		private IapRewards GetRewards(TransactionDefinition transactionDef)
		{
			IapRewards iapRewards = new IapRewards();
			foreach (QuantityItem output in transactionDef.Outputs)
			{
				if (output.ID == 0)
				{
					iapRewards.AddCurrency("GRIND", output.Quantity);
					continue;
				}
				if (output.ID == 1)
				{
					iapRewards.AddCurrency("PREMIUM", output.Quantity);
					continue;
				}
				logger.Error("Unsupported reward type. Proper name is required. Sending as item");
				string resourceName = string.Format("ID_{0}", output.ID);
				iapRewards.AddItem(resourceName, output.Quantity);
			}
			return iapRewards;
		}

		private string FormatVariantRequest(List<string> variants)
		{
			return string.Join("_", variants.ToArray());
		}

		private List<string> GetVariantValuesOfAllResources()
		{
			List<string> list = new List<string>();
			SwrveResourceManager resourceManager = SwrveComponent.Instance.SDK.ResourceManager;
			if (resourceManager.UserResources != null)
			{
				foreach (KeyValuePair<string, SwrveResource> userResource in resourceManager.UserResources)
				{
					string attribute = userResource.Value.GetAttribute("variant", "default");
					if (!attribute.Equals("default"))
					{
						list.Add(attribute);
					}
				}
			}
			return list;
		}

		private void DebugLogDeleteMe()
		{
			KampaiLogLevel level = KampaiLogLevel.Debug;
			logger.Log(level, "{0}DebugLog()...", "Swrve: ");
			SwrveResourceManager resourceManager = SwrveComponent.Instance.SDK.ResourceManager;
			logger.Log(level, "{0}UserResources...: {1} resources", "Swrve: ", (resourceManager.UserResources == null) ? "0" : resourceManager.UserResources.Count.ToString());
			if (resourceManager.UserResources != null)
			{
				foreach (KeyValuePair<string, SwrveResource> userResource in resourceManager.UserResources)
				{
					Dictionary<string, string> attributes = userResource.Value.Attributes;
					logger.Log(level, "{0}UserResource...: name: {1}, attributes count: {2}", "Swrve: ", userResource.Key, attributes.Count);
					foreach (KeyValuePair<string, string> item in attributes)
					{
						logger.Log(level, "{0}: attr name: {1}, value: {2}", "Swrve: ", item.Key, item.Value);
					}
					logger.Log(level, "{0}...UserResource", "Swrve: ");
				}
			}
			logger.Log(level, "{0}...UserResources", "Swrve: ");
			logger.Log(level, "{0}...DebugLog()", "Swrve: ");
		}

		private void DebugLogRewards(IapRewards rewards)
		{
			KampaiLogLevel level = KampaiLogLevel.Debug;
			logger.Log(level, "{0}DebugLogRewards()...: count: {1}", "Swrve: ", rewards.getRewards().Count);
			foreach (KeyValuePair<string, Dictionary<string, string>> reward in rewards.getRewards())
			{
				logger.Log(level, "{0} reward: name: {1}", "Swrve: ", reward.Key);
				foreach (KeyValuePair<string, string> item in reward.Value)
				{
					logger.Log(level, "{0} reward: key {1}, value: {2}", "Swrve: ", item.Key, item.Value);
				}
			}
			logger.Log(level, "{0}...DebugLogRewards()", "Swrve: ");
		}
	}
}
