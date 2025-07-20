using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class WelcomeHutBuilding : RepairableBuilding<WelcomeHutBuildingDefinition>
	{
		public WelcomeHutBuilding(WelcomeHutBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<WelcomeHutBuildingObject>();
		}
	}
}
