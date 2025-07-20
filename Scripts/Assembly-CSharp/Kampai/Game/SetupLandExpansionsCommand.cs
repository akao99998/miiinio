using System.Collections.Generic;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupLandExpansionsCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventoryBuildingSignal { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetupForSaleSignsSignal setupForSaleSignsSignal { get; set; }

		[Inject]
		public SetupFlowersSignal setupFlowersSignal { get; set; }

		public override void Execute()
		{
			IList<LandExpansionDefinition> all = definitionService.GetAll<LandExpansionDefinition>();
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			foreach (LandExpansionDefinition item in all)
			{
				if (!byInstanceId.PurchasedExpansions.Contains(item.ExpansionID))
				{
					BuildingDefinition buildingDefinition = definitionService.Get(item.BuildingDefinitionID) as BuildingDefinition;
					LandExpansionBuilding landExpansionBuilding = buildingDefinition.Build() as LandExpansionBuilding;
					landExpansionBuilding.State = BuildingState.Idle;
					Location location = null;
					location = new Location(item.Location.x, item.Location.y);
					landExpansionBuilding.ID = item.ID;
					landExpansionBuilding.ExpansionID = item.ExpansionID;
					landExpansionBuilding.Location = location;
					landExpansionBuilding.MinimumLevel = item.MinimumLevel;
					if (item.Grass)
					{
						landExpansionService.AddBuilding(landExpansionBuilding);
						createInventoryBuildingSignal.Dispatch(landExpansionBuilding, location);
					}
					else
					{
						landExpansionService.TrackFlower(landExpansionBuilding);
					}
				}
			}
			setupFlowersSignal.Dispatch();
			setupForSaleSignsSignal.Dispatch();
		}
	}
}
