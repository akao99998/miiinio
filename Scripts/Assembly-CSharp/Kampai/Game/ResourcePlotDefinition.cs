namespace Kampai.Game
{
	public class ResourcePlotDefinition
	{
		public string descriptionKey { get; set; }

		public bool isAutomaticallyUnlocked { get; set; }

		public Location location { get; set; }

		public int unlockTransactionID { get; set; }

		public int rotation { get; set; }
	}
}
