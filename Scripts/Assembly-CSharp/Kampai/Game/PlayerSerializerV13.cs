using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV13 : PlayerSerializerV12
	{
		public override int Version
		{
			get
			{
				return 13;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 13)
			{
				player.Version = 13;
			}
			return player;
		}
	}
}
