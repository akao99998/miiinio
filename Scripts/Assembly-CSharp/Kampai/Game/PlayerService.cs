using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class PlayerService : IPlayerService
	{
		protected Player player;

		private object mutex = new object();

		private HashSet<string> segments;

		protected TransactionEngine _engine;

		protected IKampaiLogger logger = LogManager.GetClassLogger("PlayerService") as IKampaiLogger;

		private string swrveGroup;

		protected TransactionEngine engine
		{
			get
			{
				if (_engine == null)
				{
					_engine = new TransactionEngine(logger, definitionService, randService, this);
				}
				return _engine;
			}
		}

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRandomService randService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public PostTransactionSignal postTransactionSignal { get; set; }

		[Inject]
		public InsufficientInputsSignal insufficientInputsSignal { get; set; }

		[Inject]
		public DisplayRandomDropIconSignal randomDropSignal { get; set; }

		[Inject]
		public OpenStorageBuildingSignal openStorageBuildingSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeState { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ITelemetryUtil telemetryUtil { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ShowCraftingBuildingMenuSignal showCraftingBuildingMenuSignal { get; set; }

		[Inject]
		public FTUELevelChangedSignal ftueLevelChangedSignal { get; set; }

		[Inject]
		public SendBuildingToInventorySignal sendBuildingToInventorySignal { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		public long ID
		{
			get
			{
				return player.ID;
			}
			set
			{
				player.ID = value;
			}
		}

		public Player LastSave { get; set; }

		public bool PlayerDataIsLoaded { get; set; }

		public int LevelUpUTC
		{
			get
			{
				return player.lastLevelUpTime;
			}
			set
			{
				player.lastLevelUpTime = value;
			}
		}

		public int LastGameStartUTC
		{
			get
			{
				return player.lastGameStartTime;
			}
			set
			{
				player.lastGameStartTime = value;
			}
		}

		public int FirstGameStartUTC
		{
			get
			{
				return player.firstGameStartTime;
			}
			set
			{
				player.firstGameStartTime = value;
			}
		}

		public int LastPlayedUTC
		{
			get
			{
				return player.lastPlayedTime;
			}
			set
			{
				player.lastPlayedTime = value;
			}
		}

		public int TimeZoneOffset
		{
			get
			{
				return player.timezoneOffset;
			}
			set
			{
				player.timezoneOffset = value;
			}
		}

		public string SWRVEGroup
		{
			get
			{
				return (!string.IsNullOrEmpty(swrveGroup)) ? swrveGroup : "anyVariant";
			}
			set
			{
				swrveGroup = value;
			}
		}

		public int GameplaySecondsSinceLevelUp
		{
			get
			{
				return player.totalGameplayDurationSinceLastLevelUp;
			}
			set
			{
				player.totalGameplayDurationSinceLastLevelUp = value;
			}
		}

		public int AccumulatedGameplayDuration
		{
			get
			{
				return player.totalAccumulatedGameplayDuration;
			}
			set
			{
				player.totalAccumulatedGameplayDuration = value;
			}
		}

		public List<Player.HelpTipTrackingItem> helpTipsTrackingData
		{
			get
			{
				return player.helpTipsTrackingData;
			}
		}

		public PlayerService()
		{
		}

		public PlayerService(Player player)
			: this()
		{
			this.player = player;
		}

		public uint GetQuantity(StaticItem def)
		{
			return player.GetQuantity(def);
		}

		public int GetCountByDefinitionId(int defId)
		{
			return player.GetCountByDefinitionId(defId);
		}

		public uint GetQuantityByDefinitionId(int defId)
		{
			return player.GetQuantityByDefinitionId(defId);
		}

		public uint GetTotalCountByDefinitionId(int defId)
		{
			return player.GetTotalCountByDefinitionId(defId);
		}

		public uint GetQuantityByInstanceId(int instanceId)
		{
			return player.GetQuantityByInstanceId(instanceId);
		}

		public void AlterQuantity(StaticItem def, int amount)
		{
			player.AlterQuantity(def, amount);
		}

		public void SetQuantity(StaticItem def, int amount)
		{
			if (amount >= 0)
			{
				player.SetQuantityByStaticItem(def, (uint)amount);
			}
			else
			{
				logger.Fatal(FatalCode.PS_NEGATIVE_VALUE, amount);
			}
		}

		public ICollection<int> GetAnimatingBuildingIDs()
		{
			return player.FindAllAnimatingBuildingIDs();
		}

		public List<Item> GetSellableItems()
		{
			uint totalStorableQuantity = 0u;
			uint totalSellableQuantity = 0u;
			return player.GetSellableItems(out totalStorableQuantity, out totalSellableQuantity);
		}

		public List<Item> GetSellableItems(out uint totalStorableQuantity, out uint totalSellableQuantity)
		{
			return player.GetSellableItems(out totalStorableQuantity, out totalSellableQuantity);
		}

		public IList<Instance> GetInstancesByDefinition<T>() where T : Definition
		{
			return player.GetInstancesByDefinition<T>();
		}

		public IList<Instance> GetInstancesByDefinitionID(int defId)
		{
			return player.GetInstancesByDefinitionID(defId);
		}

		public void GetInstancesByType<T>(ref List<T> list) where T : class, Instance
		{
			player.GetInstancesByType(ref list);
		}

		public List<T> GetInstancesByType<T>() where T : class, Instance
		{
			List<T> result = new List<T>();
			player.GetInstancesByType(ref result);
			return result;
		}

		public IList<Item> GetItemsByDefinition<T>() where T : Definition
		{
			return player.GetItemsByDefinition<T>();
		}

		public List<int> GetUnavailableBuildingIDSForItem(int itemID)
		{
			return GetUnavailableBuildingIDSForItem(definitionService.Get<IngredientsItemDefinition>(itemID));
		}

		private List<int> GetUnavailableBuildingIDSForItem(IngredientsItemDefinition item)
		{
			List<int> list = new List<int>();
			if (item != null)
			{
				int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(item.ID);
				if (GetFirstInstanceByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID) == null)
				{
					list.Add(buildingDefintionIDFromItemDefintionID);
				}
				IList<QuantityItem> inputs = definitionService.Get<TransactionDefinition>(item.TransactionId).Inputs;
				foreach (QuantityItem item2 in inputs)
				{
					buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(item2.ID);
					if (GetFirstInstanceByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID) == null)
					{
						list.Add(buildingDefintionIDFromItemDefintionID);
						break;
					}
					list.AddRange(GetUnavailableBuildingIDSForItem(item2.ID));
				}
			}
			return list;
		}

		public bool HasPurchasedBuildingAssociatedWithItem(IngredientsItemDefinition item)
		{
			int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(item.ID);
			if (buildingDefintionIDFromItemDefintionID == 0)
			{
				logger.Error("Can't find building for itemDefinitionID {0}", item.ID);
				return false;
			}
			return GetFirstInstanceByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID) != null;
		}

		public bool ItemUsesBuildingsInList(int itemID, List<int> buildingIDs)
		{
			return ItemUsesBuildingsInList(definitionService.Get<IngredientsItemDefinition>(itemID), buildingIDs);
		}

		public bool ItemUsesBuildingsInList(IngredientsItemDefinition item, List<int> buildingIDs)
		{
			bool result = false;
			if (item != null)
			{
				int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(item.ID);
				if (buildingIDs.Contains(buildingDefintionIDFromItemDefintionID))
				{
					result = true;
				}
				else
				{
					IList<QuantityItem> inputs = definitionService.Get<TransactionDefinition>(item.TransactionId).Inputs;
					foreach (QuantityItem item2 in inputs)
					{
						buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(item2.ID);
						if (buildingIDs.Contains(buildingDefintionIDFromItemDefintionID) || ItemUsesBuildingsInList(item2.ID, buildingIDs))
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		public ICollection<Item> GetStorableItems(out uint itemCount)
		{
			return player.FindStorableItems(out itemCount);
		}

		public Dictionary<int, int> GetBuildingOnBoardCountMap()
		{
			IList<Building> instancesByType = player.GetInstancesByType<Building>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int num = 0;
			foreach (Building item in instancesByType)
			{
				if (item.State != BuildingState.Inventory)
				{
					num = item.Definition.ID;
					if (dictionary.ContainsKey(num))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key;
						int key2 = (key = num);
						key = dictionary2[key];
						dictionary3[key2] = key + 1;
					}
					else
					{
						dictionary.Add(num, 1);
					}
				}
			}
			return dictionary;
		}

		public T GetByInstanceId<T>(int id) where T : class, Instance
		{
			return player.GetByInstanceId<T>(id);
		}

		public T GetFirstInstanceByDefinitionId<T>(int definitionId) where T : class, Instance
		{
			return player.GetFirstInstanceByDefinitionId<T>(definitionId);
		}

		public I GetFirstInstanceByDefintion<I, D>() where I : class, Instance where D : Definition
		{
			return player.GetFirstInstanceByDefintion<I, D>();
		}

		public int GetUnlockedQuantityOfID(int defId)
		{
			return player.GetUnlockedAmountFromID(defId);
		}

		public int GetPurchasedQuanityByUpsellID(int defId)
		{
			return player.GetAmountPurchased(defId);
		}

		public IDictionary<int, int> GetAllUpsellsPurchased()
		{
			return player.GetUpsellsPurchased();
		}

		public void AddUpsellToPurchased(int defID)
		{
			player.AddPurchasedUpsells(defID);
		}

		public void ClearPurchasedUpsells(int defID)
		{
			player.ClearPurchasedUpsells(defID);
		}

		public IList<T> GetUnlockedDefsByType<T>() where T : Definition
		{
			return player.FindAllUnLockedByType<T>();
		}

		public bool IsMinionPartyUnlocked()
		{
			return GetUnlockedQuantityOfID(80000) > 0;
		}

		public bool HasPurchasedMinigamePack()
		{
			return GetPurchasedQuanityByUpsellID(50002) > 0;
		}

		private bool RunTransaction(TransactionDefinition transaction, out IList<Instance> outputs, TransactionArg arg = null)
		{
			outputs = null;
			if (transaction != null)
			{
				return engine.Perform(player, transaction, out outputs, arg);
			}
			logger.Fatal(FatalCode.PS_NULL_TRANSACTION, "Null transaction");
			return false;
		}

		public Player LoadPlayerData(string serialized)
		{
			Player player = null;
			lock (mutex)
			{
				try
				{
					PlayerVersion playerVersion = JsonConvert.DeserializeObject<PlayerVersion>(serialized);
					logger.Info("Player Version is {0}", playerVersion.Version);
					player = playerVersion.CreatePlayer(serialized, definitionService, localPersistanceService, partyService, logger);
					if (player == null)
					{
						throw new FatalException(FatalCode.PS_NULL_PLAYER, "PlayerService.LoadPlayerData(): null player");
					}
				}
				catch (JsonSerializationException e)
				{
					HandleJsonParseException(serialized, e);
				}
				catch (JsonReaderException e2)
				{
					HandleJsonParseException(serialized, e2);
				}
			}
			return player;
		}

		private void HandleJsonParseException(string json, Exception e)
		{
			logger.Error("HandleJsonParseException(): player json: {0}", json ?? "null");
			throw new FatalException(FatalCode.PS_JSON_PARSE_ERR, 5, e, "Json Parse Err: {0}", e);
		}

		public void Deserialize(string serialized, bool isRetry = false)
		{
			player = LoadPlayerData(serialized);
			LastSave = LoadPlayerData(serialized);
		}

		public byte[] SavePlayerData(Player playerData)
		{
			byte[] result = null;
			lock (mutex)
			{
				try
				{
					PlayerVersion playerVersion = new PlayerVersion();
					playerData.country = localizationService.GetCountry();
					result = playerVersion.Serialize(playerData, definitionService, logger);
				}
				catch (JsonSerializationException ex)
				{
					logger.Fatal(FatalCode.PS_JSON_SERIALIZE_ERR, "Json Err: {0}", ex.ToString());
				}
			}
			return result;
		}

		public byte[] Serialize()
		{
			return SavePlayerData(player);
		}

		public bool IsPlayerInitialized()
		{
			return player != null;
		}

		public void Add(Instance i)
		{
			int iD = i.ID;
			if (iD != 0)
			{
				Instance byInstanceId = player.GetByInstanceId<Instance>(iD);
				if (byInstanceId != null)
				{
					logger.Fatal(FatalCode.PS_ITEM_ALREADY_ADDED, "Item {0} already exists in player inventory.", iD);
					return;
				}
			}
			player.AssignNextInstanceId(i);
			player.Add(i);
		}

		public void AssignNextInstanceId(Identifiable i)
		{
			player.AssignNextInstanceId(i);
		}

		public void Remove(Instance i)
		{
			if (i.ID != 0)
			{
				player.Remove(i);
			}
		}

		public ICollection<T> GetByDefinitionId<T>(int id) where T : Instance
		{
			return player.GetByDefinitionId<T>(id);
		}

		public WeightedInstance GetWeightedInstance(int defId, WeightedDefinition wd = null)
		{
			return player.GetWeightedInstance(defId, wd);
		}

		public bool CanAffordExchange(int grindNeeded)
		{
			return player.GetQuantityByDefinitionId(1) >= PremiumCostForGrind(grindNeeded);
		}

		public int PremiumCostForGrind(int grindNeeded)
		{
			return engine.RequiredPremiumForGrind(grindNeeded);
		}

		public void ExchangePremiumForGrind(int grindNeeded, Action<PendingCurrencyTransaction> callback)
		{
			int num = engine.RequiredPremiumForGrind(grindNeeded);
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.ID = int.MaxValue;
			IList<QuantityItem> list = new List<QuantityItem>();
			IList<QuantityItem> list2 = new List<QuantityItem>();
			grindNeeded = engine.PremiumToGrind(num);
			list.Add(new QuantityItem(1, (uint)num));
			list2.Add(new QuantityItem(0, (uint)grindNeeded));
			transactionDefinition.Inputs = list;
			transactionDefinition.Outputs = list2;
			ProcessItemPurchase(num, list2, true, callback, true);
		}

		public int CalculateRushCost(IList<QuantityItem> items)
		{
			return engine.CalculateRushCost(player, items);
		}

		public void ProcessSlotPurchase(int slotCost, bool showStoreOnFail, int slotNumber, Action<PendingCurrencyTransaction> callback, int instanceId)
		{
			List<int> list = new List<int>();
			list.Add(slotCost);
			string source = "SlotPurchase" + slotNumber;
			ProcessPremiumTransaction(list, null, showStoreOnFail, callback, source, instanceId, true);
		}

		public void ProcessSaleCancel(int cost, Action<PendingCurrencyTransaction> callback)
		{
			List<int> list = new List<int>();
			list.Add(cost);
			string source = "DeleteSale";
			ProcessPremiumTransaction(list, null, true, callback, source, 314, false, true);
		}

		public void ProcessRefreshMarket(int cost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			IList<int> list = new List<int>();
			list.Add(cost);
			ProcessPremiumTransaction(list, null, showStoreOnFail, callback, "RefreshMarket", 3117, true);
		}

		public void ProcessOrderFill(int orderCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			List<int> list = new List<int>();
			list.Add(orderCost);
			string source = "OrderCompletion";
			ProcessPremiumTransaction(list, items, showStoreOnFail, callback, source, 3022, false, true);
		}

		public void ProcessItemPurchase(int itemCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			List<int> list = new List<int>();
			list.Add(itemCost);
			ProcessPremiumTransaction(list, items, showStoreOnFail, callback, "ItemPurchase", 0, true, byPassStorageCheck);
		}

		public void ProcessItemPurchase(IList<int> itemCosts, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			ProcessPremiumTransaction(itemCosts, items, showStoreOnFail, callback, "ItemPurchase", 0, true, byPassStorageCheck);
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, int instanceId)
		{
			IList<int> list = new List<int>();
			list.Add(rushCost);
			ProcessPremiumTransaction(list, null, showStoreOnFail, callback, "Rush", instanceId, true);
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback)
		{
			ProcessRush(rushCost, null, showStoreOnFail, callback);
		}

		public void ProcessRush(int rushCost, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback)
		{
			ProcessRush(rushCost, null, showStoreOnFail, source, callback);
		}

		public void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			List<int> list = new List<int>();
			list.Add(rushCost);
			ProcessPremiumTransaction(list, items, showStoreOnFail, callback, "Rush", 0, true, byPassStorageCheck);
		}

		public void ProcessRush(int rushCost, IList<QuantityItem> items, bool showStoreOnFail, string source, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			List<int> list = new List<int>();
			list.Add(rushCost);
			ProcessPremiumTransaction(list, items, showStoreOnFail, callback, source, 0, true, byPassStorageCheck);
		}

		public void ProcessRush(IList<int> itemCostList, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, bool byPassStorageCheck = false)
		{
			ProcessPremiumTransaction(itemCostList, items, showStoreOnFail, callback, "Rush", 0, true, byPassStorageCheck);
		}

		public void ProcessPremiumTransaction(IList<int> rushCosts, IList<QuantityItem> items, bool showStoreOnFail, Action<PendingCurrencyTransaction> callback, string source, int instanceId, bool reportItems, bool byPassStorageCheck = false)
		{
			if (!byPassStorageCheck && !CheckStorageCapacity(items, TransactionTarget.NO_VISUAL, null))
			{
				return;
			}
			int num = 0;
			foreach (int rushCost in rushCosts)
			{
				num += rushCost;
			}
			if (player.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID) >= num)
			{
				player.AlterQuantity(StaticItem.PREMIUM_CURRENCY_ID, -num);
				if (items != null)
				{
					engine.AddOutputs(player, items);
				}
				if (items != null && reportItems)
				{
					for (int i = 0; i < items.Count; i++)
					{
						int num2 = num;
						if (rushCosts.Count - 1 >= i)
						{
							num2 = rushCosts[i];
						}
						if (items[i].ID == 0)
						{
							TransactionUpdateData transactionUpdateData = createPremiumPurchaseTransactionData(items[i], num2, source, instanceId);
							transactionUpdateData.IsFromPremiumSource = true;
							transactionUpdateData.IsNotForPlayerTraining = true;
							postTransactionSignal.Dispatch(transactionUpdateData);
						}
						else
						{
							List<QuantityItem> list = new List<QuantityItem>();
							list.Add(items[i]);
							SetAndSendUpdateData(source, instanceId, true, list, num2);
						}
					}
				}
				else
				{
					SetAndSendUpdateData(source, instanceId, false, items, num);
				}
				Success(callback, null, true, num, items, null);
			}
			else if (showStoreOnFail)
			{
				SendRushTelemetry(source, instanceId, num, items);
				insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(null, true, num, items, null, callback), false);
			}
			else
			{
				Fail(callback, null, true, num, items, null);
			}
		}

		private void SendRushTelemetry(string source, int instanceId, int rushCost, IList<QuantityItem> items)
		{
			TransactionUpdateData update = CreatePremiumPurchaseTransactionData(source, instanceId, true, items, rushCost);
			string highLevel = string.Empty;
			string specific = string.Empty;
			string type = string.Empty;
			string other = string.Empty;
			telemetryUtil.DetermineTaxonomy(update, true, out highLevel, out specific, out type, out other);
			string sourceName = telemetryUtil.GetSourceName(update);
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(1);
			telemetryService.Send_Telemetry_EVT_PINCH_PROMPT(sourceName, itemDefinition.LocalizedKey, rushCost, highLevel, specific, type, "null");
		}

		private void SetAndSendUpdateData(string source, int instanceId, bool isPremium, IList<QuantityItem> items, int totalRushCost)
		{
			TransactionUpdateData param = CreatePremiumPurchaseTransactionData(source, instanceId, isPremium, items, totalRushCost);
			postTransactionSignal.Dispatch(param);
		}

		private TransactionUpdateData CreatePremiumPurchaseTransactionData(string source, int instanceId, bool isPremium, IList<QuantityItem> items, int totalRushCost)
		{
			TransactionUpdateData transactionUpdateData = new TransactionUpdateData();
			transactionUpdateData.Type = UpdateType.OTHER;
			transactionUpdateData.Source = source;
			transactionUpdateData.InstanceId = instanceId;
			transactionUpdateData.IsFromPremiumSource = isPremium;
			transactionUpdateData.IsNotForPlayerTraining = true;
			transactionUpdateData.Outputs = items;
			transactionUpdateData.AddInput(1, totalRushCost);
			return transactionUpdateData;
		}

		private TransactionUpdateData createPremiumPurchaseTransactionData(QuantityItem output, int itemCost, string source, int instanceId)
		{
			TransactionUpdateData transactionUpdateData = new TransactionUpdateData();
			transactionUpdateData.Type = UpdateType.OTHER;
			transactionUpdateData.Source = source;
			transactionUpdateData.InstanceId = instanceId;
			IList<QuantityItem> list = new List<QuantityItem>();
			list.Add(output);
			transactionUpdateData.Outputs = list;
			transactionUpdateData.AddInput(1, itemCost);
			return transactionUpdateData;
		}

		public bool IsMissingItemFromTransaction(TransactionDefinition transactionDefinition)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				for (int i = 0; i < count; i++)
				{
					uint quantityByDefinitionId = GetQuantityByDefinitionId(inputs[i].ID);
					int num = (int)(inputs[i].Quantity - quantityByDefinitionId);
					if (num > 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public IList<QuantityItem> GetMissingItemListFromTransaction(TransactionDefinition transactionDefinition)
		{
			IList<QuantityItem> list = new List<QuantityItem>();
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				for (int i = 0; i < count; i++)
				{
					QuantityItem quantityItem = null;
					uint quantityByDefinitionId = GetQuantityByDefinitionId(inputs[i].ID);
					int num = (int)(inputs[i].Quantity - quantityByDefinitionId);
					if (num > 0)
					{
						quantityItem = new QuantityItem(inputs[i].ID, (uint)num);
						list.Add(quantityItem);
					}
				}
			}
			return list;
		}

		private void SetTransactionTime(ref TransactionArg arg)
		{
			if (arg == null)
			{
				arg = new TransactionArg();
			}
			arg.TransactionUTCTime = timeService.CurrentTime();
		}

		public MinionParty GetMinionPartyInstance()
		{
			List<MinionParty> instancesByType = GetInstancesByType<MinionParty>();
			if (instancesByType.Count == 0)
			{
				MinionPartyDefinition definition = definitionService.Get<MinionPartyDefinition>(80000);
				MinionParty minionParty = new MinionParty(definition);
				Add(minionParty);
				return minionParty;
			}
			return instancesByType[0];
		}

		public void AddXP(int xp)
		{
			AlterQuantity(StaticItem.XP_ID, xp);
			if (IsMinionPartyUnlocked())
			{
				UpdateMinionPartyPointValues();
			}
		}

		public void UpdateMinionPartyPointValues()
		{
			MinionParty minionPartyInstance = GetMinionPartyInstance();
			if (minionPartyInstance != null)
			{
				int quantity = (int)GetQuantity(StaticItem.LEVEL_PARTY_INDEX_ID);
				int quantity2 = (int)GetQuantity(StaticItem.LEVEL_ID);
				minionPartyInstance.CurrentPartyIndex = quantity;
				minionPartyInstance.TotalLevelPartiesCount = partyService.GetTotalParties(quantity2);
				minionPartyInstance.CurrentPartyPoints = GetQuantity(StaticItem.XP_ID);
				minionPartyInstance.CurrentPartyPointsRequired = partyService.GetTotalPartyPoints(quantity2, quantity);
			}
		}

		public bool VerifyTransaction(int transactionId)
		{
			TransactionDefinition transactiondef = definitionService.Get<TransactionDefinition>(transactionId);
			return VerifyTransaction(transactiondef);
		}

		public bool VerifyTransaction(TransactionDefinition transactiondef)
		{
			return engine.ValidateInputs(player, transactiondef);
		}

		public void StartTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0)
		{
			TransactionDefinition td = definitionService.Get<TransactionDefinition>(transactionId);
			StartTransaction(td, target, callback, arg, startTime, index);
		}

		public void StartTransaction(TransactionDefinition td, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null, int startTime = 0, int index = 0)
		{
			SetTransactionTime(ref arg);
			switch (target)
			{
			case TransactionTarget.INGREDIENT:
			case TransactionTarget.REPAIR_BRIDGE:
				if (!VerifyTransaction(td))
				{
					insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, true, 0, td.Inputs, null, callback, TransactionTarget.NO_VISUAL, arg), false);
					return;
				}
				break;
			case TransactionTarget.BLACKMARKETBOARD:
			{
				if (!VerifyTransaction(td))
				{
					insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, true, 0, td.Inputs, null, callback, TransactionTarget.NO_VISUAL, arg), false);
					return;
				}
				OrderBoard byInstanceId2 = GetByInstanceId<OrderBoard>(arg.InstanceId);
				foreach (OrderBoardTicket ticket in byInstanceId2.tickets)
				{
					if (ticket.BoardIndex == index)
					{
						ticket.StartTime = startTime;
						break;
					}
				}
				break;
			}
			case TransactionTarget.CLEAR_DEBRIS:
			{
				if (!VerifyTransaction(td))
				{
					insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, true, 0, td.Inputs, null, callback, target, arg), false);
					return;
				}
				DebrisBuilding byInstanceId = GetByInstanceId<DebrisBuilding>(arg.InstanceId);
				byInstanceId.PaidInputCostToClear = true;
				break;
			}
			}
			if (!engine.SubtractInputs(player, td))
			{
				insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, false, 0, null, null, callback, TransactionTarget.NO_VISUAL, arg), false);
				return;
			}
			SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_START, null, target);
			Success(callback, td, false, 0, null, null);
		}

		public bool FinishTransaction(int transactionId, TransactionTarget target, TransactionArg arg = null)
		{
			IList<Instance> outputs;
			return FinishTransaction(transactionId, target, out outputs, arg);
		}

		public bool FinishTransaction(TransactionDefinition td, TransactionTarget target, TransactionArg arg = null)
		{
			IList<Instance> outputs;
			return FinishTransaction(td, target, out outputs, arg);
		}

		public bool FinishTransaction(int transactionId, TransactionTarget target, out IList<Instance> outputs, TransactionArg arg = null)
		{
			TransactionDefinition td = definitionService.Get<TransactionDefinition>(transactionId);
			return FinishTransaction(td, target, out outputs, arg);
		}

		public bool FinishTransaction(TransactionDefinition td, TransactionTarget target, out IList<Instance> outputs, TransactionArg arg = null)
		{
			SetTransactionTime(ref arg);
			bool flag = false;
			outputs = null;
			if (!CheckStorageCapacity(td.Outputs, target, arg))
			{
				return false;
			}
			if (engine.AddOutputs(player, td, out outputs, arg))
			{
				SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_FINISH, outputs, target);
				flag = true;
			}
			else
			{
				flag = false;
			}
			CheckRandomDrop(target, arg);
			return flag;
		}

		private int RandomDropIncrement(TransactionTarget target, TransactionArg arg = null)
		{
			int result = 0;
			switch (target)
			{
			case TransactionTarget.HARVEST:
				result = 1;
				break;
			case TransactionTarget.TASK_COMPLETE:
			case TransactionTarget.TASK_COMPLETE_INGREDIENT:
			{
				if (arg == null)
				{
					logger.LogNullArgument();
					break;
				}
				TaskTransactionArgument taskTransactionArgument = arg.Get<TaskTransactionArgument>();
				if (taskTransactionArgument == null)
				{
					logger.LogNullArgument();
				}
				else
				{
					result = taskTransactionArgument.DropStep;
				}
				break;
			}
			}
			return result;
		}

		private bool IsLastItemInStack(int instanceId)
		{
			Building byInstanceId = GetByInstanceId<Building>(instanceId);
			ResourceBuilding resourceBuilding = byInstanceId as ResourceBuilding;
			if (resourceBuilding != null)
			{
				return resourceBuilding.AvailableHarvest == 1;
			}
			CraftingBuilding craftingBuilding = byInstanceId as CraftingBuilding;
			if (craftingBuilding != null)
			{
				return craftingBuilding.CompletedCrafts.Count == 1;
			}
			return true;
		}

		private void CheckRandomDrop(TransactionTarget target, TransactionArg arg = null)
		{
			int num = RandomDropIncrement(target, arg);
			if (num <= 0)
			{
				return;
			}
			int instanceId = ((arg != null) ? arg.InstanceId : 0);
			AlterQuantity(StaticItem.ACTIONS_SINCE_LAST_DROP, num);
			DropLevelBandDefinition dropLevelBandDefinition = definitionService.Get<DropLevelBandDefinition>(88889);
			int value = (int)(GetQuantity(StaticItem.LEVEL_ID) - 1);
			value = Mathf.Clamp(value, 0, dropLevelBandDefinition.HarvestsPerDrop.Count - 1);
			if (GetQuantity(StaticItem.ACTIONS_SINCE_LAST_DROP) >= dropLevelBandDefinition.HarvestsPerDrop[value] && (target != TransactionTarget.HARVEST || IsLastItemInStack(instanceId)))
			{
				player.SetQuantityByStaticItem(StaticItem.ACTIONS_SINCE_LAST_DROP, 0u);
				Action<PendingCurrencyTransaction> callback = delegate(PendingCurrencyTransaction pct)
				{
					randomDropSignal.Dispatch(Tuple.Create(pct.GetOutputs()[0].Definition.ID, instanceId));
				};
				RunEntireTransaction(5037, TransactionTarget.NO_VISUAL, callback, arg);
			}
		}

		public void RunEntireTransaction(int transactionId, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null)
		{
			TransactionDefinition td = definitionService.Get<TransactionDefinition>(transactionId);
			RunEntireTransaction(td, target, callback, arg);
		}

		public void RunEntireTransaction(TransactionDefinition td, TransactionTarget target, Action<PendingCurrencyTransaction> callback, TransactionArg arg = null)
		{
			SetTransactionTime(ref arg);
			bool isRush = false;
			IList<Instance> outputs = null;
			switch (target)
			{
			case TransactionTarget.CURRENCY:
				if (RunTransaction(td, out outputs, arg))
				{
					SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_FULL, outputs, target);
					CheckRandomDrop(target, arg);
					Success(callback, td, false, 0, null, outputs);
				}
				else
				{
					Fail(callback, td, false, 0, null, null);
					logger.Fatal(FatalCode.PS_UNABLE_TO_RUN_PENDING_TRANSACTION);
				}
				return;
			case TransactionTarget.STORAGEBUILDING:
				if (RunTransaction(td, out outputs, arg))
				{
					GetByInstanceId<StorageBuilding>(arg.InstanceId).CurrentStorageBuildingLevel++;
					SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_FULL, outputs, target);
					CheckRandomDrop(target, arg);
					Success(callback, td, false, 0, null, outputs);
				}
				else
				{
					insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, false, 0, null, null, callback, TransactionTarget.NO_VISUAL, arg), false);
				}
				return;
			case TransactionTarget.INGREDIENT:
			case TransactionTarget.TASK_COMPLETE_INGREDIENT:
				isRush = true;
				break;
			case TransactionTarget.MARKETPLACE:
				if (!CheckStorageCapacity(td.Outputs, target, arg, true))
				{
					Fail(callback, td, true, 0, null, null, CurrencyTransactionFailReason.STORAGE);
				}
				else if (VerifyTransaction(td))
				{
					RunTransaction(td, out outputs, arg);
					SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_FULL, outputs, target);
					CheckRandomDrop(target, arg);
					Success(callback, td, isRush, 0, null, outputs);
				}
				else
				{
					insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, isRush, 0, null, null, callback, TransactionTarget.NO_VISUAL, arg), false);
				}
				return;
			}
			if (VerifyTransaction(td))
			{
				RunTransaction(td, out outputs, arg);
				SendTransactionUpdate(td, arg, UpdateType.TRANSACTION_FULL, outputs, target);
				CheckRandomDrop(target, arg);
				Success(callback, td, isRush, 0, null, outputs);
			}
			else
			{
				insufficientInputsSignal.Dispatch(new PendingCurrencyTransaction(td, isRush, 0, null, null, callback, TransactionTarget.NO_VISUAL, arg), false);
			}
		}

		private void SendTransactionUpdate(TransactionDefinition td, TransactionArg arg, UpdateType type, IList<Instance> newItems, TransactionTarget target)
		{
			TransactionUpdateData transactionUpdateData = new TransactionUpdateData();
			transactionUpdateData.Type = type;
			transactionUpdateData.TransactionId = td.ID;
			transactionUpdateData.InstanceId = ((arg != null) ? arg.InstanceId : 0);
			transactionUpdateData.startPosition = ((arg != null) ? arg.StartPosition : Vector3.zero);
			transactionUpdateData.fromGlass = arg != null && arg.fromGlass;
			transactionUpdateData.Source = ((arg != null) ? arg.Source : null);
			transactionUpdateData.NewItems = newItems;
			transactionUpdateData.IsFromPremiumSource = arg.IsFromPremiumSource;
			transactionUpdateData.Target = target;
			transactionUpdateData.IsNotForPlayerTraining = arg.IsFromQuestSource != 0;
			transactionUpdateData.craftableXPEarned = ((arg != null) ? arg.CraftableXPEarned : 0);
			if (arg.IsFromQuestSource == 1)
			{
				transactionUpdateData.Source = "QuestStep";
			}
			else if (arg.IsFromQuestSource == 2)
			{
				transactionUpdateData.Source = "TSMQuestStep";
			}
			else if (arg.IsFromQuestSource == 3)
			{
				transactionUpdateData.Source = "MasterPlanQuestStep";
			}
			switch (type)
			{
			case UpdateType.TRANSACTION_START:
				transactionUpdateData.Inputs = td.Inputs;
				break;
			case UpdateType.TRANSACTION_FINISH:
				transactionUpdateData.Outputs = td.Outputs;
				break;
			case UpdateType.TRANSACTION_FULL:
				transactionUpdateData.Inputs = td.Inputs;
				transactionUpdateData.Outputs = td.Outputs;
				break;
			}
			postTransactionSignal.Dispatch(transactionUpdateData);
		}

		public void StopTask(int minionId)
		{
			Minion byInstanceId = GetByInstanceId<Minion>(minionId);
			if (byInstanceId == null)
			{
				logger.Fatal(FatalCode.PS_NO_SUCH_MINION, minionId);
			}
			TaskableBuilding byInstanceId2 = GetByInstanceId<TaskableBuilding>(byInstanceId.BuildingID);
			if (byInstanceId2 == null)
			{
				logger.Fatal(FatalCode.PS_NO_SUCH_INSTANCE_TASKING, byInstanceId.BuildingID);
			}
			byInstanceId.BuildingID = -1;
			byInstanceId.UTCTaskStartTime = -1;
			byInstanceId.State = MinionState.Idle;
			byInstanceId.PartyTimeReduction = 0;
			byInstanceId2.RemoveMinion(minionId, timeService.CurrentTime());
			if (byInstanceId2.GetMinionsInBuilding() == 0)
			{
				changeState.Dispatch(byInstanceId2.ID, BuildingState.Idle);
			}
			byInstanceId2.StateStartTime = 0;
		}

		public void BuyCraftingSlot(int buildingID)
		{
			GetByInstanceId<CraftingBuilding>(buildingID).Slots++;
		}

		public void UpdateCraftingQueue(int buildingID, int itemDefId)
		{
			CraftingBuilding byInstanceId = GetByInstanceId<CraftingBuilding>(buildingID);
			byInstanceId.RecipeInQueue.Add(itemDefId);
		}

		public bool VerifyPlayerHasRequiredInputs(IList<QuantityItem> inputs)
		{
			bool result = true;
			foreach (QuantityItem input in inputs)
			{
				if (GetQuantityByDefinitionId(input.ID) < input.Quantity)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public void PurchaseSlotForBuilding(int buildingID, int level)
		{
			ResourceBuilding byInstanceId = GetByInstanceId<ResourceBuilding>(buildingID);
			int num = byInstanceId.BuildingNumber - 1;
			if (num < 0)
			{
				num = GetInstancesByDefinitionID(byInstanceId.Definition.ID).Count;
			}
			int count = byInstanceId.Definition.SlotUnlocks.Count;
			num = ((num <= count - 1) ? num : (count - 1));
			if (byInstanceId.MinionSlotsOwned < byInstanceId.Definition.SlotUnlocks[num].SlotUnlockLevels.Count && byInstanceId.Definition.SlotUnlocks[num].SlotUnlockLevels[byInstanceId.MinionSlotsOwned] == level)
			{
				byInstanceId.IncrementMinionSlotsOwned();
			}
		}

		public int GetMinionCount()
		{
			return GetInstancesByType<Minion>().Count;
		}

		private bool CheckStorageCapacity(IList<QuantityItem> outputs, TransactionTarget target, TransactionArg arg, bool allowDrops = false)
		{
			if (target == TransactionTarget.AUTOMATIC)
			{
				return true;
			}
			uint num = 0u;
			CraftingBuilding craftingBuilding = null;
			bool flag = false;
			if (arg != null && arg.InstanceId != 0)
			{
				craftingBuilding = GetByInstanceId<CraftingBuilding>(arg.InstanceId);
				if (craftingBuilding != null)
				{
					flag = true;
				}
			}
			if (outputs != null)
			{
				foreach (QuantityItem output in outputs)
				{
					int iD = output.ID;
					if (iD != 0 && iD != 2 && iD != 1 && iD != 3)
					{
						ItemDefinition definition;
						definitionService.TryGet<ItemDefinition>(output.ID, out definition);
						if (definition != null && (allowDrops || !(definition is DropItemDefinition)) && !(definition is CostumeItemDefinition) && !(definition is PartyFavorAnimationItemDefinition))
						{
							num += output.Quantity;
						}
					}
				}
			}
			if (num == 0)
			{
				return true;
			}
			StorageBuilding storageBuilding = null;
			using (IEnumerator<StorageBuilding> enumerator2 = player.GetByDefinitionId<StorageBuilding>(3018).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					StorageBuilding current2 = enumerator2.Current;
					storageBuilding = current2;
				}
			}
			if (storageBuilding == null)
			{
				return true;
			}
			uint currentStorageCapacity = GetCurrentStorageCapacity();
			uint itemCount = GetStorageCount();
			player.FindStorableItems(out itemCount);
			uint num2 = itemCount + num;
			if (num2 > currentStorageCapacity)
			{
				telemetryService.Send_Telemetry_EVT_STORAGE_LIMIT_HIT((int)currentStorageCapacity);
				if (target.Equals(TransactionTarget.HARVEST) && flag && craftingBuilding != null)
				{
					showCraftingBuildingMenuSignal.Dispatch(craftingBuilding);
				}
				else if (!target.Equals(TransactionTarget.MARKETPLACE))
				{
					openStorageBuildingSignal.Dispatch(storageBuilding, true);
				}
				return false;
			}
			return true;
		}

		public void CreateAndRunCustomTransaction(int defID, int quantity, TransactionTarget target, TransactionArg args = null)
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			QuantityItem item = new QuantityItem(defID, (uint)quantity);
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Outputs = new List<QuantityItem>();
			transactionDefinition.Outputs.Add(item);
			transactionDefinition.ID = int.MaxValue;
			RunEntireTransaction(transactionDefinition, target, null, args);
		}

		public int GetInvestmentTimeForTransaction(int transactionID)
		{
			int num = 0;
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(transactionID);
			if (transactionDefinition == null)
			{
				logger.Fatal(FatalCode.PS_NO_TRANSACTION, transactionID);
				return 0;
			}
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				foreach (QuantityItem item in inputs)
				{
					IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(item.ID);
					int quantity = (int)item.Quantity;
					num += IngredientsItemUtil.GetHarvestTimeFromIngredientDefinition(ingredientsItemDefinition, definitionService) * quantity;
					int tier = ingredientsItemDefinition.Tier;
					if (tier > 0)
					{
						num += GetInvestmentTimeForTransaction(ingredientsItemDefinition.TransactionId);
					}
				}
			}
			return num;
		}

		public KampaiPendingTransaction GetPendingTransaction(string externalIdentifier)
		{
			return player.GetPendingTransaction(externalIdentifier);
		}

		public bool PlayerAlreadyHasPlatformStoreTransactionID(string identifier)
		{
			return player.HasPlatformStoreTransactionID(identifier);
		}

		public void AddPlatformStoreTransactionID(string identifier)
		{
			player.AddPlatformStoreTransactionID(identifier);
		}

		public void QueuePendingTransaction(KampaiPendingTransaction pendingTransaction)
		{
			logger.Debug("QUEUE: {0}", pendingTransaction);
			if (player.GetPendingTransaction(pendingTransaction.ExternalIdentifier) == null)
			{
				player.QueuePendingTransaction(pendingTransaction);
			}
			else
			{
				logger.Fatal(FatalCode.PS_DUPLICATE_PENDING_TRANSACTION);
			}
		}

		public KampaiPendingTransaction ProcessPendingTransaction(string externalIdentifier, bool isFromPremium, Action<PendingCurrencyTransaction> callback = null)
		{
			logger.Debug("PROCESS: {0}", externalIdentifier);
			KampaiPendingTransaction pendingTransaction = player.GetPendingTransaction(externalIdentifier);
			if (pendingTransaction != null)
			{
				TransactionDefinition transactionDefinition = pendingTransaction.TransactionInstance.ToDefinition();
				TransactionTarget target = TransactionTarget.CURRENCY;
				if (pendingTransaction.StoreItemDefinitionId == 50002)
				{
					target = TransactionTarget.LAND_EXPANSION;
				}
				int grindOutputForTransaction = TransactionUtil.GetGrindOutputForTransaction(transactionDefinition);
				int premiumOutputForTransaction = TransactionUtil.GetPremiumOutputForTransaction(transactionDefinition);
				TransactionArg transactionArg = new TransactionArg();
				if (grindOutputForTransaction > 0 || premiumOutputForTransaction > 0)
				{
					transactionArg.Source = "STORE";
					transactionArg.IsFromPremiumSource = isFromPremium;
				}
				RunEntireTransaction(transactionDefinition, target, callback, transactionArg);
				player.RemovePendingTransaction(pendingTransaction);
				return pendingTransaction;
			}
			return null;
		}

		public KampaiPendingTransaction CancelPendingTransaction(string externalIdentifier)
		{
			KampaiPendingTransaction pendingTransaction = player.GetPendingTransaction(externalIdentifier);
			if (pendingTransaction != null)
			{
				player.RemovePendingTransaction(pendingTransaction);
			}
			return pendingTransaction;
		}

		public IList<KampaiPendingTransaction> GetPendingTransactions()
		{
			return player.GetPendingTransactions();
		}

		private void Success(Action<PendingCurrencyTransaction> callback, TransactionDefinition pendingTransaction, bool isRush, int rushCost, IList<QuantityItem> rushOutputs, IList<Instance> outputs)
		{
			if (callback != null)
			{
				PendingCurrencyTransaction pendingCurrencyTransaction = new PendingCurrencyTransaction(pendingTransaction, isRush, rushCost, rushOutputs, outputs);
				pendingCurrencyTransaction.Success = true;
				callback(pendingCurrencyTransaction);
			}
		}

		private void Fail(Action<PendingCurrencyTransaction> callback, TransactionDefinition pendingTransaction, bool isRush, int rushCost, IList<QuantityItem> rushOutputs, IList<Instance> outputs, CurrencyTransactionFailReason failReason = CurrencyTransactionFailReason.NONE)
		{
			if (callback != null)
			{
				PendingCurrencyTransaction pendingCurrencyTransaction = new PendingCurrencyTransaction(pendingTransaction, isRush, rushCost, rushOutputs, outputs);
				pendingCurrencyTransaction.FailReason = failReason;
				callback(pendingCurrencyTransaction);
			}
		}

		public IList<Building> GetBuildingsWithoutState(BuildingState excludedState)
		{
			List<Building> result = new List<Building>();
			player.GetInstancesByType(ref result, (Building building) => building.State != excludedState);
			return result;
		}

		public void RemoveTrigger(TriggerInstance triggerInstance)
		{
			logger.Info("Removing trigger instance id {0}", triggerInstance.ID);
			player.RemoveTrigger(triggerInstance.ID);
		}

		public IList<TriggerInstance> GetTriggers()
		{
			List<TriggerInstance> triggers = player.GetTriggers();
			if (triggers == null || triggers.Count == 0)
			{
				return new List<TriggerInstance>();
			}
			triggers.Sort();
			return triggers;
		}

		public TriggerInstance AddTrigger(TriggerDefinition triggerDefinition)
		{
			if (triggerDefinition == null)
			{
				logger.Error("Can't add null trigger definition");
				return null;
			}
			TriggerInstance triggerInstance = triggerDefinition.Build();
			player.Add(triggerInstance);
			return triggerInstance;
		}

		public bool HasTrigger(int triggerId)
		{
			return player.GetTriggerByDefinitionId(triggerId) != null;
		}

		public TriggerInstance GetTriggerByDefinitionId(int defId)
		{
			return player.GetTriggerByDefinitionId(defId);
		}

		public IList<Building> GetBuildingsWithState(BuildingState state)
		{
			List<Building> result = new List<Building>();
			player.GetInstancesByType(ref result, (Building building) => building.State == state);
			return result;
		}

		public void AddLandExpansion(LandExpansionConfig expansionConfig)
		{
			player.AddLandExpansion(expansionConfig);
		}

		public bool IsExpansionPurchased(int expansionId)
		{
			return player.IsExpansionPurchased(expansionId);
		}

		public int GetPurchasedExpansionCount()
		{
			return player.GetPurchasedExpansionCount();
		}

		public void QueueVillain(Prestige villainPrestige)
		{
			player.QueueVillain(villainPrestige);
		}

		public int PopVillain()
		{
			return player.PopVillain();
		}

		public void SetTargetExpansion(int id)
		{
			player.targetExpansionID = id;
		}

		public int GetTargetExpansion()
		{
			return player.targetExpansionID;
		}

		public void ClearTargetExpansion()
		{
			player.targetExpansionID = 0;
		}

		public bool HasTargetExpansion()
		{
			return player.targetExpansionID != 0;
		}

		public bool HasStorageBuilding()
		{
			StorageBuilding firstInstanceByDefintion = player.GetFirstInstanceByDefintion<StorageBuilding, StorageBuildingDefinition>();
			BuildingState state = firstInstanceByDefintion.State;
			return state != BuildingState.Broken && state != BuildingState.Inaccessible && state != BuildingState.Disabled;
		}

		public uint GetStorageCount()
		{
			uint itemCount = 0u;
			player.FindStorableItems(out itemCount);
			return itemCount;
		}

		public bool isStorageFull()
		{
			if (GetStorageCount() >= GetCurrentStorageCapacity())
			{
				return true;
			}
			return false;
		}

		public uint GetAvailableStorageCapacity()
		{
			return GetCurrentStorageCapacity() - GetStorageCount();
		}

		public uint GetCurrentStorageCapacity()
		{
			StorageBuilding firstInstanceByDefintion = player.GetFirstInstanceByDefintion<StorageBuilding, StorageBuildingDefinition>();
			if (firstInstanceByDefintion == null)
			{
				return 0u;
			}
			uint quantity = player.GetQuantity(StaticItem.STORAGE_ADDITIONAL_CAPACITY_ID);
			uint storageCapacity = firstInstanceByDefintion.Definition.StorageUpgrades[firstInstanceByDefintion.CurrentStorageBuildingLevel].StorageCapacity;
			return storageCapacity + quantity;
		}

		public List<Minion> GetMinions(bool has, params MinionState[] states)
		{
			List<Minion> list = new List<Minion>();
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < states.Length; j++)
				{
					if (instancesByType[i].State == states[j])
					{
						flag = true;
						break;
					}
				}
				if ((flag && has) || (!flag && !has))
				{
					list.Add(instancesByType[i]);
				}
			}
			return list;
		}

		public int GetMinionCountByLevel(int level)
		{
			int num = 0;
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.Level == level && !item.HasPrestige)
				{
					num++;
				}
			}
			return num;
		}

		public int GetMinionCountAtOrAboveLevel(int level)
		{
			int num = 0;
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.Level >= level && !item.HasPrestige)
				{
					num++;
				}
			}
			return num;
		}

		public List<Minion> GetMinionsByLevel(int level)
		{
			List<Minion> list = new List<Minion>();
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.Level == level && !item.HasPrestige)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public List<Minion> GetMinionsSortedByLevel()
		{
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			instancesByType.Sort((Minion x, Minion y) => y.Level.CompareTo(x.Level));
			return instancesByType;
		}

		public int GetHighestUntaskedMinionLevel()
		{
			int num = 0;
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.State == MinionState.Idle && item.Level > num)
				{
					num = item.Level;
				}
			}
			return num;
		}

		public int GetHighestMinionForLeisure(int requiredMinionCount)
		{
			List<Minion> list = new List<Minion>();
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.State == MinionState.Idle)
				{
					list.Add(item);
				}
			}
			if (list.Count < requiredMinionCount)
			{
				return 0;
			}
			list.Sort((Minion x, Minion y) => x.Level.CompareTo(y.Level));
			return list[requiredMinionCount - 1].Level;
		}

		public Minion GetUntaskedMinionWithHighestLevel()
		{
			MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(89898);
			int num = minionBenefitLevelBandDefintion.minionBenefitLevelBands.Count - 1;
			Minion result = null;
			int num2 = -1;
			List<Minion> instancesByType = GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.State == MinionState.Idle && item.Level > num2)
				{
					result = item;
					if (item.Level == num)
					{
						break;
					}
					num2 = item.Level;
				}
			}
			return result;
		}

		public void LevelupMinion(int instanceID)
		{
			player.GetByInstanceId<Minion>(instanceID).Level++;
		}

		public List<Minion> GetIdleMinions()
		{
			return GetMinions(true, MinionState.Idle, MinionState.Selectable, MinionState.Uninitialized);
		}

		public void IncreaseCompletedOrders()
		{
			player.completedOrders++;
		}

		public void IncreaseCompletedQuests()
		{
			player.completedQuestsTotal++;
		}

		public int GetHighestFtueCompleted()
		{
			return player.HighestFtueLevel;
		}

		public void SetHighestFtueCompleted(int newLevel)
		{
			int highestFtueLevel = player.HighestFtueLevel;
			if (newLevel > highestFtueLevel)
			{
				player.HighestFtueLevel = newLevel;
				ftueLevelChangedSignal.Dispatch();
			}
			else if (newLevel < highestFtueLevel)
			{
				logger.Warning("New FTUE level lower than current {0} -> {1}", highestFtueLevel, newLevel);
			}
		}

		public int GetInventoryCountByDefinitionID(int defId)
		{
			int num = 0;
			ICollection<Instance> byDefinitionId = GetByDefinitionId<Instance>(defId);
			if (byDefinitionId.Count != 0)
			{
				foreach (Instance item in byDefinitionId)
				{
					Building building = item as Building;
					if (building != null && building.State == BuildingState.Inventory)
					{
						num++;
					}
				}
			}
			return num;
		}

		public bool CheckIfBuildingIsCapped(int defID)
		{
			int unlockedQuantityOfID = GetUnlockedQuantityOfID(defID);
			if (unlockedQuantityOfID < 0)
			{
				return false;
			}
			int num = 0;
			ICollection<Building> byDefinitionId = GetByDefinitionId<Building>(defID);
			if (byDefinitionId.Count != 0)
			{
				foreach (Building item in byDefinitionId)
				{
					if (item.State != BuildingState.Inventory)
					{
						num++;
					}
				}
			}
			return num >= unlockedQuantityOfID;
		}

		public SocialClaimRewardItem.ClaimState GetSocialClaimReward(int eventID)
		{
			if (player == null)
			{
				logger.Warning("Failed to get claim reward state for event {0}", eventID);
				return SocialClaimRewardItem.ClaimState.UNKNOWN;
			}
			IDictionary<int, SocialClaimRewardItem.ClaimState> socialClaimRewards = player.GetSocialClaimRewards();
			if (socialClaimRewards.ContainsKey(eventID))
			{
				return socialClaimRewards[eventID];
			}
			return SocialClaimRewardItem.ClaimState.UNKNOWN;
		}

		public void AddSocialClaimReward(int eventID, SocialClaimRewardItem.ClaimState claimState)
		{
			if (player == null)
			{
				logger.Warning("Failed to update claim reward state for event {0}", eventID);
			}
			else
			{
				player.AddSocialClaimRewards(eventID, claimState);
			}
		}

		public void CleanupSocialClaimReward(List<int> recentEventIDs)
		{
			if (player == null)
			{
				logger.Warning("Failed to clean up claim reward state for past events");
			}
			else
			{
				player.CleanupSocialClaimReward(recentEventIDs);
			}
		}

		public void TrackMTXPurchase(string SKU)
		{
			player.AddMTXPurchaseTracking(SKU);
		}

		public IList<string> GetMTXPurchaseTracking()
		{
			return player.GetMTXPurchaseTracking();
		}

		public int MTXPurchaseCount(string sku)
		{
			return player.MTXPurchaseCount(sku);
		}

		public Player.SanityCheckFailureReason DeepScan(Player prevSave)
		{
			return player.ValidateSaveData(prevSave);
		}

		public void addPendingRemption(ReceiptValidationResult validationResult)
		{
			logger.Log(KampaiLogLevel.Debug, "Adding redemption validation result, SKU:" + validationResult.sku);
			player.addPendingRedemption(validationResult);
		}

		public ReceiptValidationResult popPendingRedemption()
		{
			return player.popPendingRedemption();
		}

		public ReceiptValidationResult topPendingRedemption()
		{
			return player.topPendingRedemption();
		}

		public void AddSocialInvitationSeen(long invitationId)
		{
			player.addSocialInvitationSeen(invitationId);
		}

		public bool SeenSocialInvitation(long invitationId)
		{
			return player.seenSocialInvitiation(invitationId);
		}

		public int getAndIncrementRequestCounter()
		{
			return player.getAndIncrementRequestCounter();
		}

		public void NotifyBuildMenuNewBuilding(int buildingID)
		{
			sendBuildingToInventorySignal.Dispatch(buildingID);
		}

		public void GrantInputs(TransactionDefinition transaction)
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.ID = int.MaxValue;
			transactionDefinition.Outputs = transaction.Inputs;
			RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
		}

		public void TrackHelpTipShown(int tipDefinitionId, int time)
		{
			List<Player.HelpTipTrackingItem> list = player.helpTipsTrackingData;
			if (list == null)
			{
				list = new List<Player.HelpTipTrackingItem>(1);
				player.helpTipsTrackingData = list;
			}
			for (int i = 0; i < list.Count; i++)
			{
				Player.HelpTipTrackingItem value = list[i];
				if (value.tipDifinitionId == tipDefinitionId)
				{
					value.showsCount++;
					value.lastShownTime = time;
					list[i] = value;
					return;
				}
			}
			list.Add(new Player.HelpTipTrackingItem(tipDefinitionId, time));
		}

		public void IngestPlayerMeta(PlayerMetaData meta)
		{
			if (meta == null)
			{
				return;
			}
			SetQuantity(StaticItem.TOTAL_USD, meta.USD);
			if (!string.IsNullOrEmpty(meta.Segments))
			{
				segments = new HashSet<string>();
				string[] array = meta.Segments.Split(',');
				foreach (string text in array)
				{
					string text2 = text.Trim();
					if (!string.IsNullOrEmpty(text2))
					{
						segments.Add(text2.ToLower());
					}
				}
			}
			else
			{
				segments = null;
			}
			string churn = meta.Churn;
			if (string.IsNullOrEmpty(churn))
			{
				return;
			}
			float result = 0f;
			if (float.TryParse(churn, out result))
			{
				if (result < 0f)
				{
					result = 0f;
				}
				if (result > 1f)
				{
					result = 1f;
				}
				float num = Churn();
				float num2 = result - num + 1f;
				int amount = (int)(result * 10000f);
				int amount2 = (int)(num2 * 10000f);
				SetQuantity(StaticItem.CHURN_ID, amount);
				SetQuantity(StaticItem.CHURN_DELTA_ID, amount2);
			}
			else
			{
				logger.Error("Invalid churn {0}", churn);
			}
		}

		public bool IsInSegment(string segmentName)
		{
			if (segments != null && !string.IsNullOrEmpty(segmentName))
			{
				return segments.Contains(segmentName.ToLower());
			}
			return false;
		}

		public float Churn()
		{
			uint quantity = GetQuantity(StaticItem.CHURN_ID);
			return (float)quantity / 10000f;
		}
	}
}
