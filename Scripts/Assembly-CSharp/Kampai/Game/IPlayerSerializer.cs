using Kampai.Util;

namespace Kampai.Game
{
	public interface IPlayerSerializer
	{
		int Version { get; }

		Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger);

		byte[] Serialize(Player player, IDefinitionService defintitionService, IKampaiLogger logger);
	}
}
