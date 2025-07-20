using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV2 : PlayerSerializerV1
	{
		public override int Version
		{
			get
			{
				return 2;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 2)
			{
				if (player.GetByDefinitionId<Building>(3502).Count == 0)
				{
					AspirationalBuildingDefinition aspirationalBuildingDefinition = definitionService.Get<AspirationalBuildingDefinition>(46002);
					BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(3502);
					Building building = buildingDefinition.BuildBuilding();
					building.Location = new Location(aspirationalBuildingDefinition.Location.x, aspirationalBuildingDefinition.Location.y);
					building.SetState(BuildingState.Idle);
					player.AssignNextInstanceId(building);
					player.Add(building);
				}
				PurchasedLandExpansion byInstanceId = player.GetByInstanceId<PurchasedLandExpansion>(354);
				if (!byInstanceId.HasPurchased(197379) && !byInstanceId.IsAdjacentExpansion(197379))
				{
					byInstanceId.AdjacentExpansions.Add(197379);
				}
				if (!byInstanceId.HasPurchased(789516) && !byInstanceId.IsAdjacentExpansion(789516))
				{
					byInstanceId.AdjacentExpansions.Add(789516);
				}
				if (byInstanceId.HasPurchased(592137))
				{
					byInstanceId.PurchasedExpansions.Remove(592137);
				}
				if (byInstanceId.IsAdjacentExpansion(592137))
				{
					byInstanceId.AdjacentExpansions.Remove(592137);
				}
				player.Version = 2;
			}
			return player;
		}
	}
}
