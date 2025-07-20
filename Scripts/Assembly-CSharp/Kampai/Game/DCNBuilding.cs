using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class DCNBuilding : Building<DCNBuildingDefinition>
	{
		public DCNBuilding(DCNBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<DCNBuildingObjectView>();
		}
	}
}
