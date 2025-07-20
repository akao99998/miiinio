using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV7 : PlayerSerializerV6
	{
		public override int Version
		{
			get
			{
				return 7;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 7)
			{
				if (player.HighestFtueLevel >= 7)
				{
					player.GetByInstanceId<StageBuilding>(370).State = BuildingState.Idle;
				}
				DCNBuilding firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<DCNBuilding>(3128);
				if (firstInstanceByDefinitionId == null)
				{
					DCNBuildingDefinition definition = null;
					if (definitionService.TryGet<DCNBuildingDefinition>(3128, out definition))
					{
						firstInstanceByDefinitionId = definition.Build() as DCNBuilding;
						firstInstanceByDefinitionId.State = BuildingState.Idle;
						firstInstanceByDefinitionId.Location = new Location(107, 172);
						player.AssignNextInstanceId(firstInstanceByDefinitionId);
						player.Add(firstInstanceByDefinitionId);
					}
				}
				player.Version = 7;
			}
			return player;
		}
	}
}
