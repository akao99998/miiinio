using Kampai.Common;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelBuildingMovementCommand : Command
	{
		[Inject]
		public bool InvalidLocation { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public CancelPurchaseSignal cancelPurchaseSignal { get; set; }

		[Inject]
		public SetBuildingPositionSignal setBuildingPositionSignal { get; set; }

		[Inject]
		public DeselectBuildingSignal deselectBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UpdateConnectablesSignal updateConnectablesSignal { get; set; }

		public override void Execute()
		{
			if (!pickControllerModel.SelectedBuilding.HasValue)
			{
				return;
			}
			if (pickControllerModel.SelectedBuilding == -1)
			{
				cancelPurchaseSignal.Dispatch(InvalidLocation);
			}
			else
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(pickControllerModel.SelectedBuilding.Value);
				Location location = byInstanceId.Location;
				setBuildingPositionSignal.Dispatch(byInstanceId.ID, new Vector3(location.x, 0f, location.y));
				deselectBuildingSignal.Dispatch(byInstanceId.ID);
				ConnectableBuilding connectableBuilding = byInstanceId as ConnectableBuilding;
				if (connectableBuilding != null)
				{
					updateConnectablesSignal.Dispatch(location, connectableBuilding.Definition.connectableType);
				}
			}
			pickControllerModel.SelectedBuilding = null;
		}
	}
}
