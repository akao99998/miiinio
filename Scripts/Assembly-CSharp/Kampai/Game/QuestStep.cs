namespace Kampai.Game
{
	public class QuestStep
	{
		public QuestStepState state { get; set; }

		public int AmountCompleted { get; set; }

		public int AmountReady { get; set; }

		public int TrackedID { get; set; }
	}
}
