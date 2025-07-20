using System.Collections.Generic;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EnableBuildingAnimatorsCommand : Command
	{
		[Inject]
		public bool enable { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			ICollection<int> animatingBuildingIDs = playerService.GetAnimatingBuildingIDs();
			foreach (int item in animatingBuildingIDs)
			{
				AnimatingBuildingObject animatingBuildingObject = component.GetBuildingObject(item) as AnimatingBuildingObject;
				if (animatingBuildingObject != null)
				{
					animatingBuildingObject.EnableAnimators(enable);
				}
			}
		}
	}
}
