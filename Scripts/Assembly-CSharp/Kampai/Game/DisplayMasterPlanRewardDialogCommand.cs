using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class DisplayMasterPlanRewardDialogCommand : Command
	{
		[Inject]
		public MasterPlan masterPlan { get; set; }

		[Inject]
		public StartMasterPlanCooldownSignal startCooldownSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayfinderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			startCooldownSignal.Dispatch(masterPlan);
			Character firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Character>(70004);
			removeWayfinderSignal.Dispatch(firstInstanceByDefinitionId.ID);
		}
	}
}
