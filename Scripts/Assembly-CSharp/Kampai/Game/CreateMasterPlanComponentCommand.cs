using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CreateMasterPlanComponentCommand : Command
	{
		[Inject]
		public MasterPlanDefinition masterPlanDefinition { get; set; }

		[Inject]
		public int componentIndex { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PurchaseNewBuildingSignal purchaseNewBuildingSignal { get; set; }

		[Inject]
		public EnableOneVillainLairColliderSignal enableOneVillainLairColliderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayfinderSignal { get; set; }

		public override void Execute()
		{
			int id = masterPlanDefinition.ComponentDefinitionIDs[componentIndex];
			MasterPlanComponentDefinition masterPlanComponentDefinition = definitionService.Get<MasterPlanComponentDefinition>(id);
			int num = masterPlanDefinition.CompBuildingDefinitionIDs[componentIndex];
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(masterPlanComponentDefinition.ID);
			firstInstanceByDefinitionId.State = MasterPlanComponentState.Scaffolding;
			MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(num);
			Building building = masterPlanComponentBuildingDefinition.BuildBuilding();
			building.Location = firstInstanceByDefinitionId.buildingLocation;
			playerService.Add(building);
			purchaseNewBuildingSignal.Dispatch(building);
			enableOneVillainLairColliderSignal.Dispatch(false, num);
			Villain firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<Villain>(masterPlanDefinition.VillainCharacterDefID);
			removeWayfinderSignal.Dispatch(firstInstanceByDefinitionId2.ID);
		}
	}
}
