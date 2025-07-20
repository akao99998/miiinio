using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch.View
{
	public class ButterflyCatchButterflyViewObject : MonoBehaviour
	{
		private enum StingState
		{
			None = 0,
			BounceToStartPoint = 1,
			StingIn = 2,
			Stinging = 3,
			StingOut = 4
		}

		public bool IsGettingCaught;

		public bool IsReallyABee;

		public GameObject BeeOrButterflyModel;

		public float TimeTillFlyAway;

		public Renderer RendererForMaterial;

		public Animator ButterflyAnimator;

		public Animator BeeAnimator;

		private ButterflyCatchMignetteManagerView myParentView;

		private MinionObject MinionToSting;

		private GoSpline myPath;

		private float pathProgress;

		private float totalPathTime;

		private float timeTillExpire;

		private float mySpeed = 1f;

		public int myScore;

		private bool isFlyingAway;

		private Vector3 flyAwayTarget;

		private float flapTimer = 2f;

		private float stingTimer;

		private Vector3 stingStartPos;

		private StingState BeeStingState;

		public ParticleSystem AngryBeeParticleSystem;

		public ParticleSystem StingBurstParticleSystem;

		public float TimeToGetInStingPosition = 1.3f;

		private Transform myTransform;

		public AnimationCurve beeCurveZ;

		private bool stingTriggered;

		private void Start()
		{
			isFlyingAway = false;
			myTransform = base.transform;
		}

		public void FollowPath(ButterflyCatchMignetteManagerView parentView, GoSpline path, float speed)
		{
			myParentView = parentView;
			myPath = path;
			base.transform.position = myPath.getPointOnPath(0f);
			mySpeed = speed;
			totalPathTime = myPath.pathLength / mySpeed;
			StartFlapping();
		}

		public void StingMinion(MinionObject mo)
		{
			MinionToSting = mo;
			BeeStingState = StingState.BounceToStartPoint;
			stingTimer = 0f;
			stingStartPos = base.transform.position;
			AngryBeeParticleSystem.Stop();
			AngryBeeParticleSystem.Clear();
			AngryBeeParticleSystem.Play();
		}

		private void Update()
		{
			if (myParentView.IsPaused || UpdateStingStatus() || UpdateFlyAwayStatus())
			{
				return;
			}
			if (myPath != null)
			{
				pathProgress += Time.deltaTime * mySpeed;
				if (pathProgress >= totalPathTime)
				{
					pathProgress -= totalPathTime;
				}
				float t = pathProgress / totalPathTime;
				Vector3 pointOnPath = myPath.getPointOnPath(t);
				Vector3 vector = pointOnPath - myTransform.position;
				myTransform.forward = vector.normalized;
				myTransform.position = pointOnPath;
			}
			if (TimeTillFlyAway > 0f)
			{
				TimeTillFlyAway -= Time.deltaTime;
				if (TimeTillFlyAway <= 0f)
				{
					isFlyingAway = true;
					flyAwayTarget = myTransform.position;
					if (myTransform.position.x >= myParentView.MignetteBuildingObject.GetComponent<Collider>().bounds.center.x)
					{
						flyAwayTarget.x += 20f;
					}
					else
					{
						flyAwayTarget.x -= 20f;
					}
					flyAwayTarget.y += 10f;
					myTransform.forward = (flyAwayTarget - myTransform.position).normalized;
					timeTillExpire = 5f;
				}
			}
			if (!(flapTimer > 0f))
			{
				return;
			}
			flapTimer -= Time.deltaTime;
			if (flapTimer <= 0f)
			{
				if (ButterflyAnimator != null)
				{
					ButterflyAnimator.SetBool("isFlapping", false);
				}
				Invoke("StartFlapping", Random.Range(0f, 1.5f));
			}
		}

		public bool UpdateFlyAwayStatus()
		{
			if (isFlyingAway)
			{
				myTransform.position = Vector3.MoveTowards(myTransform.position, flyAwayTarget, Time.deltaTime * mySpeed * 5f);
				myTransform.LookAt(flyAwayTarget);
				timeTillExpire -= Time.deltaTime;
				if (timeTillExpire <= 0f)
				{
					myParentView.RemoveMeFromLists(this);
					Object.Destroy(base.gameObject);
				}
				return true;
			}
			return false;
		}

		public bool UpdateStingStatus()
		{
			if (BeeStingState == StingState.None)
			{
				return false;
			}
			Transform transform = MinionToSting.transform;
			switch (BeeStingState)
			{
			case StingState.BounceToStartPoint:
			{
				stingTimer += Time.deltaTime;
				Vector3 position = transform.position;
				position.y += 1f;
				position.x -= 1f;
				Vector3 position2 = Vector3.Lerp(stingStartPos, position, stingTimer / TimeToGetInStingPosition);
				position2.z += beeCurveZ.Evaluate(stingTimer / TimeToGetInStingPosition);
				position.x += 2f;
				myTransform.position = position2;
				myTransform.LookAt(position);
				if (stingTimer >= TimeToGetInStingPosition)
				{
					BeeStingState = StingState.Stinging;
				}
				stingTriggered = false;
				break;
			}
			case StingState.Stinging:
			{
				Vector3 position = transform.position;
				if (BeeAnimator != null && !stingTriggered)
				{
					stingTriggered = true;
					BeeAnimator.Play("beeStinging");
				}
				break;
			}
			}
			Vector3 vector = myParentView.mignetteCamera.transform.position - myTransform.position;
			AngryBeeParticleSystem.transform.position = myTransform.position + vector.normalized;
			return true;
		}

		public void StartFlapping()
		{
			if (ButterflyAnimator != null)
			{
				ButterflyAnimator.SetBool("isFlapping", true);
			}
			flapTimer = Random.Range(1f, 3f);
		}

		public void BeeFireStingFX()
		{
			StingBurstParticleSystem.Stop();
			StingBurstParticleSystem.Clear();
			StingBurstParticleSystem.Play();
			myParentView.globalAudioSignal.Dispatch("Play_balloon_pop_01");
		}

		public void CompleteStingAnim()
		{
			isFlyingAway = true;
			timeTillExpire = 5f;
			stingTriggered = false;
			BeeStingState = StingState.None;
		}
	}
}
