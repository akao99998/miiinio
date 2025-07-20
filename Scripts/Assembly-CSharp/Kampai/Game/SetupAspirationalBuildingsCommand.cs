using System.Collections;
using System.Collections.Generic;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupAspirationalBuildingsCommand : Command
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

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		public override void Execute()
		{
			coroutineProgressMonitor.StartTask(Setup(), "setup aspirational");
		}

		private bool IsBuildingInInventory(AspirationalBuildingDefinition aspirationalDef)
		{
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(aspirationalDef.BuildingDefinitionID);
			foreach (Building item in byDefinitionId)
			{
				if (aspirationalDef.Location.x == item.Location.x && aspirationalDef.Location.y == item.Location.y)
				{
					return true;
				}
			}
			return false;
		}

		private IEnumerator Setup()
		{
			PurchasedLandExpansion purchasedLandExpansion = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			foreach (int expansionId in landExpansionConfigService.GetExpansionIds())
			{
				if (purchasedLandExpansion.HasPurchased(expansionId))
				{
					continue;
				}
				LandExpansionConfig config = landExpansionConfigService.GetExpansionConfig(expansionId);
				if (config.containedAspirationalBuildings == null)
				{
					continue;
				}
				foreach (int aspirationalDefId in landExpansionConfigService.GetExpansionConfig(expansionId).containedAspirationalBuildings)
				{
					AspirationalBuildingDefinition aspirationalDef = definitionService.Get<AspirationalBuildingDefinition>(aspirationalDefId);
					if (!IsBuildingInInventory(aspirationalDef))
					{
						BuildingDefinition buildingDef = definitionService.Get<BuildingDefinition>(aspirationalDef.BuildingDefinitionID);
						Building aspirationalBuilding = buildingDef.BuildBuilding();
						aspirationalBuilding.Location = new Location(aspirationalDef.Location.x, aspirationalDef.Location.y);
						aspirationalBuilding.SetState(BuildingState.Idle);
						aspirationalBuilding.ID = -aspirationalDefId;
						landExpansionService.TrackAspirationalBuilding(aspirationalDefId, aspirationalBuilding);
						createInventoryBuildingSignal.Dispatch(aspirationalBuilding, aspirationalBuilding.Location);
					}
				}
				yield return null;
			}
		}
	}
}
