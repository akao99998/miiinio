using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CompositeBuildingObject : BuildingObject
	{
		public CompositeBuilding compositeBuilding { get; set; }

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			compositeBuilding = (CompositeBuilding)building;
		}
	}
}
