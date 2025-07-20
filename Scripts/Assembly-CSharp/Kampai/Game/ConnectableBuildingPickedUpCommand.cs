using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class ConnectableBuildingPickedUpCommand : Command
	{
		private BuildingManagerView bmv;

		[Inject]
		public int buildingId { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			ConnectableBuilding connectableBuilding = playerService.GetByInstanceId<Building>(buildingId) as ConnectableBuilding;
			if (connectableBuilding != null)
			{
				bmv = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = bmv.GetBuildingObject(buildingId);
				Vector3 position = buildingObject.transform.position;
				Vector3 localEulerAngles = buildingObject.transform.localEulerAngles;
				bmv.RemoveBuilding(connectableBuilding.ID);
				GameObject gameObject = bmv.CreateBuilding(connectableBuilding, 2);
				BuildingObject component = gameObject.GetComponent<BuildingObject>();
				component.transform.localEulerAngles = localEulerAngles;
				component.transform.position = position;
				component.ID = connectableBuilding.ID;
			}
		}
	}
}
