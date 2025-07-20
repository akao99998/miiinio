using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class BigThreeCharacter : NamedCharacter<BigThreeCharacterDefinition>
	{
		public BigThreeCharacter(BigThreeCharacterDefinition def)
			: base(def)
		{
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			switch (base.Definition.Type)
			{
			case NamedCharacterType.KEVIN:
				return go.AddComponent<KevinView>();
			case NamedCharacterType.STUART:
				return go.AddComponent<StuartView>();
			case NamedCharacterType.PHIL:
				return go.AddComponent<PhilView>();
			default:
				return null;
			}
		}
	}
}
