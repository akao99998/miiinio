using Kampai.Common;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class InventoryBuildingMovementCommand : Command
	{
		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RemoveBuildingSignal removeBuildingSignal { get; set; }

		[Inject]
		public DebugUpdateGridSignal gridSignal { get; set; }

		[Inject]
		public DeselectBuildingSignal deselectBuildingSignal { get; set; }

		[Inject]
		public SendBuildingToInventorySignal sendBuildingToInventorySignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			if (pickControllerModel.SelectedBuilding.HasValue && pickControllerModel.SelectedBuilding != -1)
			{
				int value = pickControllerModel.SelectedBuilding.Value;
				buildingChangeStateSignal.Dispatch(value, BuildingState.Inventory);
				Building byInstanceId = playerService.GetByInstanceId<Building>(value);
				if (byInstanceId != null)
				{
					removeBuildingSignal.Dispatch(byInstanceId.Location, definitionService.GetBuildingFootprint(byInstanceId.Definition.FootprintID));
					gridSignal.Dispatch();
				}
				LocalPersistBuildingInventoryStorageAction();
				sendBuildingToInventorySignal.Dispatch(value);
				deselectBuildingSignal.Dispatch(value);
			}
		}

		private void LocalPersistBuildingInventoryStorageAction()
		{
			if (!localPersistService.HasKey("didyouknow_PutBuildingInInventory"))
			{
				localPersistService.PutDataInt("didyouknow_PutBuildingInInventory", 1);
			}
		}
	}
}
