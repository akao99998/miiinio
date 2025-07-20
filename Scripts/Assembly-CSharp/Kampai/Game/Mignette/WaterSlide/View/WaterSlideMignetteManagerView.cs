using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using FMOD.Studio;
using Kampai.Common.Service.Audio;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.Mignette.WaterSlide.View
{
	public class WaterSlideMignetteManagerView : MignetteManagerView
	{
		private const float MUSIC_PROGRESSION_INCREMENTER = 5f;

		private IKampaiLogger logger = LogManager.GetClassLogger("WaterSlideMignetteManagerView") as IKampaiLogger;

		private WaterSlideBuildingViewObject buildingViewReference;

		private PathAgent pathRider;

		private MinionObject minionObject;

		private Transform minionTransform;

		private Animator minionAnimator;

		private Transform minionParent;

		private GameObject minionGO;

		private bool rollDiveRoulette;

		private int diveIndex;

		private bool diveSelected;

		private float diveTotalTime = 2f;

		private CustomFMOD_StudioEventEmitter waterEmitter;

		public bool isGameOver;

		private WaterslideSpinnerViewObject spinnerViewObject;

		private Dictionary<int, int> diveScoreMap = new Dictionary<int, int>();

		internal GameObject mignetteDooberGO;

		private float diveElapsedTime;

		private bool introSequencePlaying;

		[Inject]
		public StopMignetteSignal stopMignette { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnMignetteDooberSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public WaterSlideMignettePathCompletedSignal pathCompletedSignal { get; set; }

		[Inject]
		public WaterslideMignetteOnDiveTriggerSignal diveTriggerSignal { get; set; }

		[Inject]
		public WaterslideMignetteOnPlayDiveTriggerSignal diveAnimationSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalAudioSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal localAudioSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			buildingViewReference = MignetteBuildingObject.GetComponent<WaterSlideBuildingViewObject>();
			if (buildingViewReference == null)
			{
				logger.Error("Couldn't find building ref component");
				return;
			}
			if (!TryInitializeScoreValues())
			{
				logger.Error("Couldn't initialize the score values");
				return;
			}
			InititalizePathRider();
			InititalizeMinion();
			stopMignette.AddListener(OnGameOver);
			pathCompletedSignal.AddListener(OnPathComplete);
			diveTriggerSignal.AddListener(OnDisplayDiveButton);
			diveAnimationSignal.AddListener(OnPlayDiveAnimation);
			pathRider.pathCompletedSignal = pathCompletedSignal;
			pathRider.diveTriggerSignal = diveTriggerSignal;
			pathRider.playDiveAnimation = diveAnimationSignal;
			SaveCurrentCameraPosition();
			SetCameraMoveTime(1f);
			StartCoroutine(StartAnimationSequence());
			TotalEventTime = diveTotalTime;
			spinnerViewObject = GetComponentInChildren<WaterslideSpinnerViewObject>();
			spinnerViewObject.transform.parent = base.mignetteCamera.transform;
			spinnerViewObject.transform.localPosition = buildingViewReference.SpinnerBallOffset;
			spinnerViewObject.transform.forward = base.mignetteCamera.transform.forward;
			spinnerViewObject.gameObject.SetActive(false);
			waterEmitter = base.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
			waterEmitter.shiftPosition = false;
			waterEmitter.staticSound = false;
			waterEmitter.path = fmodService.GetGuid("Play_minion_ski_01");
			waterEmitter.startEventOnAwake = false;
		}

		private void OnPlayDiveAnimation()
		{
			minionObject.SetAnimInteger("InDive", diveIndex + 1);
		}

		private void InititalizeMinion()
		{
			minionObject = MignetteBuildingObject.GetChildMinion(0).Minion;
			minionGO = minionObject.gameObject;
			minionParent = minionGO.transform.parent;
			Transform transform = minionGO.transform;
			transform.SetParent(pathRider.MinionHardpoint);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(Vector3.zero);
			pathRider.MinionObject = minionObject;
			minionAnimator = minionGO.GetComponent<Animator>();
			minionTransform = transform;
			minionObject.EnableBlobShadow(false);
		}

		private void InititalizePathRider()
		{
			PathController componentInChildren = GetComponentInChildren<PathController>();
			pathRider = GetComponentInChildren<PathAgent>();
			pathRider.View = this;
			pathRider.Path = componentInChildren;
		}

		private bool TryInitializeScoreValues()
		{
			if (buildingViewReference == null)
			{
				return false;
			}
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(MignetteBuildingObject.ID);
			if (byInstanceId == null)
			{
				return false;
			}
			MignetteBuildingDefinition definition = byInstanceId.Definition;
			if (definition == null)
			{
				return false;
			}
			IList<MignetteRuleDefinition> mignetteRules = definition.MignetteRules;
			if (mignetteRules == null)
			{
				return false;
			}
			int count = mignetteRules.Count;
			int num = 4;
			if (count != num)
			{
				logger.Warning("Mismatch between number of dives and rule definitions.  Should there be a unique score for each dive?");
			}
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = mignetteRules[i].EffectAmount;
			}
			Array.Sort(array);
			for (int j = 0; j < num; j++)
			{
				if (j < array.Length)
				{
					diveScoreMap.Add(j, array[j]);
				}
			}
			return true;
		}

		private IEnumerator StartAnimationSequence()
		{
			yield return null;
			pathRider.CameraController.SetCamera(base.mignetteCamera);
			pathRider.BuildPath();
			Transform pathRiderTransform = pathRider.transform;
			pathRiderTransform.position = buildingViewReference.ClimbPoint.transform.position;
			pathRiderTransform.rotation = buildingViewReference.ClimbPoint.transform.rotation;
			minionObject.PlayAnimation(Animator.StringToHash("ClimbLadder"), 0, 0f);
			introSequencePlaying = true;
			pathRider.VFXManager.DisplayMinionWake(false);
		}

		public void EnableWaterAudio(bool enable)
		{
			if (enable)
			{
				if (waterEmitter.getPlaybackState() != 0)
				{
					waterEmitter.Play();
				}
			}
			else if (waterEmitter.getPlaybackState() == PLAYBACK_STATE.PLAYING)
			{
				waterEmitter.Stop();
			}
		}

		private void BeginGame()
		{
			waterEmitter.Play();
			pathRider.SetAtPathStart();
			pathRider.StartFollowPath();
		}

		public void OnDisplayDiveButton(bool startPlay)
		{
			if (startPlay && !isGameOver)
			{
				TimeElapsed = 0f;
				rollDiveRoulette = true;
				spinnerViewObject.gameObject.SetActive(true);
				spinnerViewObject.StartIntro(playGlobalAudioSignal);
			}
		}

		public void OnScreenTapped()
		{
			if (rollDiveRoulette && !isGameOver)
			{
				OnGameButtonPushed();
			}
		}

		private void OnGameButtonPushed(bool playerInput = true)
		{
			if (!rollDiveRoulette)
			{
				return;
			}
			rollDiveRoulette = false;
			float animationPct = spinnerViewObject.GetAnimationPct();
			diveIndex = (int)Mathf.Lerp(0f, 4f, animationPct);
			if (!playerInput)
			{
				diveIndex = 0;
			}
			if (!diveSelected)
			{
				Action<int> action = delegate(int newScore)
				{
					soundFXSignal.Dispatch("Play_mignette_collect");
					spawnMignetteDooberSignal.Dispatch(mignetteHUD, minionGO.transform.position, newScore, true);
					changeScoreSignal.Dispatch(newScore);
				};
				int value = 0;
				if (diveScoreMap.TryGetValue(diveIndex, out value))
				{
					action(value);
				}
				else if (diveScoreMap.Count > 0)
				{
					action(diveScoreMap[0]);
				}
				spinnerViewObject.SelectDive(diveIndex);
			}
			diveSelected = true;
		}

		public override void Update()
		{
			base.Update();
			if (base.IsPaused || isGameOver)
			{
				return;
			}
			if (introSequencePlaying)
			{
				minionTransform.localPosition = Vector3.zero;
				if (minionAnimator != null && minionAnimator.GetCurrentAnimatorStateInfo(0).IsName("IntroComplete"))
				{
					minionObject.PlayAnimation(Animator.StringToHash("Sliding"), 0, 0f);
					introSequencePlaying = false;
					BeginGame();
				}
			}
			else if (rollDiveRoulette)
			{
				diveElapsedTime += Time.deltaTime;
				if (diveElapsedTime <= diveTotalTime)
				{
					TimeElapsed += Time.deltaTime;
				}
				else
				{
					OnGameButtonPushed(false);
				}
			}
		}

		protected override void OnDestroy()
		{
			if (minionGO != null)
			{
				minionGO.transform.parent = minionParent;
			}
			if (buildingViewReference != null && !playerService.HasPurchasedMinigamePack())
			{
				buildingViewReference.UpdateCooldownView(localAudioSignal, 0, 0f);
			}
			base.OnDestroy();
			UnityEngine.Object.Destroy(pathRider);
			UnityEngine.Object.Destroy(spinnerViewObject);
			stopMignette.RemoveListener(OnGameOver);
			diveTriggerSignal.RemoveListener(OnDisplayDiveButton);
			pathCompletedSignal.RemoveListener(OnPathComplete);
			diveAnimationSignal.RemoveListener(OnPlayDiveAnimation);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void ResetMignetteObjects()
		{
			isGameOver = true;
			spinnerViewObject.gameObject.SetActive(false);
			Go.killAllTweensWithTarget(Camera.main.transform);
			if (minionObject != null)
			{
				minionObject.EnableBlobShadow(true);
			}
			if (mignetteDooberGO != null)
			{
				mignetteDooberGO = null;
			}
		}

		private void OnGameOver(bool isInterrupted)
		{
		}

		private void OnPathComplete()
		{
			requestStopMignetteSignal.Dispatch(true);
		}
	}
}
