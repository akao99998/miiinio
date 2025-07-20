using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class UpdateConnectablesCommand : Command
	{
		private BuildingManagerView bmv;

		[Inject]
		public Location location { get; set; }

		[Inject]
		public int connectableType { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public DecoGridModel decoGridModel { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			int[] array = new int[2] { -1, 1 };
			bmv = buildingManager.GetComponent<BuildingManagerView>();
			for (int i = 0; i < array.Length; i++)
			{
				Building building = environment.GetBuilding(location.x, location.y + array[i]);
				if (building != null && building is ConnectableBuilding)
				{
					UpdateConnectable(building);
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				Building building2 = environment.GetBuilding(location.x + array[j], location.y);
				if (building2 != null && building2 is ConnectableBuilding)
				{
					UpdateConnectable(building2);
				}
			}
			Building building3 = environment.GetBuilding(location.x, location.y);
			if (building3 != null && building3 is ConnectableBuilding)
			{
				UpdateConnectable(building3);
			}
		}

		private void UpdateConnectable(Building building)
		{
			ConnectableBuildingDefinition connectableBuildingDefinition = building.Definition as ConnectableBuildingDefinition;
			if (connectableType == connectableBuildingDefinition.connectableType)
			{
				int outDirection;
				ConnectableBuildingPieceType connectablePieceType = decoGridModel.GetConnectablePieceType(building.Location.x, building.Location.y, connectableBuildingDefinition.connectableType, out outDirection);
				ConnectableBuilding connectableBuilding = building as ConnectableBuilding;
				connectableBuilding.pieceType = connectablePieceType;
				connectableBuilding.rotation = outDirection;
				bmv.RemoveBuilding(connectableBuilding.ID);
				GameObject gameObject = bmv.CreateBuilding(building, (int)connectablePieceType);
				BuildingObject component = gameObject.GetComponent<BuildingObject>();
				component.transform.localEulerAngles = new Vector3(0f, outDirection, 0f);
				component.ID = building.ID;
			}
		}
	}
}
