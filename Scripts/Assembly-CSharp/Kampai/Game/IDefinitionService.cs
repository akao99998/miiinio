using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Util;

namespace Kampai.Game
{
	public interface IDefinitionService
	{
		bool Has(int id);

		bool Has<T>(int id) where T : Definition;

		T Get<T>(int id) where T : Definition;

		T Get<T>(StaticItem staticItem) where T : Definition;

		T Get<T>() where T : Definition;

		Definition Get(int id);

		bool TryGet<T>(int id, out T definition) where T : Definition;

		IList<string> GetEnvironemtDefinition();

		void ReclaimEnfironmentDefinitions();

		List<T> GetAll<T>() where T : Definition;

		WeightedDefinition GetGachaWeightsForNumMinions(int numMinions, bool party);

		List<WeightedDefinition> GetAllGachaDefinitions();

		Dictionary<int, Definition> GetAllDefinitions();

		void DeserializeJson(TextReader textReader, bool validateDefinitions = true);

		void DeserializeBinary(BinaryReader binaryReader, bool validateDefinitions = false);

		void DeserializeEnvironmentDefinition(TextReader textReader);

		int GetHarvestItemDefinitionIdFromTransactionId(int transactionId);

		string GetHarvestIconFromTransactionID(int transactionId);

		bool HasUnlockItemInTransactionOutput(int transactionID);

		int GetBuildingDefintionIDFromItemDefintionID(int itemDefinitionID);

		BridgeDefinition GetBridgeDefinition(int itemDefinitionID);

		int ExtractQuantityFromTransaction(int transactionID, int definitionID);

		int GetLevelItemUnlocksAt(int definitionID);

		TaskLevelBandDefinition GetTaskLevelBandForLevel(int level);

		int getItemTransactionID(int id);

		RushTimeBandDefinition GetRushTimeBandForTime(int timeRemainingInSeconds);

		string GetInitialPlayer();

		string GetBuildingFootprint(int ID);

		int GetIncrementalCost(Definition definition);

		void Add(Definition definition);

		void Remove(Definition definition);

		AchievementDefinition GetAchievementDefinitionFromDefinitionID(int defID);

		IList<TriggerDefinition> GetTriggerDefinitions();

		IList<CurrencyStoreCategoryDefinition> GetCurrencyStoreCategoryDefinitions();

		void SetPerformanceQualityLevel(TargetPerformance targetPerformance);

		TransactionDefinition GetPackTransaction(int transactionId);

		PackDefinition GetPackDefinitionFromTransactionId(int transactionId);

		SalePackType getSKUSalePackType(string ExternalIdentifier);

		int GetPartyFavorDefinitionIDByItemID(int itemID);

		string GetLegalURL(LegalDocuments.LegalType type, string language);
	}
}
