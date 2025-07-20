using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class MasterPlanLeftOverBuilding : Building<MasterPlanLeftOverBuildingDefinition>
	{
		public MasterPlanLeftOverBuilding(MasterPlanLeftOverBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<MasterPlanLeftOverBuildingObject>();
		}
	}
}
