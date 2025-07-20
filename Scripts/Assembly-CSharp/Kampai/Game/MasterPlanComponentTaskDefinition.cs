namespace Kampai.Game
{
	public class MasterPlanComponentTaskDefinition
	{
		public int requiredItemId { get; set; }

		public uint requiredQuantity { get; set; }

		public bool ShowWayfinder { get; set; }

		public MasterPlanComponentTaskType Type { get; set; }

		public MasterPlanComponentTask Build()
		{
			MasterPlanComponentTask masterPlanComponentTask = new MasterPlanComponentTask();
			masterPlanComponentTask.Definition = this;
			return masterPlanComponentTask;
		}
	}
}
