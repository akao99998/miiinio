using System.Collections.Generic;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageMignetteManagerView : MignetteManagerView
	{
		private const string GROUND_IDLE_ANIM_STATE = "Base Layer.IdleGround";

		private const string PILOT_TAKE_MANGO_ANIM_STATE = "Base Layer.PilotTakeMango";

		private const string BASKET_PILOT_WALK_ANIM_STATE = "Base Layer.IntroMinionPilot";

		private const string BASKET_COPILOT_WALK_ANIM_STATE = "Base Layer.IntroMinionCoPilot";

		private const string PILOT_IDLE_ANIM_STATE = "Base Layer.PilotIdle";

		private const string PILOT_IDLE_MANGO_ANIM_STATE = "Base Layer.PilotIdleWithMango";

		private const string PILOT_THROW_RELEASE_ANIM_STATE = "Base Layer.PilotThrowMangoB";

		private const string COPILOT_WAIT_ANIM_STATE = "Base Layer.CopilotWaiting";

		private const string COPILOT_IDLE_RETURN_ANIM_STATE = "Base Layer.Minion2MangoGrabC";

		private const string GROUND_CATCH_ANIM_STATE = "Base Layer.CatchGround";

		private const string CATCH_ANIM_TRIGGER_NAME = "OnMinionCatch";

		private const string TRANSFER_ANIM_TRIGGER_NAME = "OnTransferMango";

		private const string TRANSFER_COMPLETE_ANIM_TRIGGER_NAME = "OnTransferMangoComplete";

		private const string WAVE_GROUND_ANIM_TRIGGER_NAME = "OnWaveGround";

		private const string THROW_ANIM_TRIGGER_NAME = "OnThrow";

		private const string INTRO_ANIM_TRIGGER_NAME = "OnIntro";

		private const string OUTRO_ANIM_TRIGGER_NAME = "OnOutro";

		private MinionObject pilotMinion;

		private MinionObject coPilotMinion;

		private Transform pilotThrowingHand;

		private bool isInPilotHand;

		private bool isThrowing;

		private Transform coPilotThrowingHand;

		private BalloonBarrageBuildingViewObject BuildingViewReference;

		private BalloonBarrageGameController gameController;

		private bool introComplete;

		private bool forceCameraToFollowCameraLocator;

		private bool pilotShadowsEnabled;

		private bool balloonIsLanding;

		private BalloonBarrageTargetAnimatorViewObject floatingMinionTarget;

		private List<BalloonBarrageTargetAnimatorViewObject> staticTargetsRemaining = new List<BalloonBarrageTargetAnimatorViewObject>();

		private int minionsUsedSoFar;

		private bool mangoInFlight;

		private Vector3 dragStartPoint = Vector3.zero;

		private Vector3 throwInputVector = Vector3.zero;

		private float groundMinionWaveTimer;

		private Vector3 lastInputPos;

		private Vector3 throwTarget;

		private Vector3 mangoThrowStart;

		private float throwForce;

		private Vector3 lastgroundPos;

		private List<BalloonBarrageTargetAnimatorViewObject> groundMinionsHit = new List<BalloonBarrageTargetAnimatorViewObject>();

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal localAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopLocalAudioSignal { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			BuildingViewReference = MignetteBuildingObject.GetComponent<BalloonBarrageBuildingViewObject>();
			gameController = base.gameObject.GetComponentInChildren<BalloonBarrageGameController>();
			BuildingViewReference.BalloonIsTakingOff = false;
			RelocateCameraForMignette(BuildingViewReference.CameraTransform, BuildingViewReference.FieldOfView, BuildingViewReference.NearClipPlane, 1f);
			pilotShadowsEnabled = true;
			TimeElapsed = 0f;
			TotalEventTime = 45f;
			minionsUsedSoFar = 0;
			pilotMinion = MignetteBuildingObject.GetChildMinion(minionsUsedSoFar).Minion;
			pilotThrowingHand = pilotMinion.gameObject.FindChild("minion:R_wrist_jnt").transform;
			minionsUsedSoFar++;
			pilotMinion.EnableRenderers(false);
			coPilotMinion = MignetteBuildingObject.GetChildMinion(minionsUsedSoFar).Minion;
			coPilotThrowingHand = coPilotMinion.gameObject.FindChild("minion:R_wrist_jnt").transform;
			minionsUsedSoFar++;
			coPilotMinion.EnableRenderers(false);
			SetupFloatingTarget();
			SetupStaticTargets();
			isInPilotHand = false;
			balloonIsLanding = false;
			groundMinionWaveTimer = 2f;
		}

		private void SetupFloatingTarget()
		{
			TaskingMinionObject childMinion = MignetteBuildingObject.GetChildMinion(minionsUsedSoFar);
			childMinion.Minion.EnableBlobShadow(false);
			GameObject gameObject = Object.Instantiate(BuildingViewReference.BasketPrefab);
			gameObject.transform.parent = base.transform;
			BalloonBarrageTargetAnimatorViewObject component = gameObject.GetComponent<BalloonBarrageTargetAnimatorViewObject>();
			component.Score = 25;
			component.GetComponent<Collider>().enabled = false;
			component.ShowMango(false);
			component.AddTarget(2);
			BalloonBarrageColliderViewObject[] componentsInChildren = component.GetComponentsInChildren<BalloonBarrageColliderViewObject>();
			BalloonBarrageColliderViewObject[] array = componentsInChildren;
			foreach (BalloonBarrageColliderViewObject balloonBarrageColliderViewObject in array)
			{
				balloonBarrageColliderViewObject.ParentTargetBalloonViewObject = component;
			}
			component.transform.position = gameController.FloatingMinionLocator.position;
			component.transform.forward = gameController.FloatingMinionLocator.forward;
			component.MinionToAnimate = childMinion.Minion;
			component.MyParentBuildingViewObject = BuildingViewReference;
			component.MyParentGameControllerObject = gameController;
			component.StartFloating(BuildingViewReference.transform.GetComponent<Collider>().bounds.min.z, BuildingViewReference.transform.GetComponent<Collider>().bounds.max.z, true, 1.5f);
			component.HoldPosition = true;
			component.ShowModel(false);
			floatingMinionTarget = component;
			minionsUsedSoFar++;
		}

		private void SetupStaticTargets()
		{
			groundMinionsHit.Clear();
			staticTargetsRemaining.Clear();
			for (int i = 0; i < gameController.MinionStaticTargetLocators.Length; i++)
			{
				BalloonBarrageGameController.StaticBasketAndPoints staticBasketAndPoints = gameController.MinionStaticTargetLocators[i];
				GameObject[] basketLocators = staticBasketAndPoints.BasketLocators;
				foreach (GameObject gameObject in basketLocators)
				{
					if (MignetteBuildingObject.GetMignetteMinionCount() > minionsUsedSoFar)
					{
						TaskingMinionObject childMinion = MignetteBuildingObject.GetChildMinion(minionsUsedSoFar);
						childMinion.Minion.EnableBlobShadow(false);
						minionsUsedSoFar++;
						GameObject gameObject2 = Object.Instantiate(BuildingViewReference.BasketPrefab);
						gameObject2.transform.parent = base.transform;
						gameObject2.transform.position = gameObject.transform.position;
						Vector3 vector = BuildingViewReference.CameraTransform.position - gameObject2.transform.position;
						vector.y = 0f;
						gameObject2.transform.forward = vector.normalized;
						BalloonBarrageColliderViewObject[] componentsInChildren = gameObject2.GetComponentsInChildren<BalloonBarrageColliderViewObject>();
						BalloonBarrageColliderViewObject[] array = componentsInChildren;
						foreach (BalloonBarrageColliderViewObject balloonBarrageColliderViewObject in array)
						{
							balloonBarrageColliderViewObject.gameObject.SetActive(false);
						}
						BalloonBarrageTargetAnimatorViewObject component = gameObject2.GetComponent<BalloonBarrageTargetAnimatorViewObject>();
						component.Score = staticBasketAndPoints.ScoreValue;
						component.MinionToAnimate = childMinion.Minion;
						component.SyncAnimationStates("Base Layer.IdleGround");
						staticTargetsRemaining.Add(component);
						component.ShowMango(false);
						component.AddTarget(staticBasketAndPoints.BasketMaterialIndex);
					}
				}
			}
		}

		protected override void CameraTransitionComplete()
		{
			if (!introComplete)
			{
				introComplete = true;
				MignetteBuildingObject.GetComponent<Animator>().SetTrigger("OnIntro");
				pilotMinion.PlayAnimation(Animator.StringToHash("Base Layer.IntroMinionPilot"), 0, 0f);
				coPilotMinion.PlayAnimation(Animator.StringToHash("Base Layer.IntroMinionCoPilot"), 0, 0f);
				forceCameraToFollowCameraLocator = true;
				Invoke("EnablePilots", 0.1f);
			}
		}

		public void EnablePilots()
		{
			pilotMinion.EnableRenderers(true);
			coPilotMinion.EnableRenderers(true);
		}

		public void ResetMignetteObjects()
		{
			MignetteBuildingObject.GetComponent<Animator>().SetTrigger("OnOutro");
			balloonIsLanding = true;
			BuildingViewReference.UpdateCooldownView(localAudioSignal, 0, 0f);
		}

		public bool MangoHitMovingTarget(BalloonBarrageMangoViewObject mango, BalloonBarrageColliderViewObject tvo)
		{
			if (tvo.ParentTargetBalloonViewObject == null || !tvo.ParentTargetBalloonViewObject.CanBeHit())
			{
				return false;
			}
			switch (tvo.TargetType)
			{
			case BalloonBarrageColliderViewObject.TargetTypes.Basket:
			{
				globalAudioSignal.Dispatch("Play_minion_balloon_catchMango_01");
				globalAudioSignal.Dispatch("Play_mignette_collect");
				tvo.ParentTargetBalloonViewObject.SetAnimTriggers("OnMinionCatch");
				changeScoreSignal.Dispatch(tvo.ParentTargetBalloonViewObject.Score);
				tvo.ParentTargetBalloonViewObject.ShowMango(true);
				spawnDooberSignal.Dispatch(mignetteHUD, mango.transform.position, tvo.ParentTargetBalloonViewObject.Score, true);
				GameObject gameObject = Object.Instantiate(BuildingViewReference.MangoCaughtVfxPrefab);
				gameObject.transform.SetParent(base.transform, false);
				Vector3 position = mango.transform.position;
				gameObject.transform.position = position;
				Object.Destroy(gameObject, 5f);
				break;
			}
			case BalloonBarrageColliderViewObject.TargetTypes.Minion:
				globalAudioSignal.Dispatch("Play_minion_balloon_hitFace_01");
				tvo.ParentTargetBalloonViewObject.MinionWasHit();
				break;
			case BalloonBarrageColliderViewObject.TargetTypes.Balloon:
				globalAudioSignal.Dispatch("Play_balloon_pop_01");
				tvo.ParentTargetBalloonViewObject.PlayFallAnimation(mango.transform.position);
				break;
			}
			return true;
		}

		public void MangoHasBeenResolved(bool hitGround)
		{
			mangoInFlight = false;
			if (hitGround && !isThrowing && !isInPilotHand)
			{
				globalAudioSignal.Dispatch("Play_mango_splat_01");
			}
		}

		public void MangoHitStaticTarget(BalloonBarrageMangoViewObject mango, BalloonBarrageTargetAnimatorViewObject avo)
		{
			avo.SetAnimTriggers("OnMinionCatch");
			avo.GetComponent<Collider>().enabled = false;
			avo.ShowMango(true);
			globalAudioSignal.Dispatch("Play_mignette_collect");
			globalAudioSignal.Dispatch("Play_minion_balloon_catchMango_01");
			groundMinionsHit.Add(avo);
			staticTargetsRemaining.Remove(avo);
			if (staticTargetsRemaining.Count <= 0)
			{
				floatingMinionTarget.ShowModel(true);
				floatingMinionTarget.HoldPosition = false;
			}
			changeScoreSignal.Dispatch(avo.Score);
			spawnDooberSignal.Dispatch(mignetteHUD, mango.transform.position, avo.Score, true);
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (!base.IsPaused)
			{
				UpdatePilotPosition();
				if (pilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.PilotThrowMangoB")) && isThrowing)
				{
					ThrowAMango();
				}
			}
		}

		private void UpdatePilotPosition()
		{
			if (!shutdownInProgress)
			{
				if (pilotShadowsEnabled && BuildingViewReference.BalloonIsTakingOff)
				{
					pilotShadowsEnabled = false;
					pilotMinion.EnableBlobShadow(false);
					coPilotMinion.EnableBlobShadow(false);
				}
				Vector3 position = BuildingViewReference.BalloonPilotIntroLocator.position;
				Quaternion rotation = BuildingViewReference.BalloonPilotIntroLocator.rotation;
				pilotMinion.transform.position = position;
				pilotMinion.transform.rotation = rotation;
				coPilotMinion.transform.position = position;
				coPilotMinion.transform.rotation = rotation;
				GameObject mangoToShowForPrepareThrow = GetMangoToShowForPrepareThrow();
				if (isInPilotHand)
				{
					mangoToShowForPrepareThrow.transform.position = pilotThrowingHand.position - pilotThrowingHand.up * 0.1f;
					mangoToShowForPrepareThrow.transform.rotation = pilotThrowingHand.rotation;
				}
				else
				{
					mangoToShowForPrepareThrow.transform.position = coPilotThrowingHand.position - coPilotThrowingHand.up * 0.1f;
					mangoToShowForPrepareThrow.transform.rotation = coPilotThrowingHand.rotation;
				}
			}
		}

		public override void Update()
		{
			base.Update();
			if (base.IsPaused)
			{
				return;
			}
			if (forceCameraToFollowCameraLocator)
			{
				base.mignetteCamera.transform.position = BuildingViewReference.CameraTransform.position;
				base.mignetteCamera.transform.rotation = BuildingViewReference.CameraTransform.rotation;
			}
			if (TimeElapsed >= TotalEventTime || shutdownInProgress || balloonIsLanding)
			{
				return;
			}
			if (introComplete)
			{
				if (pilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.PilotTakeMango")) && coPilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.Minion2MangoGrabC")))
				{
					isInPilotHand = true;
					TriggerForBothInBasket("OnTransferMangoComplete");
				}
				if (coPilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.CopilotWaiting")))
				{
					GetMangoToShowForPrepareThrow().SetActive(true);
					if (pilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.PilotIdle")))
					{
						TriggerForBothInBasket("OnTransferMango");
					}
				}
			}
			if (countdownTimer > 0f)
			{
				return;
			}
			if (staticTargetsRemaining.Count > 0)
			{
				groundMinionWaveTimer -= Time.deltaTime;
				if (groundMinionWaveTimer <= 0f)
				{
					BalloonBarrageTargetAnimatorViewObject balloonBarrageTargetAnimatorViewObject = staticTargetsRemaining[Random.Range(0, staticTargetsRemaining.Count)];
					balloonBarrageTargetAnimatorViewObject.SetAnimTriggers("OnWaveGround");
					groundMinionWaveTimer = Random.Range(2f, 10f);
				}
			}
			TimeElapsed += Time.deltaTime;
			if (TimeElapsed >= TotalEventTime)
			{
				TimeElapsed = 45f;
				Invoke("ShutDownMignette", 2f);
			}
			UpdateGroundMinionLocations();
		}

		private void UpdateGroundMinionLocations()
		{
			List<BalloonBarrageTargetAnimatorViewObject> list = new List<BalloonBarrageTargetAnimatorViewObject>();
			foreach (BalloonBarrageTargetAnimatorViewObject item in groundMinionsHit)
			{
				if (item.IsInState("Base Layer.CatchGround"))
				{
					BalloonBarrageTargetAnimatorViewObject thisAvo = item;
					list.Add(thisAvo);
					Vector3 endValue = thisAvo.transform.position + thisAvo.transform.forward * 10f;
					Go.to(thisAvo.gameObject.transform, 3f, new GoTweenConfig().setEaseType(GoEaseType.SineIn).position(endValue).setDelay(thisAvo.WalkoffAnimDelay)
						.onComplete(delegate(AbstractGoTween thisTween)
						{
							thisAvo.ShowModel(false);
							thisTween.destroy();
						}));
				}
			}
			foreach (BalloonBarrageTargetAnimatorViewObject item2 in list)
			{
				groundMinionsHit.Remove(item2);
			}
		}

		public void OnPress(Vector3 pos, bool pressed)
		{
			if (!pilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.PilotIdleWithMango")) || TimeElapsed >= TotalEventTime || isThrowing || countdownTimer > 0f)
			{
				return;
			}
			if (pressed)
			{
				Ray ray = base.mignetteCamera.ScreenPointToRay(pos);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 8192))
				{
					dragStartPoint = pos;
				}
			}
			else
			{
				if (!(dragStartPoint != Vector3.zero))
				{
					return;
				}
				if (throwInputVector != Vector3.zero)
				{
					isThrowing = true;
					TriggerForBothInBasket("OnThrow");
					lastgroundPos = gameContext.injectionBinder.GetInstance<CameraUtils>().GroundPlaneRaycast(lastInputPos);
					mangoThrowStart = base.mignetteCamera.transform.position;
					throwTarget = lastgroundPos;
					throwTarget.y = mangoThrowStart.y;
					Vector3 throwVector = throwTarget - mangoThrowStart;
					throwForce = CalculateForceForInput(throwVector);
					float num = Vector3.Dot(pilotMinion.transform.forward, throwVector.normalized);
					num = 1f - num;
					if (Vector3.Dot(pilotMinion.transform.right, throwVector.normalized) < 0f)
					{
						num *= -1f;
					}
					float state = MapInterval(num, -0.5f, 0.5f, -1f, 1f);
					pilotMinion.SetAnimFloat("Direction", state);
				}
				dragStartPoint = Vector3.zero;
			}
		}

		public void OnPressed(Vector3 pos)
		{
			if (TimeElapsed >= TotalEventTime || isThrowing || countdownTimer > 0f)
			{
				return;
			}
			throwInputVector = pos - dragStartPoint;
			bool flag = true;
			if (throwInputVector.magnitude < 2f)
			{
				flag = false;
			}
			switch (BuildingViewReference.BalloonBarrageThrowType)
			{
			case BalloonBarrageBuildingViewObject.BalloonBarrageThrowTypes.Pull:
				if (throwInputVector.y > 0f)
				{
					flag = false;
				}
				break;
			case BalloonBarrageBuildingViewObject.BalloonBarrageThrowTypes.Push:
				if (throwInputVector.y < 0f)
				{
					flag = false;
				}
				break;
			}
			if (mangoInFlight || !pilotMinion.IsInAnimatorState(Animator.StringToHash("Base Layer.PilotIdleWithMango")))
			{
				flag = false;
			}
			if (!flag)
			{
				throwInputVector = Vector3.zero;
			}
			else
			{
				lastInputPos = pos;
			}
		}

		private float MapInterval(float val, float srcMin, float srcMax, float dstMin, float dstMax)
		{
			if (val >= srcMax)
			{
				return dstMax;
			}
			if (val <= srcMin)
			{
				return dstMin;
			}
			return dstMin + (val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin);
		}

		public float CalculateForceForInput(Vector3 throwVector)
		{
			return MapInterval(throwVector.magnitude, 2f, 8f, 2000f, 7000f);
		}

		public void ThrowAMango()
		{
			isThrowing = false;
			if (!mangoInFlight)
			{
				GameObject gameObject = Object.Instantiate(BuildingViewReference.MangoPrefab);
				gameObject.transform.parent = base.transform;
				BalloonBarrageMangoViewObject component = gameObject.GetComponent<BalloonBarrageMangoViewObject>();
				isInPilotHand = false;
				component.ThrowMango(this, GetMangoToShowForPrepareThrow(), lastgroundPos, throwForce);
				mangoInFlight = true;
			}
		}

		public void TriggerForBothInBasket(string triggerName)
		{
			pilotMinion.SetAnimTrigger(triggerName);
			coPilotMinion.SetAnimTrigger(triggerName);
		}

		public void ShutDownMignette()
		{
			forceCameraToFollowCameraLocator = false;
			requestStopMignetteSignal.Dispatch(true);
		}

		public void SilencePilots()
		{
			stopLocalAudioSignal.Dispatch(coPilotMinion.gameObject.GetComponent<CustomFMOD_StudioEventEmitter>());
			stopLocalAudioSignal.Dispatch(pilotMinion.gameObject.GetComponent<CustomFMOD_StudioEventEmitter>());
		}

		public GameObject GetMangoToShowForPrepareThrow()
		{
			return gameController.MangoToShowForPrepareThrow;
		}
	}
}
