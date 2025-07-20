using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanComponentTask
	{
		public bool isComplete { get; set; }

		public uint earnedQuantity { get; set; }

		public MasterPlanComponentTaskDefinition Definition { get; set; }

		[JsonIgnore]
		public bool isHarvestable
		{
			get
			{
				return earnedQuantity >= Definition.requiredQuantity && !isComplete;
			}
		}

		[JsonIgnore]
		public uint remainingQuantity
		{
			get
			{
				return Definition.requiredQuantity - earnedQuantity;
			}
		}

		public override string ToString()
		{
			return string.Format("Base: {0}, earnedQuantity: {1}, isComplete: {2}, isHarvestable: {3}, remainingQuantity: {4}", base.ToString(), earnedQuantity, isComplete, isHarvestable, remainingQuantity);
		}
	}
}
