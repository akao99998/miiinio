using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util
{
	public interface IDummyCharacterBuilder
	{
		DummyCharacterObject BuildMinion(Minion minion, CostumeItemDefinition costume, Transform parent, bool isHigh, Vector3 villainScale, Vector3 villainPositionOffset);

		DummyCharacterObject BuildNamedChacter(NamedCharacter namedCharacter, Transform parent, bool isHigh, Vector3 villainScale, Vector3 villainPositionOffset);
	}
}
