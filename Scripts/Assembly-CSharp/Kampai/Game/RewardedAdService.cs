using System;
using System.Collections.Generic;
using System.Text;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class RewardedAdService : IRewardedAdService
	{
		private sealed class AdPlacementKey : IEquatable<AdPlacementKey>
		{
			public AdPlacementName PlacementName;

			public int PlacementInstanceId;

			public override bool Equals(object obj)
			{
				return Equals(obj as AdPlacementKey);
			}

			public bool Equals(AdPlacementKey p)
			{
				if (p == null)
				{
					return false;
				}
				return PlacementName == p.PlacementName && PlacementInstanceId == p.PlacementInstanceId;
			}

			public override int GetHashCode()
			{
				return (int)PlacementName ^ PlacementInstanceId;
			}
		}

		private sealed class PendingAdImpressionData
		{
			public AdPlacementInstance PlacementInstance { get; private set; }

			public TransactionDefinition Reward { get; private set; }

			public PendingAdImpressionData(AdPlacementInstance placementInstance, TransactionDefinition reward)
			{
				PlacementInstance = placementInstance;
				Reward = reward;
			}
		}

		private const string TAG = "Ads: ";

		private IKampaiLogger logger = LogManager.GetClassLogger("RewardedAdService") as IKampaiLogger;

		private int rewardPerDayCountTotal;

		private Dictionary<AdPlacementKey, AdPlacementInstance> adPlacements = new Dictionary<AdPlacementKey, AdPlacementInstance>();

		private Signal<int> updatePlacementConditionsByTimerSignal = new Signal<int>();

		private PendingAdImpressionData pendingAdImpressionData;

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public AppResumeCompletedSignal appResumeCompletedSignal { get; set; }

		[Inject]
		public PlayerSessionCountUpdatedSignal playerSessionCountUpdatedSignal { get; set; }

		[Inject]
		public ISupersonicService supersonicService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ResetRewardedAdLimitSignal resetRewardedAdLimitSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		[Inject]
		public SupersonicVideoAdAvailabilityChangedSignal supersonicVideoAdAvailabilityChangedSignal { get; set; }

		[Inject]
		public SupersonicVideoAdShowSignal supersonicVideoAdShowSignal { get; set; }

		[Inject]
		public SupersonicVideoAdRewardedSignal supersonicVideoAdRewardedSignal { get; set; }

		[Inject]
		public DeclineRewardedAdShowSignal declineRewadedAdShowSignal { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		private AdPlacementKey GetKey(AdPlacementName placementName, int instanceId)
		{
			AdPlacementKey adPlacementKey = new AdPlacementKey();
			adPlacementKey.PlacementName = placementName;
			adPlacementKey.PlacementInstanceId = instanceId;
			return adPlacementKey;
		}

		public void Initialize()
		{
			reInitializeGameSignal.AddListener(OnReinitializeGame);
			SubscribeOnTriggerConditionsSignals(true);
			SubscribeOnCooldownConditionSignals(true);
			SubscribeOnRewardedVideoSignals(true);
			LoadPlacementsFromPlayer();
			CleanupInvalidPlacements();
			CheckPlacementConditions();
		}

		public void Destroy()
		{
			reInitializeGameSignal.RemoveListener(OnReinitializeGame);
			SubscribeOnRewardedVideoSignals(false);
			SubscribeForCooldownUpdate(false);
			SubscribeOnCooldownConditionSignals(false);
			SubscribeOnTriggerConditionsSignals(false);
		}

		private void OnReinitializeGame(string notused)
		{
			Destroy();
		}

		private void LoadPlacementsFromPlayer()
		{
			foreach (AdPlacementInstance item in playerService.GetInstancesByType<AdPlacementInstance>())
			{
				AdPlacementKey key = GetKey(item.Definition.Name, item.PlacementInstanceId);
				adPlacements.Add(key, item);
			}
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				value.ActivityStateOnLastCheck = IsPlacementActive(value);
			}
		}

		public bool IsRewardedVideoAvailable()
		{
			return supersonicService.IsRewardedVideoAvailable();
		}

		public bool IsPlacementAvailable(AdPlacementName placementName)
		{
			AdPlacementDefinition placement = GetPlacement(placementName);
			return placement != null;
		}

		public AdPlacementInstance GetPlacementInstance(AdPlacementName placementName, int instanceId = 0)
		{
			AdPlacementDefinition placement = GetPlacement(placementName);
			if (placement == null)
			{
				logger.Debug("{0}IsPlacementActive(): placement is not available", "Ads: ");
				return null;
			}
			return GetOrCreatePlacementInstance(placement, instanceId);
		}

		public bool IsPlacementActive(AdPlacementName placementName, int instanceId = 0)
		{
			AdPlacementInstance placementInstance = GetPlacementInstance(placementName, instanceId);
			return IsPlacementActive(placementInstance);
		}

		private bool IsPlacementActive(AdPlacementInstance instance)
		{
			if (instance == null)
			{
				return false;
			}
			if (!IsRewardedVideoAvailable())
			{
				return false;
			}
			if (IsTotalRewardLimitPerPeriodExceeded())
			{
				return false;
			}
			return instance.IsActive(timeService.CurrentTime());
		}

		private AdPlacementDefinition GetPlacement(AdPlacementName placementName)
		{
			RewardedAdvertisementDefinition rewardedAdvertisementDefinition = definitionService.Get<RewardedAdvertisementDefinition>();
			if (rewardedAdvertisementDefinition != null)
			{
				List<AdPlacementDefinition> placementDefinitions = rewardedAdvertisementDefinition.PlacementDefinitions;
				for (int i = 0; i < placementDefinitions.Count; i++)
				{
					AdPlacementDefinition adPlacementDefinition = placementDefinitions[i];
					if (adPlacementDefinition != null && adPlacementDefinition.Name == placementName && adPlacementDefinition.IsAvailable(gameContext))
					{
						return adPlacementDefinition;
					}
				}
			}
			return null;
		}

		private AdPlacementInstance GetOrCreatePlacementInstance(AdPlacementDefinition placementDefinition, int instanceId = 0)
		{
			AdPlacementKey key = GetKey(placementDefinition.Name, instanceId);
			AdPlacementInstance value;
			if (!adPlacements.TryGetValue(key, out value))
			{
				value = (AdPlacementInstance)placementDefinition.Build();
				value.PlacementInstanceId = key.PlacementInstanceId;
				playerService.AssignNextInstanceId(value);
				adPlacements.Add(key, value);
				playerService.Add(value);
			}
			return value;
		}

		private void CheckPlacementConditions()
		{
			RewardedAdvertisementDefinition rewardedAdvertisementDefinition = definitionService.Get<RewardedAdvertisementDefinition>();
			if (rewardedAdvertisementDefinition == null)
			{
				logger.Error("{0}RewardedAdvertisementDefinition does not exist.", "Ads: ");
				return;
			}
			List<AdPlacementDefinition> placementDefinitions = rewardedAdvertisementDefinition.PlacementDefinitions;
			for (int j = 0; j < placementDefinitions.Count; j++)
			{
				HandlePlacementDefinitionChange(placementDefinitions[j]);
			}
			bool flag = !IsTotalRewardLimitPerPeriodExceeded(rewardedAdvertisementDefinition);
			for (int k = 0; k < placementDefinitions.Count; k++)
			{
				AdPlacementDefinition adPlacementDefinition = placementDefinitions[k];
				bool isPlacementActive = flag && adPlacementDefinition.IsAvailable(gameContext) && IsRewardedVideoAvailable();
				NotifyPlacementActivityStateChange(adPlacementDefinition, isPlacementActive);
			}
			RemovePlacements((AdPlacementInstance i) => !i.Definition.IsAvailable(gameContext));
			SubscribeForCooldownUpdate(true);
		}

		private bool IsTotalRewardLimitPerPeriodExceeded(RewardedAdvertisementDefinition adDefinition = null)
		{
			if (adDefinition == null)
			{
				adDefinition = definitionService.Get<RewardedAdvertisementDefinition>();
				if (adDefinition == null)
				{
					return true;
				}
			}
			if (adDefinition.MaxRewardsPerDayGlobal < 0)
			{
				return false;
			}
			UpdateTotalRewardPerDayCount();
			return rewardPerDayCountTotal >= adDefinition.MaxRewardsPerDayGlobal;
		}

		private void SubscribeForCooldownUpdate(bool subscribe)
		{
			int num = timeService.CurrentTime();
			AdPlacementInstance adPlacementInstance = null;
			int num2 = int.MaxValue;
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				int num3 = value.LastRewardPeriodStartTimestamp + 86400;
				if (num3 < num2 && num3 > num)
				{
					num2 = num3;
					adPlacementInstance = value;
				}
				if (!value.IsActive(num))
				{
					int iD = value.ID;
					timeEventService.RemoveEvent(iD);
					if (subscribe)
					{
						int cooldownEndTimestamp = value.GetCooldownEndTimestamp(num);
						SchedulePlacementConditionsUpdate(iD, num, cooldownEndTimestamp);
					}
				}
			}
			if (adPlacementInstance != null)
			{
				int cooldownEndTimestamp2 = adPlacementInstance.GetCooldownEndTimestamp(num);
				int num4 = num2;
				if (cooldownEndTimestamp2 < num4 && cooldownEndTimestamp2 > num)
				{
					num4 = cooldownEndTimestamp2;
				}
				int iD2 = adPlacementInstance.ID;
				timeEventService.RemoveEvent(iD2);
				if (subscribe)
				{
					SchedulePlacementConditionsUpdate(iD2, num, num4);
				}
			}
		}

		private void SchedulePlacementConditionsUpdate(int eventID, int currentTimeUTC, int nextUpdateTimestamp)
		{
			int num = nextUpdateTimestamp - currentTimeUTC;
			if (num > 0)
			{
				num++;
				timeEventService.AddEvent(eventID, currentTimeUTC, num, updatePlacementConditionsByTimerSignal);
			}
		}

		private void UpdateTotalRewardPerDayCount()
		{
			rewardPerDayCountTotal = CalcTotalRewardPerDayCount();
		}

		private int CalcTotalRewardPerDayCount()
		{
			int num = 0;
			int currentTimeUTC = timeService.CurrentTime();
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				value.UpdateRewardLimitCounter(currentTimeUTC);
				num += value.RewardPerPeriodCount;
			}
			return num;
		}

		private void HandlePlacementDefinitionChange(AdPlacementDefinition placementDefinition)
		{
			AdPlacementName name = placementDefinition.Name;
			int currentTimeUTC = timeService.CurrentTime();
			HashSet<AdPlacementKey> hashSet = null;
			bool flag = IsPlacementAvailable(name);
			bool flag2 = true;
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				if (adPlacement.Key.PlacementName != name)
				{
					continue;
				}
				flag2 = false;
				bool flag3 = flag && value.IsActive(currentTimeUTC);
				if (value.ActivityStateOnLastCheck && (!flag3 || placementDefinition.ID != value.Definition.ID))
				{
					flag3 = (value.ActivityStateOnLastCheck = false);
					adPlacementActivityStateChangedSignal.Dispatch(value, flag3);
				}
				if (placementDefinition.ID != value.Definition.ID)
				{
					if (hashSet == null)
					{
						hashSet = new HashSet<AdPlacementKey>();
					}
					hashSet.Add(adPlacement.Key);
				}
			}
			if (flag2)
			{
				GetOrCreatePlacementInstance(placementDefinition);
			}
			if (hashSet == null)
			{
				return;
			}
			foreach (AdPlacementKey item in hashSet)
			{
				AdPlacementInstance i = adPlacements[item];
				adPlacements.Remove(item);
				playerService.Remove(i);
				GetOrCreatePlacementInstance(placementDefinition, item.PlacementInstanceId);
			}
		}

		private void NotifyPlacementActivityStateChange(AdPlacementDefinition placementDefinition, bool isPlacementActive)
		{
			int currentTimeUTC = timeService.CurrentTime();
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				if (adPlacement.Key.PlacementName != placementDefinition.Name)
				{
					continue;
				}
				bool flag = isPlacementActive && value.IsActive(currentTimeUTC);
				if (flag)
				{
					if (!value.ActivityStateOnLastCheck && flag)
					{
						value.ActivityStateOnLastCheck = true;
						adPlacementActivityStateChangedSignal.Dispatch(value, flag);
					}
				}
				else if (value.ActivityStateOnLastCheck && !flag)
				{
					value.ActivityStateOnLastCheck = false;
					adPlacementActivityStateChangedSignal.Dispatch(value, flag);
				}
			}
		}

		private void SubscribeOnTriggerConditionsSignals(bool subscribe)
		{
			if (subscribe)
			{
				awardLevelSignal.AddListener(OnAwardLevel);
				appResumeCompletedSignal.AddListener(CheckPlacementConditions);
				playerSessionCountUpdatedSignal.AddListener(CheckPlacementConditions);
			}
			else
			{
				awardLevelSignal.RemoveListener(OnAwardLevel);
				appResumeCompletedSignal.RemoveListener(CheckPlacementConditions);
				playerSessionCountUpdatedSignal.RemoveListener(CheckPlacementConditions);
			}
		}

		private void SubscribeOnCooldownConditionSignals(bool subscribe)
		{
			if (subscribe)
			{
				resetRewardedAdLimitSignal.AddListener(OnResetRewardedAdLimits);
				updatePlacementConditionsByTimerSignal.AddListener(CheckPlacementConditionsByTimer);
			}
			else
			{
				resetRewardedAdLimitSignal.RemoveListener(OnResetRewardedAdLimits);
				updatePlacementConditionsByTimerSignal.RemoveListener(CheckPlacementConditionsByTimer);
			}
		}

		private void SubscribeOnRewardedVideoSignals(bool subscribe)
		{
			if (subscribe)
			{
				supersonicVideoAdAvailabilityChangedSignal.AddListener(OnVideoAdAvailabilityChanged);
				supersonicVideoAdShowSignal.AddListener(OnVideoAdShow);
				supersonicVideoAdRewardedSignal.AddListener(OnVideoAdRewarded);
				declineRewadedAdShowSignal.AddListener(OnAdWatchDecline);
			}
			else
			{
				supersonicVideoAdAvailabilityChangedSignal.RemoveListener(OnVideoAdAvailabilityChanged);
				supersonicVideoAdShowSignal.RemoveListener(OnVideoAdShow);
				supersonicVideoAdRewardedSignal.RemoveListener(OnVideoAdRewarded);
				declineRewadedAdShowSignal.RemoveListener(OnAdWatchDecline);
			}
		}

		private void CleanupInvalidPlacements()
		{
			RemovePlacements((AdPlacementInstance i) => i.Definition.Name == AdPlacementName.INVALID);
		}

		private void RemovePlacements(Func<AdPlacementInstance, bool> predicate)
		{
			List<AdPlacementKey> list = new List<AdPlacementKey>();
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				if (predicate(adPlacement.Value))
				{
					list.Add(adPlacement.Key);
				}
			}
			foreach (AdPlacementKey item in list)
			{
				AdPlacementInstance i = adPlacements[item];
				adPlacements.Remove(item);
				playerService.Remove(i);
			}
		}

		private void OnAwardLevel(TransactionDefinition td)
		{
			CheckPlacementConditions();
		}

		private void CheckPlacementConditionsByTimer(int id)
		{
			CheckPlacementConditions();
		}

		private void OnResetRewardedAdLimits()
		{
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				adPlacement.Value.ResetCooldown();
			}
			UpdateTotalRewardPerDayCount();
			CheckPlacementConditions();
		}

		private void OnVideoAdAvailabilityChanged(bool videoAvailable)
		{
			CheckPlacementConditions();
		}

		public void ShowRewardedVideo(AdPlacementInstance instance, TransactionDefinition reward)
		{
			if (pendingAdImpressionData != null)
			{
				logger.Error("ShowRewardedVideo(): Unexpected invokation: previous watch-reward exists", "Ads: ");
			}
			pendingAdImpressionData = new PendingAdImpressionData(instance, reward);
			supersonicService.ShowRewardedVideo();
		}

		private void OnVideoAdShow()
		{
			if (pendingAdImpressionData != null)
			{
				pendingAdImpressionData.PlacementInstance.ActivateWatchAdAcceptCooldown(timeService.CurrentTime());
			}
			CheckPlacementConditions();
		}

		private void OnAdWatchDecline(AdPlacementInstance instance)
		{
			instance.ActivateWatchAdDeclineCooldown(timeService.CurrentTime());
			CheckPlacementConditions();
		}

		private void OnVideoAdRewarded()
		{
			if (pendingAdImpressionData == null)
			{
				logger.Error("{0}OnVideoAdRewarded(): Unexpected null pending ad impression data", "Ads: ");
				return;
			}
			AdPlacementInstance placementInstance = pendingAdImpressionData.PlacementInstance;
			TransactionDefinition reward = pendingAdImpressionData.Reward;
			if (reward != null)
			{
				RewardPlayer(reward, placementInstance);
			}
			rewardedAdRewardSignal.Dispatch(placementInstance);
			pendingAdImpressionData = null;
			CheckPlacementConditions();
		}

		public void RewardPlayer(TransactionDefinition reward, AdPlacementInstance placementInstance)
		{
			if (reward != null)
			{
				TransactionArg transactionArg = new TransactionArg();
				transactionArg.Source = "RewardedVideo";
				TransactionArg arg = transactionArg;
				playerService.RunEntireTransaction(reward, TransactionTarget.NO_VISUAL, null, arg);
			}
			placementInstance.RegisterReward(timeService.CurrentTime());
			CheckPlacementConditions();
		}

		public string GetPlacementsReport()
		{
			StringBuilder stringBuilder = new StringBuilder();
			RewardedAdvertisementDefinition rewardedAdvertisementDefinition = definitionService.Get<RewardedAdvertisementDefinition>();
			if (rewardedAdvertisementDefinition == null)
			{
				stringBuilder.AppendFormat("RewardedAdvertisementDefinition is missing in definitions, ad is disabled");
				return stringBuilder.ToString();
			}
			stringBuilder.AppendFormat("Supersonic rewarded video available: {0}\n", supersonicService.IsRewardedVideoAvailable());
			KillSwitch killSwitch = KillSwitch.SUPERSONIC;
			stringBuilder.AppendFormat("{0} killswitch status: {1}\n", killSwitch, (!configurationsService.isKillSwitchOn(killSwitch)) ? "off" : "on");
			stringBuilder.AppendFormat("Country: {0}\n", localizationService.GetCountry() ?? "null");
			stringBuilder.AppendFormat("Player level: {0}\n", playerService.GetQuantity(StaticItem.LEVEL_ID));
			stringBuilder.AppendFormat("Player session count: {0}\n", playerService.GetQuantity(StaticItem.PLAYER_SESSION_COUNT));
			bool flag = IsTotalRewardLimitPerPeriodExceeded(rewardedAdvertisementDefinition);
			stringBuilder.AppendFormat("Global reward limit per day exceeded: {0}, global reward count: {1}, limit: {2}\n", flag, rewardPerDayCountTotal, rewardedAdvertisementDefinition.MaxRewardsPerDayGlobal);
			stringBuilder.AppendLine("Placements availablitity status:");
			List<AdPlacementDefinition> placementDefinitions = rewardedAdvertisementDefinition.PlacementDefinitions;
			string empty = string.Empty;
			for (int i = 0; i < placementDefinitions.Count; i++)
			{
				AdPlacementDefinition adPlacementDefinition = placementDefinitions[i];
				stringBuilder.AppendFormat("{0}placement {1}(ID:{2}) available: {3}\n", empty + "\t", adPlacementDefinition.Name, adPlacementDefinition.ID, adPlacementDefinition.IsAvailable(gameContext));
			}
			stringBuilder.AppendLine("Placement instances activity status:");
			int num = timeService.CurrentTime();
			foreach (KeyValuePair<AdPlacementKey, AdPlacementInstance> adPlacement in adPlacements)
			{
				AdPlacementInstance value = adPlacement.Value;
				AdPlacementDefinition definition = value.Definition;
				bool flag2 = value.IsActive(num);
				stringBuilder.AppendFormat("{0}placement instance {1}(ID:{2}) available: {3}\n", empty + "\t", definition.Name, value.ID, flag2);
				if (!flag2)
				{
					stringBuilder.AppendFormat("{0}reward limit per day exceeded: {1}, reward per period count: {2}, limit: {3}\n", empty + "\t\t", value.IsRewardLimitPerPeriodExceeded(num), value.RewardPerPeriodCount, definition.MaxRewardsPerDay);
					bool flag3 = num - value.LastWatchAdAcceptTimestamp < definition.CooldownSeconds;
					stringBuilder.AppendFormat("{0}cooldown after watch accept: {1}. Cooldown duration sec: {2}\n", empty + "\t\t", flag3, definition.CooldownSeconds);
					bool flag4 = num - value.LastWatchAdDeclineTimestamp < definition.CooldownWatchDeclineSeconds;
					stringBuilder.AppendFormat("{0}cooldown after watch decline: {1}. Cooldown duration sec: {2}\n", empty + "\t\t", flag4, definition.CooldownWatchDeclineSeconds);
					DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value.GetCooldownEndTimestamp(num)).ToLocalTime();
					stringBuilder.AppendFormat("{0}cooldown ends at: {1}(local device time)\n", empty + "\t\t", dateTime);
				}
			}
			string text = stringBuilder.ToString();
			logger.Error("{0}GetPlacementsReport(): {1}", "Ads: ", text);
			return text;
		}
	}
}
