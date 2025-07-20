using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DisplayVolcanoLairVillainWayfinderCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IMasterPlanService planService { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		public override void Execute()
		{
			NamedCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<NamedCharacter>(70004);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			Prestige prestige = prestigeService.GetPrestige(40001);
			if (prestige == null)
			{
				return;
			}
			VillainLair firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<VillainLair>(3137);
			if (firstInstanceByDefinitionId2 == null || !firstInstanceByDefinitionId2.hasVisited)
			{
				return;
			}
			MasterPlan currentMasterPlan = planService.CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				return;
			}
			MasterPlanDefinition definition = currentMasterPlan.Definition;
			MasterPlanComponentBuilding firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(definition.BuildingDefID);
			if (firstInstanceByDefinitionId3 == null || firstInstanceByDefinitionId3.State != BuildingState.Complete)
			{
				MasterPlanComponent activeComponentFromPlanDefinition = planService.GetActiveComponentFromPlanDefinition(definition.ID);
				if (activeComponentFromPlanDefinition == null || activeComponentFromPlanDefinition.State != MasterPlanComponentState.Scaffolding)
				{
					createWayFinderSignal.Dispatch(new WayFinderSettings(firstInstanceByDefinitionId.ID));
				}
			}
		}
	}
}
