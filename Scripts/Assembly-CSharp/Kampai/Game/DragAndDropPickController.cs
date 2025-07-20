using Kampai.Common;
using Kampai.Game.View;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DragAndDropPickController : Command
	{
		[Inject]
		public int pickEvent { get; set; }

		[Inject]
		public Vector3 inputPosition { get; set; }

		[Inject]
		public DragOffsetType dragOffsetType { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public MoveBuildingSignal moveBuildingSignal { get; set; }

		[Inject]
		public MoveScaffoldingSignal moveScaffoldingSignal { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public UpdateMovementValidity updateSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtilies { get; set; }

		[Inject]
		public Scaffolding currentScaffolding { get; set; }

		[Inject]
		public ShowMoveBuildingMenuSignal showMoveBuildingMenuSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		public override void Execute()
		{
			switch (pickEvent)
			{
			case 1:
				PickEventStart();
				break;
			case 2:
				PickEventHold();
				break;
			case 3:
				PickEventEnd();
				break;
			}
		}

		private void PickEventStart()
		{
			disableCameraSignal.Dispatch(1);
			enableCameraSignal.Dispatch(8);
			model.OffsetType = dragOffsetType;
		}

		private void PickEventHold()
		{
			Vector3 vector = cameraUtils.GroundPlaneRaycast(inputPosition);
			vector.x = Mathf.Clamp(vector.x, 15f, 223f);
			vector.z = Mathf.Clamp(vector.z, 0f, 214f);
			if (model.DragPreviousPosition == Vector3.zero)
			{
				model.DragPreviousPosition = vector;
			}
			if (model.OffsetType == DragOffsetType.ONE_X_ONE)
			{
				vector = new Vector3(vector.x + -1.4f, vector.y, vector.z + 1.2f);
			}
			Vector3 moveToPosition = vector;
			Drag(moveToPosition);
			model.DragPreviousPosition = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		}

		private void Drag(Vector3 moveToPosition)
		{
			if (!model.SelectedBuilding.HasValue)
			{
				return;
			}
			int value = model.SelectedBuilding.Value;
			if (value == -1)
			{
				moveToPosition = anchorToCornerBuilding(moveToPosition, currentScaffolding.Definition);
				Location location = new Location(moveToPosition);
				currentScaffolding.Location = location;
				bool flag = buildingUtilies.ValidateScaffoldingPlacement(currentScaffolding.Definition, location);
				moveScaffoldingSignal.Dispatch(moveToPosition, flag);
				updateSignal.Dispatch(flag);
				return;
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(value);
			if (byInstanceId != null && byInstanceId.Definition.Movable)
			{
				moveToPosition = anchorToCornerBuilding(moveToPosition, byInstanceId.Definition);
				Location location2 = new Location(moveToPosition);
				bool flag2 = buildingUtilies.ValidateLocation(byInstanceId, location2);
				moveBuildingSignal.Dispatch(value, moveToPosition, flag2);
				updateSignal.Dispatch(flag2);
			}
		}

		private Vector3 anchorToCornerBuilding(Vector3 originalPosition, BuildingDefinition buildingDef)
		{
			Vector3 result = originalPosition;
			FootprintDefinition footprintDefinition = definitionService.Get<FootprintDefinition>(buildingDef.FootprintID);
			if (footprintDefinition != null)
			{
				int num = BuildingUtil.GetFootprintDepth(footprintDefinition.Footprint) / 2;
				int num2 = BuildingUtil.GetFootprintWidth(footprintDefinition.Footprint) / 2;
				result = new Vector3(result.x - (float)num2, result.y, result.z + (float)num);
			}
			return result;
		}

		private void PickEventEnd()
		{
			disableCameraSignal.Dispatch(8);
			enableCameraSignal.Dispatch(1);
			if (model.SelectedBuilding == -1)
			{
				showMoveBuildingMenuSignal.Dispatch(true, new MoveBuildingSetting(-1, 1, questService.ShouldPulseMoveButtonAccept(), true));
				bool type = buildingUtilies.ValidateScaffoldingPlacement(currentScaffolding.Definition, currentScaffolding.Location);
				updateSignal.Dispatch(type);
			}
			Reset();
		}

		private void Reset()
		{
			model.DragPreviousPosition = Vector3.zero;
		}
	}
}
