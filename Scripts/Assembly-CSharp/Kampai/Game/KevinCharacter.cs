using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class KevinCharacter : FrolicCharacter<KevinCharacterDefinition>
	{
		public KevinCharacter(KevinCharacterDefinition def)
			: base(def)
		{
			Name = "Kevin";
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			return go.AddComponent<KevinView>();
		}
	}
}
