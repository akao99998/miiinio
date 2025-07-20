using Kampai.UI.View;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VolcanoEntranceMediator : Mediator
	{
		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			CreateWayfinder();
		}

		private void CreateWayfinder()
		{
			VillainLair firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<VillainLair>(3137);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(firstInstanceByDefinitionId.portalInstanceID);
			if (byInstanceId == null || byInstanceId.State != BuildingState.Inaccessible)
			{
				MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
				if (currentMasterPlan == null || currentMasterPlan.cooldownUTCStartTime <= 0)
				{
					WayFinderSettings type = new WayFinderSettings(374);
					createWayFinderSignal.Dispatch(type);
				}
			}
		}
	}
}
