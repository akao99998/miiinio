using System;
using System.Collections.Generic;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;

namespace Kampai.Game
{
	public class Player
	{
		public enum SanityCheckFailureReason
		{
			Passed = 0,
			OrderBoardTicketCount = 1,
			OrderBoardTickerInputsOutputs = 2,
			OrderBoardMissing = 3,
			TSMOutputs = 4,
			BuildingCount = 5,
			RequiredBuilding = 6,
			MignetteMissing = 7,
			MinionCount = 8,
			CostumedMinion = 9,
			NamedMinion = 10,
			LandExpansionEmpty = 11,
			PurchasedLandExpansionMissing = 12,
			BuildingMissingLocation = 13
		}

		public struct HelpTipTrackingItem
		{
			public int tipDifinitionId;

			public int showsCount;

			public int lastShownTime;

			public HelpTipTrackingItem(int defId, int time)
			{
				tipDifinitionId = defId;
				showsCount = 1;
				lastShownTime = time;
			}
		}

		protected Dictionary<int, Instance> byInstanceId = new Dictionary<int, Instance>();

		protected Dictionary<int, List<Instance>> byDefinitionId = new Dictionary<int, List<Instance>>();

		protected List<KampaiPendingTransaction> pendingTransactions = new List<KampaiPendingTransaction>();

		protected IDictionary<int, int> unlocks = new Dictionary<int, int>();

		protected IDictionary<int, int> purchasedUpsells = new Dictionary<int, int>();

		protected IList<int> villainQueue = new List<int>();

		protected IList<string> PlatformStoreTransactionIDs = new List<string>();

		protected IDictionary<int, SocialClaimRewardItem.ClaimState> socialRewards = new Dictionary<int, SocialClaimRewardItem.ClaimState>();

		protected IList<string> mtxPurchaseTracking = new List<string>();

		protected IList<string> completedQuests = new List<string>();

		protected IDictionary<int, TriggerInstance> triggers = new Dictionary<int, TriggerInstance>();

		protected IList<ReceiptValidationResult> pendingRedemptions = new List<ReceiptValidationResult>();

		public int nextId = 1000;

		public int Version { get; set; }

		public IKampaiLogger logger { get; set; }

		public IDefinitionService definitionService { get; set; }

		public long ID { get; set; }

		public int HighestFtueLevel { get; set; }

		public int NextId
		{
			get
			{
				return nextId;
			}
		}

		public int lastLevelUpTime { get; set; }

		public int lastGameStartTime { get; set; }

		public int firstGameStartTime { get; set; }

		public int lastPlayedTime { get; set; }

		public int totalGameplayDurationSinceLastLevelUp { get; set; }

		public int totalAccumulatedGameplayDuration { get; set; }

		public int targetExpansionID { get; set; }

		public int timezoneOffset { get; set; }

		public string country { get; set; }

		public int completedOrders { get; set; }

		public int completedQuestsTotal { get; set; }

		public IList<long> socialTeamInvitationsSeen { get; set; }

		public int requestCounter { get; set; }

		public List<HelpTipTrackingItem> helpTipsTrackingData { get; set; }

		public Player(IDefinitionService defService, IKampaiLogger log)
		{
			definitionService = defService;
			logger = log;
		}

		public uint GetQuantity(StaticItem def)
		{
			return GetQuantityByDefinitionId((int)def);
		}

		public uint GetQuantityByDefinitionId(int defId)
		{
			Item itemIfExistsByDefId = GetItemIfExistsByDefId(defId);
			return (itemIfExistsByDefId != null) ? itemIfExistsByDefId.Quantity : 0u;
		}

		public int GetCountByDefinitionId(int defId)
		{
			if (byDefinitionId.ContainsKey(defId))
			{
				return byDefinitionId[defId].Count;
			}
			return 0;
		}

		public uint GetTotalCountByDefinitionId(int defId)
		{
			List<Instance> value;
			if (!byDefinitionId.TryGetValue(defId, out value))
			{
				return 0u;
			}
			uint num = 0u;
			for (int i = 0; i < value.Count; i++)
			{
				Instance instance = value[i];
				Item item = instance as Item;
				num += ((item == null) ? 1 : item.Quantity);
			}
			return num;
		}

		public uint GetQuantityByInstanceId(int instanceId)
		{
			Item itemIfExistsByInstanceId = GetItemIfExistsByInstanceId(instanceId);
			return (itemIfExistsByInstanceId != null) ? itemIfExistsByInstanceId.Quantity : 0u;
		}

