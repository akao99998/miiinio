using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.Mignette.View
{
	public class MignetteManagerView : strange.extensions.mediation.impl.View
	{
		private sealed class CameraParams
		{
			public Vector3 position;

			public Quaternion rotation;

			public float fov;

			public float nearClip;
		}

		protected MignetteHUDView mignetteHUD;

		public MignetteBuildingObject MignetteBuildingObject;

		public MignetteBuildingViewObject MignetteReferenceBuildingViewObject;

		private float CameraMoveTimer;

		private float CameraMoveTime;

		private bool DestroyMignetteAfterCameraMovement;

		private bool ShowScore;

		private bool cameraIsMoving;

		private CameraParams TargetParams = new CameraParams();

		private CameraParams ResetParams = new CameraParams();

		private CameraParams StartParams = new CameraParams();

		public float TimeElapsed;

		public float TotalEventTime;

		public float PercentCompleted;

		protected bool useCountDown = true;

		protected float preCountdownDelay = 3f;

		protected float countdownTimer = 3f;

		private bool countDownSignalDispatched;

		protected bool shutdownInProgress;

		private static bool IsPlaying;

		private GameObject dooberInstance;

		[Inject]
		public ShowAllWayFindersSignal ShowAllWayFindersSignal { get; set; }

		[Inject]
		public ShowAllResourceIconsSignal ShowAllResourceIconsSignal { get; set; }

		[Inject]
		public HideAllResourceIconsSignal HideAllResourceIconsSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mignetteCamera { get; set; }

		[Inject]
		public MignetteEndedSignal mignetteEndedSignal { get; set; }

		[Inject]
		public StartMignetteHUDCountdownSignal startMignetteHUDCountdownSignal { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		public bool IsPaused { get; protected set; }

		protected override void Start()
		{
			SetIsPlaying(true);
			HideAllResourceIconsSignal.Dispatch();
			base.Start();
			ResetParams.position = mignetteCamera.transform.position;
			ResetParams.rotation = mignetteCamera.transform.rotation;
			ResetParams.fov = mignetteCamera.fieldOfView;
			ResetParams.nearClip = mignetteCamera.nearClipPlane;
			CameraMoveTimer = CameraMoveTime;
			MignetteReferenceBuildingViewObject = MignetteBuildingObject.GetComponent<MignetteBuildingViewObject>();
			if (MignetteReferenceBuildingViewObject != null)
			{
				preCountdownDelay = MignetteReferenceBuildingViewObject.PreCountdownDelay;
				useCountDown = MignetteReferenceBuildingViewObject.UseCountDownTimer;
				countdownTimer = 3f;
				countDownSignalDispatched = false;
			}
			else
			{
				preCountdownDelay = 0f;
				countdownTimer = 0f;
				countDownSignalDispatched = true;
			}
			for (int i = 0; i < MignetteBuildingObject.GetMignetteMinionCount(); i++)
			{
				TaskingMinionObject childMinion = MignetteBuildingObject.GetChildMinion(i);
				childMinion.Minion.EnableRenderers(true);
			}
			mignetteHUD = glassCanvas.transform.GetComponentInChildren<MignetteHUDView>();
			dooberInstance = Object.Instantiate(KampaiResources.Load<GameObject>("NumberedDoober"));
		}

		protected override void OnDestroy()
		{
			ShowAllWayFindersSignal.Dispatch();
			ShowAllResourceIconsSignal.Dispatch();
			Object.Destroy(dooberInstance);
			base.OnDestroy();
			SetIsPlaying(false);
		}

		protected void SetCameraMoveTime(float t)
		{
			CameraMoveTime = t;
		}

		protected void SaveCurrentCameraPosition()
		{
			StartParams.position = mignetteCamera.transform.position;
			StartParams.rotation = mignetteCamera.transform.rotation;
			StartParams.fov = mignetteCamera.fieldOfView;
			StartParams.nearClip = mignetteCamera.nearClipPlane;
		}

		protected void RelocateCameraForMignette(Transform newTransform, float fieldOfView, float nearClip, float duration)
		{
			SaveCurrentCameraPosition();
			if (newTransform != null)
			{
				TargetParams.position = newTransform.position;
				TargetParams.rotation = newTransform.rotation;
				TargetParams.fov = fieldOfView;
				TargetParams.nearClip = nearClip;
				CameraMoveTimer = 0f;
				CameraMoveTime = duration;
				cameraIsMoving = true;
			}
		}

		public virtual void Update()
		{
			if (IsPaused)
			{
				return;
			}
			if (preCountdownDelay > 0f)
			{
				preCountdownDelay -= Time.deltaTime;
			}
			else if (useCountDown && countdownTimer > 0f)
			{
				if (!countDownSignalDispatched)
				{
					countDownSignalDispatched = true;
					startMignetteHUDCountdownSignal.Dispatch();
				}
				countdownTimer -= Time.deltaTime;
			}
		}

		public virtual void LateUpdate()
		{
			if (!IsPaused && cameraIsMoving)
			{
				UpdateMignetteCamera();
			}
		}

		private void UpdateMignetteCamera()
		{
			if (!(CameraMoveTimer < CameraMoveTime))
			{
				return;
			}
			CameraMoveTimer += Time.deltaTime;
			if (CameraMoveTimer >= CameraMoveTime)
			{
				CameraMoveTimer = CameraMoveTime;
				CameraTransitionComplete();
				if (DestroyMignetteAfterCameraMovement)
				{
					mignetteEndedSignal.Dispatch(ShowScore);
					shutdownInProgress = true;
				}
				cameraIsMoving = false;
			}
			float t = CameraMoveTimer / CameraMoveTime;
			mignetteCamera.nearClipPlane = Mathf.Lerp(StartParams.nearClip, TargetParams.nearClip, t);
			mignetteCamera.transform.position = Vector3.Lerp(StartParams.position, TargetParams.position, t);
			mignetteCamera.transform.rotation = Quaternion.Slerp(StartParams.rotation, TargetParams.rotation, t);
			mignetteCamera.fieldOfView = Mathf.Lerp(StartParams.fov, TargetParams.fov, t);
		}

		protected virtual void CameraTransitionComplete()
		{
		}

		public void ResetCameraAndStopMignette(bool showScore)
		{
			StartParams.position = mignetteCamera.transform.position;
			StartParams.rotation = mignetteCamera.transform.rotation;
			StartParams.fov = mignetteCamera.fieldOfView;
			StartParams.nearClip = mignetteCamera.nearClipPlane;
			ShowScore = showScore;
			DestroyMignetteAfterCameraMovement = true;
			TargetParams.position = ResetParams.position;
			TargetParams.rotation = ResetParams.rotation;
			TargetParams.fov = ResetParams.fov;
			TargetParams.nearClip = ResetParams.nearClip;
			CameraMoveTimer = 0f;
			cameraIsMoving = true;
		}

		public virtual void OnMignettePause(bool isPaused)
		{
			IsPaused = isPaused;
		}

		public static bool GetIsPlaying()
		{
			return IsPlaying;
		}

		private static void SetIsPlaying(bool isPlaying)
		{
			IsPlaying = isPlaying;
		}
	}
}
