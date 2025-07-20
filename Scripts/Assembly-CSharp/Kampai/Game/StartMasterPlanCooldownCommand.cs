using Kampai.Common;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartMasterPlanCooldownCommand : Command
	{
		[Inject]
		public MasterPlan masterPlan { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public MasterPlanCooldownCompleteSignal cooldownCompleteSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayfinderSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		public override void Execute()
		{
			int num = timeService.CurrentTime();
			masterPlan.cooldownUTCStartTime = num;
			timeEventService.AddEvent(masterPlan.ID, num, masterPlan.Definition.CooldownDuration, cooldownCompleteSignal);
			masterPlan.displayCooldownAlert = true;
			Villain firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Villain>(70004);
			removeWayfinderSignal.Dispatch(firstInstanceByDefinitionId.ID);
			removeWayfinderSignal.Dispatch(374);
			int gameTimeDuration = playerDurationService.GetGameTimeDuration(masterPlan);
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(masterPlan.Definition.BuildingDefID);
			telemetryService.Send_Telemetry_EVT_MASTER_PLAN_COMPLETE(masterPlanComponentBuildingDefinition.TaxonomySpecific, firstInstanceByDefinitionId.Definition.LocalizedKey, gameTimeDuration);
		}
	}
}
