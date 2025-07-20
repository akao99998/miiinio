using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BuildingChangeStateCommand : Command
	{
		[Inject]
		public int buildingID { get; set; }

		[Inject]
		public BuildingState newState { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public AddFootprintSignal addFootprintSignal { get; set; }

		public override void Execute()
		{
			Building byInstanceId = playerService.GetByInstanceId<Building>(buildingID);
			if (byInstanceId == null)
			{
				return;
			}
			BuildingState state = byInstanceId.State;
			TaskableBuilding taskableBuilding = byInstanceId as TaskableBuilding;
			if (taskableBuilding != null && newState == BuildingState.Working && state == BuildingState.Harvestable && taskableBuilding.GetNumCompleteMinions() > 0)
			{
				return;
			}
			if (buildingManager != null)
			{
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(buildingID);
				if (buildingObject != null)
				{
					buildingObject.UpdateColliderState(newState);
					if (state == BuildingState.Disabled && newState != state)
					{
						addFootprintSignal.Dispatch(byInstanceId, byInstanceId.Location);
					}
					if ((state == BuildingState.Disabled || newState == BuildingState.Disabled) && newState != state)
					{
						if (newState == BuildingState.Disabled)
						{
							buildingObject.SetMaterialColor(Color.gray);
						}
						else
						{
							buildingObject.SetMaterialColor(Color.white);
						}
					}
					IStartAudio startAudio = buildingObject as IStartAudio;
					if (startAudio != null)
					{
						startAudio.NotifyBuildingState(newState);
					}
				}
			}
			byInstanceId.SetState(newState);
			byInstanceId.StateStartTime = timeService.CurrentTime();
		}
	}
}
