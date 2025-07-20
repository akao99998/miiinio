using System;
using System.Collections.Generic;
using Kampai.Game.Trigger;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	internal abstract class DefaultPlayerSerializer : IPlayerSerializer
	{
		public abstract int Version { get; }

		protected virtual PlayerData GeneratePlayerData(string serialized, IDefinitionService definitionService, IPartyService partyService, IKampaiLogger logger)
		{
			PlayerData playerData = null;
			try
			{
				JsonConverters jsonConverters = new JsonConverters();
				jsonConverters.instanceConverter = new InventoryFastConverter(definitionService, logger);
				jsonConverters.transactionDefinitionConverter = new TransactionDefinitionFastConverter(definitionService);
				jsonConverters.questDefinitionConverter = new QuestDefinitionFastConverter();
				jsonConverters.triggerInstanceConverter = new TriggerInstanceFastConverter(definitionService);
				playerData = FastJSONDeserializer.Deserialize<PlayerData>(serialized, jsonConverters);
				if (playerData == null)
				{
					throw new FatalException(FatalCode.PS_NULL_PLAYER, "PlayerDataV1: json parse error: player is null after deserialization");
				}
			}
			catch (JsonSerializationException e)
			{
				HandleJsonParseException(serialized, e, logger);
			}
			catch (JsonReaderException e2)
			{
				HandleJsonParseException(serialized, e2, logger);
			}
			return playerData;
		}

		private void HandleJsonParseException(string json, Exception e, IKampaiLogger logger)
		{
			logger.Error("HandleJsonParseException(): player json: {0}", json ?? "null");
			throw new FatalException(FatalCode.PS_JSON_PARSE_ERR, e, "Json Parse Err: {0}", e);
		}

		public virtual Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			PlayerData playerData = GeneratePlayerData(json, definitionService, partyService, logger);
			Player player = new Player(definitionService, logger);
			player.ID = playerData.ID;
			player.Version = playerData.version;
			player.nextId = playerData.nextId;
			player.lastLevelUpTime = playerData.lastLevelUpTime;
			player.lastGameStartTime = playerData.lastGameStartTime;
			player.firstGameStartTime = playerData.firstGameStartTime;
			player.lastPlayedTime = playerData.lastPlayedTime;
			player.totalGameplayDurationSinceLastLevelUp = playerData.totalGameplayDurationSinceLastLevelUp;
			player.totalAccumulatedGameplayDuration = playerData.totalAccumulatedGameplayDuration;
			player.targetExpansionID = playerData.targetExpansionID;
			player.HighestFtueLevel = playerData.highestFtueLevel;
			player.timezoneOffset = playerData.timezoneOffset;
			player.country = playerData.country;
			player.completedOrders = playerData.completedOrders;
			player.completedQuestsTotal = playerData.completedQuestsTotal;
			if (playerData.inventory == null)
			{
				throw new FatalException(FatalCode.PS_JSON_PARSE_ERR, 1, "Player inventory is null");
			}
			if (playerData.helpTipsTrackingData != null)
			{
				player.helpTipsTrackingData = playerData.helpTipsTrackingData;
			}
			foreach (Instance item in playerData.inventory)
			{
				player.Add(item);
			}
			if (playerData.pendingTransactions != null)
			{
				player.AddPendingTransactions(playerData.pendingTransactions);
			}
			if (playerData.unlocks != null && playerData.unlocks.Count > 0)
			{
				foreach (UnlockedItem unlock in playerData.unlocks)
				{
					player.AddUnlock(unlock.defID, unlock.quantity);
				}
			}
			if (playerData.purchasedSales != null && playerData.purchasedSales.Count > 0)
			{
				foreach (TrackedSale purchasedSale in playerData.purchasedSales)
				{
					player.AddPurchasedUpsell(purchasedSale.defID, purchasedSale.numberPurchased);
				}
			}
			DeserializeSocialTeam(playerData, player);
			DeserializeVillians(playerData, player);
			DeserializeMTXPurchaseTracking(playerData, player);
			DeserializeTriggers(playerData, player);
			DeserializeSales(definitionService, player);
			if (playerData.PlatformStoreTransactionIDs != null && playerData.PlatformStoreTransactionIDs.Count > 0)
			{
				foreach (string platformStoreTransactionID in playerData.PlatformStoreTransactionIDs)
				{
					player.AddPlatformStoreTransactionID(platformStoreTransactionID);
				}
			}
			return player;
		}

		private static void DeserializeSocialTeam(PlayerData pd, Player p)
		{
			if (pd.socialRewards == null || pd.socialRewards.Count <= 0)
			{
				return;
			}
			foreach (SocialClaimRewardItem socialReward in pd.socialRewards)
			{
				p.AddSocialClaimRewards(socialReward.eventID, socialReward.claimState);
			}
		}

		private static void DeserializeMTXPurchaseTracking(PlayerData pd, Player p)
		{
			if (pd.mtxPurchaseTracking == null || pd.mtxPurchaseTracking.Count <= 0)
			{
				return;
			}
			foreach (string item in pd.mtxPurchaseTracking)
			{
				p.AddMTXPurchaseTracking(item);
			}
		}

		private static void DeserializeVillians(PlayerData pd, Player p)
		{
			if (pd.villainQueue == null || pd.villainQueue.Count <= 0)
			{
				return;
			}
			foreach (int item in pd.villainQueue)
			{
				p.AddVillainQueue(item);
			}
		}

		private static void DeserializeSales(IDefinitionService definitionService, Player player)
		{
			List<Sale> instancesByType = player.GetInstancesByType<Sale>();
			foreach (Sale item in instancesByType)
			{
				if (item.Definition.Type.Equals(SalePackType.Upsell))
				{
					if (!definitionService.Has(item.Definition.ID))
					{
						definitionService.Add(item.Definition);
					}
					if (!definitionService.Has(item.Definition.TransactionDefinition.ID))
					{
						definitionService.Add(item.Definition.TransactionDefinition.ToDefinition());
					}
				}
			}
		}

		private static void DeserializeTriggers(PlayerData playerData, Player player)
		{
			if (playerData.triggers == null)
			{
				return;
			}
			for (int i = 0; i < playerData.triggers.Count; i++)
			{
				if (playerData.triggers[i] != null)
				{
					player.Add(playerData.triggers[i]);
				}
			}
		}

		public virtual byte[] Serialize(Player player, IDefinitionService defintitionService, IKampaiLogger logger)
		{
			PlayerData playerData = new PlayerData();
			playerData.ID = player.ID;
			playerData.version = player.Version;
			playerData.nextId = player.NextId;
			playerData.inventory = new List<Instance>();
			playerData.unlocks = new List<UnlockedItem>();
			playerData.purchasedSales = new List<TrackedSale>();
			playerData.villainQueue = new List<int>();
			playerData.lastLevelUpTime = player.lastLevelUpTime;
			playerData.lastGameStartTime = player.lastGameStartTime;
			playerData.firstGameStartTime = player.firstGameStartTime;
			playerData.lastPlayedTime = player.lastPlayedTime;
			playerData.totalGameplayDurationSinceLastLevelUp = player.totalGameplayDurationSinceLastLevelUp;
			playerData.totalAccumulatedGameplayDuration = player.totalAccumulatedGameplayDuration;
			playerData.targetExpansionID = player.targetExpansionID;
			playerData.highestFtueLevel = player.HighestFtueLevel;
			playerData.mtxPurchaseTracking = new List<string>();
			playerData.timezoneOffset = player.timezoneOffset;
			playerData.socialRewards = new List<SocialClaimRewardItem>();
			playerData.completedQuestsTotal = player.completedQuestsTotal;
			playerData.currentItemCount = player.GetStorableItemCount();
			playerData.country = player.country;
			playerData.completedOrders = player.completedOrders;
			playerData.PlatformStoreTransactionIDs = new List<string>();
			playerData.helpTipsTrackingData = player.helpTipsTrackingData;
			SerializeCollectionsPart1(playerData, player);
			return FastJSONSerializer.SerializeUTF8(playerData);
		}

		private void SerializeCollectionsPart1(PlayerData playerData, Player player)
		{
			foreach (Instance item in player.GetInstancesByDefinition())
			{
				playerData.inventory.Add(item);
			}
			IList<KampaiPendingTransaction> pendingTransactions = player.GetPendingTransactions();
			if (pendingTransactions != null && pendingTransactions.Count > 0)
			{
				playerData.pendingTransactions = pendingTransactions;
			}
			IDictionary<int, int> unlockedItems = player.GetUnlockedItems();
			if (unlockedItems != null && unlockedItems.Count > 0)
			{
				foreach (KeyValuePair<int, int> item2 in unlockedItems)
				{
					playerData.unlocks.Add(new UnlockedItem(item2.Key, item2.Value));
				}
			}
			IDictionary<int, int> upsellsPurchased = player.GetUpsellsPurchased();
			if (upsellsPurchased != null && upsellsPurchased.Count > 0)
			{
				foreach (KeyValuePair<int, int> item3 in upsellsPurchased)
				{
					playerData.purchasedSales.Add(new TrackedSale(item3.Key, item3.Value));
				}
			}
			SerializeCollectionsPart2(playerData, player);
		}

		private void SerializeCollectionsPart2(PlayerData playerData, Player player)
		{
			IList<int> villainQueue = player.GetVillainQueue();
			if (villainQueue != null && villainQueue.Count > 0)
			{
				foreach (int item in villainQueue)
				{
					playerData.villainQueue.Add(item);
				}
			}
			IList<string> platformStoreTransactionIDs = player.GetPlatformStoreTransactionIDs();
			if (platformStoreTransactionIDs != null && platformStoreTransactionIDs.Count > 0)
			{
				foreach (string item2 in platformStoreTransactionIDs)
				{
					playerData.PlatformStoreTransactionIDs.Add(item2);
				}
			}
			IDictionary<int, SocialClaimRewardItem.ClaimState> socialClaimRewards = player.GetSocialClaimRewards();
			if (socialClaimRewards != null && socialClaimRewards.Count > 0)
			{
				foreach (KeyValuePair<int, SocialClaimRewardItem.ClaimState> item3 in socialClaimRewards)
				{
					playerData.socialRewards.Add(new SocialClaimRewardItem(item3.Key, item3.Value));
				}
			}
			IList<string> mTXPurchaseTracking = player.GetMTXPurchaseTracking();
			if (mTXPurchaseTracking != null && mTXPurchaseTracking.Count > 0)
			{
				foreach (string item4 in mTXPurchaseTracking)
				{
					playerData.mtxPurchaseTracking.Add(item4);
				}
			}
			SerializeTriggers(playerData, player);
		}

		private static void SerializeTriggers(PlayerData playerData, Player player)
		{
			IList<TriggerInstance> triggers = player.GetTriggers();
			if (triggers != null && triggers.Count != 0)
			{
				playerData.triggers = new List<TriggerInstance>();
				for (int i = 0; i < triggers.Count; i++)
				{
					playerData.triggers.Add(triggers[i]);
				}
			}
		}
	}
}
