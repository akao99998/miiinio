using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKitPlayerService : IPlayerService
	{
		private int nextId = 1000;

		private Dictionary<int, Instance> byInstanceId;

		protected Dictionary<int, List<Instance>> byDefinitionId;

		public int NextId
		{
			get
			{
				return nextId;
			}
		}

		public long ID
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Player LastSave
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool PlayerDataIsLoaded
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int LevelUpUTC
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int LastGameStartUTC
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int FirstGameStartUTC
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int LastPlayedUTC
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int TimeZoneOffset
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string SWRVEGroup { get; set; }

		public int GameplaySecondsSinceLevelUp
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int AccumulatedGameplayDuration
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public List<Player.HelpTipTrackingItem> helpTipsTrackingData
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public AnimationToolKitPlayerService()
		{
			byInstanceId = new Dictionary<int, Instance>();
			byDefinitionId = new Dictionary<int, List<Instance>>();
		}

		public uint GetQuantity(StaticItem def)
		{
			return 0u;
		}

		public uint GetStorageCount()
		{
			throw new NotImplementedException();
		}

		public uint GetQuantityByDefinitionId(int defId)
		{
			throw new NotImplementedException();
		}

		public void ProcessSlotPurchase(int slotCost, bool showStoreOnFail, int slotNumber, Action<PendingCurrencyTransaction> callback, int instanceId)
		{
			throw new NotImplementedException();
		}

		public void ProcessSaleCancel(int cost, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public void ProcessRefreshMarket(int cost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public void ProcessItemPurchase(int itemCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			throw new NotImplementedException();
		}

		public void ProcessOrderFill(int slotCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, int instanceId)
		{
			throw new NotImplementedException();
		}

		public int GetCountByDefinitionId(int defId)
		{
			throw new NotImplementedException();
		}

		public uint GetQuantityByInstanceId(int instanceId)
		{
			throw new NotImplementedException();
		}

		public bool isStorageFull()
		{
			throw new NotImplementedException();
		}

		public uint GetAvailableStorageCapacity()
		{
			throw new NotImplementedException();
		}

		public void AlterQuantity(StaticItem def, int amount)
		{
			throw new NotImplementedException();
		}

		public void SetQuantity(StaticItem def, int amount)
		{
			throw new NotImplementedException();
		}

		public int GetPartyBuffRemainingTime(MinionParty minionParty)
		{
			throw new NotImplementedException();
		}

		public int GetPartyBuffDurationTime(MinionParty minionParty)
		{
			throw new NotImplementedException();
		}

		public bool IsMinionPartyUnlocked()
		{
			throw new NotImplementedException();
		}

		public bool IsMinigamePackAccessible()
		{
			throw new NotImplementedException();
		}

		public bool HasPurchasedMinigamePack()
		{
			throw new NotImplementedException();
		}

		public void UpdateMinionPartyPointValues()
		{
			throw new NotImplementedException();
		}

		public ICollection<Building> GetBuildings()
		{
			LinkedList<Building> linkedList = new LinkedList<Building>();
			foreach (Instance value in byInstanceId.Values)
			{
				if (value.Definition is BuildingDefinition)
				{
					linkedList.AddLast((Building)value);
				}
			}
			return linkedList;
		}

		public ICollection<int> GetAnimatingBuildingIDs()
		{
			LinkedList<int> linkedList = new LinkedList<int>();
			foreach (Instance value in byInstanceId.Values)
			{
				if (value.Definition is AnimatingBuildingDefinition)
				{
					linkedList.AddLast(((Building)value).ID);
				}
			}
			return linkedList;
		}

		public MinionParty GetMinionPartyInstance()
		{
			throw new NotImplementedException();
		}

		public IList<IngredientsItemDefinition> GetAllUnLockedIngredients()
		{
			throw new NotImplementedException();
		}

		public IList<T> GetUnlockedDefsByType<T>() where T : Definition
		{
			throw new NotImplementedException();
		}

		public ICollection<DestructibleBuilding> GetDestructibles()
		{
			throw new NotImplementedException();
		}

		public ICollection<Minion> GetMinions()
		{
			throw new NotImplementedException();
		}

		public ICollection<Quest> GetQuests()
		{
			throw new NotImplementedException();
		}

		public List<Item> GetSellableItems()
		{
			throw new NotImplementedException();
		}

		public Dictionary<int, int> GetBuildingOnBoardCountMap()
		{
			throw new NotImplementedException();
		}

		public bool IsMissingItemFromTransaction(TransactionDefinition transactionDefinition)
		{
			throw new NotImplementedException();
		}

		public IList<QuantityItem> GetMissingItemListFromTransaction(TransactionDefinition transactionDefinition)
		{
			throw new NotImplementedException();
		}

		public ICollection<Item> GetStorableItems(out uint totalQuantity)
		{
			throw new NotImplementedException();
		}

		public int GetUnlockedQuantityOfID(int defId)
		{
			throw new NotImplementedException();
		}

		public int GetPurchasedQuanityByUpsellID(int defId)
		{
			throw new NotImplementedException();
		}

		public void AddUpsellToPurchased(int defID)
		{
			throw new NotImplementedException();
		}

		public void ClearPurchasedUpsells(int defID)
		{
			throw new NotImplementedException();
		}

		public Player LoadPlayerData(string serialized)
		{
			throw new NotImplementedException();
		}

		public void Deserialize(string serialized, bool isRetry = false)
		{
			throw new NotImplementedException();
		}

		public byte[] SavePlayerData(Player PlayerDataLoaded)
		{
			throw new NotImplementedException();
		}

		public byte[] Serialize()
		{
			throw new NotImplementedException();
		}

		public bool IsPlayerInitialized()
		{
			return byInstanceId != null;
		}

		public void Add(Instance i)
		{
			if (i != null && i.Definition != null)
			{
				AssignNextInstanceId(i);
				byInstanceId.Add(i.ID, i);
				int iD = i.Definition.ID;
				if (!byDefinitionId.ContainsKey(iD))
				{
					byDefinitionId.Add(iD, new List<Instance>());
				}
				byDefinitionId[iD].Add(i);
			}
		}

		public void Remove(Instance i)
		{
			int iD = i.ID;
			if (i != null && iD != 0 && byInstanceId.ContainsKey(iD))
			{
				byInstanceId.Remove(iD);
				int iD2 = i.Definition.ID;
				byDefinitionId[iD2].Remove(i);
				if (byDefinitionId[iD2].Count == 0)
				{
					byDefinitionId.Remove(iD2);
				}
			}
		}

		public T GetByInstanceId<T>(int id) where T : class, Instance
		{
			if (byInstanceId.ContainsKey(id))
			{
				return byInstanceId[id] as T;
			}
			return (T)null;
		}

		public T GetFirstInstanceByDefinitionId<T>(int definitionId) where T : class, Instance
		{
			if (byDefinitionId.ContainsKey(definitionId) && byDefinitionId[definitionId].Count > 0)
			{
				return (T)byDefinitionId[definitionId][0];
			}
			return (T)null;
		}

		public I GetFirstInstanceByDefintion<I, D>() where I : class, Instance where D : Definition
		{
			throw new NotImplementedException();
		}

		public ICollection<T> GetByDefinitionId<T>(int id) where T : Instance
		{
			if (byDefinitionId.ContainsKey(id))
			{
				List<T> list = new List<T>();
				{
					foreach (Instance item in byDefinitionId[id])
					{
						if (item is T)
						{
							list.Add((T)item);
						}
					}
					return list;
				}
			}
			return new List<T>();
		}

		public WeightedInstance GetWeightedInstance(int defId, WeightedDefinition wd = null)
		{
			throw new NotImplementedException();
		}

		public int CalculateRushCost(IList<QuantityItem> items)
		{
			throw new NotImplementedException();
		}

		public bool FinishTransaction(int transactionId, TransactionTarget target, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public bool FinishTransaction(int transactionId, TransactionTarget target, TransactionArg arg, float bonusDropRate = 0f, float premiumAwardRate = 0f)
		{
			throw new NotImplementedException();
		}

		public bool FinishTransaction(int transactionId, TransactionTarget target, out IList<Instance> outputs, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public bool FinishTransaction(TransactionDefinition td, TransactionTarget target, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public bool FinishTransaction(TransactionDefinition td, TransactionTarget target, out IList<Instance> outputs, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public bool VerifyTransaction(TransactionDefinition transactiondef)
		{
			throw new NotImplementedException();
		}

		public bool VerifyTransaction(int transactionId)
		{
			throw new NotImplementedException();
		}

		public void StopTask(int minionId)
		{
			throw new NotImplementedException();
		}

		public void BuyCraftingSlot(int buildingID)
		{
			throw new NotImplementedException();
		}

		public void UpdateCraftingQueue(int buildingID, int itemDefId)
		{
			throw new NotImplementedException();
		}

		public bool VerifyPlayerHasRequiredInputs(IList<QuantityItem> inputs)
		{
			throw new NotImplementedException();
		}

		public void PurchaseSlotForBuilding(int buildingID, int level)
		{
			throw new NotImplementedException();
		}

		public int GetMinionCount()
		{
			throw new NotImplementedException();
		}

		public void CreateAndRunCustomTransaction(int defID, int quantity, TransactionTarget target, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public int GetInvestmentTimeForTransaction(int transactionID)
		{
			throw new NotImplementedException();
		}

		public void AssignNextInstanceId(Identifiable i)
		{
			if (i != null)
			{
				i.ID = nextId++;
			}
		}

		public KampaiPendingTransaction GetPendingTransaction(string externalIdentifier)
		{
			throw new NotImplementedException();
		}

		public bool PlayerAlreadyHasPlatformStoreTransactionID(string identifier)
		{
			throw new NotImplementedException();
		}

		public void AddPlatformStoreTransactionID(string identifier)
		{
			throw new NotImplementedException();
		}

		public void QueuePendingTransaction(KampaiPendingTransaction pendingTransaction)
		{
			throw new NotImplementedException();
		}

		public KampaiPendingTransaction ProcessPendingTransaction(string externalIdentifier, bool isFromPremium, Action<PendingCurrencyTransaction> callback = null)
		{
			throw new NotImplementedException();
		}

		public KampaiPendingTransaction CancelPendingTransaction(string externalIdentifier)
		{
			throw new NotImplementedException();
		}

		public IList<KampaiPendingTransaction> GetPendingTransactions()
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			throw new NotImplementedException();
		}

		public void StartTransaction(TransactionDefinition td, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0)
		{
			throw new NotImplementedException();
		}

		public void StartTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0)
		{
			throw new NotImplementedException();
		}

		public void RunEntireTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public void RunEntireTransaction(TransactionDefinition td, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null)
		{
			throw new NotImplementedException();
		}

		public void ExchangePremiumForGrind(int grindNeeded, Action<PendingCurrencyTransaction> callback)
		{
			throw new NotImplementedException();
		}

		public int PremiumCostForGrind(int grindNeeded)
		{
			throw new NotImplementedException();
		}

		public QuestDefinition GetProcedurallyGeneratedQuestDefinition(int id)
		{
			throw new NotImplementedException();
		}

		public bool CanAffordExchange(int grindNeeded)
		{
			throw new NotImplementedException();
		}

		public IList<Instance> GetInstancesByDefinition<T>() where T : Definition
		{
			throw new NotImplementedException();
		}

		public IList<Instance> GetInstancesByDefinitionID(int defId)
		{
			throw new NotImplementedException();
		}

		public List<T> GetInstancesByType<T>() where T : class, Instance
		{
			List<T> list = new List<T>();
			foreach (Instance value in byInstanceId.Values)
			{
				T val = value as T;
				if (val != null)
				{
					list.Add(val);
				}
			}
			return list;
		}

		public void GetInstancesByType<T>(ref List<T> list) where T : class, Instance
		{
			list.AddRange(GetInstancesByType<T>());
		}

		public IList<Item> GetItemsByDefinition<T>() where T : Definition
		{
			throw new NotImplementedException();
		}

		public IList<LandExpansionBuilding> GetBuildingsWithExpansionID(int expansionID)
		{
			throw new NotImplementedException();
		}

		public IList<Building> GetBuildingsWithoutState(BuildingState state)
		{
			throw new NotImplementedException();
		}

		public IList<Building> GetBuildingsWithState(BuildingState state)
		{
			throw new NotImplementedException();
		}

		public void AddLandExpansion(LandExpansionConfig expansionConfig)
		{
			throw new NotImplementedException();
		}

		public bool IsExpansionPurchased(int expansionId)
		{
			throw new NotImplementedException();
		}

		public int GetPurchasedExpansionCount()
		{
			throw new NotImplementedException();
		}

		public void QueueVillain(Prestige villainPrestige)
		{
			throw new NotImplementedException();
		}

		public int PopVillain()
		{
			throw new NotImplementedException();
		}

		public void SetTargetExpansion(int id)
		{
			throw new NotImplementedException();
		}

		public int GetTargetExpansion()
		{
			throw new NotImplementedException();
		}

		public void ClearTargetExpansion()
		{
			throw new NotImplementedException();
		}

		public bool HasTargetExpansion()
		{
			throw new NotImplementedException();
		}

		public int GetFreezeTime()
		{
			throw new NotImplementedException();
		}

		public bool HasStorageBuilding()
		{
			throw new NotImplementedException();
		}

		public List<Minion> GetMinions(bool has, params MinionState[] states)
		{
			throw new NotImplementedException();
		}

		public List<Minion> GetIdleMinions()
		{
			throw new NotImplementedException();
		}

		public int GetHighestFtueCompleted()
		{
			throw new NotImplementedException();
		}

		public void SetHighestFtueCompleted(int newLevel)
		{
			throw new NotImplementedException();
		}

		public void IncreaseCompletedOrders()
		{
			throw new NotImplementedException();
		}

		public void IncreaseCompletedQuests()
		{
			throw new NotImplementedException();
		}

		public int GetInventoryCountByDefinitionID(int defId)
		{
			throw new NotImplementedException();
		}

		public bool CheckIfBuildingIsCapped(int defID)
		{
			throw new NotImplementedException();
		}

		public void ProcessRush(IList<int> itemCostList, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			throw new NotImplementedException();
		}

		public void ProcessItemPurchase(IList<int> itemCosts, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			throw new NotImplementedException();
		}

		public SocialClaimRewardItem.ClaimState GetSocialClaimReward(int eventID)
		{
			throw new NotImplementedException();
		}

		public void AddSocialClaimReward(int eventID, SocialClaimRewardItem.ClaimState claimState)
		{
			throw new NotImplementedException();
		}

		public void CleanupSocialClaimReward(List<int> recentEventIDs)
		{
			throw new NotImplementedException();
		}

		public Player.SanityCheckFailureReason DeepScan(Player prevSave)
		{
			return Player.SanityCheckFailureReason.Passed;
		}

		public void TrackMTXPurchase(string SKU)
		{
			throw new NotImplementedException();
		}

		public int MTXPurchaseCount(string sku)
		{
			throw new NotImplementedException();
		}

		public void AddXP(int partyPoints)
		{
			throw new NotImplementedException();
		}

		public void addPendingRemption(ReceiptValidationResult validationResult)
		{
			throw new NotImplementedException();
		}

		public ReceiptValidationResult popPendingRedemption()
		{
			throw new NotImplementedException();
		}

		public ReceiptValidationResult topPendingRedemption()
		{
			throw new NotImplementedException();
		}

		public void SetPartyFavorQuantityToZero(int partyFavorID)
		{
		}

		public bool AllSubIngredientBRBAvailable(int itemID)
		{
			throw new NotImplementedException();
		}

		public bool ItemUsesBuildingsInList(IngredientsItemDefinition item, List<int> buildingIDs)
		{
			throw new NotImplementedException();
		}

		public bool ItemUsesBuildingsInList(int itemID, List<int> buildingIDs)
		{
			throw new NotImplementedException();
		}

		public List<int> GetUnavailableBuildingIDSForItem(int itemID)
		{
			throw new NotImplementedException();
		}

		public void NotifyBuildMenuNewBuilding(int buildingID)
		{
			throw new NotImplementedException();
		}

		public void AddSocialInvitationSeen(long invitationId)
		{
			throw new NotImplementedException();
		}

		public bool SeenSocialInvitation(long invitationId)
		{
			throw new NotImplementedException();
		}

		public int getAndIncrementRequestCounter()
		{
			throw new NotImplementedException();
		}

		public void RemoveTrigger(TriggerInstance triggerInstance)
		{
			throw new NotImplementedException();
		}

		public IList<TriggerInstance> GetTriggers()
		{
			throw new NotImplementedException();
		}

		public int GetMinionCountByLevel(int level)
		{
			throw new NotImplementedException();
		}

		public TriggerInstance AddTrigger(TriggerDefinition triggerDefinition)
		{
			throw new NotImplementedException();
		}

		public List<Minion> GetMinionsSortedByLevel()
		{
			throw new NotImplementedException();
		}

		public List<Minion> GetMinionsByLevel(int level)
		{
			throw new NotImplementedException();
		}

		public bool HasTrigger(int triggerId)
		{
			throw new NotImplementedException();
		}

		public TriggerInstance GetTriggerByDefinitionId(int defId)
		{
			throw new NotImplementedException();
		}

		public void LevelupMinion(int instanceID)
		{
			throw new NotImplementedException();
		}

		public uint GetCurrentStorageCapacity()
		{
			throw new NotImplementedException();
		}

		public void AwardMasterPlanComponents(MasterPlanDefinition definition)
		{
			throw new NotImplementedException();
		}

		public void GrantInputs(TransactionDefinition transaction)
		{
			throw new NotImplementedException();
		}

		public int GetHighestUntaskedMinionLevel()
		{
			throw new NotImplementedException();
		}

		public void IngestPlayerMeta(PlayerMetaData meta)
		{
			throw new NotImplementedException();
		}

		public Minion GetUntaskedMinionWithHighestLevel()
		{
			throw new NotImplementedException();
		}

		public int GetHighestMinionForLeisure(int requiredMinionCount)
		{
			throw new NotImplementedException();
		}

		public bool IsInSegment(string segmentName)
		{
			throw new NotImplementedException();
		}

		public void TrackHelpTipShown(int tipDefinitionId, int time)
		{
			throw new NotImplementedException();
		}

		public float Churn()
		{
			throw new NotImplementedException();
		}

		public int GetMinionCountAtOrAboveLevel(int level)
		{
			throw new NotImplementedException();
		}

		public bool HasPurchasedBuildingAssociatedWithItem(IngredientsItemDefinition item)
		{
			throw new NotImplementedException();
		}

		public uint GetTotalCountByDefinitionId(int id)
		{
			throw new NotImplementedException();
		}

		public List<Item> GetSellableItems(out uint totalStorableQuantity, out uint totalSellableQuantity)
		{
			throw new NotImplementedException();
		}

		public IList<string> GetMTXPurchaseTracking()
		{
			throw new NotImplementedException();
		}
	}
}
