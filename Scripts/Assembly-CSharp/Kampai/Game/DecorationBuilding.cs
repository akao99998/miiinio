using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class DecorationBuilding : Building<DecorationBuildingDefinition>
	{
		public DecorationBuilding(DecorationBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<DecorationBuildingObject>();
		}
	}
}
