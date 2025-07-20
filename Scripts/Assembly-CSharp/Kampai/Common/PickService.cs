using System.Collections.Generic;
using Kampai.Common.Service.HealthMetrics;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using strange.extensions.context.api;

namespace Kampai.Common
{
	public class PickService : IPickService
	{
		private RaycastHit hit;

		private Vector3 inputPosition;

		private int input;

		private bool startedOnHUD;

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera camera { get; set; }

		[Inject]
		public ICameraControlsService cameraControlsService { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public MinionPickSignal minionSignal { get; set; }

		[Inject]
		public EnvironmentalMignetteTappedSignal environmentalMignetteTappedSignal { get; set; }

		[Inject]
		public BuildingPickSignal buildingSignal { get; set; }

		[Inject]
		public VillainIslandMessageSignal villainIslandMessageSignal { get; set; }

		[Inject]
		public LairEnvironmentElementClickedSignal lairEnvironmentElementClickedSignal { get; set; }

		[Inject]
		public MagnetFingerPickSignal magnetFingerSignal { get; set; }

		[Inject]
		public DragAndDropPickSignal dragAndDropSignal { get; set; }

		[Inject]
		public LandExpansionPickSignal landExpansionSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITapEventMetricsService tapEventMetricsService { get; set; }

		[Inject]
		public TikiBarViewPickSignal tikiBarViewPickSignal { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllMinionsSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public ShowActivitySpinnerSignal showActivitySpinnerSignal { get; set; }

		public void OnGameInput(Vector3 inputPosition, int input, bool pressed)
		{
			if (!model.Enabled || model.PanningCameraBlocked)
			{
				return;
			}
			this.inputPosition = inputPosition;
			this.input = input;
			bool flag = pressed && IsPointerOverUIObject(inputPosition);
			bool previousPressState = model.PreviousPressState;
			bool flag2 = model.InvalidateMovement;
			if (!flag2 && flag)
			{
				model.InvalidateMovement = flag;
				flag2 = flag;
			}
			if (input == 0)
			{
				cameraControlsService.OnGameInput(inputPosition, 0);
			}
			if (pressed && !flag2 && !zoomCameraModel.ZoomedIn)
			{
				showHiddenBuildingsSignal.Dispatch();
			}
			if (!previousPressState && pressed && flag2)
			{
				startedOnHUD = true;
			}
			if (model.CurrentMode == PickControllerModel.Mode.DragAndDrop)
			{
				model.InvalidateMovement = false;
				flag2 = false;
				startedOnHUD = false;
			}
			if (!previousPressState && pressed && !flag2 && model.CurrentMode == PickControllerModel.Mode.None)
			{
				cameraControlsService.OnGameInput(inputPosition, input);
				TouchStart();
			}
			else if (previousPressState && pressed)
			{
				if (!startedOnHUD)
				{
					cameraControlsService.OnGameInput(inputPosition, input);
				}
				else
				{
					cameraControlsService.OnGameInput(inputPosition, 0);
				}
				if (!flag2)
				{
					TouchHold();
				}
				else if (model.activitySpinnerExists)
				{
					model.activitySpinnerExists = false;
					showActivitySpinnerSignal.Dispatch(false, Vector3.zero);
				}
			}
			else if (previousPressState && !pressed)
			{
				EndTouch();
			}
			model.PreviousPressState = pressed;
			if (((uint)input & 2u) != 0 && zoomCameraModel.ZoomedIn)
			{
				ZoomOutOfBuilding();
			}
		}

		private bool IsPointerOverUIObject(Vector3 position)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = position;
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			return list.Count > 0;
		}

		private void EndTouch()
		{
			if (!model.DetectedMovement)
			{
				float num = Mathf.Abs(inputPosition.magnitude - model.StartTouchPosition.magnitude);
				if (num >= 15f)
				{
					model.DetectedMovement = true;
				}
				else if (!model.InvalidateMovement)
				{
					moveBuildMenuSignal.Dispatch(false);
				}
			}
			TouchEnd();
		}

		private void Reset()
		{
			model.StartHitObject = null;
			model.EndHitObject = null;
			model.InvalidateMovement = false;
			model.Blocked = false;
			model.HeldTimer = 0f;
			model.CurrentMode = PickControllerModel.Mode.None;
			model.DetectedMovement = false;
			model.ValidLocation = true;
			startedOnHUD = false;
		}

		private void TouchStart()
		{
			model.StartTouchPosition = inputPosition;
			model.StartTouchTimeMs = Time.time;
			Ray ray = camera.ScreenPointToRay(inputPosition);
			if (Physics.Raycast(ray, out hit))
			{
				model.StartHitObject = hit.collider.gameObject;
			}
			if (zoomCameraModel.ZoomedIn)
			{
				if (zoomCameraModel.LastZoomBuildingType == BuildingZoomType.TIKIBAR)
				{
					model.CurrentMode = PickControllerModel.Mode.TikiBarView;
					return;
				}
				if (zoomCameraModel.LastZoomBuildingType == BuildingZoomType.STAGE)
				{
					model.CurrentMode = PickControllerModel.Mode.StageView;
					return;
				}
			}
			if (!(model.StartHitObject != null))
			{
				return;
			}
			switch (model.StartHitObject.layer)
			{
			case 9:
			case 14:
				TouchStartHitBuilding();
				break;
			case 8:
				model.CurrentMode = PickControllerModel.Mode.Minion;
				break;
			case 11:
				model.CurrentMode = PickControllerModel.Mode.EnvironmentalMignette;
				break;
			case 12:
				if (!model.SelectedBuilding.HasValue)
				{
					model.CurrentMode = PickControllerModel.Mode.LandExpansion;
				}
				break;
			case 15:
				model.CurrentMode = PickControllerModel.Mode.VillainIsland;
				break;
			case 16:
				model.CurrentMode = PickControllerModel.Mode.VillainLair;
				break;
			case 10:
			case 13:
				break;
			}
		}

		private void TouchHold()
		{
			if (!model.DetectedMovement)
			{
				float num = Mathf.Abs(inputPosition.magnitude - model.StartTouchPosition.magnitude);
				if (num >= 15f)
				{
					model.DetectedMovement = true;
				}
			}
			if (!model.Blocked)
			{
				model.Blocked = true;
			}
			model.HeldTimer += Time.deltaTime;
			int type = 2;
			switch (model.CurrentMode)
			{
			case PickControllerModel.Mode.TikiBarView:
			case PickControllerModel.Mode.StageView:
				if (model.DetectedMovement && !timedSocialEventService.isRewardCutscene())
				{
					ZoomOutOfBuilding();
				}
				break;
			case PickControllerModel.Mode.Building:
				buildingSignal.Dispatch(type, inputPosition);
				break;
			case PickControllerModel.Mode.DragAndDrop:
				dragAndDropSignal.Dispatch(type, inputPosition, DragOffsetType.NONE);
				break;
			case PickControllerModel.Mode.MagnetFinger:
				if (model.ValidLocation && !model.SelectedBuilding.HasValue)
				{
					magnetFingerSignal.Dispatch(type, inputPosition);
				}
				break;
			case PickControllerModel.Mode.None:
			case PickControllerModel.Mode.Minion:
				HandleNoneAndMinion();
				break;
			case PickControllerModel.Mode.EnvironmentalMignette:
			case PickControllerModel.Mode.LandExpansion:
				break;
			}
		}

		private void HandleNoneAndMinion()
		{
			if (playerService.GetHighestFtueCompleted() >= 9 && model.HeldTimer > 1f && !model.DetectedMovement && input == 1 && !model.SelectedBuilding.HasValue)
			{
				model.CurrentMode = PickControllerModel.Mode.MagnetFinger;
				magnetFingerSignal.Dispatch(1, inputPosition);
				if (model.ValidLocation)
				{
					Vector3 groundPosition = gameContext.injectionBinder.GetInstance<CameraUtils>().GroundPlaneRaycast(inputPosition);
					MoveMinion(groundPosition);
				}
			}
		}

		private void TouchEnd()
		{
			deselectAllMinionsSignal.Dispatch();
			if (model.minionMoveIndicator != null)
			{
				Object.Destroy(model.minionMoveIndicator);
			}
			if (!model.InvalidateMovement)
			{
				Ray ray = camera.ScreenPointToRay(inputPosition);
				if (Physics.Raycast(ray, out hit))
				{
					model.EndHitObject = hit.collider.gameObject;
					if (model.CurrentMode != PickControllerModel.Mode.DragAndDrop && (model.EndHitObject.name == "StampAlbum" || model.EndHitObject.name == "Shelve"))
					{
						model.CurrentMode = PickControllerModel.Mode.TikiBarView;
					}
				}
				CheckCases();
			}
			tapEventMetricsService.Mark();
			Reset();
		}

		private void CheckCases()
		{
			switch (model.CurrentMode)
			{
			case PickControllerModel.Mode.Building:
				buildingSignal.Dispatch(3, inputPosition);
				break;
			case PickControllerModel.Mode.DragAndDrop:
				dragAndDropSignal.Dispatch(3, inputPosition, DragOffsetType.NONE);
				break;
			case PickControllerModel.Mode.MagnetFinger:
				magnetFingerSignal.Dispatch(3, inputPosition);
				break;
			case PickControllerModel.Mode.EnvironmentalMignette:
				if (model.EndHitObject != null)
				{
					environmentalMignetteTappedSignal.Dispatch(model.EndHitObject);
				}
				break;
			case PickControllerModel.Mode.LandExpansion:
				landExpansionSignal.Dispatch(3, inputPosition);
				break;
			case PickControllerModel.Mode.TikiBarView:
				TikiBarViewClick();
				break;
			case PickControllerModel.Mode.StageView:
				StageViewClick();
				break;
			case PickControllerModel.Mode.VillainIsland:
				if (model.EndHitObject != null)
				{
					VillainIslandClick();
				}
				break;
			case PickControllerModel.Mode.VillainLair:
				VillainLairClick();
				break;
			case PickControllerModel.Mode.None:
			case PickControllerModel.Mode.Minion:
				NoneClick();
				break;
			}
		}

		private void ZoomOutOfBuilding()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, zoomCameraModel.LastZoomBuildingType));
			}
		}

		private void TikiBarViewClick()
		{
			if (!model.DetectedMovement)
			{
				GameObject endHitObject = model.EndHitObject;
				if (endHitObject != null)
				{
					tikiBarViewPickSignal.Dispatch(endHitObject);
				}
			}
		}

		public void SkipStagePerformance()
		{
			showHUDSignal.Dispatch(true);
			showStoreSignal.Dispatch(true);
			ZoomOutOfBuilding();
		}

		private void StageViewClick()
		{
			if (timedSocialEventService.isRewardCutscene())
			{
				SkipStagePerformance();
			}
			else
			{
				ZoomOutOfBuilding();
			}
		}

		private void VillainIslandClick()
		{
			if (!model.DetectedMovement)
			{
				villainIslandMessageSignal.Dispatch(true);
			}
		}

		private void NoneClick()
		{
			if (model.DetectedMovement)
			{
				return;
			}
			GameObject startHitObject = model.StartHitObject;
			GameObject endHitObject = model.EndHitObject;
			if (startHitObject != null && endHitObject != null && startHitObject == endHitObject)
			{
				minionSignal.Dispatch(model.EndHitObject);
				return;
			}
			Vector3 xZProjection = gameContext.injectionBinder.GetInstance<CameraUtils>().GroundPlaneRaycast(inputPosition);
			Point p = default(Point);
			p.XZProjection = xZProjection;
			Environment instance = gameContext.injectionBinder.GetInstance<Environment>();
			if (!instance.Contains(p) || !instance.IsWalkable(p.x, p.y))
			{
				model.ValidLocation = false;
			}
		}

		private void MoveMinion(Vector3 groundPosition)
		{
			if (model.MinionMoveToIndicator == null)
			{
				model.MinionMoveToIndicator = KampaiResources.Load("MinionMoveToIndicator");
			}
			if (model.minionMoveIndicator != null)
			{
				Object.Destroy(model.minionMoveIndicator);
			}
			model.minionMoveIndicator = Object.Instantiate(model.MinionMoveToIndicator) as GameObject;
			model.minionMoveIndicator.transform.position = groundPosition;
		}

		private void TouchStartHitBuilding()
		{
			if (model.SelectedBuilding.HasValue)
			{
				Ray ray = camera.ScreenPointToRay(inputPosition);
				if (Physics.Raycast(ray, out hit, float.PositiveInfinity, 16384))
				{
					model.StartHitObject = hit.collider.gameObject;
				}
				BuildingDefinitionObject component = model.StartHitObject.GetComponent<BuildingDefinitionObject>();
				if (component != null)
				{
					int? selectedBuilding = model.SelectedBuilding;
					if ((selectedBuilding.GetValueOrDefault() == component.ID && selectedBuilding.HasValue) || (component.ID == 0 && model.SelectedBuilding == -1))
					{
						model.CurrentMode = PickControllerModel.Mode.DragAndDrop;
						dragAndDropSignal.Dispatch(1, inputPosition, DragOffsetType.NONE);
					}
				}
			}
			else
			{
				model.CurrentMode = PickControllerModel.Mode.Building;
			}
		}

		private void VillainLairClick()
		{
			lairEnvironmentElementClickedSignal.Dispatch(model.StartHitObject.GetInstanceID());
		}

		public void SetIgnoreInstanceInput(int instanceId, bool isIgnored)
		{
			model.SetIgnoreInstance(instanceId, isIgnored);
		}

		public PickState GetPickState()
		{
			PickState pickState = new PickState();
			pickState.MinionsSelected = new List<int>(model.SelectedMinions.Keys);
			return pickState;
		}
	}
}
