using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class MasterPlanComponentBuildingObjectMediator : Mediator
	{
		[Inject]
		public MasterPlanComponentBuildingObject view { get; set; }

		[Inject]
		public CleanupMasterPlanComponentsSignal cleanupComponentSignal { get; set; }

		public override void OnRegister()
		{
			view.cleanupComponentSignal = cleanupComponentSignal;
		}
	}
}
