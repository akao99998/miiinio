using System;
using Kampai.Game.Mignette.AlligatorSkiing;
using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorAgent : MonoBehaviour
{
	private const string CHOMP_ANIM_STATE_NAME = "Chomp";

	private const string CHOMP_EXIT_ANIM_STATE_NAME = "ChompExit";

	private const string SLOWSWIM_ANIM_STATE_NAME = "SlowSwim";

	private const string FASTSWIM_ANIM_STATE_NAME = "Swimming";

	public const float MIN_MINION_DISTANCE = 4f;

	public const float MAX_MINION_DISTANCE = 5f;

	public const float MINION_SPEED_ADJUSTMENT = 0.025f;

	public const float MINION_SLOWTOSTOP_ADJUSTMENT = 0.01f;

	public Animator AlligatorAnimator;

	public GameObject FishingPoleGO;

	public Animator FishingPoleAnimator;

	public Animator MinionAnimator;

	public AlligatorWaypointController Waypoints;

	public float StartingAlligatorSpeed = 5f;

	public Transform MinionRiderTransform;

	public AlligatorVFXManager VFXManager;

	public AlligatorSkiingJumpController JumpController;

	public Transform MinionHardpointTransform;

	public float SinkRate = -1f;

	public float SinkDelay = 0.2f;

	public float MinionTurnAmount;

	public bool Initialized;

	public Transform AlligatorLineTransform;

	public Action StartGameViewCallback;

	public GameObject HamGO;

	private GoSpline alligatorSpline;

	private GoSpline minionSpline;

	private float totalPathLength;

	private float alligatorTraveledDistance;

	private float currrentAlligatorSpeed = 1f;

	private float alligatorMaxSpeed = 1f;

	private float alligatorAcceleration = 1f;

	private float totalMinionPathLength;

	private float totalMinionPathTime;

	private float minionElapsedTravelTime;

	private float minionSpeedCoefficient = 1f;

	private LineRenderer _ropeRenderer;

	private GoTween minionRotationTween;

	private Transform FishingPoleTransform;

	private bool gameOver;

	private bool minionAttachedToAlligator;

	private bool updateAlligatorPosition;

	private bool updateMinionPosition;

	public bool introChompAnimationPlaying;

	private GoTween introTween;

	public AlligatorMignettePathCompletedSignal pathCompletedSignal { get; set; }

	public AlligatorMignetteMinionHitObstacleSignal hitObstacleSignal { get; set; }

	public AlligatorMignetteMinionHitCollectableSignal hitCollectableSignal { get; set; }

	public AlligatorMignetteJumpLandedSignal jumpLandedSignal { get; set; }

	public AlligatorSkiingMignetteManagerView View { get; set; }

	private void Start()
	{
		_ropeRenderer = GetComponent<LineRenderer>();
		GameObject fishingPoleGO = FishingPoleGO;
		FishingPoleGO = UnityEngine.Object.Instantiate(fishingPoleGO);
		FishingPoleGO.transform.parent = JumpController.transform;
		FishingPoleGO.transform.localPosition = Vector3.zero;
		FishingPoleAnimator = FishingPoleGO.GetComponent<Animator>();
		FishingPoleTransform = FishingPoleGO.FindChild("LineAnchor").transform;
		FishingPoleGO.GetComponent<AlligatorHamController>().DisplayHam(true);
		_ropeRenderer.enabled = false;
	}

	private void Update()
	{
		if (!Initialized || View.IsPaused)
		{
			return;
		}
		if (introChompAnimationPlaying && (AlligatorAnimator.GetCurrentAnimatorStateInfo(0).IsName("ChompExit") || AlligatorAnimator.GetCurrentAnimatorStateInfo(0).IsName("Swimming")))
		{
			StartAlligator();
			introChompAnimationPlaying = false;
			Invoke("OnStartGame", 0.5f);
		}
		if (!gameOver)
		{
			if (minionAttachedToAlligator)
			{
				MaintainMinionDistance();
			}
			if (alligatorSpline != null && updateAlligatorPosition)
			{
				UpdateAlligatorPosition();
			}
			if (minionSpline != null && updateMinionPosition)
			{
				UpdateMinionPosition();
			}
		}
	}

	public void LateUpdate()
	{
		if (Initialized && !View.IsPaused && MinionHardpointTransform != null)
		{
			_ropeRenderer.SetPosition(0, FishingPoleTransform.position);
			_ropeRenderer.SetPosition(1, AlligatorLineTransform.position);
		}
	}

	public void StartAlligator()
	{
		updateAlligatorPosition = true;
	}

	public void AttachMinionAndGo()
	{
		minionAttachedToAlligator = true;
		updateMinionPosition = true;
		_ropeRenderer.enabled = true;
	}

	public void InitializePaths()
	{
		alligatorSpline = Waypoints.GetAlligatorSpline();
		totalPathLength = alligatorSpline.pathLength;
		currrentAlligatorSpeed = StartingAlligatorSpeed;
		alligatorMaxSpeed = StartingAlligatorSpeed;
		minionSpline = Waypoints.GetMinionSpline();
		totalMinionPathLength = minionSpline.pathLength;
		totalMinionPathTime = totalMinionPathLength / StartingAlligatorSpeed;
	}

	public float GetPctComplete()
	{
		return alligatorTraveledDistance / totalPathLength;
	}

	public void SetMinionRiderParent()
	{
		MinionRiderTransform.SetParent(base.transform.parent);
	}

	private void UpdateAlligatorPosition()
	{
		if (currrentAlligatorSpeed < alligatorMaxSpeed)
		{
			if (currrentAlligatorSpeed + alligatorAcceleration > alligatorMaxSpeed)
			{
				currrentAlligatorSpeed = alligatorMaxSpeed;
			}
			else
			{
				currrentAlligatorSpeed += alligatorAcceleration;
			}
		}
		else if (currrentAlligatorSpeed > alligatorMaxSpeed)
		{
			if (currrentAlligatorSpeed - alligatorAcceleration < alligatorMaxSpeed)
			{
				currrentAlligatorSpeed = alligatorMaxSpeed;
			}
			else
			{
				currrentAlligatorSpeed -= alligatorAcceleration;
			}
		}
		alligatorTraveledDistance += Time.deltaTime * currrentAlligatorSpeed;
		float t = alligatorTraveledDistance / totalPathLength;
		Vector3 pointOnPath = alligatorSpline.getPointOnPath(t);
		base.transform.rotation = ((!(pointOnPath == base.transform.position)) ? Quaternion.LookRotation(pointOnPath - base.transform.position) : Quaternion.identity);
		base.transform.position = pointOnPath;
	}

	private void UpdateMinionPosition()
	{
		minionElapsedTravelTime += Time.deltaTime * minionSpeedCoefficient;
		float num = minionElapsedTravelTime / totalMinionPathTime;
		Vector3 pointOnPath = minionSpline.getPointOnPath(num);
		MinionRiderTransform.rotation = ((!(pointOnPath == MinionRiderTransform.position)) ? Quaternion.LookRotation(pointOnPath - MinionRiderTransform.position) : Quaternion.identity);
		MinionRiderTransform.position = pointOnPath;
		if (num >= 1f)
		{
			pathCompletedSignal.Dispatch();
			gameOver = true;
		}
	}

	private void MaintainMinionDistance()
	{
		if (Vector3.Distance(base.transform.position, MinionRiderTransform.position) > 5f)
		{
			minionSpeedCoefficient += 0.025f;
		}
		else if (Vector3.Distance(base.transform.position, MinionRiderTransform.position) < 4f)
		{
			minionSpeedCoefficient -= 0.025f;
		}
		else
		{
			minionSpeedCoefficient = 1f;
		}
	}

	public void AttachMinionToPathrider(GameObject minionGO)
	{
		minionGO.transform.parent = MinionHardpointTransform;
		minionGO.transform.localPosition = Vector3.zero;
		minionGO.transform.localRotation = Quaternion.Euler(Vector3.zero);
		MinionRiderTransform.position = minionSpline.getPointOnPath(0f);
		MinionRiderTransform.rotation = Quaternion.LookRotation(minionSpline.getPointOnPath(0.1f) - minionSpline.getPointOnPath(0f));
		minionGO.SetActive(true);
	}

	public void OnMinionHitObstacle()
	{
		hitObstacleSignal.Dispatch();
		VFXManager.PlayImpactVfx(MinionRiderTransform.position);
	}

	public void OnMinionLand()
	{
		VFXManager.DisplayMinionWake(true);
		jumpLandedSignal.Dispatch();
	}

	public void OnMinionJump()
	{
		VFXManager.DisplayMinionWake(false);
	}

	public void InputDown()
	{
		JumpController.DoJump();
	}

	public void InputUp()
	{
		JumpController.JumpReleased();
	}

	public void OnMinionHitCollectable(Vector3 collectablePosition, int collectablePoints, CollectibleType type)
	{
		hitCollectableSignal.Dispatch(collectablePosition, collectablePoints, type);
	}

	public void OnRotateMinion(Quaternion rot)
	{
		if (!JumpController.IsJumping() && !rot.Equals(MinionHardpointTransform.localRotation))
		{
			if (minionRotationTween != null && minionRotationTween.state == GoTweenState.Running)
			{
				Go.removeTween(minionRotationTween);
			}
			GoTweenConfig goTweenConfig = new GoTweenConfig();
			RotationQuaternionTweenProperty tweenProp = new RotationQuaternionTweenProperty(rot, false, true);
			goTweenConfig.addTweenProperty(tweenProp);
			minionRotationTween = new GoTween(MinionHardpointTransform, 0.25f, goTweenConfig);
			Go.addTween(minionRotationTween);
		}
	}

	public void ChangeSpeed(float Speed, float acceleration)
	{
		alligatorMaxSpeed = Speed;
		alligatorAcceleration = acceleration;
	}

	public void TrySetAnimationState(string AnimationStateName)
	{
		AlligatorAnimator.Play(Animator.StringToHash(AnimationStateName), 0, 0f);
	}

	public void OnStartGame()
	{
		AlligatorAnimator.Play(Animator.StringToHash("Swimming"), 0);
		StartGameViewCallback();
	}

	public void IntroPointArrived(AbstractGoTween tween)
	{
		AlligatorAnimator.Play(Animator.StringToHash("Chomp"), 0);
		View.PlayMinionPulledIn();
		introChompAnimationPlaying = true;
	}

	public void SwimToHam()
	{
		if (introTween != null && introTween.state == GoTweenState.Running)
		{
			Go.removeTween(introTween);
		}
		HamGO.SetActive(false);
		AlligatorAnimator.Play(Animator.StringToHash("SlowSwim"), 0);
		base.transform.LookAt(Waypoints.StartingHampoint);
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
		AlligatorAnimator.Play(Animator.StringToHash("SlowSwim"), 0);
		GoTweenConfig goTweenConfig = new GoTweenConfig();
		PositionTweenProperty tweenProp = new PositionTweenProperty(Waypoints.StartingHampoint.position);
		goTweenConfig.addTweenProperty(tweenProp);
		goTweenConfig.onComplete(IntroPointArrived);
		introTween = new GoTween(base.transform, 1f, goTweenConfig);
		Go.addTween(introTween);
	}

	public void OnAlligatorBiteHam()
	{
		FishingPoleGO.GetComponent<AlligatorHamController>().DisplayHam(false);
		HamGO.SetActive(true);
		_ropeRenderer.enabled = true;
	}
}
