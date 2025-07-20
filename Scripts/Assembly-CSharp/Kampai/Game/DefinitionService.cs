using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class DefinitionService : IDefinitionService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DefinitionService") as IKampaiLogger;

		protected Dictionary<int, Definition> AllDefinitions;

		protected IList<string> environmentDefinition;

		protected Dictionary<int, WeightedDefinition> gachaDefinitionsByNumMinions;

		protected Dictionary<int, WeightedDefinition> partyDefinitionsByNumMinions;

		protected Dictionary<int, TriggerRewardDefinition> triggerRewardsDefinitions;

		protected Dictionary<int, int> levelUnlockLookUpTable;

		protected TaskDefinition taskDefinition;

		protected IList<RushTimeBandDefinition> rushDefinitions;

		protected IList<AchievementDefinition> achievementDefinitions;

		protected IList<LandExpansionConfig> expansionConfigs;

		protected IList<TriggerDefinition> triggerDefinitions;

		protected IList<CurrencyStoreCategoryDefinition> currencyStoreCategoryDefinitions;

		protected Dictionary<int, TransactionDefinition> packTransactionMap;

		protected TargetPerformance targetPerformance = TargetPerformance.HIGH;

		protected Dictionary<int, int> itemTransactionTable;

		protected string initialPlayer;

		protected bool validateDefinitions = true;

		protected string binaryDefinitionsPath;

		private Dictionary<Type, IList> definitionsTypeMap;

		public DefinitionService()
		{
			AllDefinitions = new Dictionary<int, Definition>();
			binaryDefinitionsPath = GetBinaryDefinitionsPath();
		}

		public void DeserializeJson(TextReader textReader, bool validateDefinitions = true)
		{
			if (textReader != null)
			{
				Definitions definitions = null;
				try
				{
					using (JsonTextReader reader = new JsonTextReader(textReader))
					{
						definitions = FastJSONDeserializer.Deserialize<Definitions>(reader, GetJsonDefinitionsFastConverters());
					}
				}
				catch (JsonSerializationException ex)
				{
					logger.Error(ex.StackTrace);
					throw new FatalException(FatalCode.DS_PARSE_ERROR, ex, "Def json error: {0}", ex);
				}
				catch (JsonReaderException ex2)
				{
					logger.Error(ex2.StackTrace);
					throw new FatalException(FatalCode.DS_PARSE_ERROR, ex2, "Def json error: {0}", ex2);
				}
				LoadDefinitions(definitions, validateDefinitions, "DefinitionService.DeserializeJson()");
				if (validateDefinitions)
				{
					SerializeBinary(definitions);
				}
				return;
			}
			throw new FatalException(FatalCode.DS_EMPTY_JSON, "DefinitionService.Deserialize(): empty json");
		}

		public JsonConverters GetJsonDefinitionsFastConverters()
		{
			JsonConverters jsonConverters = new JsonConverters();
			jsonConverters.buildingDefinitionConverter = new BuildingDefinitionFastConverter();
			jsonConverters.questDefinitionConverter = new QuestDefinitionFastConverter();
			jsonConverters.itemDefinitionConverter = new ItemDefinitionFastConverter();
			jsonConverters.currencyItemDefinitionConverter = new CurrencyItemFastConverter();
			jsonConverters.playerVersionConverter = new PlayerDefinitionFastConverter(this);
			jsonConverters.plotDefinitionConverter = new PlotDefinitionFastConverter(logger);
			jsonConverters.namedCharacterDefinitionConverter = new NamedCharacterDefinitionFastConverter();
			jsonConverters.salePackDefinitionConverter = new SalePackConverter();
			jsonConverters.currencyStorePackDefinitionConverter = new CurrencyStorePackConverter();
			jsonConverters.triggerDefinitionConverter = new TriggerDefinitionFastConverter(logger);
			jsonConverters.triggerConditionDefinitionConverter = new TriggerConditionDefinitionFastConverter(logger);
			jsonConverters.triggerRewardDefinitionConverter = new TriggerRewardDefinitionFastConverter(logger);
			jsonConverters.adPlacementDefinitionConverter = new AdPlacementDefinitionFastConverter();
			return jsonConverters;
		}

		public JsonConverter[] GetJsonDefinitionsConverters()
		{
			return new JsonConverter[11]
			{
				new BuildingDefinitionConverter(),
				new QuestDefinitionConverter(),
				new ItemDefinitionConverter(),
				new CurrencyItemConverter(),
				new PlayerDefinitionConverter(this),
				new PlotDefinitionConverter(logger),
				new NamedCharacterDefinitionConverter(),
				new TriggerDefinitionConverter(logger),
				new TriggerConditionDefinitionConverter(logger),
				new TriggerRewardDefinitionConverter(logger),
				new AdPlacementDefinitionConverter()
			};
		}

		public static string GetBinaryDefinitionsPath()
		{
			return Path.Combine(GameConstants.PERSISTENT_DATA_PATH, "definitions.dat");
		}

		public static void DeleteBinarySerialization()
		{
			string path = GetBinaryDefinitionsPath();
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private void SerializeBinary(Definitions definitions)
		{
			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(binaryDefinitionsPath)))
				{
					BinarySerializationUtil.WriteString(writer, initialPlayer);
					BinarySerializationUtil.WriteObject(writer, definitions);
				}
			}
			catch (Exception ex)
			{
				logger.Error("DL: DefinitionService.SerializeBinary: unable to serizalize definitions to binary file {0}. Reason: {1}", binaryDefinitionsPath, ex);
				DeleteBinarySerialization();
			}
		}

		public void DeserializeBinary(BinaryReader binaryReader, bool validateDefinitions = false)
		{
			Definitions definitions = null;
			initialPlayer = BinarySerializationUtil.ReadString(binaryReader);
			definitions = BinarySerializationUtil.ReadObject<Definitions>(binaryReader);
			LoadDefinitions(definitions, validateDefinitions, "DeserializeBinary");
		}

		private void LoadDefinitions(Definitions definitions, bool validateDefinitions, string callerTag)
		{
			if (definitions == null)
			{
				throw new FatalException(FatalCode.DS_NULL_ERROR, "LoadDefinitions(): definitions are null, caller: {0}", callerTag);
			}
			this.validateDefinitions = validateDefinitions;
			AllDefinitions = new Dictionary<int, Definition>(2500);
			definitionsTypeMap = new Dictionary<Type, IList>();
			MarkDefinitions(definitions);
			MarkMoreDefinitions(definitions);
			MarkMarketplaceDefinitions(definitions);
			MarkMinionPartyDefinitions(definitions);
			MarkNamedDefinitions(definitions.namedCharacterDefinitions);
			MarkSalePackTransactionDefinitions(definitions.salePackDefinitions);
			MarkRewardedAdvertisementDefinition(definitions.rewardedAdvertisementDefinition);
			MarkCurrencyStorePackDefinitions(definitions.currencyStorePackDefinitions);
			MarkMasterPlanDefinitions(definitions);
			MarkXPromoDefinitions(definitions);
			MarkHindsightDefinitions(definitions);
			AssembleGachaAnimationDefinitions();
			AddLevelUpDefinition(definitions);
			AddMinionBenefitDefinition(definitions);
			AddNotificationSystemDefinition(definitions);
			AddLevelXPTable(definitions);
			AddLevelFunTable(definitions);
			AddDropLevelBandDefinition(definitions);
			taskDefinition = GetTaskDefintion(definitions);
			rushDefinitions = GetRushDefinitions(definitions);
			achievementDefinitions = GetAchievementDefinitions(definitions);
			triggerRewardsDefinitions = GetTriggerRewardDefinitions(definitions);
			triggerDefinitions = GetTriggerDefinitions(definitions);
			currencyStoreCategoryDefinitions = GetCurrencyStoreCategoryDefinitions(definitions);
		}

		public void DeserializeEnvironmentDefinition(TextReader textReader)
		{
			Definitions definitions = null;
			using (JsonTextReader reader = new JsonTextReader(textReader))
			{
				definitions = FastJSONDeserializer.Deserialize<Definitions>(reader);
			}
			environmentDefinition = definitions.environmentDefinition;
		}

		private void MarkDefinitions(Definitions definitions)
		{
			MarkDefinitionsAsUsed(definitions.weightedDefinitions);
			MarkDefinitionsAsUsed(definitions.transactions);
			MarkDefinitionsAsUsed(definitions.buildingDefinitions);
			MarkDefinitionsAsUsed(definitions.plotDefinitions);
			MarkDefinitionsAsUsed(definitions.itemDefinitions);
			MarkDefinitionsAsUsed(definitions.minionDefinitions);
			MarkDefinitionsAsUsed(definitions.currencyItemDefinitions);
			MarkDefinitionsAsUsed(definitions.storeItemDefinitions);
			MarkDefinitionsAsUsed(definitions.purchasedExpansionDefinitions);
			MarkDefinitionsAsUsed(definitions.commonExpansionDefinitions);
			MarkDefinitionsAsUsed(definitions.expansionDefinitions);
			MarkDefinitionsAsUsed(definitions.debrisDefinitions);
			MarkDefinitionsAsUsed(definitions.aspirationalBuildingDefinitions);
			MarkDefinitionsAsUsed(definitions.footprintDefinitions);
			MarkDefinitionsAsUsed(definitions.playerTrainingDefinitions);
			MarkDefinitionsAsUsed(definitions.playerTrainingCardDefinitions);
			MarkDefinitionsAsUsed(definitions.playerTrainingCategoryDefinitions);
			MarkDefinitionsAsUsed(definitions.buffDefinitions);
			MarkDefinitionsAsUsed(definitions.guestOfHonorDefinitions);
			MarkDefinitionsAsUsed(definitions.customCameraPositionDefinitions);
			MarkDefinitionsAsUsed(definitions.helpTipDefinitions);
			MarkDefinitionsAsUsed(definitions.populationBenefitDefinitions);
			MarkDefinitionsAsUsed(definitions.villainLairDefinitions);
		}

		private void MarkMoreDefinitions(Definitions definitions)
		{
			MarkDefinitionsAsUsed(definitions.gachaConfig.GatchaAnimationDefinitions);
			MarkDefinitionsAsUsed(definitions.gachaConfig.DistributionTables);
			MarkDefinitionsAsUsed(definitions.expansionConfigs);
			MarkDefinitionsAsUsed(definitions.MinionAnimationDefinitions);
			MarkDefinitionsAsUsed(definitions.quests);
			MarkDefinitionsAsUsed(definitions.questResources);
			MarkDefinitionsAsUsed(definitions.notificationDefinitions);
			MarkDefinitionsAsUsed(definitions.collectionDefinitions);
			MarkDefinitionsAsUsed(definitions.prestigeDefinitions);
			MarkDefinitionsAsUsed(definitions.definitionGroups);
			MarkDefinitionsAsUsed(definitions.timedSocialEventDefinitions);
			MarkDefinitionsAsUsed(definitions.compositeBuildingPieceDefinitions);
			MarkDefinitionsAsUsed(definitions.questChains);
			MarkDefinitionsAsUsed(definitions.stickerDefinitions);
			MarkDefinitionsAsUsed(definitions.achievementDefinitions);
			MarkDefinitionAsUsed(definitions.wayFinderDefinition);
			MarkDefinitionsAsUsed(definitions.flyOverDefinitions);
			MarkDefinitionsAsUsed(definitions.loadInTipBucketDefinitions);
			MarkDefinitionsAsUsed(definitions.loadInTipDefinitions);
			MarkDefinitionAsUsed(definitions.cameraSettingsDefinition);
			MarkDefinitionAsUsed(definitions.socialSettingsDefinition);
			MarkDefinitionsAsUsed(definitions.uiAnimationDefinitions);
			MarkDefinitionsAsUsed(definitions.pendingRewardDefinitions);
			MarkDefinitionsAsUsed(definitions.legalDocumentDefinitions);
		}

		private void MarkMarketplaceDefinitions(Definitions definitions)
		{
			MarkDefinitionAsUsed(definitions.marketplaceDefinition);
			MarkDefinitionsAsUsed(definitions.marketplaceDefinition.itemDefinitions);
			MarkDefinitionsAsUsed(definitions.marketplaceDefinition.slotDefinitions);
			MarkDefinitionAsUsed(definitions.marketplaceDefinition.refreshTimerDefinition);
		}

		private void MarkMinionPartyDefinitions(Definitions definitions)
		{
			MarkDefinitionAsUsed(definitions.minionPartyDefinition);
			MarkDefinitionsAsUsed(definitions.partyFavorAnimationDefinitions);
		}

		private void MarkMasterPlanDefinitions(Definitions definitions)
		{
			MarkDefinitionsAsUsed(definitions.masterPlanDefinitions);
			MarkDefinitionsAsUsed(definitions.masterPlanComponentDefinitions);
			MarkDefinitionsAsUsed(definitions.onboardDefinitions);
			MarkDefinitionAsUsed(definitions.dynamicMasterPlanDefinition);
		}

		private void MarkXPromoDefinitions(Definitions definitions)
		{
			MarkDefinitionAsUsed(definitions.petsXPromoDefinition);
		}

		private void MarkHindsightDefinitions(Definitions definitions)
		{
			MarkDefinitionsAsUsed(definitions.hindsightCampaignDefinitions);
		}

		private void MarkNamedDefinitions(IList<NamedCharacterDefinition> namedCharacterDefinitions)
		{
			if (namedCharacterDefinitions == null)
			{
				return;
			}
			MarkDefinitionsAsUsed(namedCharacterDefinitions);
			foreach (NamedCharacterDefinition namedCharacterDefinition in namedCharacterDefinitions)
			{
				FrolicCharacterDefinition frolicCharacterDefinition = namedCharacterDefinition as FrolicCharacterDefinition;
				if (frolicCharacterDefinition != null && frolicCharacterDefinition.WanderWeightedDeck != null)
				{
					MarkDefinitionAsUsed(frolicCharacterDefinition.WanderWeightedDeck);
				}
			}
		}

		private void MarkSalePackTransactionDefinitions(IList<SalePackDefinition> salePackDefinitions)
		{
			if (salePackDefinitions == null)
			{
				return;
			}
			packTransactionMap = new Dictionary<int, TransactionDefinition>();
			MarkDefinitionsAsUsed(salePackDefinitions);
			for (int i = 0; i < salePackDefinitions.Count; i++)
			{
				SalePackDefinition salePackDefinition = salePackDefinitions[i];
				if (salePackDefinition.TransactionDefinition != null && salePackDefinition.TransactionDefinition.ID != 0)
				{
					TransactionDefinition transactionDefinition = salePackDefinition.TransactionDefinition.ToDefinition();
					packTransactionMap.Add(salePackDefinition.TransactionDefinition.ID, transactionDefinition);
					MarkDefinitionAsUsed(transactionDefinition);
				}
			}
		}

		private void MarkRewardedAdvertisementDefinition(RewardedAdvertisementDefinition rewardedAdvertisementDefinition)
		{
			if (rewardedAdvertisementDefinition == null)
			{
				return;
			}
			MarkDefinitionAsUsed(rewardedAdvertisementDefinition);
			if (rewardedAdvertisementDefinition.PlacementDefinitions != null)
			{
				List<AdPlacementDefinition> placementDefinitions = rewardedAdvertisementDefinition.PlacementDefinitions;
				for (int i = 0; i < placementDefinitions.Count; i++)
				{
					AdPlacementDefinition d = placementDefinitions[i];
					MarkDefinitionAsUsed(d);
				}
			}
		}

		private void MarkCurrencyStorePackDefinitions(IList<CurrencyStorePackDefinition> currencyStorePackDefinitions)
		{
			if (currencyStorePackDefinitions == null)
			{
				return;
			}
			packTransactionMap = new Dictionary<int, TransactionDefinition>();
			MarkDefinitionsAsUsed(currencyStorePackDefinitions);
			for (int i = 0; i < currencyStorePackDefinitions.Count; i++)
			{
				CurrencyStorePackDefinition currencyStorePackDefinition = currencyStorePackDefinitions[i];
				if (currencyStorePackDefinition.TransactionDefinition != null && currencyStorePackDefinition.TransactionDefinition.ID != 0)
				{
					TransactionDefinition transactionDefinition = currencyStorePackDefinition.TransactionDefinition.ToDefinition();
					packTransactionMap.Add(currencyStorePackDefinition.TransactionDefinition.ID, transactionDefinition);
					MarkDefinitionAsUsed(transactionDefinition);
				}
			}
		}

		public bool Has(int id)
		{
			return AllDefinitions.ContainsKey(id);
		}

		public bool Has<T>(int id) where T : Definition
		{
			Definition definition = null;
			if (Has(id))
			{
				definition = AllDefinitions[id] as T;
			}
			return definition != null;
		}

		public Definition Get(int id)
		{
			if (Has(id))
			{
				return AllDefinitions[id];
			}
			logger.Fatal(FatalCode.DS_NO_ITEM_DEF, id);
			return null;
		}

		public T Get<T>(int id) where T : Definition
		{
			T val = (T)null;
			if (Has(id))
			{
				val = AllDefinitions[id] as T;
			}
			if (val == null)
			{
				logger.Fatal(FatalCode.DS_NO_ITEM_TYPE_DEF, id);
			}
			return val;
		}

		public T Get<T>(StaticItem staticItem) where T : Definition
		{
			return Get<T>((int)staticItem);
		}

		public bool TryGet<T>(int id, out T definition) where T : Definition
		{
			definition = (T)null;
			if (Has(id))
			{
				definition = AllDefinitions[id] as T;
			}
			return definition != null;
		}

		public List<T> GetAll<T>() where T : Definition
		{
			IList value;
			if (definitionsTypeMap.TryGetValue(typeof(T), out value))
			{
				return value as List<T>;
			}
			List<T> list = new List<T>();
			foreach (Definition value2 in AllDefinitions.Values)
			{
				T val = value2 as T;
				if (val != null)
				{
					list.Add(val);
				}
			}
			definitionsTypeMap.Add(typeof(T), list);
			return list;
		}

		public T Get<T>() where T : Definition
		{
			foreach (Definition value in AllDefinitions.Values)
			{
				T val = value as T;
				if (val != null)
				{
					return val;
				}
			}
			return (T)null;
		}

		public Dictionary<int, Definition> GetAllDefinitions()
		{
			return AllDefinitions;
		}

		public IList<string> GetEnvironemtDefinition()
		{
			return environmentDefinition;
		}

		public void ReclaimEnfironmentDefinitions()
		{
			environmentDefinition = null;
		}

		public WeightedDefinition GetGachaWeightsForNumMinions(int numMinions, bool party)
		{
			Dictionary<int, WeightedDefinition> dictionary = ((!party) ? gachaDefinitionsByNumMinions : partyDefinitionsByNumMinions);
			if (dictionary.ContainsKey(numMinions))
			{
				return dictionary[numMinions];
			}
			return dictionary[4];
		}

		public List<WeightedDefinition> GetAllGachaDefinitions()
		{
			List<WeightedDefinition> list = new List<WeightedDefinition>();
			list.AddRange(gachaDefinitionsByNumMinions.Values);
			list.AddRange(partyDefinitionsByNumMinions.Values);
			return list;
		}

		public int GetHarvestItemDefinitionIdFromTransactionId(int transactionId)
		{
			TransactionDefinition transactionDefinition = Get<TransactionDefinition>(transactionId);
			IList<QuantityItem> outputs = transactionDefinition.Outputs;
			if (outputs[0] != null)
			{
				return outputs[0].ID;
			}
			logger.Fatal(FatalCode.PS_NO_TRANSACTION, transactionId);
			return -1;
		}

		public string GetHarvestIconFromTransactionID(int transactionId)
		{
			TransactionDefinition transactionDefinition = Get<TransactionDefinition>(transactionId);
			IList<QuantityItem> outputs = transactionDefinition.Outputs;
			if (outputs[0] != null)
			{
				ItemDefinition itemDefinition = Get<ItemDefinition>(outputs[0].ID);
				return itemDefinition.Image;
			}
			logger.Fatal(FatalCode.PS_NO_TRANSACTION, transactionId);
			return string.Empty;
		}

		public int ExtractQuantityFromTransaction(int transactionID, int definitionID)
		{
			int result = 0;
			TransactionDefinition transactionDefinition = Get<TransactionDefinition>(transactionID);
			if (transactionDefinition.Outputs != null)
			{
				result = TransactionUtil.ExtractQuantityFromTransaction(transactionDefinition, definitionID);
			}
			return result;
		}

		public int GetBuildingDefintionIDFromItemDefintionID(int itemDefinitionID)
		{
			IList<VillainLairDefinition> all = GetAll<VillainLairDefinition>();
			foreach (VillainLairDefinition item in all)
			{
				if (item != null && item.ResourceItemID == itemDefinitionID)
				{
					return item.ResourceBuildingDefID;
				}
			}
			IList<AnimatingBuildingDefinition> all2 = GetAll<AnimatingBuildingDefinition>();
			foreach (AnimatingBuildingDefinition item2 in all2)
			{
				ResourceBuildingDefinition resourceBuildingDefinition = item2 as ResourceBuildingDefinition;
				if (resourceBuildingDefinition != null && resourceBuildingDefinition.ItemId == itemDefinitionID)
				{
					return resourceBuildingDefinition.ID;
				}
				CraftingBuildingDefinition craftingBuildingDefinition = item2 as CraftingBuildingDefinition;
				if (craftingBuildingDefinition == null)
				{
					continue;
				}
				foreach (RecipeDefinition recipeDefinition in craftingBuildingDefinition.RecipeDefinitions)
				{
					if (recipeDefinition.ItemID == itemDefinitionID)
					{
						return craftingBuildingDefinition.ID;
					}
				}
			}
			return 0;
		}

		public string GetBuildingFootprint(int ID)
		{
			IList<FootprintDefinition> all = GetAll<FootprintDefinition>();
			foreach (FootprintDefinition item in all)
			{
				if (item.ID == ID)
				{
					return item.Footprint;
				}
			}
			return string.Empty;
		}

		public int GetIncrementalCost(Definition definition)
		{
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			return (buildingDefinition != null) ? buildingDefinition.IncrementalCost : 0;
		}

		public BridgeDefinition GetBridgeDefinition(int itemDefinitionID)
		{
			IList<BridgeDefinition> all = GetAll<BridgeDefinition>();
			foreach (BridgeDefinition item in all)
			{
				if (item.ID == itemDefinitionID)
				{
					return item;
				}
			}
			return null;
		}

		public bool HasUnlockItemInTransactionOutput(int transactionID)
		{
			TransactionDefinition transactionDefinition = Get<TransactionDefinition>(transactionID);
			if (transactionDefinition.Outputs != null)
			{
				foreach (QuantityItem output in transactionDefinition.Outputs)
				{
					Definition definition = Get<Definition>(output.ID);
					if (definition is UnlockDefinition)
					{
						return true;
					}
				}
			}
			return false;
		}

		public int GetLevelItemUnlocksAt(int definitionID)
		{
			if (levelUnlockLookUpTable == null)
			{
				levelUnlockLookUpTable = new Dictionary<int, int>();
				LevelUpDefinition levelUpDefinition = Get<LevelUpDefinition>(88888);
				for (int i = 0; i < levelUpDefinition.transactionList.Count; i++)
				{
					if (levelUpDefinition.transactionList[i] == 0)
					{
						continue;
					}
					TransactionDefinition transactionDefinition = Get<TransactionDefinition>(levelUpDefinition.transactionList[i]);
					if (transactionDefinition == null || transactionDefinition.Outputs == null)
					{
						continue;
					}
					foreach (QuantityItem output in transactionDefinition.Outputs)
					{
						UnlockDefinition definition = null;
						if (TryGet<UnlockDefinition>(output.ID, out definition) && !levelUnlockLookUpTable.ContainsKey(definition.ReferencedDefinitionID))
						{
							levelUnlockLookUpTable.Add(definition.ReferencedDefinitionID, i);
						}
					}
				}
			}
			if (!levelUnlockLookUpTable.ContainsKey(definitionID))
			{
				logger.Fatal(FatalCode.TE_MISSING_UNLOCK, definitionID);
				return 0;
			}
			return levelUnlockLookUpTable[definitionID];
		}

		public TaskLevelBandDefinition GetTaskLevelBandForLevel(int level)
		{
			if (taskDefinition != null)
			{
				IList<TaskLevelBandDefinition> levelBands = taskDefinition.levelBands;
				if (levelBands != null)
				{
					TaskLevelBandDefinition result = levelBands[0];
					{
						foreach (TaskLevelBandDefinition item in levelBands)
						{
							if (item.MinLevel <= level)
							{
								result = item;
							}
						}
						return result;
					}
				}
			}
			return null;
		}

		public RushTimeBandDefinition GetRushTimeBandForTime(int timeRemainingInSeconds)
		{
			if (rushDefinitions != null)
			{
				IList<RushTimeBandDefinition> list = rushDefinitions;
				if (list != null)
				{
					foreach (RushTimeBandDefinition item in list)
					{
						if (timeRemainingInSeconds <= item.TimeRemainingInSeconds)
						{
							return item;
						}
					}
					return list[list.Count - 1];
				}
			}
			return null;
		}

		public AchievementDefinition GetAchievementDefinitionFromDefinitionID(int defID)
		{
			if (achievementDefinitions != null)
			{
				IList<AchievementDefinition> list = achievementDefinitions;
				if (list != null)
				{
					foreach (AchievementDefinition item in list)
					{
						if (item.DefinitionID == defID)
						{
							return item;
						}
					}
				}
			}
			return null;
		}

		public int getItemTransactionID(int id)
		{
			if (itemTransactionTable == null)
			{
				itemTransactionTable = new Dictionary<int, int>();
				IList<StoreItemDefinition> all = GetAll<StoreItemDefinition>();
				foreach (StoreItemDefinition item in all)
				{
					itemTransactionTable[item.ReferencedDefID] = item.TransactionID;
				}
			}
			if (!itemTransactionTable.ContainsKey(id))
			{
				logger.Fatal(FatalCode.PS_NO_TRANSACTION, id);
				return 0;
			}
			return itemTransactionTable[id];
		}

		public TransactionDefinition GetPackTransaction(int transactionId)
		{
			if (packTransactionMap == null || !packTransactionMap.ContainsKey(transactionId))
			{
				return null;
			}
			return packTransactionMap[transactionId];
		}

		public PackDefinition GetPackDefinitionFromTransactionId(int transactionId)
		{
			IList<PackDefinition> all = GetAll<PackDefinition>();
			if (all == null)
			{
				return null;
			}
			for (int i = 0; i < all.Count; i++)
			{
				PackDefinition packDefinition = all[i];
				if (packDefinition.TransactionDefinition.ID == transactionId)
				{
					return packDefinition;
				}
			}
			return null;
		}

		private Dictionary<int, TriggerRewardDefinition> GetTriggerRewardDefinitions(Definitions definitions)
		{
			Dictionary<int, TriggerRewardDefinition> dictionary = new Dictionary<int, TriggerRewardDefinition>();
			IList<TriggerRewardDefinition> triggerRewardDefinitions = definitions.triggerRewardDefinitions;
			MarkDefinitionsAsUsed(triggerRewardDefinitions);
			if (triggerRewardDefinitions == null)
			{
				return dictionary;
			}
			for (int i = 0; i < definitions.triggerRewardDefinitions.Count; i++)
			{
				if (!dictionary.ContainsKey(triggerRewardDefinitions[i].ID))
				{
					dictionary.Add(triggerRewardDefinitions[i].ID, triggerRewardDefinitions[i]);
				}
			}
			return dictionary;
		}

		private IList<TriggerDefinition> GetTriggerDefinitions(Definitions definitions)
		{
			if (definitions.triggerDefinitions == null)
			{
				return null;
			}
			MarkDefinitionsAsUsed(definitions.triggerDefinitions);
			List<TriggerDefinition> list = new List<TriggerDefinition>(definitions.triggerDefinitions);
			TriggerDefinition triggerDefinition = null;
			for (int i = 0; i < list.Count; i++)
			{
				triggerDefinition = list[i];
				if (triggerDefinition.reward == null || triggerDefinition.reward.Count == 0)
				{
					logger.Error("Trigger Definition doesn't contain any reward ids {0}", triggerDefinition);
					continue;
				}
				for (int j = 0; j < triggerDefinition.reward.Count; j++)
				{
					int key = triggerDefinition.reward[j];
					if (triggerRewardsDefinitions.ContainsKey(key))
					{
						TriggerRewardDefinition item = triggerRewardsDefinitions[key];
						triggerDefinition.rewards.Add(item);
					}
				}
			}
			list.Sort();
			return list;
		}

		private IList<CurrencyStoreCategoryDefinition> GetCurrencyStoreCategoryDefinitions(Definitions definitions)
		{
			if (definitions.currencyStoreDefinition == null)
			{
				return null;
			}
			MarkDefinitionAsUsed(definitions.currencyStoreDefinition);
			MarkDefinitionsAsUsed(definitions.currencyStoreDefinition.CategoryDefinitions);
			List<CurrencyStoreCategoryDefinition> list = new List<CurrencyStoreCategoryDefinition>(definitions.currencyStoreDefinition.CategoryDefinitions);
			list.Sort();
			return list;
		}

		private void AssembleGachaAnimationDefinitions()
		{
			gachaDefinitionsByNumMinions = new Dictionary<int, WeightedDefinition>();
			partyDefinitionsByNumMinions = new Dictionary<int, WeightedDefinition>();
			foreach (GachaWeightedDefinition item in GetAll<GachaWeightedDefinition>())
			{
				WeightedDefinition weightedDefinition = item.WeightedDefinition;
				MarkDefinitionAsUsed(weightedDefinition);
				int minions = item.Minions;
				bool partyOnly = item.PartyOnly;
				if ((!partyOnly && gachaDefinitionsByNumMinions.ContainsKey(minions)) || (partyOnly && partyDefinitionsByNumMinions.ContainsKey(minions)))
				{
					throw new FatalException(FatalCode.DS_DUPLICATE_GACHA, minions);
				}
				IList<WeightedQuantityItem> entities = weightedDefinition.Entities;
				foreach (WeightedQuantityItem item2 in entities)
				{
					int iD = item2.ID;
					if (iD > 0)
					{
						GachaAnimationDefinition gachaAnimationDefinition = Get<GachaAnimationDefinition>(iD);
						if (gachaAnimationDefinition == null)
						{
							throw new FatalException(FatalCode.DS_RELATION_DOES_NOT_EXIST, iD);
						}
						if (gachaAnimationDefinition.Minions > 0 && gachaAnimationDefinition.Minions != minions)
						{
							throw new FatalException(FatalCode.DS_NUM_MINION_GACHA_MISMATCH, item2.ID);
						}
						if (gachaAnimationDefinition.MinPerformance > targetPerformance)
						{
							item2.Weight = 0u;
						}
					}
				}
				if (partyOnly)
				{
					partyDefinitionsByNumMinions.Add(minions, weightedDefinition);
				}
				else
				{
					gachaDefinitionsByNumMinions.Add(minions, weightedDefinition);
				}
			}
		}

		private void AddMinionBenefitDefinition(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(89898))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_ID, 89898);
			}
			AllDefinitions[89898] = definitions.minionBenefitDefinition;
		}

		private void AddLevelUpDefinition(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(88888))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_LEVELUP, 88888);
			}
			AllDefinitions[88888] = definitions.levelUpDefinition;
		}

		private void AddNotificationSystemDefinition(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(66666))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_LEVELUP, 66666);
			}
			AllDefinitions[66666] = definitions.notificationSystemDefinition;
		}

		private void AddDropLevelBandDefinition(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(88889))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_RANDOM_BANDS, 88889);
			}
			AllDefinitions[88889] = definitions.randomDropLevelBandDefinition;
		}

		private void AddLevelXPTable(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(99999))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_XP, 99999);
			}
			AllDefinitions[99999] = definitions.levelXPTable;
		}

		private void AddLevelFunTable(Definitions definitions)
		{
			if (AllDefinitions.ContainsKey(1000009681))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_XP, 1000009681);
			}
			AllDefinitions[1000009681] = definitions.levelFunTable;
		}

		private void MarkDefinitionAsUsed(Definition d)
		{
			int iD = d.ID;
			if (validateDefinitions && AllDefinitions.ContainsKey(iD))
			{
				throw new FatalException(FatalCode.DS_DUPLICATE_ID, iD, "DefinitionService.MarkDefinitionAsUsed(): defId = {0} {1}", iD, d.ToString());
			}
			AllDefinitions[d.ID] = d;
		}

		private IEnumerable<T> MarkDefinitionsAsUsed<T>(IEnumerable<T> used) where T : Definition
		{
			if (used != null)
			{
				foreach (T item in used)
				{
					if (!item.Disabled)
					{
						MarkDefinitionAsUsed(item);
					}
				}
			}
			return used;
		}

		private TaskDefinition GetTaskDefintion(Definitions defs)
		{
			TaskDefinition tasks = defs.tasks;
			if (tasks != null)
			{
				List<TaskLevelBandDefinition> list = new List<TaskLevelBandDefinition>(tasks.levelBands);
				list.Sort((TaskLevelBandDefinition p1, TaskLevelBandDefinition p2) => p1.MinLevel - p2.MinLevel);
				tasks.levelBands = list;
			}
			return tasks;
		}

		private IList<RushTimeBandDefinition> GetRushDefinitions(Definitions defs)
		{
			IList<RushTimeBandDefinition> list = defs.rushDefinitions;
			if (list != null)
			{
				List<RushTimeBandDefinition> list2 = new List<RushTimeBandDefinition>(list);
				RushUtil.SortByTime(list2);
				list = list2;
			}
			return list;
		}

		private IList<AchievementDefinition> GetAchievementDefinitions(Definitions defs)
		{
			IList<AchievementDefinition> list = defs.achievementDefinitions;
			if (list != null)
			{
				return list;
			}
			return null;
		}

		public SalePackType getSKUSalePackType(string ExternalIdentifier)
		{
			foreach (SalePackDefinition item in GetAll<SalePackDefinition>())
			{
				if (ItemUtil.CompareSKU(item.SKU, ExternalIdentifier))
				{
					return item.Type;
				}
			}
			return SalePackType.StarterPack;
		}

		public int GetPartyFavorDefinitionIDByItemID(int itemID)
		{
			foreach (PartyFavorAnimationDefinition item in GetAll<PartyFavorAnimationDefinition>())
			{
				if (item.ItemID == itemID)
				{
					return item.ID;
				}
			}
			return -1;
		}

		public string GetLegalURL(LegalDocuments.LegalType type, string language)
		{
			string result = string.Empty;
			foreach (LegalDocumentDefinition item in GetAll<LegalDocumentDefinition>())
			{
				if (item.type != type)
				{
					continue;
				}
				foreach (LegalDocumentURL url in item.urls)
				{
					if (url.language == "en")
					{
						result = url.url;
					}
					if (url.language == language)
					{
						return url.url;
					}
				}
			}
			return result;
		}

		public void SetInitialPlayer(string playerJson)
		{
			initialPlayer = playerJson;
		}

		public string GetInitialPlayer()
		{
			return initialPlayer;
		}

		public void Add(Definition definition)
		{
			AllDefinitions.Add(definition.ID, definition);
			definitionsTypeMap.Clear();
		}

		public void Remove(Definition definition)
		{
			AllDefinitions.Remove(definition.ID);
			definitionsTypeMap.Clear();
		}

		public void SetPerformanceQualityLevel(TargetPerformance targetPerformance)
		{
			this.targetPerformance = targetPerformance;
		}

		public IList<TriggerDefinition> GetTriggerDefinitions()
		{
			return triggerDefinitions;
		}

		public IList<CurrencyStoreCategoryDefinition> GetCurrencyStoreCategoryDefinitions()
		{
			return currencyStoreCategoryDefinitions;
		}
	}
}
