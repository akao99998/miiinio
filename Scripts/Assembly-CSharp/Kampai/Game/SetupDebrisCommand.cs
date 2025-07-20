using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupDebrisCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventoryBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			foreach (int expansionId in landExpansionConfigService.GetExpansionIds())
			{
				if (byInstanceId.HasPurchased(expansionId))
				{
					continue;
				}
				LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(expansionId);
				if (expansionConfig.containedDebris == null)
				{
					continue;
				}
				foreach (int containedDebri in expansionConfig.containedDebris)
				{
					DebrisDefinition debrisDefinition = definitionService.Get<DebrisDefinition>(containedDebri);
					DebrisBuildingDefinition debrisBuildingDefinition = definitionService.Get<DebrisBuildingDefinition>(debrisDefinition.BuildingDefinitionID);
					DebrisBuilding debrisBuilding = debrisBuildingDefinition.Build() as DebrisBuilding;
					debrisBuilding.Location = new Location(debrisDefinition.Location.x, debrisDefinition.Location.y);
					debrisBuilding.SetState(BuildingState.Disabled);
					debrisBuilding.ID = -containedDebri;
					landExpansionService.TrackDebris(containedDebri, debrisBuilding);
					createInventoryBuildingSignal.Dispatch(debrisBuilding, debrisBuilding.Location);
				}
			}
		}
	}
}
