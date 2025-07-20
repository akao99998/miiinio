using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class PanMediator : Mediator, CameraMediator
	{
		protected bool blocked;

		protected float previousFraction;

		protected int toReenable;

		protected bool isAutoPanning;

		[Inject]
		public ICameraControlsService cameraControlsService { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public CameraAutoPanSignal autoPanSignal { get; set; }

		[Inject]
		public CameraCinematicPanSignal cinematicPanSignal { get; set; }

		[Inject]
		public ShowBuildingDetailMenuSignal showDetailMenuSignal { get; set; }

		[Inject]
		public ShowQuestPanelSignal showQuestPanelSignal { get; set; }

		[Inject]
		public ShowQuestRewardSignal showQuestRewardSignal { get; set; }

		[Inject]
		public ShowProceduralQuestPanelSignal showProceduralQuestSignal { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		[Inject]
		public CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal { get; set; }

		[Inject]
		public CameraResetPanVelocitySignal cameraResetPanVelocitySignal { get; set; }

		[Inject]
		public StopAutopanSignal stopAutopanSignal { get; set; }

		[Inject]
		public DeviceInformation deviceInformation { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		[Inject]
		public MignetteCallMinionsSignal mignetteCallMinionsSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		public float Fraction { get; set; }

		public override void OnRegister()
		{
			cameraControlsService.RegisterListener(OnGameInput);
			disableCameraSignal.AddListener(OnDisableBehaviour);
			enableCameraSignal.AddListener(OnEnableBehaviour);
			autoPanSignal.AddListener(OnAutoPan);
			cinematicPanSignal.AddListener(OnCinematicPan);
			cameraResetPanVelocitySignal.AddListener(OnResetPanVelocity);
			stopAutopanSignal.AddListener(OnStopAutopan);
		}

		public override void OnRemove()
		{
			cameraControlsService.UnregisterListener(OnGameInput);
			disableCameraSignal.RemoveListener(OnDisableBehaviour);
			enableCameraSignal.RemoveListener(OnEnableBehaviour);
			autoPanSignal.RemoveListener(OnAutoPan);
			cinematicPanSignal.RemoveListener(OnCinematicPan);
			cameraResetPanVelocitySignal.RemoveListener(OnResetPanVelocity);
			stopAutopanSignal.RemoveListener(OnStopAutopan);
		}

		private void OnStopAutopan()
		{
			isAutoPanning = false;
		}

		public virtual void OnResetPanVelocity()
		{
		}

		public virtual void OnGameInput(Vector3 position, int input)
		{
		}

		public virtual void Uninitialize()
		{
		}

		public virtual void OnDisableBehaviour(int behaviour)
		{
		}

		public virtual void OnEnableBehaviour(int behaviour)
		{
		}

		public virtual void OnAutoPan(Vector3 panTo, CameraMovementSettings modalSettings, Boxed<Building> building, Boxed<Quest> quest)
		{
			OnCinematicPan(Tuple.Create(panTo, modalSettings.cameraSpeed), modalSettings, building, quest);
		}

		public virtual void OnCinematicPan(Tuple<Vector3, float> panInfo, CameraMovementSettings modalSettings, Boxed<Building> building, Boxed<Quest> quest)
		{
			CacheCameraPosition();
		}

		public virtual void ReenablePickService()
		{
			pickModel.PanningCameraBlocked = false;
		}

		protected void OnComplete()
		{
			cameraAutoPanCompleteSignal.Dispatch();
		}

		public virtual void SetupAutoPan(Vector3 panTo)
		{
		}

		public virtual void PerformAutoPan(float delta)
		{
		}

		public void ShowMenu(CameraMovementSettings modalSettings, Building building, Quest quest)
		{
			if (modalSettings.bypassModal)
			{
				mignetteCallMinionsSignal.Dispatch();
			}
			else if (modalSettings.settings == CameraMovementSettings.Settings.ShowMenu)
			{
				showDetailMenuSignal.Dispatch(building);
			}
			else if (modalSettings.settings == CameraMovementSettings.Settings.Quest)
			{
				if (quest.GetActiveDefinition().SurfaceType == QuestSurfaceType.ProcedurallyGenerated)
				{
					showProceduralQuestSignal.Dispatch(quest.ID);
				}
				else if (quest.state == QuestState.Harvestable || quest.state == QuestState.Complete)
				{
					showQuestRewardSignal.Dispatch(quest.ID);
				}
				else
				{
					showQuestPanelSignal.Dispatch(quest.ID);
				}
			}
		}

		protected void CacheCameraPosition()
		{
			if (zoomCameraModel.LastResourceZoomRotation.CompareTo(0f) == 0)
			{
				zoomCameraModel.LastResourceZoomRotation = mainCamera.transform.localEulerAngles.x;
			}
		}

		protected int GetFingerID()
		{
			int num = ((Input.touchCount <= 0) ? (-1) : Input.GetTouch(0).fingerId);
			if (num == -1 && (deviceInformation.IsSamsung() || Application.isEditor) && Input.GetMouseButton(0))
			{
				num = int.MaxValue;
			}
			return num;
		}
	}
}
