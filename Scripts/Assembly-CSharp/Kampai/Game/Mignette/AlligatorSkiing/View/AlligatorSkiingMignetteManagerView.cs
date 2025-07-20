using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common.Service.Audio;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.Mignette.AlligatorSkiing.View
{
	public class AlligatorSkiingMignetteManagerView : MignetteManagerView
	{
		private const string FISHING_ANIM_STATE_NAME = "Fishing";

		private const string CAST_ANIM_TRIGGER_NAME = "Casting";

		private const string PULLIN_ANIM_TRIGGER_NAME = "OnPulledIn";

		private const string JUMP_ANIM_TRIGGER_NAME = "OnJump";

		private const string CRASH_ANIM_TRIGGER_NAME = "OnCrash";

		private const string STUMBLE_ANIM_TRIGGER_NAME = "OnStumble";

		private const float OBSTACLE_DEFAULT_PENALTY = 1.5f;

		private const float OBSTACLE_FAILUP_PENALTY = 0.25f;

		private const float MUSIC_PROGRESS_INCREMENTER = 5f;

		private IKampaiLogger logger = LogManager.GetClassLogger("AlligatorSkiingMignetteManagerView") as IKampaiLogger;

		private AlligatorSkiingBuildingViewObject buildingViewReference;

		private AlligatorWaypointController waypointsController;

		private AlligatorAgent alligatorAgent;

		private GameObject MinionShadow;

		private Transform cameraTransform;

		private Transform cameraMarker;

		private Transform alligatorTransform;

		private Transform minionParentTransform;

		private bool isGameStarted;

		public bool isGameOver;

		private MinionObject minion;

		private bool jumping;

		private bool obstaclePenalty;

		private float obstacleTimer = 1.5f;

		private float obstacleElapsedTime;

		private float randomGrowlTimer = 7f;

		private float growlElapsedTime;

		private Dictionary<MinionObject, Vector3> crowdMinions = new Dictionary<MinionObject, Vector3>();

		private CustomFMOD_StudioEventEmitter waterEmitter;

		private CustomFMOD_StudioEventEmitter minionSoundsEmitter;

		private bool initialized;

		private Transform startingCameraParentTransform;

		private bool firstCheckpointPassed;

		[Inject]
		public AlligatorMignettePathCompletedSignal pathCompleteSignal { get; set; }

		[Inject]
		public AlligatorMignetteMinionHitObstacleSignal hitObstacleSignal { get; set; }

		[Inject]
		public AlligatorMignetteMinionHitCollectableSignal hitCollectableSignal { get; set; }

		[Inject]
		public AlligatorMignetteJumpLandedSignal jumpLandedSignal { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnMignetteDooberSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		public bool IsOnPenalty
		{
			get
			{
				return obstaclePenalty;
			}
		}

		protected override void Start()
		{
			TimeProfiler.StartMonoProfiler("alligator");
			base.Start();
			buildingViewReference = MignetteBuildingObject.GetComponent<AlligatorSkiingBuildingViewObject>();
			if (buildingViewReference == null)
			{
				logger.Error("Couldn't find building ref component");
				return;
			}
			pathCompleteSignal.AddListener(OnPathComplete);
			hitObstacleSignal.AddListener(OnObstableHit);
			hitCollectableSignal.AddListener(OnCollectable);
			jumpLandedSignal.AddListener(OnJumpLanded);
			SaveCurrentCameraPosition();
			SetCameraMoveTime(1f);
			StartCoroutine(InitializeSetup());
		}

		private void InitializeCamera()
		{
			cameraMarker = waypointsController.FollowCameraMarker;
			cameraMarker.SetParent(base.gameObject.transform, false);
			cameraTransform = base.mignetteCamera.transform;
			startingCameraParentTransform = cameraTransform.parent;
			cameraMarker.position = cameraTransform.position;
			cameraMarker.rotation = cameraTransform.rotation;
			cameraTransform.SetParent(cameraMarker, false);
			cameraTransform.localPosition = Vector3.zero;
			cameraTransform.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}

		private IEnumerator InitializeSetup()
		{
			yield return null;
			if (!isGameOver)
			{
				waypointsController = base.transform.parent.GetComponentInChildren<AlligatorWaypointController>();
				alligatorAgent = GetComponentInChildren<AlligatorAgent>();
				alligatorTransform = alligatorAgent.transform;
				alligatorTransform.position = waypointsController.StartingWaypoint.position;
				alligatorTransform.rotation = waypointsController.StartingWaypoint.rotation;
				alligatorAgent.View = this;
				alligatorAgent.Waypoints = waypointsController;
				alligatorAgent.pathCompletedSignal = pathCompleteSignal;
				alligatorAgent.hitObstacleSignal = hitObstacleSignal;
				alligatorAgent.hitCollectableSignal = hitCollectableSignal;
				alligatorAgent.jumpLandedSignal = jumpLandedSignal;
				alligatorAgent.StartGameViewCallback = OnStartGame;
				alligatorAgent.VFXManager.DisplayMinionWake(false);
				minion = MignetteBuildingObject.GetChildMinion(0).Minion;
				GameObject minionGO = minion.gameObject;
				minionParentTransform = minionGO.transform.parent;
				alligatorAgent.Initialized = true;
				alligatorAgent.SetMinionRiderParent();
				alligatorAgent.InitializePaths();
				if (minion.GetBlobShadow() != null)
				{
					MinionShadow = Object.Instantiate(minion.GetBlobShadow()) as GameObject;
					MinionShadow.transform.parent = alligatorAgent.MinionRiderTransform;
					MinionShadow.transform.localPosition = Vector3.zero;
					MinionShadow.SetActive(false);
					minion.EnableBlobShadow(false);
				}
				waterEmitter = alligatorAgent.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
				waterEmitter.shiftPosition = false;
				waterEmitter.staticSound = false;
				waterEmitter.path = fmodService.GetGuid("Play_minion_ski_01");
				waterEmitter.startEventOnAwake = false;
				minionSoundsEmitter = alligatorAgent.MinionRiderTransform.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
				minionSoundsEmitter.startEventOnAwake = false;
				minionSoundsEmitter.shiftPosition = false;
				minionSoundsEmitter.staticSound = false;
				minion.DisableSelection();
				InitializeCamera();
				MoveCameraToStartPosition();
				alligatorAgent.AttachMinionToPathrider(minion.gameObject);
				minion.PlayAnimation(Animator.StringToHash("Fishing"), 0, 0f);
				alligatorAgent.FishingPoleAnimator.Play(Animator.StringToHash("Fishing"), 0, 0f);
				StartCoroutine(DelayAlligatorIntro());
				initialized = true;
			}
		}

		private IEnumerator DelayAlligatorIntro()
		{
			yield return new WaitForSeconds(1f);
			minion.PlayAnimation(Animator.StringToHash("Casting"), 0, 0f);
			alligatorAgent.FishingPoleAnimator.Play(Animator.StringToHash("Casting"), 0, 0f);
			yield return new WaitForSeconds(2f);
			alligatorAgent.SwimToHam();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			pathCompleteSignal.RemoveListener(OnPathComplete);
			hitObstacleSignal.RemoveListener(OnObstableHit);
			hitCollectableSignal.RemoveListener(OnCollectable);
		}

		public override void Update()
		{
			base.Update();
			if (initialized && !base.IsPaused && !shutdownInProgress)
			{
				TimeElapsed += Time.deltaTime;
				if (obstaclePenalty)
				{
					UpdateClearObstaclePenalty();
				}
				if (!isGameOver && isGameStarted)
				{
					UpdateAlligatorGrowl();
					UpdateGameCamera();
				}
				PercentCompleted = alligatorAgent.GetPctComplete();
			}
		}

		private void UpdateAlligatorGrowl()
		{
			growlElapsedTime += Time.deltaTime;
			if (growlElapsedTime >= randomGrowlTimer)
			{
				randomGrowlTimer = Random.Range(5f, 10f);
				growlElapsedTime = 0f;
				globalAudioSignal.Dispatch("Play_alligator_pull_growl_01");
			}
		}

		private void UpdateClearObstaclePenalty()
		{
			obstacleElapsedTime += Time.deltaTime;
			if (obstacleTimer < obstacleElapsedTime)
			{
				obstaclePenalty = false;
			}
		}

		public void ResetMignetteObjects()
		{
			isGameOver = true;
			if (!initialized)
			{
				return;
			}
			Go.killAllTweensWithTarget(Camera.main.transform);
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.MINION_MANAGER);
			MinionManagerMediator component = instance.GetComponent<MinionManagerMediator>();
			foreach (KeyValuePair<MinionObject, Vector3> crowdMinion in crowdMinions)
			{
				component.stopTaskSignal.Dispatch(crowdMinion.Key.ID);
				crowdMinion.Key.GetAgent().enabled = true;
				crowdMinion.Key.setLocation(crowdMinion.Value);
				crowdMinion.Key.Idle();
			}
			cameraTransform.SetParent(startingCameraParentTransform);
			MignetteBuildingObject.GetChildMinion(0).Minion.gameObject.transform.parent = minionParentTransform;
			if (minion != null)
			{
				minion.EnableBlobShadow(true);
			}
		}

		public void OnPathComplete()
		{
			requestStopMignetteSignal.Dispatch(true);
			waterEmitter.Stop();
			isGameOver = true;
			TimeProfiler.StopMonoProfiler();
		}

		public void OnObstableHit()
		{
			if (isGameOver)
			{
				return;
			}
			if (obstaclePenalty)
			{
				jumping = false;
				if (obstacleTimer - obstacleElapsedTime < 0.25f)
				{
					obstacleTimer = 0.25f;
					obstacleElapsedTime = 0f;
				}
				minion.SetAnimTrigger("OnStumble");
				alligatorAgent.FishingPoleAnimator.SetTrigger("OnStumble");
			}
			else
			{
				obstaclePenalty = true;
				obstacleTimer = 1.5f;
				obstacleElapsedTime = 0f;
				minion.SetAnimTrigger("OnCrash");
				alligatorAgent.FishingPoleAnimator.SetTrigger("OnCrash");
				alligatorAgent.InputDown();
			}
			minionSoundsEmitter.Stop();
			minionSoundsEmitter.path = fmodService.GetGuid("Play_minion_crash_01");
			minionSoundsEmitter.StartEvent();
		}

		public void OnCollectable(Vector3 collectablePosition, int collectablePoints, CollectibleType type)
		{
			if (!isGameOver)
			{
				if (!firstCheckpointPassed)
				{
					firstCheckpointPassed = true;
				}
				if (!obstaclePenalty && type != CollectibleType.Checkpoint)
				{
					globalAudioSignal.Dispatch("Play_mignette_collect");
					changeScoreSignal.Dispatch(collectablePoints);
					spawnMignetteDooberSignal.Dispatch(mignetteHUD, collectablePosition, collectablePoints, true);
				}
				else if (type == CollectibleType.Checkpoint)
				{
					globalAudioSignal.Dispatch("Play_mignette_checkpoint");
					changeScoreSignal.Dispatch(collectablePoints);
					spawnMignetteDooberSignal.Dispatch(mignetteHUD, collectablePosition, collectablePoints, true);
				}
			}
		}

		private void MoveCameraToStartPosition()
		{
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			PositionTweenProperty tweenProp = new PositionTweenProperty(buildingViewReference.IntroCamera.position);
			goTweenConfig.addTweenProperty(tweenProp);
			goTweenConfig.easeType = GoEaseType.QuadInOut;
			GoTween tween = new GoTween(cameraMarker, 1f, goTweenConfig);
			Go.addTween(tween);
		}

		public void PlayMinionPulledIn()
		{
			minion.SetAnimTrigger("OnPulledIn");
			alligatorAgent.FishingPoleAnimator.SetTrigger("OnPulledIn");
		}

		private void UpdateGameCamera()
		{
			if (!isGameOver)
			{
				Vector3 position = cameraMarker.position;
				Vector3 position2 = alligatorTransform.position;
				position2.y = position.y;
				position2.x += 15f;
				position2.z -= 15.5f;
				cameraMarker.position = Vector3.Lerp(position, position2, 2f * Time.deltaTime);
				Quaternion b = Quaternion.LookRotation(alligatorAgent.MinionRiderTransform.position - position);
				cameraMarker.rotation = Quaternion.Slerp(cameraMarker.rotation, b, 2f * Time.deltaTime);
			}
		}

		public void OnStartGame()
		{
			isGameStarted = true;
			alligatorAgent.AttachMinionAndGo();
			waterEmitter.Play();
			alligatorAgent.VFXManager.DisplayMinionWake(true);
		}

		public void OnInputDown()
		{
			if (isGameStarted && !jumping && !isGameOver && !obstaclePenalty)
			{
				jumping = true;
				minion.SetAnimTrigger("OnJump");
				alligatorAgent.FishingPoleAnimator.SetTrigger("OnJump");
				globalAudioSignal.Dispatch("Play_minion_ski_jump_01");
				waterEmitter.Stop();
				if (MinionShadow != null)
				{
					MinionShadow.SetActive(true);
				}
				alligatorAgent.InputDown();
			}
		}

		public void OnInputUp()
		{
			if (!isGameOver && jumping && !obstaclePenalty)
			{
				alligatorAgent.InputUp();
			}
		}

		public void OnJumpLanded()
		{
			jumping = false;
			if (MinionShadow != null)
			{
				MinionShadow.SetActive(false);
			}
			waterEmitter.StartEvent();
		}
	}
}
