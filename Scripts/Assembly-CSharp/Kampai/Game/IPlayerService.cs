using System;
using System.Collections.Generic;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;

namespace Kampai.Game
{
	public interface IPlayerService
	{
		long ID { get; set; }

		Player LastSave { get; set; }

		int LevelUpUTC { get; set; }

		int FirstGameStartUTC { get; set; }

		int LastGameStartUTC { get; set; }

		int LastPlayedUTC { get; set; }

		string SWRVEGroup { get; set; }

		int GameplaySecondsSinceLevelUp { get; set; }

		int AccumulatedGameplayDuration { get; set; }

		List<Player.HelpTipTrackingItem> helpTipsTrackingData { get; }

		uint GetQuantity(StaticItem def);

		int GetCountByDefinitionId(int defId);

		uint GetQuantityByDefinitionId(int defId);

		uint GetTotalCountByDefinitionId(int defId);

		uint GetQuantityByInstanceId(int instanceId);

		ICollection<int> GetAnimatingBuildingIDs();

		void AlterQuantity(StaticItem def, int amount);

		void SetQuantity(StaticItem def, int amount);

		ICollection<Item> GetStorableItems(out uint totalQuantity);

		List<Item> GetSellableItems();

		List<Item> GetSellableItems(out uint totalStorableQuantity, out uint totalSellableQuantity);

		Dictionary<int, int> GetBuildingOnBoardCountMap();

		int GetUnlockedQuantityOfID(int defId);

		int GetPurchasedQuanityByUpsellID(int defId);

		void AddUpsellToPurchased(int defID);

		void ClearPurchasedUpsells(int defID);

		IList<T> GetUnlockedDefsByType<T>() where T : Definition;

		Player LoadPlayerData(string serialized);

		void Deserialize(string serialized, bool isRetry = false);

		byte[] SavePlayerData(Player playerData);

		byte[] Serialize();

		bool IsPlayerInitialized();

		void Add(Instance i);

		void AssignNextInstanceId(Identifiable i);

		void Remove(Instance i);

		T GetByInstanceId<T>(int id) where T : class, Instance;

		T GetFirstInstanceByDefinitionId<T>(int definitionId) where T : class, Instance;

		ICollection<T> GetByDefinitionId<T>(int id) where T : Instance;

		WeightedInstance GetWeightedInstance(int defId, WeightedDefinition wd = null);

		MinionParty GetMinionPartyInstance();

		void AddXP(int xp);

		void UpdateMinionPartyPointValues();

		int CalculateRushCost(IList<QuantityItem> items);

		void ProcessSlotPurchase(int slotCost, bool showStoreOnFail, int slotNumber, Action<PendingCurrencyTransaction> callback, int instanceId);

		void ProcessSaleCancel(int cost, Action<PendingCurrencyTransaction> callback);

		void ProcessRefreshMarket(int cost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback);

		void ProcessItemPurchase(int itemCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false);

		void ProcessItemPurchase(IList<int> itemCosts, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false);

		void ProcessOrderFill(int slotCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback);

		void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, int instanceId);

