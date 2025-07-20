namespace Kampai.Game
{
	public class MasterPlanComponentRewardDefinition
	{
		public int rewardItemId { get; set; }

		public uint rewardQuantity { get; set; }

		public uint grindReward { get; set; }

		public uint premiumReward { get; set; }

		public MasterPlanComponentReward Build()
		{
			MasterPlanComponentReward masterPlanComponentReward = new MasterPlanComponentReward();
			masterPlanComponentReward.Definition = this;
			return masterPlanComponentReward;
		}
	}
}
