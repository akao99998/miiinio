using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal sealed class PlayerSerializerV15 : PlayerSerializerV14
	{
		public override int Version
		{
			get
			{
				return 15;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 15)
			{
				UpdateLairPosition(player);
				UpdateBridgePosition(player);
				player.Version = 15;
			}
			return player;
		}

		private void UpdateLairPosition(Player player)
		{
			VillainLairEntranceBuilding byInstanceId = player.GetByInstanceId<VillainLairEntranceBuilding>(374);
			if (byInstanceId != null)
			{
				byInstanceId.Location.x = 164;
				byInstanceId.Location.y = 209;
			}
		}

		private void UpdateBridgePosition(Player player)
		{
			List<BridgeBuilding> instancesByType = player.GetInstancesByType<BridgeBuilding>();
			foreach (BridgeBuilding item in instancesByType)
			{
				switch (item.Definition.ID)
				{
				case 3102:
				case 3105:
					item.Location.x = 159;
					break;
				case 3103:
				case 3104:
					item.Location.x = 98;
					break;
				}
			}
		}
	}
}
