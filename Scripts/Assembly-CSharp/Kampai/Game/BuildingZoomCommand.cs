using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class BuildingZoomCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("BuildingZoomCommand") as IKampaiLogger;

		private BuildingZoomType buildingZoomType;

		[Inject]
		public BuildingZoomSettings zoomBuildingSettings { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public ToggleHitboxSignal toggleHitboxSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public CameraModel model { get; set; }

		[Inject]
		public CameraResetPanVelocitySignal cameraResetPanVelocitySignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public StageService stageService { get; set; }

		[Inject]
		public FTUETikiOpened ftueSignal { get; set; }

		[Inject]
		public CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal cameraAutoMoveToBuildingSignal { get; set; }

		[Inject]
		public FadeBackgroundAudioSignal fadeBackgroundAudioSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		public override void Execute()
		{
			ZoomType zoomType = zoomBuildingSettings.ZoomType;
			buildingZoomType = zoomBuildingSettings.ZoomBuildingType;
			logger.Debug("We are going to zoom {0} {1} building", zoomType, buildingZoomType);
			switch (zoomType)
			{
			case ZoomType.IN:
				Zoom(true);
				break;
			case ZoomType.OUT:
				Zoom(false);
				break;
			default:
				logger.Fatal(FatalCode.EX_INVALID_ENUM);
				break;
			}
		}

		private void Zoom(bool zoomIn)
		{
			if (zoomCameraModel.ZoomInProgress)
			{
				CallCallback();
				return;
			}
			if (zoomCameraModel.ZoomedIn && zoomIn)
			{
				CallCallback();
				return;
			}
			if (!zoomCameraModel.ZoomedIn && !zoomIn)
			{
				CallCallback();
				return;
			}
			if (!zoomIn && zoomCameraModel.LastZoomBuildingType != buildingZoomType)
			{
				CallCallback();
				return;
			}
			zoomCameraModel.LastZoomBuildingType = buildingZoomType;
			OnZoomBegin(zoomIn);
			if (buildingZoomType == BuildingZoomType.TIKIBAR)
			{
				ZoomTikiBar(zoomIn);
			}
			else if (buildingZoomType == BuildingZoomType.STAGE)
			{
				StageBuilding byInstanceId = playerService.GetByInstanceId<StageBuilding>(370);
				ZoomBuilding(zoomIn, byInstanceId);
			}
			else if (buildingZoomType == BuildingZoomType.ORDERBOARD)
			{
				OrderBoard byInstanceId2 = playerService.GetByInstanceId<OrderBoard>(309);
				ZoomBuilding(zoomIn, byInstanceId2);
			}
		}

		private void ZoomTikiBar(bool zoomIn)
		{
			fadeBackgroundAudioSignal.Dispatch(!zoomIn, "Play_tikiBar_snapshotDuck_01");
			if (zoomIn)
			{
				ftueSignal.Dispatch();
				int toReenable = model.CurrentBehaviours;
				disableCameraSignal.Dispatch(toReenable);
				TikiBarBuilding byInstanceId = playerService.GetByInstanceId<TikiBarBuilding>(313);
				cameraAutoPanCompleteSignal.AddOnce(delegate
				{
					enableCameraSignal.Dispatch(toReenable);
					OnZoomEnd(zoomIn);
				});
				cameraAutoMoveToBuildingSignal.Dispatch(byInstanceId, new PanInstructions(byInstanceId));
			}
			else
			{
				OnZoomEnd(zoomIn);
			}
		}

		private void ZoomBuilding(bool zoomIn, ZoomableBuilding building)
		{
			fadeBackgroundAudioSignal.Dispatch(!zoomIn, "Play_tikiBar_snapshotDuck_01");
			int toReenable = model.CurrentBehaviours;
			disableCameraSignal.Dispatch(toReenable);
			if (zoomIn)
			{
				OrderBoard orderBoard = building as OrderBoard;
				if (orderBoard != null)
				{
					SetHitbox(false);
				}
				if (building is StageBuilding)
				{
					toReenable = 0;
				}
				zoomCameraModel.PreviousCameraPosition = mainCamera.transform.position;
				zoomCameraModel.PreviousCameraRotation = mainCamera.transform.localEulerAngles;
				zoomCameraModel.PreviousCameraFieldOfView = mainCamera.fieldOfView;
			}
			else if (building is StageBuilding)
			{
				stageService.HideStageBackdrop();
				toReenable = 3;
			}
			PositionTweenProperty position = new PositionTweenProperty((!zoomIn) ? zoomCameraModel.PreviousCameraPosition : zoomCameraModel.GetZoomedCameraPosition(building));
			AbstractTweenProperty abstractTweenProperty = null;
			abstractTweenProperty = ((!zoomIn) ? ((AbstractTweenProperty)new RotationTweenProperty(zoomCameraModel.PreviousCameraRotation)) : ((AbstractTweenProperty)new RotationQuaternionTweenProperty(zoomCameraModel.GetZoomedCameraRotation(building))));
			GoTweenFlow goTweenFlow = CreateFlow(position, abstractTweenProperty, (!zoomIn) ? zoomCameraModel.PreviousCameraFieldOfView : zoomCameraModel.GetZoomedFOV(building));
			goTweenFlow.setOnCompleteHandler(delegate
			{
				if (zoomBuildingSettings.EnableCamera)
				{
					enableCameraSignal.Dispatch(toReenable);
				}
				OnZoomEnd(zoomIn);
				OrderBoard orderBoard2 = building as OrderBoard;
				if (!zoomIn && orderBoard2 != null && orderBoard2.HarvestableCharacterDefinitionId != 0)
				{
					Prestige prestige = prestigeService.GetPrestige(orderBoard2.HarvestableCharacterDefinitionId);
					prestigeService.PostOrderCompletion(prestige);
				}
			});
			goTweenFlow.play();
		}

		private GoTweenFlow CreateFlow(AbstractTweenProperty position, AbstractTweenProperty rotation, float fieldOfView)
		{
			GoTweenConfig config = new GoTweenConfig().addTweenProperty(position).addTweenProperty(rotation).setEaseType(GoEaseType.SineOut);
			GoTweenConfig config2 = new GoTweenConfig().floatProp("fieldOfView", fieldOfView).setEaseType(GoEaseType.SineOut);
			GoTween tween = new GoTween(mainCamera, 1f, config2);
			GoTween tween2 = new GoTween(mainCamera.transform, 1f, config);
			return new GoTweenFlow().insert(0f, tween2).insert(0f, tween);
		}

		private void CallCallback()
		{
			if (zoomBuildingSettings.OnComplete != null)
			{
				zoomBuildingSettings.OnComplete();
			}
		}

		private void OnZoomBegin(bool zoomIn)
		{
			if (zoomIn)
			{
				cameraResetPanVelocitySignal.Dispatch();
				HideUI();
				SetHitbox(false);
			}
			playSoundFXSignal.Dispatch("Play_low_woosh_01");
			zoomCameraModel.ZoomInProgress = true;
		}

		private void OnZoomEnd(bool zoomIn)
		{
			zoomCameraModel.ZoomedIn = zoomIn;
			zoomCameraModel.ZoomInProgress = false;
			if (buildingZoomType == BuildingZoomType.ORDERBOARD && zoomIn)
			{
				ShowUI();
			}
			if (!zoomIn)
			{
				ShowUI();
				SetHitbox(true);
			}
			CallCallback();
		}

		private void HideUI()
		{
			closeSignal.Dispatch(null);
			ToggleUI(false);
		}

		private void ShowUI()
		{
			ToggleUI(true);
		}

		private void ToggleUI(bool enable)
		{
			if (!playerService.GetMinionPartyInstance().IsPartyHappening)
			{
				showHUDSignal.Dispatch(enable);
				showStoreSignal.Dispatch(enable);
			}
		}

		private void SetHitbox(bool enabled)
		{
			toggleHitboxSignal.Dispatch(buildingZoomType, enabled);
		}
	}
}
