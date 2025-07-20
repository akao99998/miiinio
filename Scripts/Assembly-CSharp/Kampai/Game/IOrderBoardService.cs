using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public interface IOrderBoardService
	{
		void Initialize();

		void ReplaceCharacterTickets(int characterDefinitionID);

		int GetLongestIdleOrderDuration();

		TransactionDefinition GetLongestIdleOrderTransaction();

		void AddPriorityPrestigeCharacter(int prestigeDefinitionID);

		void GetNewTicket(int orderBoardIndex);

		void UpdateOrderNumber();

		OrderBoard GetBoard();

		void SetEnabled(bool b);
	}
}
