using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class PhilCharacter : NamedCharacter<PhilCharacterDefinition>
	{
		public PhilCharacter(PhilCharacterDefinition def)
			: base(def)
		{
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			return go.AddComponent<PhilView>();
		}
	}
}
