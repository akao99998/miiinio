using Elevation.Logging;
using Kampai.Util;

namespace Kampai.Game
{
	public class TikiBarService : ITikiBarService
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("TikiBarService") as IKampaiLogger;

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IPrestigeService PrestigeService { get; set; }

		public Prestige GetPrestigeForSeatableCharacter(Character character)
		{
			Prestige prestigeFromMinionInstance = PrestigeService.GetPrestigeFromMinionInstance(character);
			if (prestigeFromMinionInstance == null)
			{
				logger.Warning("Unable to find prestige for character with def id: {0} ", character.Definition.ID);
				return null;
			}
			if (prestigeFromMinionInstance.Definition.ID == 40014)
			{
				return null;
			}
			return prestigeFromMinionInstance;
		}

		public bool IsCharacterSitting(Prestige prestige)
		{
			TikiBarBuilding firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<TikiBarBuilding>(3041);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Warning("Unable to find tikibar building!");
				return false;
			}
			int minionSlotIndex = firstInstanceByDefinitionId.GetMinionSlotIndex(prestige.Definition.ID);
			return minionSlotIndex >= 0 && minionSlotIndex < 3;
		}

		public bool IsCharacterSitting(Character character)
		{
			Prestige prestigeForSeatableCharacter = GetPrestigeForSeatableCharacter(character);
			if (prestigeForSeatableCharacter == null)
			{
				return false;
			}
			return IsCharacterSitting(prestigeForSeatableCharacter);
		}
	}
}
