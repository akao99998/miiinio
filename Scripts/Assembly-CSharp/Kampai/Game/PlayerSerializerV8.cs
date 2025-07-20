using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV8 : PlayerSerializerV7
	{
		public override int Version
		{
			get
			{
				return 8;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 8)
			{
				MIBBuilding firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<MIBBuilding>(3129);
				if (firstInstanceByDefinitionId == null)
				{
					MIBBuildingDefinition definition = null;
					if (definitionService.TryGet<MIBBuildingDefinition>(3129, out definition))
					{
						firstInstanceByDefinitionId = definition.Build() as MIBBuilding;
						firstInstanceByDefinitionId.State = BuildingState.Idle;
						firstInstanceByDefinitionId.Location = new Location(123, 176);
						player.AssignNextInstanceId(firstInstanceByDefinitionId);
						player.Add(firstInstanceByDefinitionId);
					}
				}
				player.Version = 8;
			}
			return player;
		}
	}
}
