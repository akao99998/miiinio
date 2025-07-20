namespace Kampai.Game
{
	public interface ITikiBarService
	{
		Prestige GetPrestigeForSeatableCharacter(Character character);

		bool IsCharacterSitting(Prestige prestige);

		bool IsCharacterSitting(Character character);
	}
}
