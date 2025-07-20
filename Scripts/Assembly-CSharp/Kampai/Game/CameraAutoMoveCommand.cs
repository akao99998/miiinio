using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class CameraAutoMoveCommand : Command
	{
		private float zoomPercentage;

		private ScreenPosition screenPosition;

		[Inject]
		public Vector3 position { get; set; }

		[Inject]
		public Boxed<ScreenPosition> boxedScreenPosition { get; set; }

		[Inject]
		public CameraMovementSettings modalInfo { get; set; }

		[Inject]
		public bool absolutePosition { get; set; }

		[Inject]
		public CameraAutoZoomSignal autoZoomSignal { get; set; }

		[Inject]
		public CameraAutoPanSignal autoPanSignal { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		[Inject]
		public ShowHiddenBuildingsSignal showHiddenBuildingsSignal { get; set; }

		[Inject]
		public CancelAutozoomSignal cancelAutozoomSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public BuildingZoomSignal buildingZoomSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public GameObject mainCameraGO { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		private void SetupZoomCameraModel()
		{
			zoomCameraModel.PreviousCameraPosition = new Vector3(position.x, Mathf.Lerp(30f, 13f, zoomPercentage), position.z);
			zoomCameraModel.PreviousCameraRotation = new Vector3(Mathf.Lerp(55f, 25f, zoomPercentage), zoomCameraModel.PreviousCameraRotation.y, zoomCameraModel.PreviousCameraRotation.z);
			zoomCameraModel.PreviousCameraFieldOfView = Mathf.Lerp(40f, 9f, zoomPercentage);
			modalInfo.cameraSpeed = 1f;
		}

		public override void Execute()
		{
			CalculatePosition();
			showHiddenBuildingsSignal.Dispatch();
			if ((model.CurrentBehaviours & 8) == 8)
			{
				return;
			}
			pickModel.PanningCameraBlocked = true;
			if (zoomCameraModel.ZoomedIn || zoomCameraModel.ZoomInProgress)
			{
				BuildingZoomSettings type = new BuildingZoomSettings(ZoomType.OUT, zoomCameraModel.LastZoomBuildingType);
				if (zoomCameraModel.LastZoomBuildingType == BuildingZoomType.ORDERBOARD)
				{
					SetupZoomCameraModel();
				}
				buildingZoomSignal.Dispatch(type);
			}
			cancelAutozoomSignal.Dispatch();
			autoPanSignal.Dispatch(position, modalInfo, new Boxed<Building>(modalInfo.building), new Boxed<Quest>(modalInfo.quest));
			if (zoomPercentage > 0f)
			{
				autoZoomSignal.Dispatch(zoomPercentage);
			}
			if (pickModel.PanningCameraBlocked || pickModel.ZoomingCameraBlocked)
			{
				playSoundFXSignal.Dispatch("Play_low_woosh_01");
			}
			if (modalInfo.settings != CameraMovementSettings.Settings.KeepUIOpen)
			{
				GetUISignal<CloseAllOtherMenuSignal>().Dispatch(null);
			}
		}

		private T GetUISignal<T>()
		{
			return uiContext.injectionBinder.GetInstance<T>();
		}

		private void CalculatePosition()
		{
			if (boxedScreenPosition != null)
			{
				screenPosition = boxedScreenPosition.Value;
			}
			if (screenPosition == null)
			{
				screenPosition = new ScreenPosition();
			}
			zoomPercentage = screenPosition.zoom;
			if (!absolutePosition)
			{
				Vector3 vector = cameraUtils.GroundPlaneRaycast(screenPosition.x, screenPosition.z);
				Vector3 pointInSpace = position;
				Vector3 vector2 = cameraUtils.GroundPlaneRaycastFromPoint(pointInSpace);
				Vector3 vector3 = vector2 - vector;
				Vector3 vector4 = mainCameraGO.transform.position;
				position = new Vector3(vector4.x + vector3.x, vector4.y, vector4.z + vector3.z);
			}
		}
	}
}