		public ICollection<int> FindAllAnimatingBuildingIDs()
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

		public ICollection<Item> FindStorableItems(out uint itemCount)
		{
			LinkedList<Item> linkedList = new LinkedList<Item>();
			itemCount = 0u;
			foreach (Instance value in byInstanceId.Values)
			{
				Item item = value as Item;
				if (item != null && item.Definition.Storable && item.Quantity != 0)
				{
					itemCount += item.Quantity;
					linkedList.AddLast(item);
				}
			}
			return linkedList;
		}

		public List<Item> GetSellableItems(out uint totalStorableQuantity, out uint totalSellableQuantity)
		{
			List<Item> list = new List<Item>();
			totalStorableQuantity = 0u;
			totalSellableQuantity = 0u;
			foreach (Instance value in byInstanceId.Values)
			{
				Item item = value as Item;
				if (item == null)
				{
					continue;
				}
				ItemDefinition definition = item.Definition;
				if (item.Quantity != 0 && (definition.Storable || definition.SellableForced))
				{
					if (definition.Storable)
					{
						totalStorableQuantity += item.Quantity;
					}
					totalSellableQuantity += item.Quantity;
					list.Add(item);
				}
			}
			return list;
		}

		public IList<T> FindAllUnLockedByType<T>() where T : Definition
		{
			IList<T> list = new List<T>();
			foreach (KeyValuePair<int, int> unlock in unlocks)
			{
				T definition = (T)null;
				if (definitionService.TryGet<T>(unlock.Key, out definition))
				{
					list.Add(definition);
				}
			}
			return list;
		}

		public IList<T> GetByDefinitionId<T>(int id) where T : Instance
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

		public T GetFirstInstanceByDefinitionId<T>(int definitionId) where T : Instance
		{
			if (byDefinitionId.ContainsKey(definitionId) && byDefinitionId[definitionId].Count > 0)
			{
				return (T)byDefinitionId[definitionId][0];
			}
			return default(T);
		}

		public I GetFirstInstanceByDefintion<I, D>() where I : class, Instance where D : Definition
		{
			foreach (Instance value in byInstanceId.Values)
			{
				D val = value.Definition as D;
				if (val != null)
				{
					I val2 = value as I;
					if (val2 != null)
					{
						return val2;
					}
				}
			}
			return (I)null;
		}

		public T GetByInstanceId<T>(int id) where T : class, Instance
		{
			Instance value;
			if (byInstanceId.TryGetValue(id, out value))
			{
				return value as T;
			}
			return (T)null;
		}

		public List<Instance> GetInstancesByDefinition<T>() where T : Definition
		{
			Type typeFromHandle = typeof(T);
			List<Instance> list = new List<Instance>();
			foreach (Instance value in byInstanceId.Values)
			{
				Type type = value.Definition.GetType();
				if (typeFromHandle.IsAssignableFrom(type))
				{
					list.Add(value);
				}
			}
			return list;
		}

		public List<Instance> GetInstancesByDefinitionID(int defId)
		{
			List<Instance> list = new List<Instance>();
			foreach (Instance value in byInstanceId.Values)
			{
				if (value.Definition.ID == defId)
				{
					list.Add(value);
				}
			}
			return list;
		}