		void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback);

		void ProcessRush(int rushCost, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback);

		void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false);

		void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false);

		bool IsMissingItemFromTransaction(TransactionDefinition transactionDefinition);

		IList<QuantityItem> GetMissingItemListFromTransaction(TransactionDefinition transactionDefinition);

		void StartTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0);

		void StartTransaction(TransactionDefinition td, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0);

		bool FinishTransaction(int transactionId, TransactionTarget target, TransactionArg arg = null);

		bool FinishTransaction(TransactionDefinition td, TransactionTarget target, TransactionArg arg = null);

		bool FinishTransaction(int transactionId, TransactionTarget target, out IList<Instance> outputs, TransactionArg arg = null);

		void RunEntireTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null);

		void RunEntireTransaction(TransactionDefinition def, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null);

		bool VerifyTransaction(TransactionDefinition transactiondef);

		bool VerifyTransaction(int transactionId);

		void StopTask(int minionId);

		void BuyCraftingSlot(int buildingID);

		void UpdateCraftingQueue(int buildingID, int itemDefId);

		bool VerifyPlayerHasRequiredInputs(IList<QuantityItem> inputs);

		void PurchaseSlotForBuilding(int buildingID, int level);

		int GetMinionCount();

		void CreateAndRunCustomTransaction(int defID, int quantity, TransactionTarget target, TransactionArg args = null);

		KampaiPendingTransaction GetPendingTransaction(string externalIdentifier);

		bool PlayerAlreadyHasPlatformStoreTransactionID(string identifier);

		void AddPlatformStoreTransactionID(string identifier);

		IList<KampaiPendingTransaction> GetPendingTransactions();

		void QueuePendingTransaction(KampaiPendingTransaction pendingTransaction);

		KampaiPendingTransaction ProcessPendingTransaction(string externalIdentifier, bool isFromPremium, Action<PendingCurrencyTransaction> callback = null);

		KampaiPendingTransaction CancelPendingTransaction(string externalIdentifier);

		void ExchangePremiumForGrind(int grindNeeded, Action<PendingCurrencyTransaction> callback);

		int PremiumCostForGrind(int grindNeeded);

		bool CanAffordExchange(int grindNeeded);

		int GetInvestmentTimeForTransaction(int transactionID);

		IList<Instance> GetInstancesByDefinition<T>() where T : Definition;

		I GetFirstInstanceByDefintion<I, D>() where I : class, Instance where D : Definition;

		IList<Instance> GetInstancesByDefinitionID(int defId);

		List<T> GetInstancesByType<T>() where T : class, Instance;

		void GetInstancesByType<T>(ref List<T> list) where T : class, Instance;

		IList<Item> GetItemsByDefinition<T>() where T : Definition;

		IList<Building> GetBuildingsWithState(BuildingState state);

		IList<Building> GetBuildingsWithoutState(BuildingState excludedState);

		IList<TriggerInstance> GetTriggers();

		void RemoveTrigger(TriggerInstance triggerInstance);

		TriggerInstance AddTrigger(TriggerDefinition triggerDefinition);

		bool HasTrigger(int triggerId);

		TriggerInstance GetTriggerByDefinitionId(int defId);

		void AddLandExpansion(LandExpansionConfig expansionConfig);

		bool IsExpansionPurchased(int expansionId);

		int GetPurchasedExpansionCount();

		void QueueVillain(Prestige villainPrestige);

		int PopVillain();

		void SetTargetExpansion(int id);

		int GetTargetExpansion();

		void ClearTargetExpansion();

		bool HasTargetExpansion();

		bool HasStorageBuilding();

		bool isStorageFull();

		uint GetAvailableStorageCapacity();

		uint GetCurrentStorageCapacity();

		List<Minion> GetMinions(bool has, params MinionState[] states);

		List<Minion> GetIdleMinions();

		void IncreaseCompletedOrders();

		void IncreaseCompletedQuests();

		int GetHighestFtueCompleted();

		void SetHighestFtueCompleted(int newLevel);

		int GetInventoryCountByDefinitionID(int defId);

		bool CheckIfBuildingIsCapped(int defID);

		SocialClaimRewardItem.ClaimState GetSocialClaimReward(int eventID);

		void AddSocialClaimReward(int eventID, SocialClaimRewardItem.ClaimState claimState);

		void CleanupSocialClaimReward(List<int> recentEventIDs);

		uint GetStorageCount();

		void TrackMTXPurchase(string SKU);

		IList<string> GetMTXPurchaseTracking();

		int MTXPurchaseCount(string sku);

		bool IsMinionPartyUnlocked();

		bool HasPurchasedMinigamePack();

		Player.SanityCheckFailureReason DeepScan(Player prevSave);

		void addPendingRemption(ReceiptValidationResult validationResult);

		ReceiptValidationResult topPendingRedemption();

		ReceiptValidationResult popPendingRedemption();

		bool ItemUsesBuildingsInList(IngredientsItemDefinition item, List<int> buildingIDs);

		bool ItemUsesBuildingsInList(int itemID, List<int> buildingIDs);

		List<int> GetUnavailableBuildingIDSForItem(int itemID);

		void NotifyBuildMenuNewBuilding(int buildingID);

		void AddSocialInvitationSeen(long invitationId);

		bool SeenSocialInvitation(long invitationId);

		int getAndIncrementRequestCounter();

		void GrantInputs(TransactionDefinition transaction);

		int GetMinionCountByLevel(int level);

		int GetMinionCountAtOrAboveLevel(int level);

		void TrackHelpTipShown(int tipDefinitionId, int time);

		int GetHighestUntaskedMinionLevel();

		int GetHighestMinionForLeisure(int requiredMinionCount);

		Minion GetUntaskedMinionWithHighestLevel();

		void IngestPlayerMeta(PlayerMetaData meta);

		List<Minion> GetMinionsSortedByLevel();

		List<Minion> GetMinionsByLevel(int level);

		bool IsInSegment(string segmentName);

		void LevelupMinion(int instanceID);

		float Churn();

		bool HasPurchasedBuildingAssociatedWithItem(IngredientsItemDefinition item);
	}
}
