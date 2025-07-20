using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class ScaffoldingBuildingObject : BuildingObject, IScaffoldingPart
	{
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		public void Init(Building building, IKampaiLogger logger, IDefinitionService definitionService)
		{
			base.Init(building, logger, null, definitionService);
		}

		public override void UpdateColliderState(BuildingState state)
		{
		}
	}
}
