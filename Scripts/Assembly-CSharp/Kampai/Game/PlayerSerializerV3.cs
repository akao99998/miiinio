using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV3 : PlayerSerializerV2
	{
		public override int Version
		{
			get
			{
				return 3;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 3)
			{
				foreach (Villain item in player.GetInstancesByType<Villain>())
				{
					CabanaBuilding cabana = item.Cabana;
					if (cabana != null)
					{
						item.CabanaBuildingId = cabana.ID;
						item.Cabana = null;
					}
				}
				player.Version = 3;
			}
			return player;
		}
	}
}
