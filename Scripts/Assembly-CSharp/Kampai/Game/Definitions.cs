using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Definitions : IBinarySerializable, IFastJSONDeserializable
	{
		public IList<CompositeBuildingPieceDefinition> compositeBuildingPieceDefinitions;

		public virtual int TypeCode
		{
			get
			{
				return 1190;
			}
		}

		public IList<DefinitionGroup> definitionGroups { get; set; }

		public IList<TransactionDefinition> transactions { get; set; }

		public IList<WeightedDefinition> weightedDefinitions { get; set; }

		public IList<BuildingDefinition> buildingDefinitions { get; set; }

		public IList<PlotDefinition> plotDefinitions { get; set; }

		public IList<ItemDefinition> itemDefinitions { get; set; }

		public IList<MinionDefinition> minionDefinitions { get; set; }

		public IList<StoreItemDefinition> storeItemDefinitions { get; set; }

		public IList<CurrencyItemDefinition> currencyItemDefinitions { get; set; }

		public MarketplaceDefinition marketplaceDefinition { get; set; }

		public MinionPartyDefinition minionPartyDefinition { get; set; }

		public IList<string> environmentDefinition { get; set; }

		public IList<PurchasedLandExpansionDefinition> purchasedExpansionDefinitions { get; set; }

		public IList<LandExpansionDefinition> expansionDefinitions { get; set; }

		public IList<DebrisDefinition> debrisDefinitions { get; set; }

		public IList<AspirationalBuildingDefinition> aspirationalBuildingDefinitions { get; set; }

		public IList<LandExpansionConfig> expansionConfigs { get; set; }

		public IList<CommonLandExpansionDefinition> commonExpansionDefinitions { get; set; }

		public GachaConfig gachaConfig { get; set; }

		public IList<MinionAnimationDefinition> MinionAnimationDefinitions { get; set; }

		public IList<QuestDefinition> quests { get; set; }

		public IList<QuestResourceDefinition> questResources { get; set; }

		public IList<NotificationDefinition> notificationDefinitions { get; set; }

		public IList<RewardCollectionDefinition> collectionDefinitions { get; set; }

		public IList<PrestigeDefinition> prestigeDefinitions { get; set; }

		public LevelUpDefinition levelUpDefinition { get; set; }

		public LevelFunTable levelFunTable { get; set; }

		public LevelXPTable levelXPTable { get; set; }

		public TaskDefinition tasks { get; set; }

		public IList<TimedSocialEventDefinition> timedSocialEventDefinitions { get; set; }

		public PlayerVersion player { get; set; }

		public IList<RushTimeBandDefinition> rushDefinitions { get; set; }

		public IList<FootprintDefinition> footprintDefinitions { get; set; }

		public IList<QuestChainDefinition> questChains { get; set; }

		public IList<NamedCharacterDefinition> namedCharacterDefinitions { get; set; }

		public IList<StickerDefinition> stickerDefinitions { get; set; }

		public DropLevelBandDefinition randomDropLevelBandDefinition { get; set; }

		public WayFinderDefinition wayFinderDefinition { get; set; }

		public IList<FlyOverDefinition> flyOverDefinitions { get; set; }

		public IList<LoadinTipBucketDefinition> loadInTipBucketDefinitions { get; set; }

		public IList<LoadInTipDefinition> loadInTipDefinitions { get; set; }

		public CameraDefinition cameraSettingsDefinition { get; set; }

		public SocialSettingsDefinition socialSettingsDefinition { get; set; }

		public IList<SalePackDefinition> salePackDefinitions { get; set; }

		public IList<CurrencyStorePackDefinition> currencyStorePackDefinitions { get; set; }

		public NotificationSystemDefinition notificationSystemDefinition { get; set; }

		public IList<PlayerTrainingDefinition> playerTrainingDefinitions { get; set; }

		public IList<PlayerTrainingCardDefinition> playerTrainingCardDefinitions { get; set; }

		public IList<PlayerTrainingCategoryDefinition> playerTrainingCategoryDefinitions { get; set; }

		public IList<AchievementDefinition> achievementDefinitions { get; set; }

		public IList<BuffDefinition> buffDefinitions { get; set; }

		public IList<CustomCameraPositionDefinition> customCameraPositionDefinitions { get; set; }

		public IList<PartyFavorAnimationDefinition> partyFavorAnimationDefinitions { get; set; }

		public IList<GuestOfHonorDefinition> guestOfHonorDefinitions { get; set; }

		public CurrencyStoreDefinition currencyStoreDefinition { get; set; }

		public IList<UIAnimationDefinition> uiAnimationDefinitions { get; set; }

		public MinionBenefitLevelBandDefintion minionBenefitDefinition { get; set; }

		public IList<PopulationBenefitDefinition> populationBenefitDefinitions { get; set; }

		public IList<TriggerDefinition> triggerDefinitions { get; set; }

		public IList<TriggerRewardDefinition> triggerRewardDefinitions { get; set; }

		public IList<HelpTipDefinition> helpTipDefinitions { get; set; }

		public IList<VillainLairDefinition> villainLairDefinitions { get; set; }

		public IList<MasterPlanDefinition> masterPlanDefinitions { get; set; }

		public IList<MasterPlanComponentDefinition> masterPlanComponentDefinitions { get; set; }

		public IList<MasterPlanOnboardDefinition> onboardDefinitions { get; set; }

		public DynamicMasterPlanDefinition dynamicMasterPlanDefinition { get; set; }

		public IList<PendingRewardDefinition> pendingRewardDefinitions { get; set; }

		public RewardedAdvertisementDefinition rewardedAdvertisementDefinition { get; set; }

		public IList<LegalDocumentDefinition> legalDocumentDefinitions { get; set; }

		public PetsXPromoDefinition petsXPromoDefinition { get; set; }

		public IList<HindsightCampaignDefinition> hindsightCampaignDefinitions { get; set; }

		public virtual void Write(BinaryWriter writer)
		{
			BinarySerializationUtil.WriteList(writer, definitionGroups);
			BinarySerializationUtil.WriteList(writer, transactions);
			BinarySerializationUtil.WriteList(writer, weightedDefinitions);
			BinarySerializationUtil.WriteList(writer, buildingDefinitions);
			BinarySerializationUtil.WriteList(writer, plotDefinitions);
			BinarySerializationUtil.WriteList(writer, itemDefinitions);
			BinarySerializationUtil.WriteList(writer, minionDefinitions);
			BinarySerializationUtil.WriteList(writer, storeItemDefinitions);
			BinarySerializationUtil.WriteList(writer, currencyItemDefinitions);
			BinarySerializationUtil.WriteObject(writer, marketplaceDefinition);
			BinarySerializationUtil.WriteObject(writer, minionPartyDefinition);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, environmentDefinition);
			BinarySerializationUtil.WriteList(writer, purchasedExpansionDefinitions);
			BinarySerializationUtil.WriteList(writer, expansionDefinitions);
			BinarySerializationUtil.WriteList(writer, debrisDefinitions);
			BinarySerializationUtil.WriteList(writer, aspirationalBuildingDefinitions);
			BinarySerializationUtil.WriteList(writer, expansionConfigs);
			BinarySerializationUtil.WriteList(writer, commonExpansionDefinitions);
			BinarySerializationUtil.WriteGachaConfig(writer, gachaConfig);
			BinarySerializationUtil.WriteList(writer, MinionAnimationDefinitions);
			BinarySerializationUtil.WriteList(writer, quests);
			BinarySerializationUtil.WriteList(writer, questResources);
			BinarySerializationUtil.WriteList(writer, notificationDefinitions);
			BinarySerializationUtil.WriteList(writer, collectionDefinitions);
			BinarySerializationUtil.WriteList(writer, prestigeDefinitions);
			BinarySerializationUtil.WriteObject(writer, levelUpDefinition);
			BinarySerializationUtil.WriteObject(writer, levelFunTable);
			BinarySerializationUtil.WriteObject(writer, levelXPTable);
			BinarySerializationUtil.WriteTaskDefinition(writer, tasks);
			BinarySerializationUtil.WriteList(writer, timedSocialEventDefinitions);
			BinarySerializationUtil.WritePlayerVersion(writer, player);
			BinarySerializationUtil.WriteList(writer, rushDefinitions);
			BinarySerializationUtil.WriteList(writer, footprintDefinitions);
			BinarySerializationUtil.WriteList(writer, questChains);
			BinarySerializationUtil.WriteList(writer, namedCharacterDefinitions);
			BinarySerializationUtil.WriteList(writer, stickerDefinitions);
			BinarySerializationUtil.WriteObject(writer, randomDropLevelBandDefinition);
			BinarySerializationUtil.WriteObject(writer, wayFinderDefinition);
			BinarySerializationUtil.WriteList(writer, flyOverDefinitions);
			BinarySerializationUtil.WriteList(writer, loadInTipBucketDefinitions);
			BinarySerializationUtil.WriteList(writer, loadInTipDefinitions);
			BinarySerializationUtil.WriteObject(writer, cameraSettingsDefinition);
			BinarySerializationUtil.WriteObject(writer, socialSettingsDefinition);
			BinarySerializationUtil.WriteList(writer, salePackDefinitions);
			BinarySerializationUtil.WriteList(writer, currencyStorePackDefinitions);
			BinarySerializationUtil.WriteObject(writer, notificationSystemDefinition);
			BinarySerializationUtil.WriteList(writer, playerTrainingDefinitions);
			BinarySerializationUtil.WriteList(writer, playerTrainingCardDefinitions);
			BinarySerializationUtil.WriteList(writer, playerTrainingCategoryDefinitions);
			BinarySerializationUtil.WriteList(writer, achievementDefinitions);
			BinarySerializationUtil.WriteList(writer, buffDefinitions);
			BinarySerializationUtil.WriteList(writer, customCameraPositionDefinitions);
			BinarySerializationUtil.WriteList(writer, partyFavorAnimationDefinitions);
			BinarySerializationUtil.WriteList(writer, guestOfHonorDefinitions);
			BinarySerializationUtil.WriteObject(writer, currencyStoreDefinition);
			BinarySerializationUtil.WriteList(writer, uiAnimationDefinitions);
			BinarySerializationUtil.WriteObject(writer, minionBenefitDefinition);
			BinarySerializationUtil.WriteList(writer, populationBenefitDefinitions);
			BinarySerializationUtil.WriteList(writer, triggerDefinitions);
			BinarySerializationUtil.WriteList(writer, triggerRewardDefinitions);
			BinarySerializationUtil.WriteList(writer, helpTipDefinitions);
			BinarySerializationUtil.WriteList(writer, villainLairDefinitions);
			BinarySerializationUtil.WriteList(writer, masterPlanDefinitions);
			BinarySerializationUtil.WriteList(writer, masterPlanComponentDefinitions);
			BinarySerializationUtil.WriteList(writer, onboardDefinitions);
			BinarySerializationUtil.WriteObject(writer, dynamicMasterPlanDefinition);
			BinarySerializationUtil.WriteList(writer, pendingRewardDefinitions);
			BinarySerializationUtil.WriteObject(writer, rewardedAdvertisementDefinition);
			BinarySerializationUtil.WriteList(writer, legalDocumentDefinitions);
			BinarySerializationUtil.WriteObject(writer, petsXPromoDefinition);
			BinarySerializationUtil.WriteList(writer, hindsightCampaignDefinitions);
			BinarySerializationUtil.WriteList(writer, compositeBuildingPieceDefinitions);
		}

		public virtual void Read(BinaryReader reader)
		{
			definitionGroups = BinarySerializationUtil.ReadList(reader, definitionGroups);
			transactions = BinarySerializationUtil.ReadList(reader, transactions);
			weightedDefinitions = BinarySerializationUtil.ReadList(reader, weightedDefinitions);
			buildingDefinitions = BinarySerializationUtil.ReadList(reader, buildingDefinitions);
			plotDefinitions = BinarySerializationUtil.ReadList(reader, plotDefinitions);
			itemDefinitions = BinarySerializationUtil.ReadList(reader, itemDefinitions);
			minionDefinitions = BinarySerializationUtil.ReadList(reader, minionDefinitions);
			storeItemDefinitions = BinarySerializationUtil.ReadList(reader, storeItemDefinitions);
			currencyItemDefinitions = BinarySerializationUtil.ReadList(reader, currencyItemDefinitions);
			marketplaceDefinition = BinarySerializationUtil.ReadObject<MarketplaceDefinition>(reader);
			minionPartyDefinition = BinarySerializationUtil.ReadObject<MinionPartyDefinition>(reader);
			environmentDefinition = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, environmentDefinition);
			purchasedExpansionDefinitions = BinarySerializationUtil.ReadList(reader, purchasedExpansionDefinitions);
			expansionDefinitions = BinarySerializationUtil.ReadList(reader, expansionDefinitions);
			debrisDefinitions = BinarySerializationUtil.ReadList(reader, debrisDefinitions);
			aspirationalBuildingDefinitions = BinarySerializationUtil.ReadList(reader, aspirationalBuildingDefinitions);
			expansionConfigs = BinarySerializationUtil.ReadList(reader, expansionConfigs);
			commonExpansionDefinitions = BinarySerializationUtil.ReadList(reader, commonExpansionDefinitions);
			gachaConfig = BinarySerializationUtil.ReadGachaConfig(reader);
			MinionAnimationDefinitions = BinarySerializationUtil.ReadList(reader, MinionAnimationDefinitions);
			quests = BinarySerializationUtil.ReadList(reader, quests);
			questResources = BinarySerializationUtil.ReadList(reader, questResources);
			notificationDefinitions = BinarySerializationUtil.ReadList(reader, notificationDefinitions);
			collectionDefinitions = BinarySerializationUtil.ReadList(reader, collectionDefinitions);
			prestigeDefinitions = BinarySerializationUtil.ReadList(reader, prestigeDefinitions);
			levelUpDefinition = BinarySerializationUtil.ReadObject<LevelUpDefinition>(reader);
			levelFunTable = BinarySerializationUtil.ReadObject<LevelFunTable>(reader);
			levelXPTable = BinarySerializationUtil.ReadObject<LevelXPTable>(reader);
			tasks = BinarySerializationUtil.ReadTaskDefinition(reader);
			timedSocialEventDefinitions = BinarySerializationUtil.ReadList(reader, timedSocialEventDefinitions);
			player = BinarySerializationUtil.ReadPlayerVersion(reader);
			rushDefinitions = BinarySerializationUtil.ReadList(reader, rushDefinitions);
			footprintDefinitions = BinarySerializationUtil.ReadList(reader, footprintDefinitions);
			questChains = BinarySerializationUtil.ReadList(reader, questChains);
			namedCharacterDefinitions = BinarySerializationUtil.ReadList(reader, namedCharacterDefinitions);
			stickerDefinitions = BinarySerializationUtil.ReadList(reader, stickerDefinitions);
			randomDropLevelBandDefinition = BinarySerializationUtil.ReadObject<DropLevelBandDefinition>(reader);
			wayFinderDefinition = BinarySerializationUtil.ReadObject<WayFinderDefinition>(reader);
			flyOverDefinitions = BinarySerializationUtil.ReadList(reader, flyOverDefinitions);
			loadInTipBucketDefinitions = BinarySerializationUtil.ReadList(reader, loadInTipBucketDefinitions);
			loadInTipDefinitions = BinarySerializationUtil.ReadList(reader, loadInTipDefinitions);
			cameraSettingsDefinition = BinarySerializationUtil.ReadObject<CameraDefinition>(reader);
			socialSettingsDefinition = BinarySerializationUtil.ReadObject<SocialSettingsDefinition>(reader);
			salePackDefinitions = BinarySerializationUtil.ReadList(reader, salePackDefinitions);
			currencyStorePackDefinitions = BinarySerializationUtil.ReadList(reader, currencyStorePackDefinitions);
			notificationSystemDefinition = BinarySerializationUtil.ReadObject<NotificationSystemDefinition>(reader);
			playerTrainingDefinitions = BinarySerializationUtil.ReadList(reader, playerTrainingDefinitions);
			playerTrainingCardDefinitions = BinarySerializationUtil.ReadList(reader, playerTrainingCardDefinitions);
			playerTrainingCategoryDefinitions = BinarySerializationUtil.ReadList(reader, playerTrainingCategoryDefinitions);
			achievementDefinitions = BinarySerializationUtil.ReadList(reader, achievementDefinitions);
			buffDefinitions = BinarySerializationUtil.ReadList(reader, buffDefinitions);
			customCameraPositionDefinitions = BinarySerializationUtil.ReadList(reader, customCameraPositionDefinitions);
			partyFavorAnimationDefinitions = BinarySerializationUtil.ReadList(reader, partyFavorAnimationDefinitions);
			guestOfHonorDefinitions = BinarySerializationUtil.ReadList(reader, guestOfHonorDefinitions);
			currencyStoreDefinition = BinarySerializationUtil.ReadObject<CurrencyStoreDefinition>(reader);
			uiAnimationDefinitions = BinarySerializationUtil.ReadList(reader, uiAnimationDefinitions);
			minionBenefitDefinition = BinarySerializationUtil.ReadObject<MinionBenefitLevelBandDefintion>(reader);
			populationBenefitDefinitions = BinarySerializationUtil.ReadList(reader, populationBenefitDefinitions);
			triggerDefinitions = BinarySerializationUtil.ReadList(reader, triggerDefinitions);
			triggerRewardDefinitions = BinarySerializationUtil.ReadList(reader, triggerRewardDefinitions);
			helpTipDefinitions = BinarySerializationUtil.ReadList(reader, helpTipDefinitions);
			villainLairDefinitions = BinarySerializationUtil.ReadList(reader, villainLairDefinitions);
			masterPlanDefinitions = BinarySerializationUtil.ReadList(reader, masterPlanDefinitions);
			masterPlanComponentDefinitions = BinarySerializationUtil.ReadList(reader, masterPlanComponentDefinitions);
			onboardDefinitions = BinarySerializationUtil.ReadList(reader, onboardDefinitions);
			dynamicMasterPlanDefinition = BinarySerializationUtil.ReadObject<DynamicMasterPlanDefinition>(reader);
			pendingRewardDefinitions = BinarySerializationUtil.ReadList(reader, pendingRewardDefinitions);
			rewardedAdvertisementDefinition = BinarySerializationUtil.ReadObject<RewardedAdvertisementDefinition>(reader);
			legalDocumentDefinitions = BinarySerializationUtil.ReadList(reader, legalDocumentDefinitions);
			petsXPromoDefinition = BinarySerializationUtil.ReadObject<PetsXPromoDefinition>(reader);
			hindsightCampaignDefinitions = BinarySerializationUtil.ReadList(reader, hindsightCampaignDefinitions);
			compositeBuildingPieceDefinitions = BinarySerializationUtil.ReadList(reader, compositeBuildingPieceDefinitions);
		}

		public virtual object Deserialize(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string propertyName = ((string)reader.Value).ToUpper();
					if (!DeserializeProperty(propertyName, reader, converters))
					{
						reader.Skip();
					}
					break;
				}
				case JsonToken.EndObject:
					return this;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, ReaderUtil.GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		protected virtual bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "DEFINITIONGROUPS":
				reader.Read();
				definitionGroups = ReaderUtil.PopulateList(reader, converters, definitionGroups);
				break;
			case "TRANSACTIONS":
				reader.Read();
				transactions = ((converters.transactionDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, transactions) : ReaderUtil.PopulateList(reader, converters, converters.transactionDefinitionConverter, transactions));
				break;
			case "WEIGHTEDDEFINITIONS":
				reader.Read();
				weightedDefinitions = ReaderUtil.PopulateList(reader, converters, weightedDefinitions);
				break;
			case "BUILDINGDEFINITIONS":
				reader.Read();
				buildingDefinitions = ReaderUtil.PopulateList(reader, converters, converters.buildingDefinitionConverter, buildingDefinitions);
				break;
			case "PLOTDEFINITIONS":
				reader.Read();
				plotDefinitions = ReaderUtil.PopulateList(reader, converters, converters.plotDefinitionConverter, plotDefinitions);
				break;
			case "ITEMDEFINITIONS":
				reader.Read();
				itemDefinitions = ((converters.itemDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, itemDefinitions) : ReaderUtil.PopulateList(reader, converters, converters.itemDefinitionConverter, itemDefinitions));
				break;
			case "MINIONDEFINITIONS":
				reader.Read();
				minionDefinitions = ReaderUtil.PopulateList(reader, converters, minionDefinitions);
				break;
			case "STOREITEMDEFINITIONS":
				reader.Read();
				storeItemDefinitions = ReaderUtil.PopulateList(reader, converters, storeItemDefinitions);
				break;
			case "CURRENCYITEMDEFINITIONS":
				reader.Read();
				currencyItemDefinitions = ((converters.currencyItemDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, currencyItemDefinitions) : ReaderUtil.PopulateList(reader, converters, converters.currencyItemDefinitionConverter, currencyItemDefinitions));
				break;
			case "MARKETPLACEDEFINITION":
				reader.Read();
				marketplaceDefinition = FastJSONDeserializer.Deserialize<MarketplaceDefinition>(reader, converters);
				break;
			case "MINIONPARTYDEFINITION":
				reader.Read();
				minionPartyDefinition = FastJSONDeserializer.Deserialize<MinionPartyDefinition>(reader, converters);
				break;
			case "ENVIRONMENTDEFINITION":
				reader.Read();
				environmentDefinition = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, environmentDefinition);
				break;
			case "PURCHASEDEXPANSIONDEFINITIONS":
				reader.Read();
				purchasedExpansionDefinitions = ReaderUtil.PopulateList(reader, converters, purchasedExpansionDefinitions);
				break;
			case "EXPANSIONDEFINITIONS":
				reader.Read();
				expansionDefinitions = ReaderUtil.PopulateList(reader, converters, expansionDefinitions);
				break;
			case "DEBRISDEFINITIONS":
				reader.Read();
				debrisDefinitions = ReaderUtil.PopulateList(reader, converters, debrisDefinitions);
				break;
			case "ASPIRATIONALBUILDINGDEFINITIONS":
				reader.Read();
				aspirationalBuildingDefinitions = ReaderUtil.PopulateList(reader, converters, aspirationalBuildingDefinitions);
				break;
			case "EXPANSIONCONFIGS":
				reader.Read();
				expansionConfigs = ReaderUtil.PopulateList(reader, converters, expansionConfigs);
				break;
			case "COMMONEXPANSIONDEFINITIONS":
				reader.Read();
				commonExpansionDefinitions = ReaderUtil.PopulateList(reader, converters, commonExpansionDefinitions);
				break;
			case "GACHACONFIG":
				reader.Read();
				gachaConfig = ReaderUtil.ReadGachaConfig(reader, converters);
				break;
			case "MINIONANIMATIONDEFINITIONS":
				reader.Read();
				MinionAnimationDefinitions = ReaderUtil.PopulateList(reader, converters, MinionAnimationDefinitions);
				break;
			case "QUESTS":
				reader.Read();
				quests = ((converters.questDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, quests) : ReaderUtil.PopulateList(reader, converters, converters.questDefinitionConverter, quests));
				break;
			case "QUESTRESOURCES":
				reader.Read();
				questResources = ReaderUtil.PopulateList(reader, converters, questResources);
				break;
			case "NOTIFICATIONDEFINITIONS":
				reader.Read();
				notificationDefinitions = ReaderUtil.PopulateList(reader, converters, notificationDefinitions);
				break;
			case "COLLECTIONDEFINITIONS":
				reader.Read();
				collectionDefinitions = ReaderUtil.PopulateList(reader, converters, collectionDefinitions);
				break;
			case "PRESTIGEDEFINITIONS":
				reader.Read();
				prestigeDefinitions = ReaderUtil.PopulateList(reader, converters, prestigeDefinitions);
				break;
			case "LEVELUPDEFINITION":
				reader.Read();
				levelUpDefinition = FastJSONDeserializer.Deserialize<LevelUpDefinition>(reader, converters);
				break;
			case "LEVELFUNTABLE":
				reader.Read();
				levelFunTable = FastJSONDeserializer.Deserialize<LevelFunTable>(reader, converters);
				break;
			case "LEVELXPTABLE":
				reader.Read();
				levelXPTable = FastJSONDeserializer.Deserialize<LevelXPTable>(reader, converters);
				break;
			case "TASKS":
				reader.Read();
				tasks = ReaderUtil.ReadTaskDefinition(reader, converters);
				break;
			case "TIMEDSOCIALEVENTDEFINITIONS":
				reader.Read();
				timedSocialEventDefinitions = ReaderUtil.PopulateList(reader, converters, timedSocialEventDefinitions);
				break;
			case "PLAYER":
				reader.Read();
				player = ((converters.playerVersionConverter == null) ? FastJSONDeserializer.Deserialize<PlayerVersion>(reader, converters) : converters.playerVersionConverter.ReadJson(reader, converters));
				break;
			case "RUSHDEFINITIONS":
				reader.Read();
				rushDefinitions = ReaderUtil.PopulateList(reader, converters, rushDefinitions);
				break;
			case "FOOTPRINTDEFINITIONS":
				reader.Read();
				footprintDefinitions = ReaderUtil.PopulateList(reader, converters, footprintDefinitions);
				break;
			case "QUESTCHAINS":
				reader.Read();
				questChains = ReaderUtil.PopulateList(reader, converters, questChains);
				break;
			case "NAMEDCHARACTERDEFINITIONS":
				reader.Read();
				namedCharacterDefinitions = ReaderUtil.PopulateList(reader, converters, converters.namedCharacterDefinitionConverter, namedCharacterDefinitions);
				break;
			case "STICKERDEFINITIONS":
				reader.Read();
				stickerDefinitions = ReaderUtil.PopulateList(reader, converters, stickerDefinitions);
				break;
			case "RANDOMDROPLEVELBANDDEFINITION":
				reader.Read();
				randomDropLevelBandDefinition = FastJSONDeserializer.Deserialize<DropLevelBandDefinition>(reader, converters);
				break;
			case "WAYFINDERDEFINITION":
				reader.Read();
				wayFinderDefinition = FastJSONDeserializer.Deserialize<WayFinderDefinition>(reader, converters);
				break;
			case "FLYOVERDEFINITIONS":
				reader.Read();
				flyOverDefinitions = ReaderUtil.PopulateList(reader, converters, flyOverDefinitions);
				break;
			case "LOADINTIPBUCKETDEFINITIONS":
				reader.Read();
				loadInTipBucketDefinitions = ReaderUtil.PopulateList(reader, converters, loadInTipBucketDefinitions);
				break;
			case "LOADINTIPDEFINITIONS":
				reader.Read();
				loadInTipDefinitions = ReaderUtil.PopulateList(reader, converters, loadInTipDefinitions);
				break;
			case "CAMERASETTINGSDEFINITION":
				reader.Read();
				cameraSettingsDefinition = FastJSONDeserializer.Deserialize<CameraDefinition>(reader, converters);
				break;
			case "SOCIALSETTINGSDEFINITION":
				reader.Read();
				socialSettingsDefinition = FastJSONDeserializer.Deserialize<SocialSettingsDefinition>(reader, converters);
				break;
			case "SALEPACKDEFINITIONS":
				reader.Read();
				salePackDefinitions = ((converters.salePackDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, salePackDefinitions) : ReaderUtil.PopulateList(reader, converters, converters.salePackDefinitionConverter, salePackDefinitions));
				break;
			case "CURRENCYSTOREPACKDEFINITIONS":
				reader.Read();
				currencyStorePackDefinitions = ((converters.currencyStorePackDefinitionConverter == null) ? ReaderUtil.PopulateList(reader, converters, currencyStorePackDefinitions) : ReaderUtil.PopulateList(reader, converters, converters.currencyStorePackDefinitionConverter, currencyStorePackDefinitions));
				break;
			case "NOTIFICATIONSYSTEMDEFINITION":
				reader.Read();
				notificationSystemDefinition = FastJSONDeserializer.Deserialize<NotificationSystemDefinition>(reader, converters);
				break;
			case "PLAYERTRAININGDEFINITIONS":
				reader.Read();
				playerTrainingDefinitions = ReaderUtil.PopulateList(reader, converters, playerTrainingDefinitions);
				break;
			case "PLAYERTRAININGCARDDEFINITIONS":
				reader.Read();
				playerTrainingCardDefinitions = ReaderUtil.PopulateList(reader, converters, playerTrainingCardDefinitions);
				break;
			case "PLAYERTRAININGCATEGORYDEFINITIONS":
				reader.Read();
				playerTrainingCategoryDefinitions = ReaderUtil.PopulateList(reader, converters, playerTrainingCategoryDefinitions);
				break;
			case "ACHIEVEMENTDEFINITIONS":
				reader.Read();
				achievementDefinitions = ReaderUtil.PopulateList(reader, converters, achievementDefinitions);
				break;
			case "BUFFDEFINITIONS":
				reader.Read();
				buffDefinitions = ReaderUtil.PopulateList(reader, converters, buffDefinitions);
				break;
			case "CUSTOMCAMERAPOSITIONDEFINITIONS":
				reader.Read();
				customCameraPositionDefinitions = ReaderUtil.PopulateList(reader, converters, customCameraPositionDefinitions);
				break;
			case "PARTYFAVORANIMATIONDEFINITIONS":
				reader.Read();
				partyFavorAnimationDefinitions = ReaderUtil.PopulateList(reader, converters, partyFavorAnimationDefinitions);
				break;
			case "GUESTOFHONORDEFINITIONS":
				reader.Read();
				guestOfHonorDefinitions = ReaderUtil.PopulateList(reader, converters, guestOfHonorDefinitions);
				break;
			case "CURRENCYSTOREDEFINITION":
				reader.Read();
				currencyStoreDefinition = FastJSONDeserializer.Deserialize<CurrencyStoreDefinition>(reader, converters);
				break;
			case "UIANIMATIONDEFINITIONS":
				reader.Read();
				uiAnimationDefinitions = ReaderUtil.PopulateList(reader, converters, uiAnimationDefinitions);
				break;
			case "MINIONBENEFITDEFINITION":
				reader.Read();
				minionBenefitDefinition = FastJSONDeserializer.Deserialize<MinionBenefitLevelBandDefintion>(reader, converters);
				break;
			case "POPULATIONBENEFITDEFINITIONS":
				reader.Read();
				populationBenefitDefinitions = ReaderUtil.PopulateList(reader, converters, populationBenefitDefinitions);
				break;
			case "TRIGGERDEFINITIONS":
				reader.Read();
				triggerDefinitions = ReaderUtil.PopulateList(reader, converters, converters.triggerDefinitionConverter, triggerDefinitions);
				break;
			case "TRIGGERREWARDDEFINITIONS":
				reader.Read();
				triggerRewardDefinitions = ReaderUtil.PopulateList(reader, converters, converters.triggerRewardDefinitionConverter, triggerRewardDefinitions);
				break;
			case "HELPTIPDEFINITIONS":
				reader.Read();
				helpTipDefinitions = ReaderUtil.PopulateList(reader, converters, helpTipDefinitions);
				break;
			case "VILLAINLAIRDEFINITIONS":
				reader.Read();
				villainLairDefinitions = ReaderUtil.PopulateList(reader, converters, villainLairDefinitions);
				break;
			case "MASTERPLANDEFINITIONS":
				reader.Read();
				masterPlanDefinitions = ReaderUtil.PopulateList(reader, converters, masterPlanDefinitions);
				break;
			case "MASTERPLANCOMPONENTDEFINITIONS":
				reader.Read();
				masterPlanComponentDefinitions = ReaderUtil.PopulateList(reader, converters, masterPlanComponentDefinitions);
				break;
			case "ONBOARDDEFINITIONS":
				reader.Read();
				onboardDefinitions = ReaderUtil.PopulateList(reader, converters, onboardDefinitions);
				break;
			case "DYNAMICMASTERPLANDEFINITION":
				reader.Read();
				dynamicMasterPlanDefinition = FastJSONDeserializer.Deserialize<DynamicMasterPlanDefinition>(reader, converters);
				break;
			case "PENDINGREWARDDEFINITIONS":
				reader.Read();
				pendingRewardDefinitions = ReaderUtil.PopulateList(reader, converters, pendingRewardDefinitions);
				break;
			case "REWARDEDADVERTISEMENTDEFINITION":
				reader.Read();
				rewardedAdvertisementDefinition = FastJSONDeserializer.Deserialize<RewardedAdvertisementDefinition>(reader, converters);
				break;
			case "LEGALDOCUMENTDEFINITIONS":
				reader.Read();
				legalDocumentDefinitions = ReaderUtil.PopulateList(reader, converters, legalDocumentDefinitions);
				break;
			case "PETSXPROMODEFINITION":
				reader.Read();
				petsXPromoDefinition = FastJSONDeserializer.Deserialize<PetsXPromoDefinition>(reader, converters);
				break;
			case "HINDSIGHTCAMPAIGNDEFINITIONS":
				reader.Read();
				hindsightCampaignDefinitions = ReaderUtil.PopulateList(reader, converters, hindsightCampaignDefinitions);
				break;
			case "COMPOSITEBUILDINGPIECEDEFINITIONS":
				reader.Read();
				compositeBuildingPieceDefinitions = ReaderUtil.PopulateList(reader, converters, compositeBuildingPieceDefinitions);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
