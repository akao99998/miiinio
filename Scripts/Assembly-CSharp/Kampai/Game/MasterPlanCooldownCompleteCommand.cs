using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MasterPlanCooldownCompleteCommand : Command
	{
		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public DisplayVolcanoLairVillainWayfinderSignal displayVolcanoWayfinderSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideFluxWayfinderSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public DisplayVillainSignal displayVillainSignal { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownRewardDialogSignal displayCooldownRewardSignal { get; set; }

		[Inject]
		public CleanupMasterPlanSignal cleanupPlanSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public ResetLairWayfinderIconSignal resetIconSignal { get; set; }

		public override void Execute()
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			cleanupPlanSignal.Dispatch(currentMasterPlan);
			currentMasterPlan.cooldownUTCStartTime = 0;
			displayVolcanoWayfinderSignal.Dispatch();
			hideFluxWayfinderSignal.Dispatch(false);
			WayFinderSettings type = new WayFinderSettings(374);
			createWayFinderSignal.Dispatch(type);
			resetIconSignal.Dispatch();
			currentMasterPlan.displayCooldownReward = true;
			if (villainLairModel.currentActiveLair != null)
			{
				displayVillainSignal.Dispatch(currentMasterPlan.Definition.VillainCharacterDefID, true);
				displayCooldownRewardSignal.Dispatch();
			}
			else
			{
				villainLairModel.seenCooldownAlert = false;
			}
		}
	}
}
