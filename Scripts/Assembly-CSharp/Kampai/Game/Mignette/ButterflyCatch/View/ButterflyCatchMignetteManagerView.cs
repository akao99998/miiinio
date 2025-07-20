using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch.View
{
	public class ButterflyCatchMignetteManagerView : MignetteManagerView
	{
		public class MinionAndTimer
		{
			public MinionObject mo;

			public float timer;

			public float fallDownTimer;

			public float beeStingTimer;

			public ButterflyCatchMignetteNetViewObject netViewObject;
		}

		private const string SWING_ANIM_TRIGGER_NAME = "Swing";

		private const string BEE_CAUGHT_ANIM_FLOAT_NAME = "BeeStingTimer";

		private const string SWING_MISS_ANIM_FLOAT_NAME = "SwingAndMissTimer";

		private const string RUN_ANIM_TRIGGER_NAME = "Run";

		private const string SWING_ANIM_STATE_NAME = "Base Layer.Swing";

		private const string SUCCESS_ANIM_STATE_NAME = "Base Layer.Success";

		private const string RUN_ANIM_STATE_NAME = "Base Layer.Run";

		private const string IDLE_ANIM_STATE_NAME = "Base Layer.Idle";

		private ButterflyCatchBuildingViewObject BuildingViewReference;

		private float butterflySpawnTimer;

		private bool minionsIdle = true;

		public int totalScore;

		private List<MinionAndTimer> activeMinionsList = new List<MinionAndTimer>();

		private List<ButterflyCatchButterflyViewObject> activeButterflies = new List<ButterflyCatchButterflyViewObject>();

		private List<ButterflyCatchButterflyViewObject> activeBees = new List<ButterflyCatchButterflyViewObject>();

		private TaskingMinionObject ptmo;

		private ButterflyCatchGameController gameController;

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public MinionReactInRadiusSignal minionReactInRadiusSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			BuildingViewReference = MignetteBuildingObject.GetComponent<ButterflyCatchBuildingViewObject>();
			gameController = base.transform.parent.GetComponentInChildren<ButterflyCatchGameController>();
			butterflySpawnTimer = 2f;
			TimeElapsed = 0f;
			minionsIdle = true;
			TotalEventTime = 45f;
			int num = 0;
			for (num = 0; num < MignetteBuildingObject.GetMignetteMinionCount(); num++)
			{
				TaskingMinionObject childMinion = MignetteBuildingObject.GetChildMinion(num);
				AddColliderToMinion(childMinion);
				childMinion.Minion.EnableBlobShadow(true);
				MinionAndTimer minionAndTimer = new MinionAndTimer();
				minionAndTimer.mo = childMinion.Minion;
				minionAndTimer.timer = 0f;
				GoSpline minionSpline = gameController.GetMinionSpline(num);
				Vector3 pointOnPath = minionSpline.getPointOnPath(0f);
				minionAndTimer.mo.transform.position = pointOnPath;
				Vector3 vector = gameController.CameraTransform.position - minionAndTimer.mo.transform.position;
				vector.y = 0f;
				minionAndTimer.mo.transform.forward = vector.normalized;
				GameObject gameObject = gameController.SpawnNet();
				minionAndTimer.netViewObject = gameObject.GetComponent<ButterflyCatchMignetteNetViewObject>();
				minionAndTimer.netViewObject.transform.parent = base.transform;
				minionAndTimer.netViewObject.transform.position = minionAndTimer.mo.transform.position;
				minionAndTimer.netViewObject.transform.rotation = minionAndTimer.mo.transform.rotation;
				activeMinionsList.Add(minionAndTimer);
			}
			BuildingViewReference.ToggleAmbientButterflies(false);
			RelocateCameraForMignette(gameController.CameraTransform, gameController.CameraFieldOfView, gameController.CameraNearClipPlane, 1f);
		}

		private void AddColliderToMinion(TaskingMinionObject tmo)
		{
			Collider component = tmo.Minion.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			else
			{
				tmo.Minion.gameObject.AddComponent<CapsuleCollider>();
			}
			ptmo = tmo;
		}

		public void OnInputDown(Vector3 inputPosition)
		{
			if (TimeElapsed >= 45f)
			{
				return;
			}
			Ray ray = base.mignetteCamera.ScreenPointToRay(inputPosition);
			RaycastHit hitInfo;
			if (!Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 8192))
			{
				return;
			}
			MinionAndTimer minionObjectForCollider = GetMinionObjectForCollider(hitInfo.collider);
			bool flag = false;
			if (minionObjectForCollider.mo != null)
			{
				if (minionObjectForCollider.mo.IsInAnimatorState(Animator.StringToHash("Base Layer.Run")) || minionObjectForCollider.mo.IsInAnimatorState(Animator.StringToHash("Base Layer.Success")))
				{
					flag = true;
				}
				if (flag)
				{
					StartCoroutine(CollectFliersAroundMinion(minionObjectForCollider));
				}
			}
		}

		private MinionAndTimer GetMinionObjectForCollider(Collider collider)
		{
			int num = 0;
			for (num = 0; num < activeMinionsList.Count; num++)
			{
				MinionAndTimer minionAndTimer = activeMinionsList[num];
				if (collider.gameObject == minionAndTimer.netViewObject.gameObject)
				{
					return minionAndTimer;
				}
			}
			return null;
		}

		private IEnumerator CollectFliersAroundMinion(MinionAndTimer mat)
		{
			MinionObject mo = mat.mo;
			mat.netViewObject.StartSwipe();
			mo.SetAnimTrigger("Swing");
			mat.netViewObject.NetAnimator.SetTrigger("Swing");
			yield return new WaitForSeconds(mat.netViewObject.CatchDelay);
			int index = 0;
			for (index = 0; index < activeMinionsList.Count; index++)
			{
				MinionAndTimer mat2 = activeMinionsList[index];
				if (mat2 != mat)
				{
					Vector3 minion2AtMinionY = mat2.mo.transform.position;
					minion2AtMinionY.y = mo.transform.position.y;
					float dist2 = Vector3.Distance(mo.transform.position, minion2AtMinionY);
					if (dist2 <= 2f && mat2.beeStingTimer <= 0f)
					{
						mat2.fallDownTimer = 1.5f;
						globalAudioSignal.Dispatch("Play_balloon_pop_01");
					}
				}
			}
			List<ButterflyCatchButterflyViewObject> caughtFliers = new List<ButterflyCatchButterflyViewObject>();
			bool beeCaught = false;
			foreach (ButterflyCatchButterflyViewObject bvo4 in activeBees)
			{
				if (!bvo4.IsGettingCaught)
				{
					Vector3 butterflyAtMinionY2 = bvo4.transform.position;
					butterflyAtMinionY2.y = mo.transform.position.y;
					float dist3 = Vector3.Distance(mo.transform.position, butterflyAtMinionY2);
					if (dist3 <= 2f && !beeCaught)
					{
						bvo4.StingMinion(mo);
						caughtFliers.Add(bvo4);
						bvo4.IsGettingCaught = true;
						beeCaught = true;
					}
				}
			}
			int butterfliesCaught = 0;
			foreach (ButterflyCatchButterflyViewObject bvo3 in activeButterflies)
			{
				if (!bvo3.IsGettingCaught)
				{
					Vector3 butterflyAtMinionY = bvo3.transform.position;
					butterflyAtMinionY.y = mo.transform.position.y;
					float dist = Vector3.Distance(mo.transform.position, butterflyAtMinionY);
					if (dist <= 2f)
					{
						caughtFliers.Add(bvo3);
						bvo3.IsGettingCaught = true;
						butterfliesCaught++;
					}
				}
			}
			if (beeCaught)
			{
				mat.beeStingTimer = 3f;
			}
			else if (caughtFliers.Count > 0)
			{
				minionReactInRadiusSignal.Dispatch(15f, mo.transform.position);
			}
			else
			{
				mat.fallDownTimer = 1.5f;
				globalAudioSignal.Dispatch("Play_balloon_pop_01");
			}
			int addedScore = 0;
			if (butterfliesCaught >= 4)
			{
				globalAudioSignal.Dispatch("Play_minon_butterfly_celebrate_01");
				addedScore += 15;
			}
			foreach (ButterflyCatchButterflyViewObject bvo2 in caughtFliers)
			{
				if (bvo2.IsReallyABee)
				{
					addedScore += -1;
					continue;
				}
				addedScore += bvo2.myScore;
				GameObject vfx = Object.Instantiate(gameController.ButterflyCaughtVfxPrefab);
				vfx.transform.SetParent(base.transform, false);
				vfx.transform.position = bvo2.transform.position;
				Object.Destroy(vfx, 5f);
			}
			foreach (ButterflyCatchButterflyViewObject bvo in caughtFliers)
			{
				RemoveMeFromLists(bvo);
				if (!bvo.IsReallyABee)
				{
					Object.Destroy(bvo.gameObject);
				}
			}
			if (addedScore > 0)
			{
				spawnDooberSignal.Dispatch(mignetteHUD, mo.transform.position, addedScore, true);
				changeScoreSignal.Dispatch(addedScore);
				globalAudioSignal.Dispatch("Play_mignette_collect");
				totalScore += addedScore;
			}
			else if (addedScore < 0)
			{
				if (totalScore + addedScore >= 0)
				{
					changeScoreSignal.Dispatch(addedScore);
					totalScore += addedScore;
				}
				else
				{
					changeScoreSignal.Dispatch(-totalScore);
					totalScore = 0;
				}
			}
		}

		public void CleanupMignette()
		{
			if (ptmo != null)
			{
				Object.Destroy(ptmo.Minion.gameObject.GetComponent<Collider>());
			}
			BuildingViewReference.ToggleAmbientButterflies(playerService.HasPurchasedMinigamePack());
		}

		public override void Update()
		{
			base.Update();
			if (base.IsPaused || TimeElapsed >= 45f)
			{
				return;
			}
			butterflySpawnTimer -= Time.deltaTime;
			if (butterflySpawnTimer <= 0f)
			{
				butterflySpawnTimer = 2f;
				for (int i = 0; i < gameController.GetButterflySplineCount(); i++)
				{
					SpawnButterflyOrBee(gameController.GetButterflySpline(i), 1.1f);
				}
			}
			if (countdownTimer > 0f)
			{
				return;
			}
			if (minionsIdle)
			{
				minionsIdle = false;
				int num = 0;
				for (num = 0; num < activeMinionsList.Count; num++)
				{
					MinionAndTimer minionAndTimer = activeMinionsList[num];
					minionAndTimer.mo.SetAnimTrigger("Run");
					minionAndTimer.netViewObject.NetAnimator.SetTrigger("Run");
				}
			}
			UpdateMinionLocations();
			TimeElapsed += Time.deltaTime;
			if (TimeElapsed >= 45f)
			{
				TimeElapsed = 45f;
				int num2 = 0;
				for (num2 = 0; num2 < activeMinionsList.Count; num2++)
				{
					MinionAndTimer minionAndTimer2 = activeMinionsList[num2];
					minionAndTimer2.mo.PlayAnimation(Animator.StringToHash("Base Layer.Idle"), 0, 0f);
					minionAndTimer2.netViewObject.NetAnimator.Play(Animator.StringToHash("Base Layer.Idle"), 0, 0f);
				}
				Invoke("ShutDownMignette", 2f);
			}
			for (int j = 0; j < activeMinionsList.Count; j++)
			{
				MinionAndTimer minionAndTimer3 = activeMinionsList[j];
				minionAndTimer3.mo.SetAnimFloat("SwingAndMissTimer", minionAndTimer3.fallDownTimer);
				minionAndTimer3.netViewObject.NetAnimator.SetFloat("SwingAndMissTimer", minionAndTimer3.fallDownTimer);
				if (minionAndTimer3.fallDownTimer > 0f)
				{
					minionAndTimer3.fallDownTimer -= Time.deltaTime;
				}
				minionAndTimer3.mo.SetAnimFloat("BeeStingTimer", minionAndTimer3.beeStingTimer);
				minionAndTimer3.netViewObject.NetAnimator.SetFloat("BeeStingTimer", minionAndTimer3.beeStingTimer);
				if (minionAndTimer3.beeStingTimer > 0f)
				{
					minionAndTimer3.beeStingTimer -= Time.deltaTime;
					Vector3 vector = base.mignetteCamera.transform.position - minionAndTimer3.mo.transform.position;
					vector.y = 0f;
					minionAndTimer3.mo.transform.forward = vector.normalized;
					minionAndTimer3.netViewObject.transform.forward = vector.normalized;
				}
			}
		}

		private void SpawnButterflyOrBee(GoSpline path, float speed)
		{
			GameObject gameObject = gameController.SpawnBee(activeBees.Count, TimeElapsed);
			if (gameObject == null)
			{
				gameObject = gameController.SpawnButterfly(activeButterflies.Count, TimeElapsed);
			}
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				ButterflyCatchButterflyViewObject component = gameObject.GetComponent<ButterflyCatchButterflyViewObject>();
				component.FollowPath(this, path, speed);
				if (component.IsReallyABee)
				{
					activeBees.Add(component);
				}
				else
				{
					activeButterflies.Add(component);
				}
			}
		}

		private void UpdateMinionLocations()
		{
			int num = 0;
			for (num = 0; num < activeMinionsList.Count; num++)
			{
				MinionAndTimer minionAndTimer = activeMinionsList[num];
				MinionObject mo = minionAndTimer.mo;
				bool flag = false;
				if (mo.IsInAnimatorState(Animator.StringToHash("Base Layer.Run")) || mo.IsInAnimatorState(Animator.StringToHash("Base Layer.Success")))
				{
					flag = true;
				}
				if (flag)
				{
					minionAndTimer.timer += Time.deltaTime * 1.5f;
					GoSpline minionSpline = gameController.GetMinionSpline(num);
					if (minionAndTimer.timer >= minionSpline.pathLength)
					{
						minionAndTimer.timer -= minionSpline.pathLength;
					}
					float t = minionAndTimer.timer / minionSpline.pathLength;
					Vector3 pointOnPath = minionSpline.getPointOnPath(t);
					Vector3 vector = pointOnPath - mo.transform.position;
					mo.transform.position = pointOnPath;
					mo.transform.rotation = Quaternion.Slerp(mo.transform.rotation, Quaternion.LookRotation(vector.normalized), Time.deltaTime * 10f);
					minionAndTimer.netViewObject.transform.position = mo.transform.position;
					minionAndTimer.netViewObject.transform.rotation = mo.transform.rotation;
				}
			}
		}

		public void ShutDownMignette()
		{
			requestStopMignetteSignal.Dispatch(true);
		}

		public void RemoveMeFromLists(ButterflyCatchButterflyViewObject butterflyOrBee)
		{
			if (activeBees.Contains(butterflyOrBee))
			{
				activeBees.Remove(butterflyOrBee);
			}
			if (activeButterflies.Contains(butterflyOrBee))
			{
				activeButterflies.Remove(butterflyOrBee);
			}
		}
	}
}
