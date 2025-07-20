using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class MasterPlanCompleteCommand : Command
	{
		[Inject]
		public MasterPlanDefinition masterPlanDefinition { get; set; }

		[Inject]
		public SetMasterPlanRewardAnimatorSignal setAnimatorSingal { get; set; }

		public override void Execute()
		{
			setAnimatorSingal.Dispatch(masterPlanDefinition);
		}
	}
}
