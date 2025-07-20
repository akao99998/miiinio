using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public class Scaffolding : Locatable
	{
		public Building Building { get; set; }

		public Location Location { get; set; }

		public BuildingDefinition Definition { get; set; }

		public TransactionDefinition Transaction { get; set; }

		public bool Lifted { get; set; }
	}
}
