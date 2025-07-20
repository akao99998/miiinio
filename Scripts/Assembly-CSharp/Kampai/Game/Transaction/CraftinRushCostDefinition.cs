using Kampai.Util;

namespace Kampai.Game.Transaction
{
	public class CraftinRushCostDefinition
	{
		public int FromSeconds { get; set; }

		public int ToSeconds { get; set; }

		public QuantityItem Cost { get; set; }
	}
}