		public void GetInstancesByType<T>(ref List<T> result, Predicate<T> condition = null) where T : class, Instance
		{
			Dictionary<int, Instance>.Enumerator enumerator = byInstanceId.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Instance value = enumerator.Current.Value;
					T val = value as T;
					if (val != null && (condition == null || condition(val)))
					{
						result.Add(val);
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		public List<T> GetInstancesByType<T>(Predicate<T> condition = null) where T : class, Instance
		{
			List<T> result = new List<T>();
			GetInstancesByType(ref result, condition);
			return result;
		}

		public IList<Item> GetItemsByDefinition<T>() where T : Definition
		{
			IList<Item> list = new List<Item>();
			foreach (Instance value in byInstanceId.Values)
			{
				T val = value.Definition as T;
				if (val != null)
				{
					Item item = value as Item;
					if (item != null)
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		public IList<Instance> GetInstancesByDefinition()
		{
			IList<Instance> list = new List<Instance>();
			foreach (Instance value in byInstanceId.Values)
			{
				list.Add(value);
			}
			return list;
		}

		public TriggerInstance GetTriggerByDefinitionId(int defId)
		{
			object result;
			if (triggers.ContainsKey(defId))
			{
				TriggerInstance triggerInstance = triggers[defId];
				result = triggerInstance;
			}
			else
			{
				result = null;
			}
			return (TriggerInstance)result;
		}

		public List<TriggerInstance> GetTriggers()
		{
			List<TriggerInstance> list = new List<TriggerInstance>();
			foreach (TriggerInstance value in triggers.Values)
			{
				list.Add(value);
			}
			return list;
		}

		protected bool AssertNewTriggerId(int instanceId)
		{
			if (instanceId < 0 || triggers.ContainsKey(instanceId))
			{
				return false;
			}
			return true;
		}

		public void Add(TriggerInstance i)
		{
			if (i == null)
			{
				logger.Error("Unable to add null trigger to triggers list.");
			}
			else if (i.Definition == null)
			{
				logger.Warning("Illegal Trigger (null definition).");
			}
			else if (AssertNewTriggerId(i.ID))
			{
				AddNewTrigger(i);
			}
		}

		public void RemoveTrigger(int triggerId)
		{
			if (triggers.ContainsKey(triggerId))
			{
				triggers.Remove(triggerId);
			}
		}

		protected void AddNewTrigger(TriggerInstance trigger)
		{
			if (trigger != null)
			{
				triggers.Add(trigger.ID, trigger);
			}
		}

		public WeightedInstance GetWeightedInstance(int defId, WeightedDefinition wd = null)
		{
			WeightedInstance weightedInstance;
			if (!byDefinitionId.ContainsKey(defId))
			{
				if (wd == null)
				{
					wd = definitionService.Get<WeightedDefinition>(defId);
				}
				weightedInstance = new WeightedInstance(wd);
				AssignNextInstanceId(weightedInstance);
				byInstanceId.Add(weightedInstance.ID, weightedInstance);
				List<Instance> list = new List<Instance>();
				list.Add(weightedInstance);
				byDefinitionId.Add(defId, list);
			}
			else
			{
				weightedInstance = byDefinitionId[defId][0] as WeightedInstance;
			}
			return weightedInstance;
		}

		public void Add(Instance i)
		{
			if (i != null)
			{
				if (i.Definition != null)
				{
					if (AssertNewId(i.ID))
					{
						AddNewInventoryItem(i);
					}
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "Illegal instance (null definition).");
				}
			}
			else
			{
				logger.Log(KampaiLogLevel.Error, "Unable to add null instance to inventory.");
			}
		}

		public void SetQuantityByStaticItem(StaticItem def, uint newQuantity)
		{
			SetQuantityByDefinitionId((int)def, newQuantity);
		}

		public void SetQuantityByDefinitionId(int defId, uint newQuantity)
		{
			Item orCreateItemByDefinition = GetOrCreateItemByDefinition(defId);
			orCreateItemByDefinition.Quantity = newQuantity;
		}

		public void SetQuantityByInstanceId(int instanceId, uint newQuantity)
		{
			Item itemIfExistsByInstanceId = GetItemIfExistsByInstanceId(instanceId);
			if (itemIfExistsByInstanceId == null)
			{
				logger.Log(KampaiLogLevel.Error, "Cannot set instance ID for quantity item if it does not exist: {0}", instanceId);
			}
			else
			{
				itemIfExistsByInstanceId.Quantity = newQuantity;
			}
		}

		public void AlterQuantity(StaticItem def, int amount = 1)
		{
			AlterQuantity((int)def, amount);
		}

		public void AlterQuantity(int definitionId, int amount = 1)
		{
			Item item = GetItemIfExistsByDefId(definitionId);
			if (item == null)
			{
				if (amount <= 0)
				{
					if (amount < 0)
					{
						logger.Error("An item cannot be negative {0}", definitionId);
					}
					return;
				}
				item = GetOrCreateItemByDefinition(definitionId);
			}
			AlterQuantityByInstanceId(item.ID, amount);
		}

		public void AlterQuantityByInstanceId(int instanceId, int amount = 1)
		{
			Item itemIfExistsByInstanceId = GetItemIfExistsByInstanceId(instanceId);
			if (itemIfExistsByInstanceId == null)
			{
				logger.Log(KampaiLogLevel.Error, "Cannot decrement inventory item which does not exist: {0}", instanceId);
			}
			else
			{
				AlterQuantity(itemIfExistsByInstanceId, amount);
			}
		}

		protected Item GetOrCreateItemByDefinition(int defId)
		{
			Item item = GetItemIfExistsByDefId(defId);
			if (item == null)
			{
				ItemDefinition def = definitionService.Get<ItemDefinition>(defId);
				item = new Item(def);
				AssignNextInstanceId(item);
				AddNewInventoryItem(item);
			}
			return item;
		}

		public Instance AlterQuantityByDefId(int defId, int amount = 1)
		{
			return AlterQuantity(GetOrCreateItemByDefinition(defId), amount);
		}

		protected Instance AlterQuantity(Item i, int amount)
		{
			if (i != null)
			{
				int num = (int)i.Quantity + amount;
				if (num > 0)
				{
					i.Quantity = (uint)num;
				}
				else if (num == 0)
				{
					RemoveInventoryItem(i);
				}
				else if (num < 0)
				{
					logger.Log(KampaiLogLevel.Error, "Cannot decrement inventory item below zero: {0}", i.ID);
				}
			}
			return i;
		}

		public void Remove(Instance i)
		{
			RemoveInventoryItem(i);
		}

		public void AssignNextInstanceId(Identifiable i)
		{
			if (i != null)
			{
				i.ID = GetNextInstanceId();
			}
		}

		protected int GetNextInstanceId()
		{
			int result = nextId;
			while (byInstanceId.ContainsKey(++nextId))
			{
			}
			return result;
		}

		protected bool AssertNewId(int instanceId)
		{
			if (byInstanceId.ContainsKey(instanceId))
			{
				logger.Log(KampaiLogLevel.Error, "Cannot add new instance ID because it already exists: {0}", instanceId);
				return false;
			}
			return true;
		}

		protected void AddNewInventoryItem(Instance i)
		{
			if (i != null)
			{
				int iD = i.Definition.ID;
				byInstanceId.Add(i.ID, i);
				if (!byDefinitionId.ContainsKey(iD))
				{
					byDefinitionId.Add(iD, new List<Instance>());
				}
				byDefinitionId[iD].Add(i);
			}
		}

		protected void RemoveInventoryItem(Instance i)
		{
			if (i != null)
			{
				int iD = i.Definition.ID;
				byInstanceId.Remove(i.ID);
				byDefinitionId[iD].Remove(i);
				if (byDefinitionId[iD].Count == 0)
				{
					byDefinitionId.Remove(iD);
				}
			}
		}

		protected Item GetItemIfExistsByInstanceId(int instanceId)
		{
			if (byInstanceId.ContainsKey(instanceId))
			{
				return byInstanceId[instanceId] as Item;
			}
			return null;
		}

		protected Item GetItemIfExistsByDefId(int defId)
		{
			if (byDefinitionId.ContainsKey(defId))
			{
				if (byDefinitionId[defId].Count == 1)
				{
					return byDefinitionId[defId][0] as Item;
				}
				logger.Log(KampaiLogLevel.Error, "Ambiguous query (multiple instances with def {0})", defId);
			}
			return null;
		}

		public KampaiPendingTransaction GetPendingTransaction(string externalIdentifier)
		{
			foreach (KampaiPendingTransaction pendingTransaction in pendingTransactions)
			{
				if (pendingTransaction.ExternalIdentifier.Trim().ToLower().Equals(externalIdentifier.Trim().ToLower()))
				{
					return pendingTransaction;
				}
			}
			return null;
		}

		public void QueuePendingTransaction(KampaiPendingTransaction kpt)
		{
			pendingTransactions.Add(kpt);
		}

		public void addPendingRedemption(ReceiptValidationResult notification)
		{
			if (pendingRedemptions == null)
			{
				pendingRedemptions = new List<ReceiptValidationResult>();
			}
			foreach (ReceiptValidationResult pendingRedemption in pendingRedemptions)
			{
				if (pendingRedemption.nimbleTransactionId == notification.nimbleTransactionId)
				{
					return;
				}
			}
			pendingRedemptions.Add(notification);
		}

		public ReceiptValidationResult popPendingRedemption()
		{
			if (pendingRedemptions == null || pendingRedemptions.Count == 0)
			{
				return null;
			}
			ReceiptValidationResult result = pendingRedemptions[0];
			pendingRedemptions.RemoveAt(0);
			return result;
		}

		public ReceiptValidationResult topPendingRedemption()
		{
			if (pendingRedemptions == null || pendingRedemptions.Count == 0)
			{
				return null;
			}
			return pendingRedemptions[0];
		}

		public void RemovePendingTransaction(KampaiPendingTransaction kpt)
		{
			pendingTransactions.Remove(kpt);
		}

		public void AddUnlockedItems(QuantityItem item)
		{
			UnlockDefinition unlockDefinition = definitionService.Get<UnlockDefinition>(item.ID);
			int quantity = (int)item.Quantity;
			int referencedDefinitionID = unlockDefinition.ReferencedDefinitionID;
			int unlockedQuantity = unlockDefinition.UnlockedQuantity;
			int num = unlockedQuantity * quantity;
			if (unlocks.ContainsKey(referencedDefinitionID))
			{
				if (unlockDefinition.Delta)
				{
					IDictionary<int, int> dictionary;
					IDictionary<int, int> dictionary2 = (dictionary = unlocks);
					int key;
					int key2 = (key = referencedDefinitionID);
					key = dictionary[key];
					dictionary2[key2] = key + num;
				}
				else
				{
					unlocks[referencedDefinitionID] = Math.Max(num, unlocks[referencedDefinitionID]);
				}
			}
			else
			{
				unlocks.Add(referencedDefinitionID, num);
			}
		}

		public void AddPurchasedUpsells(int saleDefID)
		{
			int num = 1;
			int value;
			if (purchasedUpsells.TryGetValue(saleDefID, out value))
			{
				purchasedUpsells[saleDefID] = value + num;
			}
			else
			{
				purchasedUpsells.Add(saleDefID, num);
			}
		}

		public void ClearPurchasedUpsells(int saleDefID)
		{
			if (purchasedUpsells.ContainsKey(saleDefID))
			{
				purchasedUpsells[saleDefID] = 0;
			}
		}

		public void QueueVillain(Prestige villainPrestige)
		{
			villainQueue.Add(villainPrestige.ID);
		}

		public int PopVillain()
		{
			if (villainQueue.Count == 0)
			{
				return -1;
			}
			int result = villainQueue[0];
			villainQueue.RemoveAt(0);
			return result;
		}

		public int GetUnlockedAmountFromID(int definitionID)
		{
			if (unlocks.Count == 0)
			{
				return -1;
			}
			if (unlocks.ContainsKey(definitionID))
			{
				return unlocks[definitionID];
			}
			return 0;
		}

		public IDictionary<int, int> GetUnlockedItems()
		{
			return unlocks;
		}

		public int GetAmountPurchased(int definitionID)
		{
			int value = 0;
			if (purchasedUpsells.Count == 0 || purchasedUpsells == null)
			{
				return 0;
			}
			if (purchasedUpsells.TryGetValue(definitionID, out value))
			{
				return value;
			}
			return 0;
		}

		public IDictionary<int, int> GetUpsellsPurchased()
		{
			return purchasedUpsells;
		}

		public IList<KampaiPendingTransaction> GetPendingTransactions()
		{
			return pendingTransactions;
		}

		public int GetPurchasedExpansionCount()
		{
			PurchasedLandExpansion purchasedLandExpansion = GetByInstanceId<PurchasedLandExpansion>(354);
			if (purchasedLandExpansion != null)
			{
				return purchasedLandExpansion.PurchasedExpansionsCount();
			}
			return 0;
		}

		public void AddLandExpansion(LandExpansionConfig expansionConfig)
		{
			PurchasedLandExpansion purchasedLandExpansion = GetByInstanceId<PurchasedLandExpansion>(354);
			int expansionId = expansionConfig.expansionId;
			if (purchasedLandExpansion == null)
			{
				return;
			}
			if (!purchasedLandExpansion.HasPurchased(expansionId))
			{
				purchasedLandExpansion.PurchasedExpansions.Add(expansionId);
				if (purchasedLandExpansion.IsAdjacentExpansion(expansionId))
				{
					purchasedLandExpansion.AdjacentExpansions.Remove(expansionId);
				}
			}
			foreach (int adjacentExpansionId in expansionConfig.adjacentExpansionIds)
			{
				if (!purchasedLandExpansion.HasPurchased(adjacentExpansionId) && !purchasedLandExpansion.IsAdjacentExpansion(adjacentExpansionId))
				{
					purchasedLandExpansion.AdjacentExpansions.Add(adjacentExpansionId);
				}
			}
		}

		public bool IsExpansionPurchased(int expansionId)
		{
			PurchasedLandExpansion purchasedLandExpansion = GetByInstanceId<PurchasedLandExpansion>(354);
			if (purchasedLandExpansion != null)
			{
				return purchasedLandExpansion.HasPurchased(expansionId);
			}
			return false;
		}

		public void AddPendingTransactions(IList<KampaiPendingTransaction> pendingTransactions)
		{
			this.pendingTransactions.AddRange(pendingTransactions);
		}

		public void AddUnlock(int id, int quantity)
		{
			unlocks.Add(id, quantity);
		}

		public void AddPurchasedUpsell(int id, int quantity)
		{
			purchasedUpsells.Add(id, quantity);
		}

		public void AddVillainQueue(int characterDefinitionId)
		{
			villainQueue.Add(characterDefinitionId);
		}

		public bool HasPlatformStoreTransactionID(string identifier)
		{
			return PlatformStoreTransactionIDs.Contains(identifier);
		}

		public void AddPlatformStoreTransactionID(string id)
		{
			PlatformStoreTransactionIDs.Add(id);
		}

		public IList<int> GetVillainQueue()
		{
			return villainQueue;
		}

		public IList<string> GetPlatformStoreTransactionIDs()
		{
			return PlatformStoreTransactionIDs;
		}

		public IDictionary<int, SocialClaimRewardItem.ClaimState> GetSocialClaimRewards()
		{
			return socialRewards;
		}

		public void AddSocialClaimRewards(int eventID, SocialClaimRewardItem.ClaimState claimState)
		{
			socialRewards[eventID] = claimState;
		}

		public void CleanupSocialClaimReward(List<int> recentEventIDs)
		{
			IDictionary<int, SocialClaimRewardItem.ClaimState> dictionary = new Dictionary<int, SocialClaimRewardItem.ClaimState>();
			foreach (int recentEventID in recentEventIDs)
			{
				if (socialRewards.ContainsKey(recentEventID))
				{
					dictionary[recentEventID] = socialRewards[recentEventID];
				}
			}
			socialRewards = dictionary;
		}

		public IList<string> GetMTXPurchaseTracking()
		{
			return mtxPurchaseTracking;
		}

		public void AddMTXPurchaseTracking(string SKU)
		{
			mtxPurchaseTracking.Add(SKU);
		}

		public int MTXPurchaseCount(string sku)
		{
			if (string.IsNullOrEmpty(sku))
			{
				return mtxPurchaseTracking.Count;
			}
			int num = 0;
			sku = sku.ToLower();
			foreach (string item in mtxPurchaseTracking)
			{
				if (item.ToLower().Equals(sku))
				{
					num++;
				}
			}
			return num;
		}

		public int GetStorableItemCount()
		{
			uint itemCount = 0u;
			FindStorableItems(out itemCount);
			return (int)(((int)itemCount >= 0) ? itemCount : 0);
		}

		public SanityCheckFailureReason ValidateSaveData(Player prevSave)
		{
			SanityCheckFailureReason sanityCheckFailureReason = SanityCheckFailureReason.Passed;
			sanityCheckFailureReason = ValidateOrderBoardTickets(prevSave);
			if (sanityCheckFailureReason != 0)
			{
				return sanityCheckFailureReason;
			}
			sanityCheckFailureReason = ValidateTSM();
			if (sanityCheckFailureReason != 0)
			{
				return sanityCheckFailureReason;
			}
			sanityCheckFailureReason = ValidateBuildings(prevSave);
			if (sanityCheckFailureReason != 0)
			{
				return sanityCheckFailureReason;
			}
			sanityCheckFailureReason = ValidateMinions(prevSave);
			if (sanityCheckFailureReason != 0)
			{
				return sanityCheckFailureReason;
			}
			sanityCheckFailureReason = ValidateLandExpansions(prevSave);
			if (sanityCheckFailureReason != 0)
			{
				return sanityCheckFailureReason;
			}
			return sanityCheckFailureReason;
		}

		private SanityCheckFailureReason ValidateOrderBoardTickets(Player prevSave)
		{
			OrderBoard firstInstanceByDefinitionId = GetFirstInstanceByDefinitionId<OrderBoard>(3022);
			if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.tickets != null)
			{
				foreach (OrderBoardTicket ticket in firstInstanceByDefinitionId.tickets)
				{
					if (ticket.TransactionInst != null && ticket.TransactionInst.Inputs != null && ticket.TransactionInst.Outputs != null && (ticket.TransactionInst.Inputs.Count == 0 || ticket.TransactionInst.Outputs.Count == 0))
					{
						return SanityCheckFailureReason.OrderBoardTickerInputsOutputs;
					}
				}
				if (prevSave != null)
				{
					OrderBoard firstInstanceByDefinitionId2 = prevSave.GetFirstInstanceByDefinitionId<OrderBoard>(3022);
					if (firstInstanceByDefinitionId2 != null && firstInstanceByDefinitionId2.tickets != null && firstInstanceByDefinitionId2.tickets.Count > firstInstanceByDefinitionId.tickets.Count)
					{
						return SanityCheckFailureReason.OrderBoardTicketCount;
					}
				}
				return SanityCheckFailureReason.Passed;
			}
			return SanityCheckFailureReason.OrderBoardMissing;
		}

		private SanityCheckFailureReason ValidateTSM()
		{
			TSMCharacter firstInstanceByDefinitionId = GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId != null)
			{
				Quest firstInstanceByDefinitionId2 = GetFirstInstanceByDefinitionId<Quest>(77777);
				if (firstInstanceByDefinitionId2 != null)
				{
					TransactionDefinition reward = firstInstanceByDefinitionId2.GetActiveDefinition().GetReward(null);
					if (reward == null || reward.Outputs.Count == 0)
					{
						return SanityCheckFailureReason.TSMOutputs;
					}
				}
			}
			return SanityCheckFailureReason.Passed;
		}

		private SanityCheckFailureReason ValidateBuildings(Player prevSave)
		{
			if (prevSave != null)
			{
				IList<Instance> instancesByDefinition = GetInstancesByDefinition<BuildingDefinition>();
				IList<Instance> instancesByDefinition2 = GetInstancesByDefinition<DebrisBuildingDefinition>();
				IList<Instance> instancesByDefinition3 = prevSave.GetInstancesByDefinition<BuildingDefinition>();
				IList<Instance> instancesByDefinition4 = prevSave.GetInstancesByDefinition<DebrisBuildingDefinition>();
				List<Instance> instancesByDefinition5 = GetInstancesByDefinition<MasterPlanComponentBuildingDefinition>();
				List<Instance> instancesByDefinition6 = prevSave.GetInstancesByDefinition<MasterPlanComponentBuildingDefinition>();
				int num = instancesByDefinition.Count - instancesByDefinition2.Count - instancesByDefinition5.Count;
				int num2 = instancesByDefinition3.Count - instancesByDefinition4.Count - instancesByDefinition6.Count;
				if (num2 > num)
				{
					return SanityCheckFailureReason.BuildingCount;
				}
			}
			StaticItem[] array = new StaticItem[8]
			{
				StaticItem.TIKI_BAR_BUILDING_ID_DEF,
				StaticItem.STAGE_BUILDING_DEF_ID,
				StaticItem.FOUNTAIN_BUILDING_DEF_ID,
				StaticItem.WELCOME_BOOTH_BUILDING_ID_DEF,
				StaticItem.CABANA_ONE_BUILDING_ID_DEF,
				StaticItem.CABANA_TWO_BUILDING_ID_DEF,
				StaticItem.CABANA_THREE_BUILDING_ID_DEF,
				StaticItem.COMPOSITE_TOTEM_POLE_ID_DEF
			};
			StaticItem[] array2 = array;
			foreach (StaticItem definitionId in array2)
			{
				Building firstInstanceByDefinitionId = GetFirstInstanceByDefinitionId<Building>((int)definitionId);
				if (firstInstanceByDefinitionId == null)
				{
					return SanityCheckFailureReason.RequiredBuilding;
				}
			}
			StaticItem[] array3 = new StaticItem[5]
			{
				StaticItem.MIGNETTE_ALLIGATOR_DEF_ID,
				StaticItem.MIGNETTE_BALLOON_DEF_ID,
				StaticItem.MIGNETTE_BUTTERFLY_DEF_ID,
				StaticItem.MIGNETTE_MINION_HANDS_DEF_ID,
				StaticItem.MIGNETTE_WATER_SLIDE_DEF_ID
			};
			StaticItem[] array4 = array3;
			foreach (StaticItem definitionId2 in array4)
			{
				Building firstInstanceByDefinitionId2 = GetFirstInstanceByDefinitionId<Building>((int)definitionId2);
				Building firstInstanceByDefinitionId3 = prevSave.GetFirstInstanceByDefinitionId<Building>((int)definitionId2);
				if (firstInstanceByDefinitionId2 == null && firstInstanceByDefinitionId3 != null)
				{
					return SanityCheckFailureReason.MignetteMissing;
				}
			}
			foreach (Building item in GetInstancesByType<Building>())
			{
				if (item.State != BuildingState.Inventory && item.Location == null)
				{
					return SanityCheckFailureReason.BuildingMissingLocation;
				}
			}
			return SanityCheckFailureReason.Passed;
		}

		private SanityCheckFailureReason ValidateMinions(Player prevSave)
		{
			if (prevSave != null)
			{
				IList<Instance> instancesByDefinition = GetInstancesByDefinition<MinionDefinition>();
				IList<Instance> instancesByDefinition2 = prevSave.GetInstancesByDefinition<MinionDefinition>();
				if (instancesByDefinition2.Count > instancesByDefinition.Count)
				{
					return SanityCheckFailureReason.MinionCount;
				}
				List<int> list = new List<int>();
				foreach (Minion item in instancesByDefinition)
				{
					if (item.HasPrestige)
					{
						list.Add(item.ID);
					}
				}
				foreach (Minion item2 in instancesByDefinition2)
				{
					if (item2.HasPrestige && !list.Contains(item2.ID))
					{
						return SanityCheckFailureReason.CostumedMinion;
					}
				}
			}
			StaticItem[] array = new StaticItem[4]
			{
				StaticItem.PHIL_CHARACTER_DEF_ID,
				StaticItem.STUART_CHARACTER_DEF_ID,
				StaticItem.BOB_CHARACTER_DEF_ID,
				StaticItem.KEVIN_CHARACTER_DEF_ID
			};
			StaticItem[] array2 = array;
			foreach (StaticItem staticItem in array2)
			{
				NamedCharacter firstInstanceByDefinitionId = GetFirstInstanceByDefinitionId<NamedCharacter>((int)staticItem);
				if (firstInstanceByDefinitionId == null && staticItem == StaticItem.PHIL_CHARACTER_DEF_ID)
				{
					return SanityCheckFailureReason.NamedMinion;
				}
				if (prevSave != null)
				{
					NamedCharacter firstInstanceByDefinitionId2 = prevSave.GetFirstInstanceByDefinitionId<NamedCharacter>((int)staticItem);
					if (firstInstanceByDefinitionId2 != null && firstInstanceByDefinitionId == null)
					{
						return SanityCheckFailureReason.NamedMinion;
					}
				}
			}
			return SanityCheckFailureReason.Passed;
		}

		private SanityCheckFailureReason ValidateLandExpansions(Player prevSave)
		{
			PurchasedLandExpansion purchasedLandExpansion = GetByInstanceId<PurchasedLandExpansion>(354);
			PurchasedLandExpansion purchasedLandExpansion2 = prevSave.GetByInstanceId<PurchasedLandExpansion>(354);
			if (purchasedLandExpansion.PurchasedExpansions.Count == 0 && purchasedLandExpansion.AdjacentExpansions.Count == 0)
			{
				return SanityCheckFailureReason.LandExpansionEmpty;
			}
			foreach (int purchasedExpansion in purchasedLandExpansion2.PurchasedExpansions)
			{
				if (!purchasedLandExpansion.PurchasedExpansions.Contains(purchasedExpansion))
				{
					return SanityCheckFailureReason.PurchasedLandExpansionMissing;
				}
			}
			return SanityCheckFailureReason.Passed;
		}

		public void addSocialInvitationSeen(long socialInvitationId)
		{
			if (socialTeamInvitationsSeen == null)
			{
				socialTeamInvitationsSeen = new List<long>();
			}
			socialTeamInvitationsSeen.Add(socialInvitationId);
		}

		public bool seenSocialInvitiation(long socialInvitationId)
		{
			if (socialTeamInvitationsSeen == null)
			{
				return false;
			}
			return socialTeamInvitationsSeen.Contains(socialInvitationId);
		}

		public int getAndIncrementRequestCounter()
		{
			return ++requestCounter;
		}
	}
}
