using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class SpecialBuilding : Building<SpecialBuildingDefinition>
	{
		public SpecialBuilding(SpecialBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<SpecialBuildingObject>();
		}
	}
}
