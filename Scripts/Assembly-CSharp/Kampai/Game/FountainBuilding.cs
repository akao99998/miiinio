using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class FountainBuilding : RepairableBuilding<FountainBuildingDefinition>
	{
		public FountainBuilding(FountainBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<FountainBuildingObject>();
		}
	}
}
