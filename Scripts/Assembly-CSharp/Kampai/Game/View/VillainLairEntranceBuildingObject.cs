using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class VillainLairEntranceBuildingObject : BuildingObject
	{
		public VillainLairEntranceBuilding portal { get; set; }

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			portal = building as VillainLairEntranceBuilding;
			base.gameObject.AddComponent<VolcanoEntranceView>();
		}

		protected override Vector3 GetIndicatorPosition(bool centerY)
		{
			return new Vector3(minColliderY.bounds.center.x, minColliderY.bounds.max.y, minColliderY.bounds.center.z);
		}
	}
}
