using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util
{
	public interface INamedCharacterBuilder
	{
		NamedCharacterObject Build(NamedCharacter character, GameObject parent = null);
	}
}
