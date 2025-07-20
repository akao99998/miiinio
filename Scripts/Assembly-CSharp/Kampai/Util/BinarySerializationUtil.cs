using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.Util
{
	public static class BinarySerializationUtil
	{
		public static class BinarySerializationFactory
		{
			public enum TypeCode
			{
				Kampai_Game_Definition = 1000,
				Kampai_Game_DefinitionGroup = 1001,
				Kampai_Game_DropLevelBandDefinition = 1002,
				Kampai_Game_HelpTipDefinition = 1003,
				Kampai_Game_LegalDocumentDefinition = 1004,
				Kampai_Game_LevelFunTable = 1005,
				Kampai_Game_PartyUpDefinition = 1006,
				Kampai_Game_Transaction_TransactionDefinition = 1007,
				Kampai_Util_QuantityItem = 1008,
				Kampai_Game_LevelUpDefinition = 1009,
				Kampai_Game_LevelXPTable = 1010,
				Kampai_Game_NotificationDefinition = 1011,
				Kampai_Game_NotificationSystemDefinition = 1012,
				Kampai_Game_PrestigeDefinition = 1013,
				Kampai_Game_TaxonomyDefinition = 1014,
				Kampai_Game_DisplayableDefinition = 1015,
				Kampai_Game_AchievementDefinition = 1016,
				Kampai_Game_PendingRewardDefinition = 1017,
				Kampai_Game_RewardedAdvertisementDefinition = 1018,
				Kampai_Game_AdPlacementDefinition = 1019,
				Kampai_Game_Trigger_TriggerConditionDefinition = 1020,
				Kampai_Game_OnTheGlassDailyRewardDefinition = 1021,
				Kampai_Game_Transaction_WeightedDefinition = 1022,
				Kampai_Game_Transaction_WeightedQuantityItem = 1023,
				Kampai_Game_CraftingRushRewardDefinition = 1024,
				Kampai_Game_MarketplaceRefreshRushRewardDefinition = 1025,
				Kampai_Game_MissingResourcesRewardDefinition = 1026,
				Kampai_Game_OrderboardFillOrderRewardDefinition = 1027,
				Kampai_Game_Quest2xRewardDefinition = 1028,
				Kampai_Game_OfferwallPlacementDefinition = 1029,
				Kampai_Game_AnimatingBuildingDefinition = 1030,
				Kampai_Game_BuildingAnimationDefinition = 1031,
				Kampai_Game_AnimationDefinition = 1032,
				Kampai_Game_RepairableBuildingDefinition = 1033,
				Kampai_Game_BuildingDefinition = 1034,
				Kampai_Game_BlackMarketBoardDefinition = 1035,
				Kampai_Game_BlackMarketBoardUnlockedOrderSlotDefinition = 1036,
				Kampai_Game_BlackMarketBoardSlotDefinition = 1037,
				Kampai_Game_BlackMarketBoardMultiplierDefinition = 1038,
				Kampai_Game_BridgeBuildingDefinition = 1039,
				Kampai_Game_CabanaBuildingDefinition = 1040,
				Kampai_Game_CompositeBuildingDefinition = 1041,
				Kampai_Game_CompositeBuildingPieceDefinition = 1042,
				Kampai_Game_ConnectableBuildingDefinition = 1043,
				Kampai_Game_DecorationBuildingDefinition = 1044,
				Kampai_Game_CraftingBuildingDefinition = 1045,
				Kampai_Game_RecipeDefinition = 1046,
				Kampai_Game_DCNBuildingDefinition = 1047,
				Kampai_Game_DebrisBuildingDefinition = 1048,
				Kampai_Game_TaskableBuildingDefinition = 1049,
				Kampai_Game_FountainBuildingDefinition = 1050,
				Kampai_Game_LandExpansionBuildingDefinition = 1051,
				Kampai_Game_LeisureBuildingDefintiion = 1052,
				Kampai_Game_MIBBuildingDefinition = 1053,
				Kampai_Game_MasterPlanComponentBuildingDefinition = 1054,
				Kampai_Game_MasterPlanLeftOverBuildingDefinition = 1055,
				Kampai_Game_MignetteBuildingDefinition = 1056,
				Kampai_Game_MinionPartyBuildingDefinition = 1057,
				Kampai_Game_MinionUpgradeBuildingDefinition = 1058,
				Kampai_Game_PurchasedLandExpansionDefinition = 1059,
				Kampai_Game_ResourceBuildingDefinition = 1060,
				Kampai_Game_SpecialBuildingDefinition = 1061,
				Kampai_Game_StageBuildingDefinition = 1062,
				Kampai_Game_StorageBuildingDefinition = 1063,
				Kampai_Game_TaskableMinionPartyBuildingDefinition = 1064,
				Kampai_Game_TikiBarBuildingDefinition = 1065,
				Kampai_Game_VillainLairDefinition = 1066,
				Kampai_Game_VillainLairEntranceBuildingDefinition = 1067,
				Kampai_Game_VillainLairResourcePlotDefinition = 1068,
				Kampai_Game_WelcomeHutBuildingDefinition = 1069,
				Kampai_Game_CameraDefinition = 1070,
				Kampai_Game_CustomCameraPositionDefinition = 1071,
				Kampai_Game_BigThreeCharacterDefinition = 1072,
				Kampai_Game_NamedCharacterDefinition = 1073,
				Kampai_Game_NamedCharacterAnimationDefinition = 1074,
				Kampai_Game_BobCharacterDefinition = 1075,
				Kampai_Game_MinionAnimationDefinition = 1076,
				Kampai_Game_FrolicCharacterDefinition = 1077,
				Kampai_Game_LocationIncidentalAnimationDefinition = 1078,
				Kampai_Game_KevinCharacterDefinition = 1079,
				Kampai_Game_PhilCharacterDefinition = 1080,
				Kampai_Game_SpecialEventCharacterDefinition = 1081,
				Kampai_Game_StuartCharacterDefinition = 1082,
				Kampai_Game_TSMCharacterDefinition = 1083,
				Kampai_Game_UIAnimationDefinition = 1084,
				Kampai_Game_RewardCollectionDefinition = 1085,
				Kampai_Game_EnvironmentDefinition = 1086,
				Kampai_Game_FlyOverDefinition = 1087,
				Kampai_Game_BridgeDefinition = 1088,
				Kampai_Game_ItemDefinition = 1089,
				Kampai_Game_CostumeItemDefinition = 1090,
				Kampai_Game_DropItemDefinition = 1091,
				Kampai_Game_DynamicIngredientsDefinition = 1092,
				Kampai_Game_IngredientsItemDefinition = 1093,
				Kampai_Game_PartyFavorAnimationItemDefinition = 1094,
				Kampai_Game_SpecialEventItemDefinition = 1095,
				Kampai_Game_AspirationalBuildingDefinition = 1096,
				Kampai_Game_CommonLandExpansionDefinition = 1097,
				Kampai_Game_DebrisDefinition = 1098,
				Kampai_Game_LandExpansionConfig = 1099,
				Kampai_Game_LandExpansionDefinition = 1100,
				Kampai_Game_LocalizedTextDefinition = 1101,
				Kampai_Game_MarketplaceDefinition = 1102,
				Kampai_Game_MarketplaceItemDefinition = 1103,
				Kampai_Game_MarketplaceSaleSlotDefinition = 1104,
				Kampai_Game_MarketplaceRefreshTimerDefinition = 1105,
				Kampai_Game_DynamicMasterPlanDefinition = 1106,
				Kampai_Game_MasterPlanComponentDefinition = 1107,
				Kampai_Game_MasterPlanDefinition = 1108,
				Kampai_Game_MasterPlanOnboardDefinition = 1109,
				Kampai_Game_GachaAnimationDefinition = 1110,
				Kampai_Game_GachaWeightedDefinition = 1111,
				Kampai_Game_MinionDefinition = 1112,
				Kampai_Game_PartyFavorAnimationDefinition = 1113,
				Kampai_Game_BuffDefinition = 1114,
				Kampai_Game_GuestOfHonorDefinition = 1115,
				Kampai_Game_MinionPartyDefinition = 1116,
				Kampai_Game_PartyMeterDefinition = 1117,
				Kampai_Game_PartyMeterTierDefinition = 1118,
				Kampai_Game_MinionPartyLevelBandDefinition = 1119,
				Kampai_Game_PartyPointsPerLevelDefinition = 1120,
				Kampai_Game_MinionBenefitLevelBandDefintion = 1121,
				Kampai_Game_PopulationBenefitDefinition = 1122,
				Kampai_Game_PlayerTrainingCardDefinition = 1123,
				Kampai_Game_PlayerTrainingCategoryDefinition = 1124,
				Kampai_Game_PlayerTrainingDefinition = 1125,
				Kampai_Game_FootprintDefinition = 1126,
				Kampai_Game_NoOpPlotDefinition = 1127,
				Kampai_Game_PlotDefinition = 1128,
				Kampai_Game_DynamicQuestDefinition = 1129,
				Kampai_Game_QuestDefinition = 1130,
				Kampai_Game_LimitedQuestDefinition = 1131,
				Kampai_Game_QuestChainDefinition = 1132,
				Kampai_Game_QuestResourceDefinition = 1133,
				Kampai_Game_TimedQuestDefinition = 1134,
				Kampai_Game_RushTimeBandDefinition = 1135,
				Kampai_Game_SalePackDefinition = 1136,
				Kampai_Game_PackDefinition = 1137,
				Kampai_Game_PremiumCurrencyItemDefinition = 1138,
				Kampai_Game_CurrencyItemDefinition = 1139,
				Kampai_Game_SocialSettingsDefinition = 1140,
				Kampai_Game_StickerDefinition = 1141,
				Kampai_Game_CurrencyStoreCategoryDefinition = 1142,
				Kampai_Game_CurrencyStoreDefinition = 1143,
				Kampai_Game_CurrencyStorePackDefinition = 1144,
				Kampai_Game_StoreItemDefinition = 1145,
				Kampai_Game_TaskLevelBandDefinition = 1146,
				Kampai_Game_TimedSocialEventDefinition = 1147,
				Kampai_Game_UnlockDefinition = 1148,
				Kampai_Game_Trigger_TSMTriggerDefinition = 1149,
				Kampai_Game_Trigger_TriggerDefinition = 1150,
				Kampai_Game_Trigger_UpsellTriggerDefinition = 1151,
				Kampai_Game_Trigger_AvailableLandTriggerConditionDefinition = 1152,
				Kampai_Game_Trigger_ChurnTriggerConditionDefinition = 1153,
				Kampai_Game_Trigger_ConsecutiveDaysConditionDefinition = 1154,
				Kampai_Game_Trigger_CountryTriggerConditionDefinition = 1155,
				Kampai_Game_Trigger_DaysSinceInstallTriggerConditionDefinition = 1156,
				Kampai_Game_Trigger_HelpButtonTriggerConditionDefinition = 1157,
				Kampai_Game_Trigger_HoursPlayedTriggerConditionDefinition = 1158,
				Kampai_Game_Trigger_LandExpansionTriggerConditionDefinition = 1159,
				Kampai_Game_Trigger_MignetteScoreTriggerConditionDefinition = 1160,
				Kampai_Game_Trigger_OrderBoardTriggerConditionDefinition = 1161,
				Kampai_Game_Trigger_PlatformTriggerConditionDefinition = 1162,
				Kampai_Game_Trigger_PrestigeLevelTriggerConditionDefinition = 1163,
				Kampai_Game_Trigger_PrestigeTriggerConditionDefinitionBase = 1164,
				Kampai_Game_Trigger_PrestigeStateTriggerConditionDefinition = 1165,
				Kampai_Game_Trigger_PrestigeTriggerConditionDefinition = 1166,
				Kampai_Game_Trigger_PurchaseTriggerConditionDefinition = 1167,
				Kampai_Game_Trigger_QuantityItemTriggerConditionDefinition = 1168,
				Kampai_Game_Trigger_QuestTriggerConditionDefinition = 1169,
				Kampai_Game_Trigger_SaleItemTriggerConditionDefinition = 1170,
				Kampai_Game_Trigger_SaleSlotTriggerConditionDefinition = 1171,
				Kampai_Game_Trigger_SegmentTriggerConditionDefinition = 1172,
				Kampai_Game_Trigger_SessionCountTriggerConditionDefinition = 1173,
				Kampai_Game_Trigger_SocialOrderTriggerConditionDefinition = 1174,
				Kampai_Game_Trigger_SocialTimeTriggerConditionDefinition = 1175,
				Kampai_Game_Trigger_StorageTriggerConditionDefinition = 1176,
				Kampai_Game_Trigger_CaptainTeaseTriggerRewardDefinition = 1177,
				Kampai_Game_Trigger_TriggerRewardDefinition = 1178,
				Kampai_Game_Trigger_MignetteScoreTriggerRewardDefinition = 1179,
				Kampai_Game_Trigger_PartyPointsTriggerRewardDefinition = 1180,
				Kampai_Game_Trigger_QuantityItemTriggerRewardDefinition = 1181,
				Kampai_Game_Trigger_SaleItemTriggerRewardDefinition = 1182,
				Kampai_Game_Trigger_SaleSlotTriggerRewardDefinition = 1183,
				Kampai_Game_Trigger_SocialOrderTriggerRewardDefinition = 1184,
				Kampai_Game_Trigger_TSMMessageTriggerRewardDefinition = 1185,
				Kampai_Game_Trigger_UpsellTriggerRewardDefinition = 1186,
				Kampai_Game_VillainDefinition = 1187,
				Kampai_Game_WayFinderDefinition = 1188,
				Kampai_Game_PetsXPromoDefinition = 1189,
				Kampai_Game_Definitions = 1190,
				Kampai_Splash_LoadinTipBucketDefinition = 1191,
				Kampai_Splash_LoadInTipDefinition = 1192,
				Kampai_Main_HindsightCampaignDefinition = 1193
			}

			public static IBinarySerializable CreateInstance(int typeCode)
			{
				switch (typeCode)
				{
				case 1001:
					return new DefinitionGroup();
				case 1002:
					return new DropLevelBandDefinition();
				case 1003:
					return new HelpTipDefinition();
				case 1004:
					return new LegalDocumentDefinition();
				case 1005:
					return new LevelFunTable();
				case 1006:
					return new PartyUpDefinition();
				case 1007:
					return new TransactionDefinition();
				case 1008:
					return new QuantityItem();
				case 1009:
					return new LevelUpDefinition();
				case 1010:
					return new LevelXPTable();
				case 1011:
					return new NotificationDefinition();
				case 1012:
					return new NotificationSystemDefinition();
				case 1013:
					return new PrestigeDefinition();
				case 1014:
					return new TaxonomyDefinition();
				case 1015:
					return new DisplayableDefinition();
				case 1016:
					return new AchievementDefinition();
				case 1017:
					return new PendingRewardDefinition();
				case 1018:
					return new RewardedAdvertisementDefinition();
				case 1019:
					return new AdPlacementDefinition();
				case 1021:
					return new OnTheGlassDailyRewardDefinition();
				case 1022:
					return new WeightedDefinition();
				case 1023:
					return new WeightedQuantityItem();
				case 1024:
					return new CraftingRushRewardDefinition();
				case 1025:
					return new MarketplaceRefreshRushRewardDefinition();
				case 1026:
					return new MissingResourcesRewardDefinition();
				case 1027:
					return new OrderboardFillOrderRewardDefinition();
				case 1028:
					return new Quest2xRewardDefinition();
				case 1029:
					return new OfferwallPlacementDefinition();
				case 1031:
					return new BuildingAnimationDefinition();
				case 1032:
					return new AnimationDefinition();
				case 1035:
					return new BlackMarketBoardDefinition();
				case 1036:
					return new BlackMarketBoardUnlockedOrderSlotDefinition();
				case 1037:
					return new BlackMarketBoardSlotDefinition();
				case 1038:
					return new BlackMarketBoardMultiplierDefinition();
				case 1039:
					return new BridgeBuildingDefinition();
				case 1040:
					return new CabanaBuildingDefinition();
				case 1041:
					return new CompositeBuildingDefinition();
				case 1042:
					return new CompositeBuildingPieceDefinition();
				case 1043:
					return new ConnectableBuildingDefinition();
				case 1044:
					return new DecorationBuildingDefinition();
				case 1045:
					return new CraftingBuildingDefinition();
				case 1046:
					return new RecipeDefinition();
				case 1047:
					return new DCNBuildingDefinition();
				case 1048:
					return new DebrisBuildingDefinition();
				case 1050:
					return new FountainBuildingDefinition();
				case 1051:
					return new LandExpansionBuildingDefinition();
				case 1052:
					return new LeisureBuildingDefintiion();
				case 1053:
					return new MIBBuildingDefinition();
				case 1054:
					return new MasterPlanComponentBuildingDefinition();
				case 1055:
					return new MasterPlanLeftOverBuildingDefinition();
				case 1056:
					return new MignetteBuildingDefinition();
				case 1058:
					return new MinionUpgradeBuildingDefinition();
				case 1059:
					return new PurchasedLandExpansionDefinition();
				case 1060:
					return new ResourceBuildingDefinition();
				case 1061:
					return new SpecialBuildingDefinition();
				case 1062:
					return new StageBuildingDefinition();
				case 1063:
					return new StorageBuildingDefinition();
				case 1065:
					return new TikiBarBuildingDefinition();
				case 1066:
					return new VillainLairDefinition();
				case 1067:
					return new VillainLairEntranceBuildingDefinition();
				case 1068:
					return new VillainLairResourcePlotDefinition();
				case 1069:
					return new WelcomeHutBuildingDefinition();
				case 1070:
					return new CameraDefinition();
				case 1071:
					return new CustomCameraPositionDefinition();
				case 1072:
					return new BigThreeCharacterDefinition();
				case 1074:
					return new NamedCharacterAnimationDefinition();
				case 1075:
					return new BobCharacterDefinition();
				case 1076:
					return new MinionAnimationDefinition();
				case 1078:
					return new LocationIncidentalAnimationDefinition();
				case 1079:
					return new KevinCharacterDefinition();
				case 1080:
					return new PhilCharacterDefinition();
				case 1081:
					return new SpecialEventCharacterDefinition();
				case 1082:
					return new StuartCharacterDefinition();
				case 1083:
					return new TSMCharacterDefinition();
				case 1084:
					return new UIAnimationDefinition();
				case 1085:
					return new RewardCollectionDefinition();
				case 1086:
					return new EnvironmentDefinition();
				case 1087:
					return new FlyOverDefinition();
				case 1088:
					return new BridgeDefinition();
				case 1089:
					return new ItemDefinition();
				case 1090:
					return new CostumeItemDefinition();
				case 1091:
					return new DropItemDefinition();
				case 1092:
					return new DynamicIngredientsDefinition();
				case 1093:
					return new IngredientsItemDefinition();
				case 1094:
					return new PartyFavorAnimationItemDefinition();
				case 1095:
					return new SpecialEventItemDefinition();
				case 1096:
					return new AspirationalBuildingDefinition();
				case 1097:
					return new CommonLandExpansionDefinition();
				case 1098:
					return new DebrisDefinition();
				case 1099:
					return new LandExpansionConfig();
				case 1100:
					return new LandExpansionDefinition();
				case 1101:
					return new LocalizedTextDefinition();
				case 1102:
					return new MarketplaceDefinition();
				case 1103:
					return new MarketplaceItemDefinition();
				case 1104:
					return new MarketplaceSaleSlotDefinition();
				case 1105:
					return new MarketplaceRefreshTimerDefinition();
				case 1106:
					return new DynamicMasterPlanDefinition();
				case 1107:
					return new MasterPlanComponentDefinition();
				case 1108:
					return new MasterPlanDefinition();
				case 1109:
					return new MasterPlanOnboardDefinition();
				case 1110:
					return new GachaAnimationDefinition();
				case 1111:
					return new GachaWeightedDefinition();
				case 1112:
					return new MinionDefinition();
				case 1113:
					return new PartyFavorAnimationDefinition();
				case 1114:
					return new BuffDefinition();
				case 1115:
					return new GuestOfHonorDefinition();
				case 1116:
					return new MinionPartyDefinition();
				case 1117:
					return new PartyMeterDefinition();
				case 1118:
					return new PartyMeterTierDefinition();
				case 1119:
					return new MinionPartyLevelBandDefinition();
				case 1120:
					return new PartyPointsPerLevelDefinition();
				case 1121:
					return new MinionBenefitLevelBandDefintion();
				case 1122:
					return new PopulationBenefitDefinition();
				case 1123:
					return new PlayerTrainingCardDefinition();
				case 1124:
					return new PlayerTrainingCategoryDefinition();
				case 1125:
					return new PlayerTrainingDefinition();
				case 1126:
					return new FootprintDefinition();
				case 1127:
					return new NoOpPlotDefinition();
				case 1129:
					return new DynamicQuestDefinition();
				case 1130:
					return new QuestDefinition();
				case 1131:
					return new LimitedQuestDefinition();
				case 1132:
					return new QuestChainDefinition();
				case 1133:
					return new QuestResourceDefinition();
				case 1134:
					return new TimedQuestDefinition();
				case 1135:
					return new RushTimeBandDefinition();
				case 1136:
					return new SalePackDefinition();
				case 1137:
					return new PackDefinition();
				case 1138:
					return new PremiumCurrencyItemDefinition();
				case 1139:
					return new CurrencyItemDefinition();
				case 1140:
					return new SocialSettingsDefinition();
				case 1141:
					return new StickerDefinition();
				case 1142:
					return new CurrencyStoreCategoryDefinition();
				case 1143:
					return new CurrencyStoreDefinition();
				case 1144:
					return new CurrencyStorePackDefinition();
				case 1145:
					return new StoreItemDefinition();
				case 1146:
					return new TaskLevelBandDefinition();
				case 1147:
					return new TimedSocialEventDefinition();
				case 1148:
					return new UnlockDefinition();
				case 1149:
					return new TSMTriggerDefinition();
				case 1151:
					return new UpsellTriggerDefinition();
				case 1152:
					return new AvailableLandTriggerConditionDefinition();
				case 1153:
					return new ChurnTriggerConditionDefinition();
				case 1154:
					return new ConsecutiveDaysConditionDefinition();
				case 1155:
					return new CountryTriggerConditionDefinition();
				case 1156:
					return new DaysSinceInstallTriggerConditionDefinition();
				case 1157:
					return new HelpButtonTriggerConditionDefinition();
				case 1158:
					return new HoursPlayedTriggerConditionDefinition();
				case 1159:
					return new LandExpansionTriggerConditionDefinition();
				case 1160:
					return new MignetteScoreTriggerConditionDefinition();
				case 1161:
					return new OrderBoardTriggerConditionDefinition();
				case 1162:
					return new PlatformTriggerConditionDefinition();
				case 1163:
					return new PrestigeLevelTriggerConditionDefinition();
				case 1165:
					return new PrestigeStateTriggerConditionDefinition();
				case 1166:
					return new PrestigeTriggerConditionDefinition();
				case 1167:
					return new PurchaseTriggerConditionDefinition();
				case 1168:
					return new QuantityItemTriggerConditionDefinition();
				case 1169:
					return new QuestTriggerConditionDefinition();
				case 1170:
					return new SaleItemTriggerConditionDefinition();
				case 1171:
					return new SaleSlotTriggerConditionDefinition();
				case 1172:
					return new SegmentTriggerConditionDefinition();
				case 1173:
					return new SessionCountTriggerConditionDefinition();
				case 1174:
					return new SocialOrderTriggerConditionDefinition();
				case 1175:
					return new SocialTimeTriggerConditionDefinition();
				case 1176:
					return new StorageTriggerConditionDefinition();
				case 1177:
					return new CaptainTeaseTriggerRewardDefinition();
				case 1179:
					return new MignetteScoreTriggerRewardDefinition();
				case 1180:
					return new PartyPointsTriggerRewardDefinition();
				case 1181:
					return new QuantityItemTriggerRewardDefinition();
				case 1182:
					return new SaleItemTriggerRewardDefinition();
				case 1183:
					return new SaleSlotTriggerRewardDefinition();
				case 1184:
					return new SocialOrderTriggerRewardDefinition();
				case 1185:
					return new TSMMessageTriggerRewardDefinition();
				case 1186:
					return new UpsellTriggerRewardDefinition();
				case 1187:
					return new VillainDefinition();
				case 1188:
					return new WayFinderDefinition();
				case 1189:
					return new PetsXPromoDefinition();
				case 1190:
					return new Definitions();
				case 1191:
					return new LoadinTipBucketDefinition();
				case 1192:
					return new LoadInTipDefinition();
				case 1193:
					return new HindsightCampaignDefinition();
				default:
					throw new BinarySerializationException(string.Format("BinarySerializationFactory.CreateInstance: unsupported type code {0}", typeCode));
				}
			}
		}

		private const bool NULL_MARKER = true;

		private const bool NOT_NULL_MARKER = false;

		public static void WriteLegalDocumentURL(BinaryWriter writer, LegalDocumentURL instance)
		{
			WriteString(writer, instance.language);
			WriteString(writer, instance.url);
		}

		public static LegalDocumentURL ReadLegalDocumentURL(BinaryReader reader)
		{
			LegalDocumentURL result = default(LegalDocumentURL);
			result.language = ReadString(reader);
			result.url = ReadString(reader);
			return result;
		}

		public static void WriteNotificationReminder(BinaryWriter writer, NotificationReminder instance)
		{
			writer.Write(instance.level);
			WriteString(writer, instance.messageLocalizedKey);
		}

		public static NotificationReminder ReadNotificationReminder(BinaryReader reader)
		{
			NotificationReminder result = default(NotificationReminder);
			result.level = reader.ReadInt32();
			result.messageLocalizedKey = ReadString(reader);
			return result;
		}

		public static void WriteCharacterPrestigeLevelDefinition(BinaryWriter writer, CharacterPrestigeLevelDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.UnlockLevel);
				writer.Write(instance.UnlockQuestID);
				writer.Write(instance.PointsNeeded);
				writer.Write(instance.AttachedQuestID);
				WriteString(writer, instance.WelcomePanelMessageLocalizedKey);
				WriteString(writer, instance.FarewellPanelMessageLocalizedKey);
			}
		}

		public static CharacterPrestigeLevelDefinition ReadCharacterPrestigeLevelDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			CharacterPrestigeLevelDefinition characterPrestigeLevelDefinition = new CharacterPrestigeLevelDefinition();
			characterPrestigeLevelDefinition.UnlockLevel = reader.ReadUInt32();
			characterPrestigeLevelDefinition.UnlockQuestID = reader.ReadInt32();
			characterPrestigeLevelDefinition.PointsNeeded = reader.ReadUInt32();
			characterPrestigeLevelDefinition.AttachedQuestID = reader.ReadInt32();
			characterPrestigeLevelDefinition.WelcomePanelMessageLocalizedKey = ReadString(reader);
			characterPrestigeLevelDefinition.FarewellPanelMessageLocalizedKey = ReadString(reader);
			return characterPrestigeLevelDefinition;
		}

		public static void WriteAchievementID(BinaryWriter writer, AchievementID instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.GameCenterID);
				WriteString(writer, instance.GooglePlayID);
			}
		}

		public static AchievementID ReadAchievementID(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			AchievementID achievementID = new AchievementID();
			achievementID.GameCenterID = ReadString(reader);
			achievementID.GooglePlayID = ReadString(reader);
			return achievementID;
		}

		public static void WriteRewardTiers(BinaryWriter writer, RewardTiers instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteOnTheGlassDailyRewardTier(writer, instance.Tier1);
				WriteOnTheGlassDailyRewardTier(writer, instance.Tier2);
				WriteOnTheGlassDailyRewardTier(writer, instance.Tier3);
			}
		}

		public static RewardTiers ReadRewardTiers(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			RewardTiers rewardTiers = new RewardTiers();
			rewardTiers.Tier1 = ReadOnTheGlassDailyRewardTier(reader);
			rewardTiers.Tier2 = ReadOnTheGlassDailyRewardTier(reader);
			rewardTiers.Tier3 = ReadOnTheGlassDailyRewardTier(reader);
			return rewardTiers;
		}

		public static void WriteOnTheGlassDailyRewardTier(BinaryWriter writer, OnTheGlassDailyRewardTier instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.Weight);
				WriteObject(writer, instance.PredefinedRewards);
				writer.Write(instance.CraftableRewardMinTier);
				writer.Write(instance.CraftableRewardMaxQuantity);
				writer.Write(instance.CraftableRewardWeight);
			}
		}

		public static OnTheGlassDailyRewardTier ReadOnTheGlassDailyRewardTier(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			OnTheGlassDailyRewardTier onTheGlassDailyRewardTier = new OnTheGlassDailyRewardTier();
			onTheGlassDailyRewardTier.Weight = reader.ReadInt32();
			onTheGlassDailyRewardTier.PredefinedRewards = ReadObject<WeightedDefinition>(reader);
			onTheGlassDailyRewardTier.CraftableRewardMinTier = reader.ReadInt32();
			onTheGlassDailyRewardTier.CraftableRewardMaxQuantity = reader.ReadInt32();
			onTheGlassDailyRewardTier.CraftableRewardWeight = reader.ReadInt32();
			return onTheGlassDailyRewardTier;
		}

		public static void WriteScreenPosition(BinaryWriter writer, ScreenPosition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.z);
				writer.Write(instance.zoom);
			}
		}

		public static ScreenPosition ReadScreenPosition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			ScreenPosition screenPosition = new ScreenPosition();
			screenPosition.x = reader.ReadSingle();
			screenPosition.z = reader.ReadSingle();
			screenPosition.zoom = reader.ReadSingle();
			return screenPosition;
		}

		public static void WriteVector3(BinaryWriter writer, Vector3 instance)
		{
			writer.Write(instance.x);
			writer.Write(instance.y);
			writer.Write(instance.z);
		}

		public static Vector3 ReadVector3(BinaryReader reader)
		{
			Vector3 result = default(Vector3);
			result.x = reader.ReadSingle();
			result.y = reader.ReadSingle();
			result.z = reader.ReadSingle();
			return result;
		}

		public static void WriteConnectablePiecePrefabDefinition(BinaryWriter writer, ConnectablePiecePrefabDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.straight);
				WriteString(writer, instance.cross);
				WriteString(writer, instance.post);
				WriteString(writer, instance.tshape);
				WriteString(writer, instance.endcap);
				WriteString(writer, instance.corner);
			}
		}

		public static ConnectablePiecePrefabDefinition ReadConnectablePiecePrefabDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			ConnectablePiecePrefabDefinition connectablePiecePrefabDefinition = new ConnectablePiecePrefabDefinition();
			connectablePiecePrefabDefinition.straight = ReadString(reader);
			connectablePiecePrefabDefinition.cross = ReadString(reader);
			connectablePiecePrefabDefinition.post = ReadString(reader);
			connectablePiecePrefabDefinition.tshape = ReadString(reader);
			connectablePiecePrefabDefinition.endcap = ReadString(reader);
			connectablePiecePrefabDefinition.corner = ReadString(reader);
			return connectablePiecePrefabDefinition;
		}

		public static void WriteSlotUnlock(BinaryWriter writer, SlotUnlock instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteListInt32(writer, instance.SlotUnlockLevels);
				WriteListInt32(writer, instance.SlotUnlockCosts);
			}
		}

		public static SlotUnlock ReadSlotUnlock(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			SlotUnlock slotUnlock = new SlotUnlock();
			slotUnlock.SlotUnlockLevels = ReadListInt32(reader, slotUnlock.SlotUnlockLevels);
			slotUnlock.SlotUnlockCosts = ReadListInt32(reader, slotUnlock.SlotUnlockCosts);
			return slotUnlock;
		}

		public static void WriteUserSegment(BinaryWriter writer, UserSegment instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.LevelGreaterThanOrEqualTo);
				writer.Write(instance.FirstXReturnRewardsWeightedDefinitionId);
				writer.Write(instance.SecondXReturnRewardsWeightedDefinitionId);
				writer.Write(instance.AfterXReturnRewards);
			}
		}

		public static UserSegment ReadUserSegment(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			UserSegment userSegment = new UserSegment();
			userSegment.LevelGreaterThanOrEqualTo = reader.ReadInt32();
			userSegment.FirstXReturnRewardsWeightedDefinitionId = reader.ReadInt32();
			userSegment.SecondXReturnRewardsWeightedDefinitionId = reader.ReadInt32();
			userSegment.AfterXReturnRewards = reader.ReadInt32();
			return userSegment;
		}

		public static void WriteLocation(BinaryWriter writer, Location instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.y);
			}
		}

		public static Location ReadLocation(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			Location location = new Location();
			location.x = reader.ReadInt32();
			location.y = reader.ReadInt32();
			return location;
		}

		public static void WriteMignetteRuleDefinition(BinaryWriter writer, MignetteRuleDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.CauseImage);
				WriteString(writer, instance.CauseImageMask);
				WriteString(writer, instance.EffectImage);
				WriteString(writer, instance.EffectImageMask);
				writer.Write(instance.EffectAmount);
			}
		}

		public static MignetteRuleDefinition ReadMignetteRuleDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MignetteRuleDefinition mignetteRuleDefinition = new MignetteRuleDefinition();
			mignetteRuleDefinition.CauseImage = ReadString(reader);
			mignetteRuleDefinition.CauseImageMask = ReadString(reader);
			mignetteRuleDefinition.EffectImage = ReadString(reader);
			mignetteRuleDefinition.EffectImageMask = ReadString(reader);
			mignetteRuleDefinition.EffectAmount = reader.ReadInt32();
			return mignetteRuleDefinition;
		}

		public static void WriteMignetteChildObjectDefinition(BinaryWriter writer, MignetteChildObjectDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.Prefab);
				WriteVector3(writer, instance.Position);
				writer.Write(instance.IsLocal);
				writer.Write(instance.Rotation);
			}
		}

		public static MignetteChildObjectDefinition ReadMignetteChildObjectDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MignetteChildObjectDefinition mignetteChildObjectDefinition = new MignetteChildObjectDefinition();
			mignetteChildObjectDefinition.Prefab = ReadString(reader);
			mignetteChildObjectDefinition.Position = ReadVector3(reader);
			mignetteChildObjectDefinition.IsLocal = reader.ReadBoolean();
			mignetteChildObjectDefinition.Rotation = reader.ReadSingle();
			return mignetteChildObjectDefinition;
		}

		public static void WriteMinionPartyPrefabDefinition(BinaryWriter writer, MinionPartyPrefabDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.EventType);
				WriteString(writer, instance.Prefab);
			}
		}

		public static MinionPartyPrefabDefinition ReadMinionPartyPrefabDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MinionPartyPrefabDefinition minionPartyPrefabDefinition = new MinionPartyPrefabDefinition();
			minionPartyPrefabDefinition.EventType = ReadString(reader);
			minionPartyPrefabDefinition.Prefab = ReadString(reader);
			return minionPartyPrefabDefinition;
		}

		public static void WriteArea(BinaryWriter writer, Area instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteLocation(writer, instance.a);
				WriteLocation(writer, instance.b);
			}
		}

		public static Area ReadArea(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			Area area = new Area();
			area.a = ReadLocation(reader);
			area.b = ReadLocation(reader);
			return area;
		}

		public static void WriteStorageUpgradeDefinition(BinaryWriter writer, StorageUpgradeDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.Level);
				writer.Write(instance.StorageCapacity);
				writer.Write(instance.TransactionId);
			}
		}

		public static StorageUpgradeDefinition ReadStorageUpgradeDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			StorageUpgradeDefinition storageUpgradeDefinition = new StorageUpgradeDefinition();
			storageUpgradeDefinition.Level = reader.ReadInt32();
			storageUpgradeDefinition.StorageCapacity = reader.ReadUInt32();
			storageUpgradeDefinition.TransactionId = reader.ReadInt32();
			return storageUpgradeDefinition;
		}

		public static void WritePlatformDefinition(BinaryWriter writer, PlatformDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.buildingRemovalAnimController);
				writer.Write(instance.customCameraPosID);
				WriteString(writer, instance.description);
				WriteVector3(writer, instance.offset);
				WriteLocation(writer, instance.placementLocation);
			}
		}

		public static PlatformDefinition ReadPlatformDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			PlatformDefinition platformDefinition = new PlatformDefinition();
			platformDefinition.buildingRemovalAnimController = ReadString(reader);
			platformDefinition.customCameraPosID = reader.ReadInt32();
			platformDefinition.description = ReadString(reader);
			platformDefinition.offset = ReadVector3(reader);
			platformDefinition.placementLocation = ReadLocation(reader);
			return platformDefinition;
		}

		public static void WriteResourcePlotDefinition(BinaryWriter writer, ResourcePlotDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.descriptionKey);
				writer.Write(instance.isAutomaticallyUnlocked);
				WriteLocation(writer, instance.location);
				writer.Write(instance.unlockTransactionID);
				writer.Write(instance.rotation);
			}
		}

		public static ResourcePlotDefinition ReadResourcePlotDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			ResourcePlotDefinition resourcePlotDefinition = new ResourcePlotDefinition();
			resourcePlotDefinition.descriptionKey = ReadString(reader);
			resourcePlotDefinition.isAutomaticallyUnlocked = reader.ReadBoolean();
			resourcePlotDefinition.location = ReadLocation(reader);
			resourcePlotDefinition.unlockTransactionID = reader.ReadInt32();
			resourcePlotDefinition.rotation = reader.ReadInt32();
			return resourcePlotDefinition;
		}

		public static void WriteCharacterUIAnimationDefinition(BinaryWriter writer, CharacterUIAnimationDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.StateMachine);
				writer.Write(instance.IdleWeightedAnimationID);
				writer.Write(instance.IdleCount);
				writer.Write(instance.HappyWeightedAnimationID);
				writer.Write(instance.HappyCount);
				writer.Write(instance.SelectedWeightedAnimationID);
				writer.Write(instance.SelectedCount);
				writer.Write(instance.UseLegacy);
			}
		}

		public static CharacterUIAnimationDefinition ReadCharacterUIAnimationDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			CharacterUIAnimationDefinition characterUIAnimationDefinition = new CharacterUIAnimationDefinition();
			characterUIAnimationDefinition.StateMachine = ReadString(reader);
			characterUIAnimationDefinition.IdleWeightedAnimationID = reader.ReadInt32();
			characterUIAnimationDefinition.IdleCount = reader.ReadInt32();
			characterUIAnimationDefinition.HappyWeightedAnimationID = reader.ReadInt32();
			characterUIAnimationDefinition.HappyCount = reader.ReadInt32();
			characterUIAnimationDefinition.SelectedWeightedAnimationID = reader.ReadInt32();
			characterUIAnimationDefinition.SelectedCount = reader.ReadInt32();
			characterUIAnimationDefinition.UseLegacy = reader.ReadBoolean();
			return characterUIAnimationDefinition;
		}

		public static void WriteFloatLocation(BinaryWriter writer, FloatLocation instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.y);
			}
		}

		public static FloatLocation ReadFloatLocation(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			FloatLocation floatLocation = new FloatLocation();
			floatLocation.x = reader.ReadSingle();
			floatLocation.y = reader.ReadSingle();
			return floatLocation;
		}

		public static void WriteAngle(BinaryWriter writer, Angle instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.Degrees);
			}
		}

		public static Angle ReadAngle(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			Angle angle = new Angle();
			angle.Degrees = reader.ReadSingle();
			return angle;
		}

		public static void WriteCollectionReward(BinaryWriter writer, CollectionReward instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.RequiredPoints);
				writer.Write(instance.TransactionID);
			}
		}

		public static CollectionReward ReadCollectionReward(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			CollectionReward collectionReward = new CollectionReward();
			collectionReward.RequiredPoints = reader.ReadInt32();
			collectionReward.TransactionID = reader.ReadInt32();
			return collectionReward;
		}

		public static void WriteFlyOverNode(BinaryWriter writer, FlyOverNode instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.y);
				writer.Write(instance.z);
			}
		}

		public static FlyOverNode ReadFlyOverNode(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			FlyOverNode flyOverNode = new FlyOverNode();
			flyOverNode.x = reader.ReadSingle();
			flyOverNode.y = reader.ReadSingle();
			flyOverNode.z = reader.ReadSingle();
			return flyOverNode;
		}

		public static void WriteBridgeScreenPosition(BinaryWriter writer, BridgeScreenPosition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.y);
				writer.Write(instance.z);
				writer.Write(instance.zoom);
			}
		}

		public static BridgeScreenPosition ReadBridgeScreenPosition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			BridgeScreenPosition bridgeScreenPosition = new BridgeScreenPosition();
			bridgeScreenPosition.x = reader.ReadSingle();
			bridgeScreenPosition.y = reader.ReadSingle();
			bridgeScreenPosition.z = reader.ReadSingle();
			bridgeScreenPosition.zoom = reader.ReadSingle();
			return bridgeScreenPosition;
		}

		public static void WriteKampaiColor(BinaryWriter writer, KampaiColor instance)
		{
			writer.Write(instance.r);
			writer.Write(instance.g);
			writer.Write(instance.b);
			writer.Write(instance.a);
		}

		public static KampaiColor ReadKampaiColor(BinaryReader reader)
		{
			KampaiColor result = default(KampaiColor);
			result.r = reader.ReadSingle();
			result.g = reader.ReadSingle();
			result.b = reader.ReadSingle();
			result.a = reader.ReadSingle();
			return result;
		}

		public static void WriteReward(BinaryWriter writer, Reward instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.requiredQuantity);
				writer.Write(instance.premiumReward);
			}
		}

		public static Reward ReadReward(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			Reward reward = new Reward();
			reward.requiredQuantity = reader.ReadUInt32();
			reward.premiumReward = reader.ReadUInt32();
			return reward;
		}

		public static void WriteMiniGameScoreReward(BinaryWriter writer, MiniGameScoreReward instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.MiniGameId);
				WriteList(writer, WriteReward, instance.rewardTable);
			}
		}

		public static MiniGameScoreReward ReadMiniGameScoreReward(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MiniGameScoreReward miniGameScoreReward = new MiniGameScoreReward();
			miniGameScoreReward.MiniGameId = reader.ReadInt32();
			miniGameScoreReward.rewardTable = ReadList(reader, ReadReward, miniGameScoreReward.rewardTable);
			return miniGameScoreReward;
		}

		public static void WriteMiniGameScoreRange(BinaryWriter writer, MiniGameScoreRange instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.MiniGameId);
				writer.Write(instance.ScoreRangeMax);
				writer.Write(instance.ScoreRangeMin);
			}
		}

		public static MiniGameScoreRange ReadMiniGameScoreRange(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MiniGameScoreRange miniGameScoreRange = new MiniGameScoreRange();
			miniGameScoreRange.MiniGameId = reader.ReadInt32();
			miniGameScoreRange.ScoreRangeMax = reader.ReadInt32();
			miniGameScoreRange.ScoreRangeMin = reader.ReadInt32();
			return miniGameScoreRange;
		}

		public static void WriteMasterPlanComponentRewardDefinition(BinaryWriter writer, MasterPlanComponentRewardDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.rewardItemId);
				writer.Write(instance.rewardQuantity);
				writer.Write(instance.grindReward);
				writer.Write(instance.premiumReward);
			}
		}

		public static MasterPlanComponentRewardDefinition ReadMasterPlanComponentRewardDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MasterPlanComponentRewardDefinition masterPlanComponentRewardDefinition = new MasterPlanComponentRewardDefinition();
			masterPlanComponentRewardDefinition.rewardItemId = reader.ReadInt32();
			masterPlanComponentRewardDefinition.rewardQuantity = reader.ReadUInt32();
			masterPlanComponentRewardDefinition.grindReward = reader.ReadUInt32();
			masterPlanComponentRewardDefinition.premiumReward = reader.ReadUInt32();
			return masterPlanComponentRewardDefinition;
		}

		public static void WriteMasterPlanComponentTaskDefinition(BinaryWriter writer, MasterPlanComponentTaskDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.requiredItemId);
				writer.Write(instance.requiredQuantity);
				writer.Write(instance.ShowWayfinder);
				WriteEnum(writer, instance.Type);
			}
		}

		public static MasterPlanComponentTaskDefinition ReadMasterPlanComponentTaskDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MasterPlanComponentTaskDefinition masterPlanComponentTaskDefinition = new MasterPlanComponentTaskDefinition();
			masterPlanComponentTaskDefinition.requiredItemId = reader.ReadInt32();
			masterPlanComponentTaskDefinition.requiredQuantity = reader.ReadUInt32();
			masterPlanComponentTaskDefinition.ShowWayfinder = reader.ReadBoolean();
			masterPlanComponentTaskDefinition.Type = ReadEnum<MasterPlanComponentTaskType>(reader);
			return masterPlanComponentTaskDefinition;
		}

		public static void WriteGhostFunctionDefinition(BinaryWriter writer, GhostFunctionDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteEnum(writer, instance.startType);
				WriteEnum(writer, instance.closeType);
				writer.Write(instance.componentBuildingDefID);
			}
		}

		public static GhostFunctionDefinition ReadGhostFunctionDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			GhostFunctionDefinition ghostFunctionDefinition = new GhostFunctionDefinition();
			ghostFunctionDefinition.startType = ReadEnum<GhostComponentFunctionType>(reader);
			ghostFunctionDefinition.closeType = ReadEnum<GhostFunctionCloseType>(reader);
			ghostFunctionDefinition.componentBuildingDefID = reader.ReadInt32();
			return ghostFunctionDefinition;
		}

		public static void WriteKnuckleheadednessInfo(BinaryWriter writer, KnuckleheadednessInfo instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.KnuckleheaddednessMin);
				writer.Write(instance.KnuckleheaddednessMax);
				writer.Write(instance.KnuckleheaddednessScale);
			}
		}

		public static KnuckleheadednessInfo ReadKnuckleheadednessInfo(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			KnuckleheadednessInfo knuckleheadednessInfo = new KnuckleheadednessInfo();
			knuckleheadednessInfo.KnuckleheaddednessMin = reader.ReadSingle();
			knuckleheadednessInfo.KnuckleheaddednessMax = reader.ReadSingle();
			knuckleheadednessInfo.KnuckleheaddednessScale = reader.ReadSingle();
			return knuckleheadednessInfo;
		}

		public static void WriteAnimationAlternate(BinaryWriter writer, AnimationAlternate instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.GroupID);
				writer.Write(instance.PercentChance);
			}
		}

		public static AnimationAlternate ReadAnimationAlternate(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			AnimationAlternate animationAlternate = new AnimationAlternate();
			animationAlternate.GroupID = reader.ReadInt32();
			animationAlternate.PercentChance = reader.ReadSingle();
			return animationAlternate;
		}

		public static void WriteCameraControlSettings(BinaryWriter writer, CameraControlSettings instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.customCameraPosTiki);
				writer.Write(instance.customCameraPosStage);
				writer.Write(instance.customCameraPosTownHall);
				writer.Write(instance.customCameraPosPartyDefault);
			}
		}

		public static CameraControlSettings ReadCameraControlSettings(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			CameraControlSettings cameraControlSettings = new CameraControlSettings();
			cameraControlSettings.customCameraPosTiki = reader.ReadInt32();
			cameraControlSettings.customCameraPosStage = reader.ReadInt32();
			cameraControlSettings.customCameraPosTownHall = reader.ReadInt32();
			cameraControlSettings.customCameraPosPartyDefault = reader.ReadInt32();
			return cameraControlSettings;
		}

		public static void WriteVFXAssetDefinition(BinaryWriter writer, VFXAssetDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteLocation(writer, instance.location);
				WriteString(writer, instance.Prefab);
			}
		}

		public static VFXAssetDefinition ReadVFXAssetDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			VFXAssetDefinition vFXAssetDefinition = new VFXAssetDefinition();
			vFXAssetDefinition.location = ReadLocation(reader);
			vFXAssetDefinition.Prefab = ReadString(reader);
			return vFXAssetDefinition;
		}

		public static void WriteMinionBenefit(BinaryWriter writer, MinionBenefit instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.localizedKey);
				writer.Write(instance.itemIconId);
				WriteEnum(writer, instance.type);
			}
		}

		public static MinionBenefit ReadMinionBenefit(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MinionBenefit minionBenefit = new MinionBenefit();
			minionBenefit.localizedKey = ReadString(reader);
			minionBenefit.itemIconId = reader.ReadInt32();
			minionBenefit.type = ReadEnum<Benefit>(reader);
			return minionBenefit;
		}

		public static void WriteMinionBenefitLevel(BinaryWriter writer, MinionBenefitLevel instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.doubleDropPercentage);
				writer.Write(instance.doubleDropLevel);
				writer.Write(instance.premiumDropPercentage);
				writer.Write(instance.premiumDropLevel);
				writer.Write(instance.rareDropPercentage);
				writer.Write(instance.rareDropLevel);
				writer.Write(instance.tokensToLevel);
				writer.Write(instance.costumeId);
				WriteString(writer, instance.image);
				WriteString(writer, instance.mask);
			}
		}

		public static MinionBenefitLevel ReadMinionBenefitLevel(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			MinionBenefitLevel minionBenefitLevel = new MinionBenefitLevel();
			minionBenefitLevel.doubleDropPercentage = reader.ReadSingle();
			minionBenefitLevel.doubleDropLevel = reader.ReadInt32();
			minionBenefitLevel.premiumDropPercentage = reader.ReadSingle();
			minionBenefitLevel.premiumDropLevel = reader.ReadInt32();
			minionBenefitLevel.rareDropPercentage = reader.ReadSingle();
			minionBenefitLevel.rareDropLevel = reader.ReadInt32();
			minionBenefitLevel.tokensToLevel = reader.ReadInt32();
			minionBenefitLevel.costumeId = reader.ReadInt32();
			minionBenefitLevel.image = ReadString(reader);
			minionBenefitLevel.mask = ReadString(reader);
			return minionBenefitLevel;
		}

		public static void WriteImageMaskCombo(BinaryWriter writer, ImageMaskCombo instance)
		{
			WriteString(writer, instance.image);
			WriteString(writer, instance.mask);
		}

		public static ImageMaskCombo ReadImageMaskCombo(BinaryReader reader)
		{
			ImageMaskCombo result = default(ImageMaskCombo);
			result.image = ReadString(reader);
			result.mask = ReadString(reader);
			return result;
		}

		public static void WriteTransactionInstance(BinaryWriter writer, TransactionInstance instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.ID);
				WriteList(writer, instance.Inputs);
				WriteList(writer, instance.Outputs);
			}
		}

		public static TransactionInstance ReadTransactionInstance(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			TransactionInstance transactionInstance = new TransactionInstance();
			transactionInstance.ID = reader.ReadInt32();
			transactionInstance.Inputs = ReadList(reader, transactionInstance.Inputs);
			transactionInstance.Outputs = ReadList(reader, transactionInstance.Outputs);
			return transactionInstance;
		}

		public static void WriteQuestStepDefinition(BinaryWriter writer, QuestStepDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteEnum(writer, instance.Type);
				writer.Write(instance.ItemAmount);
				writer.Write(instance.ItemDefinitionID);
				writer.Write(instance.CostumeDefinitionID);
				writer.Write(instance.ShowWayfinder);
				writer.Write(instance.QuestStepCompletePlayerTrainingCategoryItemId);
				writer.Write(instance.UpgradeLevel);
			}
		}

		public static QuestStepDefinition ReadQuestStepDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			QuestStepDefinition questStepDefinition = new QuestStepDefinition();
			questStepDefinition.Type = ReadEnum<QuestStepType>(reader);
			questStepDefinition.ItemAmount = reader.ReadInt32();
			questStepDefinition.ItemDefinitionID = reader.ReadInt32();
			questStepDefinition.CostumeDefinitionID = reader.ReadInt32();
			questStepDefinition.ShowWayfinder = reader.ReadBoolean();
			questStepDefinition.QuestStepCompletePlayerTrainingCategoryItemId = reader.ReadInt32();
			questStepDefinition.UpgradeLevel = reader.ReadInt32();
			return questStepDefinition;
		}

		public static void WriteQuestChainStepDefinition(BinaryWriter writer, QuestChainStepDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.Intro);
				WriteString(writer, instance.Voice);
				WriteString(writer, instance.Outro);
				writer.Write(instance.XP);
				writer.Write(instance.Grind);
				writer.Write(instance.Premium);
				WriteList(writer, WriteQuestChainTask, instance.Tasks);
			}
		}

		public static QuestChainStepDefinition ReadQuestChainStepDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			QuestChainStepDefinition questChainStepDefinition = new QuestChainStepDefinition();
			questChainStepDefinition.Intro = ReadString(reader);
			questChainStepDefinition.Voice = ReadString(reader);
			questChainStepDefinition.Outro = ReadString(reader);
			questChainStepDefinition.XP = reader.ReadInt32();
			questChainStepDefinition.Grind = reader.ReadInt32();
			questChainStepDefinition.Premium = reader.ReadInt32();
			questChainStepDefinition.Tasks = ReadList(reader, ReadQuestChainTask, questChainStepDefinition.Tasks);
			return questChainStepDefinition;
		}

		public static void WriteQuestChainTask(BinaryWriter writer, QuestChainTask instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteEnum(writer, instance.Type);
				writer.Write(instance.Item);
				writer.Write(instance.Count);
			}
		}

		public static QuestChainTask ReadQuestChainTask(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			QuestChainTask questChainTask = new QuestChainTask();
			questChainTask.Type = ReadEnum<QuestChainTaskType>(reader);
			questChainTask.Item = reader.ReadInt32();
			questChainTask.Count = reader.ReadInt32();
			return questChainTask;
		}

		public static void WritePlatformStoreSkuDefinition(BinaryWriter writer, PlatformStoreSkuDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteString(writer, instance.appleAppstore);
				WriteString(writer, instance.googlePlay);
				WriteString(writer, instance.defaultStore);
			}
		}

		public static PlatformStoreSkuDefinition ReadPlatformStoreSkuDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			PlatformStoreSkuDefinition platformStoreSkuDefinition = new PlatformStoreSkuDefinition();
			platformStoreSkuDefinition.appleAppstore = ReadString(reader);
			platformStoreSkuDefinition.googlePlay = ReadString(reader);
			platformStoreSkuDefinition.defaultStore = ReadString(reader);
			return platformStoreSkuDefinition;
		}

		public static void WriteVector3Serialize(BinaryWriter writer, Vector3Serialize instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.x);
				writer.Write(instance.y);
				writer.Write(instance.z);
			}
		}

		public static Vector3Serialize ReadVector3Serialize(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			Vector3Serialize vector3Serialize = new Vector3Serialize();
			vector3Serialize.x = reader.ReadInt32();
			vector3Serialize.y = reader.ReadInt32();
			vector3Serialize.z = reader.ReadInt32();
			return vector3Serialize;
		}

		public static void WriteSocialEventOrderDefinition(BinaryWriter writer, SocialEventOrderDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.OrderID);
				writer.Write(instance.Transaction);
			}
		}

		public static SocialEventOrderDefinition ReadSocialEventOrderDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			SocialEventOrderDefinition socialEventOrderDefinition = new SocialEventOrderDefinition();
			socialEventOrderDefinition.OrderID = reader.ReadInt32();
			socialEventOrderDefinition.Transaction = reader.ReadInt32();
			return socialEventOrderDefinition;
		}

		public static void WriteTriggerRewardLayout(BinaryWriter writer, TriggerRewardLayout instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.index);
				WriteListInt32(writer, instance.itemIds);
				WriteEnum(writer, instance.layout);
			}
		}

		public static TriggerRewardLayout ReadTriggerRewardLayout(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			TriggerRewardLayout triggerRewardLayout = new TriggerRewardLayout();
			triggerRewardLayout.index = reader.ReadInt32();
			triggerRewardLayout.itemIds = ReadListInt32(reader, triggerRewardLayout.itemIds);
			triggerRewardLayout.layout = ReadEnum<TriggerRewardLayout.Layout>(reader);
			return triggerRewardLayout;
		}

		public static void WriteGachaConfig(BinaryWriter writer, GachaConfig instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteList(writer, instance.GatchaAnimationDefinitions);
				WriteList(writer, instance.DistributionTables);
			}
		}

		public static GachaConfig ReadGachaConfig(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			GachaConfig gachaConfig = new GachaConfig();
			gachaConfig.GatchaAnimationDefinitions = ReadList(reader, gachaConfig.GatchaAnimationDefinitions);
			gachaConfig.DistributionTables = ReadList(reader, gachaConfig.DistributionTables);
			return gachaConfig;
		}

		public static void WriteTaskDefinition(BinaryWriter writer, TaskDefinition instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				WriteList(writer, instance.levelBands);
			}
		}

		public static TaskDefinition ReadTaskDefinition(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			TaskDefinition taskDefinition = new TaskDefinition();
			taskDefinition.levelBands = ReadList(reader, taskDefinition.levelBands);
			return taskDefinition;
		}

		public static void WritePlayerVersion(BinaryWriter writer, PlayerVersion instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.Version);
			}
		}

		public static PlayerVersion ReadPlayerVersion(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			PlayerVersion playerVersion = new PlayerVersion();
			playerVersion.Version = reader.ReadInt32();
			return playerVersion;
		}

		public static void WriteBucketAssignment(BinaryWriter writer, BucketAssignment instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.BucketId);
				writer.Write(instance.Time);
			}
		}

		public static BucketAssignment ReadBucketAssignment(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			BucketAssignment bucketAssignment = new BucketAssignment();
			bucketAssignment.BucketId = reader.ReadInt32();
			bucketAssignment.Time = reader.ReadSingle();
			return bucketAssignment;
		}

		private static bool WriteNullMarker(BinaryWriter writer, object instance)
		{
			if (instance == null)
			{
				writer.Write(true);
				return true;
			}
			writer.Write(false);
			return false;
		}

		private static bool ReadNullMarker(BinaryReader reader)
		{
			if (reader.ReadBoolean())
			{
				return true;
			}
			return false;
		}

		public static void WriteObject(BinaryWriter writer, IBinarySerializable instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance.TypeCode);
				instance.Write(writer);
			}
		}

		public static T ReadObject<T>(BinaryReader reader, T existing_instance = null) where T : class, IBinarySerializable
		{
			if (ReadNullMarker(reader))
			{
				return (T)null;
			}
			int typeCode = reader.ReadInt32();
			T result = existing_instance ?? ((T)BinarySerializationFactory.CreateInstance(typeCode));
			result.Read(reader);
			return result;
		}

		public static T ReadObjectFactory<T>(BinaryReader reader, Func<BinaryReader, T> readFactory, T existing_instance = null) where T : class, IBinarySerializable
		{
			if (ReadNullMarker(reader))
			{
				return (T)null;
			}
			T result = readFactory(reader);
			if (existing_instance != null)
			{
				result = existing_instance;
			}
			result.Read(reader);
			return result;
		}

		public static void WriteList<T>(BinaryWriter writer, Action<BinaryWriter, T> elementWriter, IList<T> list)
		{
			if (!WriteNullMarker(writer, list))
			{
				int count = list.Count;
				writer.Write(count);
				for (int i = 0; i < list.Count; i++)
				{
					T arg = list[i];
					elementWriter(writer, arg);
				}
			}
		}

		public static List<T> ReadList<T>(BinaryReader reader, Func<BinaryReader, T> elementReader, IEnumerable<T> existingValue = null)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			List<T> list = ((existingValue == null) ? new List<T>(num) : new List<T>(existingValue));
			for (int i = 0; i < num; i++)
			{
				T item = elementReader(reader);
				list.Add(item);
			}
			return list;
		}

		public static void WriteListInt32(BinaryWriter writer, IList<int> list)
		{
			if (!WriteNullMarker(writer, list))
			{
				int count = list.Count;
				writer.Write(count);
				for (int i = 0; i < list.Count; i++)
				{
					int value = list[i];
					writer.Write(value);
				}
			}
		}

		public static List<int> ReadListInt32(BinaryReader reader, IEnumerable<int> existingValue = null)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			List<int> list = ((existingValue == null) ? new List<int>(num) : new List<int>(existingValue));
			for (int i = 0; i < num; i++)
			{
				int item = reader.ReadInt32();
				list.Add(item);
			}
			return list;
		}

		public static void WriteListSingle(BinaryWriter writer, IList<float> list)
		{
			if (!WriteNullMarker(writer, list))
			{
				int count = list.Count;
				writer.Write(count);
				for (int i = 0; i < list.Count; i++)
				{
					float value = list[i];
					writer.Write(value);
				}
			}
		}

		public static List<float> ReadListSingle(BinaryReader reader, IEnumerable<float> existingValue = null)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			List<float> list = ((existingValue == null) ? new List<float>(num) : new List<float>(existingValue));
			for (int i = 0; i < num; i++)
			{
				float item = reader.ReadSingle();
				list.Add(item);
			}
			return list;
		}

		public static void WriteList<T>(BinaryWriter writer, IList<T> list) where T : class, IBinarySerializable
		{
			if (!WriteNullMarker(writer, list))
			{
				int count = list.Count;
				writer.Write(count);
				for (int i = 0; i < list.Count; i++)
				{
					T instance = list[i];
					WriteObject(writer, instance);
				}
			}
		}

		public static List<T> ReadList<T>(BinaryReader reader, IEnumerable<T> existingValue = null) where T : class, IBinarySerializable
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			List<T> list = ((existingValue == null) ? new List<T>(num) : new List<T>(existingValue));
			for (int i = 0; i < num; i++)
			{
				T item = ReadObject(reader, (T)null);
				list.Add(item);
			}
			return list;
		}

		public static void WriteDictionary<K, V>(BinaryWriter writer, Action<BinaryWriter, K> keyWriter, Action<BinaryWriter, V> valueWriter, Dictionary<K, V> dictionary)
		{
			if (WriteNullMarker(writer, dictionary))
			{
				return;
			}
			int count = dictionary.Count;
			writer.Write(count);
			foreach (KeyValuePair<K, V> item in dictionary)
			{
				keyWriter(writer, item.Key);
				valueWriter(writer, item.Value);
			}
		}

		public static Dictionary<K, V> ReadDictionary<K, V>(BinaryReader reader, Func<BinaryReader, K> keyReader, Func<BinaryReader, V> valueReader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			Dictionary<K, V> dictionary = new Dictionary<K, V>(num);
			for (int i = 0; i < num; i++)
			{
				K key = keyReader(reader);
				V value = valueReader(reader);
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		public static void WriteString(BinaryWriter writer, string instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				writer.Write(instance);
			}
		}

		public static string ReadString(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			return reader.ReadString();
		}

		public static void WriteObject(BinaryWriter writer, object instance)
		{
			if (!WriteNullMarker(writer, instance))
			{
				TypeCode typeCode = Type.GetTypeCode(instance.GetType());
				writer.Write((int)typeCode);
				switch (typeCode)
				{
				case TypeCode.Boolean:
					writer.Write((bool)instance);
					break;
				case TypeCode.Int32:
					writer.Write((int)instance);
					break;
				case TypeCode.Int64:
					writer.Write((long)instance);
					break;
				case TypeCode.String:
					writer.Write((string)instance);
					break;
				default:
					throw new ArgumentException(string.Format("WriteObject: type {0} is not supported, instance {1}", instance.GetType(), instance));
				}
			}
		}

		public static object ReadObject(BinaryReader reader)
		{
			if (ReadNullMarker(reader))
			{
				return null;
			}
			int num = reader.ReadInt32();
			switch (num)
			{
			case 3:
				return reader.ReadBoolean();
			case 9:
				return reader.ReadInt32();
			case 11:
				return reader.ReadInt64();
			case 18:
				return reader.ReadString();
			default:
				throw new ArgumentException(string.Format("ReadObject: type code {0} is not supported", num));
			}
		}

		public static void WriteDictionary(BinaryWriter writer, Dictionary<string, object> dictionary)
		{
			WriteDictionary(writer, WriteString, WriteObject, dictionary);
		}

		public static Dictionary<string, object> ReadDictionary(BinaryReader reader)
		{
			return ReadDictionary(reader, ReadString, ReadObject);
		}

		public static void WriteListString(BinaryWriter writer, IList<string> list)
		{
			WriteList(writer, WriteString, list);
		}

		public static List<string> ReadListString(BinaryReader reader, IList<string> existingValue)
		{
			return ReadList(reader, ReadString, existingValue);
		}

		public static void WriteEnum<T>(BinaryWriter writer, T value) where T : struct, IConvertible
		{
			int value2 = value.ToInt32(CultureInfo.InvariantCulture.NumberFormat);
			writer.Write(value2);
		}

		public static T ReadEnum<T>(BinaryReader reader) where T : struct, IConvertible
		{
			int num = reader.ReadInt32();
			return (T)(object)num;
		}
	}
}
