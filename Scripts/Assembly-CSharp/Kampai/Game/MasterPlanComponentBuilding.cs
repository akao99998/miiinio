using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public class MasterPlanComponentBuilding : Building<MasterPlanComponentBuildingDefinition>
	{
		public MasterPlanComponentBuilding(MasterPlanComponentBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<MasterPlanComponentBuildingObject>();
		}

		public override string GetPrefab(int index = 0)
		{
			return base.Definition.Prefab;
		}
	}
}
