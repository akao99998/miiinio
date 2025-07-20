using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public class OrderBoardTicket : IGameTimeTracker
	{
		public TransactionInstance TransactionInst { get; set; }

		public int StartGameTime { get; set; }

		public int BoardIndex { get; set; }

		public int OrderNameTableIndex { get; set; }

		public int StartTime { get; set; }

		public int CharacterDefinitionId { get; set; }
	}
}
