using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class StageBuilding : RepairableBuilding<StageBuildingDefinition>, Building, ZoomableBuilding, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		public ZoomableBuildingDefinition ZoomableDefinition
		{
			get
			{
				return base.Definition;
			}
		}

		public StageBuilding(StageBuildingDefinition def)
			: base(def)
		{
		}

		public override BuildingObject AddBuildingObject(GameObject gameObject)
		{
			return gameObject.AddComponent<StageBuildingObject>();
		}
	}
}
