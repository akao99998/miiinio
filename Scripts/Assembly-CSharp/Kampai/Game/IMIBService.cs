using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public interface IMIBService
	{
		bool IsUserReturning();

		void SetReturningKey();

		void ClearReturningKey();

		TransactionDefinition PickWeightedTransaction(int weightedDefId);

		ItemDefinition GetItemDefinition(TransactionDefinition transactionDef);

		ItemDefinition[] GetItemDefinitions(int weightedDefId);
	}
}
